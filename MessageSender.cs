using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LANTran
{
    public class MessageSender: IMessageSender
    {
        TcpClient client;
        Stream streamToServer;

        // 连接至远程
        public bool Connect(IPAddress ip, int port)
        {
            try
            {
                client = new TcpClient();
                client.Connect(ip, port);
                streamToServer = client.GetStream();    // 获取连接至远程的流
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 发送消息
        public bool SendMessage(Message msg)
        {
            try
            {
                lock (streamToServer)
                {
                    //byte[] buffer = Encoding.Unicode.GetBytes(msg.ToString());
                    BinaryFormatter b = new BinaryFormatter();
                    var ms = new MemoryStream();
                    b.Serialize(ms, msg);
                    streamToServer.Write(ms.GetBuffer(), 0, ms.GetBuffer().Length);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // 注销
        public void SignOut()
        {
            if (streamToServer != null)
                streamToServer.Dispose();
            if (client != null)
                client.Close();
        }


    }
}
