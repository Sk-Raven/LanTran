using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace LANTran
{
    interface IMessageSender
    {
        bool Connect(IPAddress ip, int port);       // 连接到服务端
        bool SendMessage(Message msg);              // 发送用户
        void SignOut();                                 // 注销系统
    }
}
