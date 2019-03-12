using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace LANTran
{
    [Serializable]
    public class Message {
    private  string userName;
    private  string content;
    private  DateTime postDate;
    private Image picture;

    public Message(string userName, string content,Image bitmap) {
        this.userName = userName;
        this.content = content;
        this.postDate = DateTime.Now;
        this.picture = bitmap;
    }

    public Message(string content) : this("System", content,null) { }
    
    public Message() { }
    public string UserName {
        get { return userName; }
    }

    public string Content {
        get { return content; }
    }

    public DateTime PostDate {
        get { return postDate; }
    }

        public Image Picture
        {
            get { return picture; }
        }

    public override string ToString() {
        return String.Format("{0}[{1}]：\r\n{2}\r\n", userName, postDate, content);
    }
}
}
