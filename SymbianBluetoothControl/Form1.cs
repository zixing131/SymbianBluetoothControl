using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using System.Windows.Forms;
using zlib;

namespace SymbianBluetoothControl
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private bool isrunning = false;
        private void button1_Click(object sender, EventArgs e)
        {
            byte[] t = new byte[] { 1, 3, 5, 4 };
            //var d = CompressData(t);
            //var c = DecompressData(d); 

            serialPort1.PortName = textBox1.Text;
            serialPort1.BaudRate = 115200;
            serialPort1.ReadTimeout = 1000;
            serialPort1.WriteTimeout = 1000;
            isrunning = true;
            serialPort1.Open();
            serialPort1.DataReceived += SerialPort1_DataReceived;
        }

        private List<byte> picdata = new List<byte>();
        private void DealPicData()
        {
            var dedata = DecompressData(picdata.ToArray());
            var img = byteArrayToImage(dedata);
            pictureBox1.Image = img;
        }
        /// <summary>
        /// 字节数组生成图片
        /// </summary>
        /// <param name="Bytes">字节数组</param>
        /// <returns>图片</returns>
        private Image byteArrayToImage(byte[] Bytes)
        {
            using (MemoryStream ms = new MemoryStream(Bytes))
            {
                Image outputImg = Image.FromStream(ms);
                return outputImg;
            }
        }

        private void SerialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {

            try
            {
                if (serialPort1.IsOpen == false || isrunning == false)
                {
                    return;
                }
                byte[] ReDatas = new byte[serialPort1.BytesToRead];
                serialPort1.Read(ReDatas, 0, ReDatas.Length);//读取数据 

                Console.WriteLine(string.Join(" ", ReDatas));

                if(ReDatas.Length>8)
                {
                     int endflag = ReDatas.Length;
                    if(ReDatas[0] == 1 && ReDatas[1] == 2 && ReDatas[2] == 3 && ReDatas[endflag - 1] == 6 && ReDatas[endflag - 2] == 5 && ReDatas[endflag - 3] == 4)
                    {
                        picdata.Clear();
                        picdata.AddRange(ReDatas.Skip(3).Take(endflag - 6).ToArray());
                        DealPicData();
                        picdata.Clear();
                        return;
                    }

                    if (ReDatas[0] == 1 && ReDatas[1] == 2 && ReDatas[2] == 3)
                    {   //起始位
                        picdata.Clear();
                        picdata.AddRange(ReDatas.Skip(3).ToArray());
                    }
                    else if (ReDatas[endflag - 1] == 6 && ReDatas[endflag - 2] == 5 && ReDatas[endflag - 3] == 4)
                    {   //结束位
                        picdata.AddRange(ReDatas.Take(endflag - 3).ToArray());
                        DealPicData();
                        picdata.Clear();
                    }
                    else
                    {
                        picdata.AddRange(ReDatas);
                    }
                }

                 
            }
            catch (Exception ex)
            {

            }

        }

        public static byte[] CompressData(byte[] inData)
        {
            byte[] byteArray = inData;
            byte[] tmpArray;

            using (MemoryStream ms = new MemoryStream())
            {

                using (zlib.ZOutputStream outZStream = new zlib.ZOutputStream(ms, zlib.zlibConst.Z_DEFAULT_COMPRESSION))
                {
                    outZStream.Write(byteArray, 0, byteArray.Length);
                    outZStream.Flush();


                }
                tmpArray = ms.ToArray();
            }
            return tmpArray;
        }

        public static byte[] DecompressData(byte[] inData)
        {
            int data = 0;
            int stopByte = -1;
            byte[] Buffer = inData; // 解base64
            MemoryStream intms = new MemoryStream(Buffer);
            zlib.ZInputStream inZStream = new zlib.ZInputStream(intms);
            int count = 1024 * 1024;
            byte[] inByteList = new byte[count];
            int i = 0;
            while (stopByte != (data = inZStream.Read()))
            {
                inByteList[i] = (byte)data;
                i++;
            }
            inZStream.Close();
            return inByteList.Take(i).ToArray();
        } 

    }
}
