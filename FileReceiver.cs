using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace LANTran
{

    class FileReceiver : IMessageReceiver
    {
        public event MessageReceivedEventHandler MessageReceived;
        public event ConnectionLostEventHandler ClientLost;
        public event ClientConnectedEventHandler ClientConnected;
        public event MessageReceivedEventHandler FileReceived;
        public event MessageReceivedEventHandler FileReceivedFinish;
        private int SendedCount = 0;
        private int bagCount = 0;

        private Thread workerThread;
        private Stopwatch stopwatch;
        private static Socket serverSocket;
        private int speed;

        public FileReceiver()
        {
            stopwatch=new Stopwatch();
            ((IMessageReceiver) this).StartListen();
        }

        void IMessageReceiver.StartListen()
        {
            ThreadStart start = new ThreadStart(ListenThreadMethod);
            workerThread = new Thread(start)
            {
                IsBackground = true
            };
            workerThread.Start();
        }

        private void ListenThreadMethod()
        {
            IPAddress localIp = IPAddress.Parse(GetInternalIP());
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(localIp, 6667));  //绑定IP地址：端口
            serverSocket.Listen(10);    //设定最多10个排队连接请求
          
            IPEndPoint endPoint = serverSocket.LocalEndPoint as IPEndPoint;
            int portNumber = endPoint.Port;
            //PortNumberReady?.Invoke(portNumber);        // 端口号已经OK，通知用户界面
            while (true)
            {
                if (serverSocket != null)
                {
                    try
                    {
                        Socket clientSocket = serverSocket.Accept();
                        Thread receiveThread = new Thread(Create);
                        receiveThread.Start(clientSocket);
                    }
                    catch (Exception ex)
                    {
                        ClientLost(ex.Message);
                        break;
                    }
                }
            }
        }

        public  void Create(object clientSocket)
        {
            while (true)
            {


                Socket client = clientSocket as Socket;
                //获得客户端节点对象
                IPEndPoint clientep = (IPEndPoint) client.RemoteEndPoint;

                //获得[文件名]   
                string SendFileName = System.Text.Encoding.Unicode.GetString(TransferFiles.ReceiveVarData(client));


                //获得[包的大小]   
                string bagSize = System.Text.Encoding.Unicode.GetString(TransferFiles.ReceiveVarData(client));

                //获得[包的总数量]   
                bagCount = int.Parse(System.Text.Encoding.Unicode.GetString(TransferFiles.ReceiveVarData(client)));

                //获得[最后一个包的大小]   
                string bagLast = System.Text.Encoding.Unicode.GetString(TransferFiles.ReceiveVarData(client));


                string fullPath = Path.Combine(Environment.CurrentDirectory, SendFileName);
                //创建一个新文件   
                FileStream MyFileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);

                //已发送包的个数   
                SendedCount = 0;
                Message temp = new Message("sys", SendFileName + "正在接收！", null);
                FileReceived(temp.ToString());
                while (SendedCount < (bagCount+1))
                {

                    stopwatch.Reset();
                    stopwatch.Start();
                    byte[] data = TransferFiles.ReceiveVarData(client);
                    stopwatch.Stop();
                    if (stopwatch.ElapsedMilliseconds == 0)
                        speed = 0;
                    else
                    {
                        speed = (1000 * data.Length) / (int)(stopwatch.ElapsedMilliseconds * 1000);
                    }
                    if (data.Length == 0)
                    {
                        break;
                    }
                    else
                    {

                        SendedCount++;
                        //将接收到的数据包写入到文件流对象   
                        MyFileStream.Write(data, 0, data.Length);

                        //显示已发送包的个数     

                    }
                }

                SendedCount = bagCount = 0;
                //关闭文件流   
                MyFileStream.Close();
                //关闭套接字   
                //client.Close();
                Message temp1 = new Message("sys", SendFileName + "接收完毕！", null);
                FileReceivedFinish(temp1.ToString());
            }

        }
    

        public void StopListen()
        {
            try
            {
                serverSocket.Close();
                serverSocket = null;
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

        public float getProgress()
        {
            if (bagCount == 0)
                return 0;
            return (float)SendedCount / bagCount;
        }

        public int getspeed()
        {
            return speed;
        }

      
    }
}
