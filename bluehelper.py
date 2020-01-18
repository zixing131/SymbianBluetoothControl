#coding = u8
import socket, appuifw,graphics,zlib,time
def cn(x):return x.decode("utf8")
def chat_client():
    conn = socket.socket(socket.AF_BT, socket.SOCK_STREAM)
    address, services = socket.bt_discover()
    print  services
    channel = services["COM11"]
    conn.connect((address, channel))
    print "Connected to server!"
    conn.send("hello")
    while 1:
        img=graphics.screenshot()
        img.save("d:\\t.png")
        t=open("d:\\t.png","rb").read()
        out=zlib.compress(t,zlib.Z_BEST_COMPRESSION)
        conn.send("010203".decode("hex")+out+"040506".decode("hex"))
        time.sleep(0)


chat_client()
