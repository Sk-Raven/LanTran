using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LANTran
{
    class FileSender 
    {
        private Socket client;
        private int PacketSize;
        private int LastDataPacket;
        private int PacketCount;
        private int Position;
        private IPEndPoint ipep;
        private Stopwatch stopwatch=new Stopwatch();
        private int speed;

        public bool Connect(IPAddress ip, int port)
        {
            try
            {
                ipep = new IPEndPoint(ip,port);
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(ipep);
            }
            catch
            {
                
                return false;
            }
            return true;
        }

        public bool SendFile(FileInfo EzoneFile)
        {
            FileStream EzoneStream = EzoneFile.OpenRead();
            PacketSize = 8192;

            //包的数量
            PacketCount = (int)(EzoneStream.Length / ((long)PacketSize));

            //最后一个包的大小
            LastDataPacket = (int)(EzoneStream.Length - ((long)(PacketSize * PacketCount)));
            //指向远程服务端节点

            IPEndPoint clientep = (IPEndPoint)client.RemoteEndPoint;


            //发送[文件名]到客户端
            TransferFiles.SendVarData(client, System.Text.Encoding.Unicode.GetBytes(EzoneFile.Name));

            //发送[包的大小]到客户端
            TransferFiles.SendVarData(client, System.Text.Encoding.Unicode.GetBytes(PacketSize.ToString()));

            //发送[包的总数量]到客户端
            TransferFiles.SendVarData(client, System.Text.Encoding.Unicode.GetBytes(PacketCount.ToString()));

            //发送[最后一个包的大小]到客户端
            TransferFiles.SendVarData(client, System.Text.Encoding.Unicode.GetBytes(LastDataPacket.ToString()));

            bool isCut = false;
            //数据包
            byte[] data = new byte[PacketSize];
            Position = 0;
            //开始循环发送数据包
            for (int i = 0; i < PacketCount; i++)
            {
                //从文件流读取数据并填充数据包
                EzoneStream.Read(data, 0, data.Length);
                //发送数据包
                Position++;
                stopwatch.Reset();
                stopwatch.Start();
                if (TransferFiles.SendVarData(client, data) == 3)
                {
                    isCut = true;
                    return false;

                }
                stopwatch.Stop();
                if (stopwatch.ElapsedMilliseconds == 0)
                    speed = 0;
                else
                {
                    speed = (1000*data.Length) / (int)(stopwatch.ElapsedMilliseconds*1000);
                }
                

            }

            //如果还有多余的数据包，则应该发送完毕！
            if (LastDataPacket != 0)
            {
                data = new byte[LastDataPacket];
                EzoneStream.Read(data, 0, data.Length);
                TransferFiles.SendVarData(client, data);
            }

            PacketCount = Position = 0;
            //关闭文件流
            EzoneStream.Close();
            return true;

        }

        public float getProgress()
        {
            if (PacketCount == 0)
                return 0;
            
            return (float)Position / PacketCount;
        }

        public void SignOut()
        {
            if (client != null)
                client.Close();
        }

        public int getspeed()
        {
            return speed;
        }

    }
}
