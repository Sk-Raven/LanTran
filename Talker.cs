using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace LANTran
{
   
    class Talker
    {
        public IMessageReceiver receiver;
        private IMessageSender sender;
        private FileReceiver fileReceiver;
        private FileSender fileSender;
        public bool IsConnect{ get; set; }
        public Talker()
        {
            this.receiver = new MessageReceiver();
            this.sender = new MessageSender();
            this.fileReceiver=new FileReceiver();
            this.fileSender=new FileSender();
            IsConnect = false;
        }

        public Talker(IMessageReceiver receiver, IMessageSender sender)
        {
            this.receiver = receiver;
            this.sender = sender;
        }
        public event MessageReceivedEventHandler MessageReceived
        {
            add
            {
                receiver.MessageReceived += value;
                fileReceiver.MessageReceived += value;
            }
            remove
            {
                receiver.MessageReceived -= value;
                fileReceiver.MessageReceived -= value;
            }
        }

        public event ClientConnectedEventHandler ClientConnected
        {
            add
            {
                receiver.ClientConnected += value;
                fileReceiver.ClientConnected += value;
            }
            remove
            {
                receiver.ClientConnected -= value;
                fileReceiver.ClientConnected -= value;
            }
        }

        public event ConnectionLostEventHandler ClientLost
        {
            add
            {
                receiver.ClientLost += value;
                fileReceiver.ClientLost += value;
            }
            remove
            {
                receiver.ClientLost -= value;
                fileReceiver.ClientLost -= value;
            }
        }

        public event MessageReceivedEventHandler FileReceived
        {
            add
            {
              
                fileReceiver.FileReceived += value;
            }
            remove
            {
             
                fileReceiver.FileReceived -= value;
            }
        }

        public event MessageReceivedEventHandler FileReceivedFinish
        {
            add
            {
                fileReceiver.FileReceivedFinish += value;
                
            }
            remove { fileReceiver.FileReceivedFinish -= value; }
        }

        // 注意这个事件
        //public event PortNumberReadyEventHandler PortNumberReady
        //{
        //    add
        //    {
        //        ((MessageReceiver)receiver).PortNumberReady += value;
        //    }
        //    remove
        //    {
        //        ((MessageReceiver)receiver).PortNumberReady -= value;
        //    }
        //}


        // 连接远程 - 使用主机名
        public bool ConnectByHost(string hostName, int port)
        {
            IPAddress[] ips = Dns.GetHostAddresses(hostName);
            return sender.Connect(ips[0], port)&&fileSender.Connect(ips[0], port+1);
        }

        // 连接远程 - 使用IP
        public bool ConnectByIp(string ip, int port)
        {
            IPAddress ipAddress;
            try
            {
                ipAddress = IPAddress.Parse(ip);
            }
            catch
            {
                return false;
            }
           
            return sender.Connect(ipAddress, port) && fileSender.Connect(ipAddress, port + 1);
        }


        // 发送消息
        public bool SendMessage(Message msg)
        {
            return sender.SendMessage(msg);
        }

        public bool SendFile(FileInfo EzoneFile)
        {
            return fileSender.SendFile(EzoneFile);
        }

        // 释放资源，停止侦听
        public void Dispose()
        {
            try
            {
                sender.SignOut();
                receiver.StopListen();
                fileReceiver.StopListen();
                fileSender.SignOut();
            }
            catch
            {
            }
        }

        // 注销
        public void SignOut()
        {
            try
            {
                sender.SignOut();
            }
            catch
            {
            }
        }

        public float getfileSendProgress()
        {
            return fileSender.getProgress();
        }

        public float getfileReceiverProgress()
        {
            return fileReceiver.getProgress();
        }

        public int SendFileSpeed()
        {
            return fileSender.getspeed();
        }

        public int ReceivedFileSpeed()
        {
            return fileReceiver.getspeed();
        }
    }
}
