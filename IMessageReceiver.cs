using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace LANTran
{
    public delegate void MessageReceivedEventHandler(string msg);
    public delegate void ClientConnectedEventHandler(IPEndPoint endPoint);
    public delegate void ConnectionLostEventHandler(string info);
    interface IMessageReceiver
    {
        event MessageReceivedEventHandler MessageReceived; // 接收到发来的消息
        event ConnectionLostEventHandler ClientLost;            // 远程主动断开连接
        event ClientConnectedEventHandler ClientConnected;  // 远程连接到了本地
        void StartListen();         // 开始侦听端口
        void StopListen();          // 停止侦听端口

    }
}
