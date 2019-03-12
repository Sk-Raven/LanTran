using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace LANTran
{
    public delegate void PortNumberReadyEventHandler(int portNumber);
    public class MessageReceiver:IMessageReceiver
    {
        public event MessageReceivedEventHandler MessageReceived;
        public event ConnectionLostEventHandler ClientLost;
        public event ClientConnectedEventHandler ClientConnected;

        // 当端口号Ok的时候调用 -- 需要告诉用户界面使用了哪个端口号在侦听
        // 这里是业务上体现不出来，在实现中才能体现出来的
        //public event PortNumberReadyEventHandler PortNumberReady;

        private Thread workerThread;
        private TcpListener listener;

        public MessageReceiver()
        {
            ((IMessageReceiver)this).StartListen();
        }

        // 开始侦听：显示实现接口
        void IMessageReceiver.StartListen()
        {
            ThreadStart start = new ThreadStart(ListenThreadMethod);
            workerThread = new Thread(start);
            workerThread.IsBackground = true;
            workerThread.Start();
        }

        // 线程入口方法
        private void ListenThreadMethod()
        {
            //string hostname = Dns.GetHostName();
            //IPHostEntry localhost = Dns.GetHostEntry(hostname);
            //IPAddress localIp = localhost.AddressList[0];
            IPAddress localIp = IPAddress.Parse(GetInternalIP());
            listener = new TcpListener(localIp, 6666);
            listener.Start();

            // 获取端口号
            IPEndPoint endPoint = listener.LocalEndpoint as IPEndPoint;
            int portNumber = endPoint.Port;
            //PortNumberReady?.Invoke(portNumber);        // 端口号已经OK，通知用户界面

            while (true)
            {
                TcpClient remoteClient;
                try
                {
                    remoteClient = listener.AcceptTcpClient();
                }
                catch 
                {
                    
                    break;
                }
                if (ClientConnected != null)
                {
                    // 连接至本机的远程端口
                    endPoint = remoteClient.Client.RemoteEndPoint as IPEndPoint;
                    ClientConnected(endPoint);      // 通知用户界面远程客户连接
                }

                //Stream streamToClient = remoteClient.GetStream();
                NetworkStream networkStream = remoteClient.GetStream();
                while (true)
                {
                    try {
                        if (networkStream.CanRead)
                        {
                            byte[] buffer = new byte[8192];
                            var ms = new MemoryStream();
                            int numberOfBytesRead = 0;
                            do
                            {
                                int bytesRead = networkStream.Read(buffer, 0, 8192);
                                ms.Write(buffer, 0, bytesRead);
                                numberOfBytesRead += bytesRead;
                            } while (networkStream.DataAvailable);
                            ms.Seek(0, SeekOrigin.Begin);
                            if (MessageReceived != null)
                            {
                                BinaryFormatter b = new BinaryFormatter();
                                Message temp = b.Deserialize(ms) as Message;
                                //string msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                                if (temp.Picture != null)
                                {
                                    temp.Picture.Save(temp.Content);
                                }
                                ms.Close();
                                MessageReceived(temp.ToString());       // 已经收到消息
                            }

                        }
                    }catch(Exception ex)
                    {
                        ClientLost(ex.Message);
                        break;
                    }
              
                    }
                    //byte[] buffer = new byte[8192];
                    //var ms = new MemoryStream();

                    //while (true)
                    //{
                    //    try
                    //    {
                    //        int bytesRead = streamToClient.Read(buffer, 0, 8192);
                    //        if (bytesRead == 0)
                    //        {
                    //            throw new Exception("客户端已断开连接");
                    //        }
                    //       // string msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                    //        ms.Write(buffer,0,bytesRead);
                    //        ms.Seek(0, SeekOrigin.Begin);
                    //        if (MessageReceived != null)
                    //        {
                    //            BinaryFormatter b = new BinaryFormatter();
                    //            Message temp = b.Deserialize(ms) as Message;
                    //            //string msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                    //            if (temp.Picture != null)
                    //            {
                    //                temp.Picture.Save("1.png");
                    //            }

                    //            MessageReceived(temp.ToString());       // 已经收到消息
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        if (ClientLost != null)
                    //        {
                    //            ClientLost(ex.Message);     // 客户连接丢失
                    //            break;                      // 退出循环
                    //        }
                    //    }
                    //}
                }
        }

        // 停止侦听端口
        public void StopListen()
        {
            try
            {
                listener.Stop();
                listener = null;
                workerThread.Abort();
            }
            catch { }
        }

        private string GetInternalIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
       
     
    }
}
