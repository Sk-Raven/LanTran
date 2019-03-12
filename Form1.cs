using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace LANTran
{
    public partial class Form1 : Form
    {
        private Talker talker;
        private string userName;
        string host = "0.0.0.0";
        private bool isTran = false;
        private bool isRec = false;
        int port = 6666;
       

        public Form1()
        {

            Ping ping = new Ping();
            PingReply pingReply = ping.Send("192.168.1.102", 500);
            if (pingReply.Status == IPStatus.Success)
            {

                Console.WriteLine("当前在线，已ping通！");
            }
            else
            {
                Console.WriteLine("不在线，ping不通！");
            }


            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            userName = Dns.GetHostName();
            talker = new Talker();
            Text = userName + " Talking ...";
            talker.ClientLost +=
                talker_ClientLost;
            talker.ClientConnected +=
               talker_ClientConnected;
            talker.MessageReceived +=
                talker_MessageReceived;
            talker.FileReceived +=
                talker_FileReceived;
            talker.FileReceivedFinish += talk_FileReceivedFinish;
            progressBar1.Maximum = 100;
            progressBar1.Minimum = 0;

            // if (talker.ConnectByHost(lanSearch.IPList[0], port))
            //  {
            //    richTextBox1.Text +=
            //        String.Format("System[{0}]：\r\n已成功连接至远程\r\n", DateTime.Now);
            //    talker.IsConnect = true;
            //  }                
            //



        }
        //接收到文件
        void talker_FileReceived(string msg)
        {
            MessageReceivedEventHandler del = delegate (string m)
            {
                richTextBox1.Text += m;

            };
            richTextBox1.Invoke(del, msg);
            isRec = false;
            Thread myThread = new Thread(() =>
            {
                while (!isRec)
                {
                    setProgressBar("received");
                }
               
            });
            myThread.IsBackground = true;
            myThread.Start();
           

        }

        void talk_FileReceivedFinish(string msg)
        {
            MessageReceivedEventHandler del = delegate (string m)
            {
                richTextBox1.Text += m;

            };
            richTextBox1.Invoke(del, msg);
            isRec = true;
        }



        // 接收到消息
        void talker_MessageReceived(string msg)
        {
            MessageReceivedEventHandler del = delegate(string m)
            {
                richTextBox1.Text += m;
              
            };
            richTextBox1.Invoke(del, msg);
        }

        // 有客户端连接到本机
        void talker_ClientConnected(IPEndPoint endPoint)
        {
            ClientConnectedEventHandler del = delegate(IPEndPoint end)
            {
                IPHostEntry host = Dns.GetHostEntry(end.Address);
                richTextBox1.Text +=
                    String.Format("System[{0}]：\r\n远程主机{1}连接至本地。\r\n", DateTime.Now, end.Address);

                if (!talker.IsConnect)
                {
                    if (talker.ConnectByHost(end.Address.ToString(), port))
                    {

                        richTextBox1.Text +=
                            String.Format("System[{0}]：\r\n已成功连接至远程\r\n", DateTime.Now);

                    }
                    else
                    {
                        richTextBox1.Text +=
                            String.Format("System[{0}]：\r\n未能连接至{1}", DateTime.Now, end.Address);
                    }
                }
            };


            if (IsHandleCreated)
                richTextBox1.Invoke(del, endPoint);
        }

        // 客户端连接断开
        void talker_ClientLost(string info)
        {
            ConnectionLostEventHandler del = delegate(string information)
            {
                richTextBox1.Text +=
                    String.Format("System[{0}]：\r\n{1}\r\n", DateTime.Now, information);
                talker.IsConnect = false;
            };
            richTextBox1.Invoke(del, info);
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

     




        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(richTextBox2.Text))
            {
                MessageBox.Show("请输入内容！");
                richTextBox2.Clear();
                richTextBox2.Focus();
                return;
            }
            string a = richTextBox2.SelectedRtf;
            richTextBox1.Rtf += a;
            Message msg = new Message(userName, richTextBox2.Text,null);
            if (talker.SendMessage(msg))
            {
                richTextBox1.Text += msg.ToString();
                richTextBox2.Clear();
            }
            else
            {
                richTextBox2.Text +=
                    String.Format("System[{0}]：\r\n远程主机已断开连接\r\n", DateTime.Now);
                
            }

        }

       
        
       
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                //取消"关闭窗口"事件
                e.Cancel = true; // 取消关闭窗体 

                //使关闭时窗口向右下角缩小的效果
                WindowState = FormWindowState.Minimized;
                notifyIcon1.Visible = true;
                //this.m_cartoonForm.CartoonClose();
                Hide();
              
            }
         
         
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                talker.Dispose();
                Application.Exit();
            }
            catch
            {
            }

        }


        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (Visible)
                {
                    WindowState = FormWindowState.Minimized;
                    notifyIcon1.Visible = true;
                    Hide();
                }
                else
                {
                    Visible = true;
                    WindowState = FormWindowState.Normal;
                    Activate();
                }

            }
        }

        private void richTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)//如果输入的是回车键
            {
                button1_Click(sender, e);//触发button事件
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1=new OpenFileDialog();
            openFileDialog1.Filter = "图片文件|*.jpg|所有文件|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Clipboard.SetDataObject(Image.FromFile(openFileDialog1.FileName), false);
                //richTextBox2.Paste();
                Message msg = new Message(userName, openFileDialog1.SafeFileName, Image.FromFile(openFileDialog1.FileName));
                if (talker.SendMessage(msg))
                {
                    richTextBox1.Text += msg.ToString();
                    richTextBox2.Clear();
                }
                else
                {
                    richTextBox2.Text +=
                        String.Format("System[{0}]：\r\n远程主机已断开连接\r\n", DateTime.Now);

                }
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (isTran)
            {
                MessageBox.Show("文件正在传输");
                return;
            }
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "所有文件|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //创建一个文件对象
                FileInfo EzoneFile = new FileInfo(openFileDialog1.FileName);
                richTextBox1.Text +="{sys}：\r\n文件发送中\r\n";
                isTran = true;
                Thread myThread = new Thread(() =>
                {
                    while (isTran)
                    {
                        setProgressBar("send");
                    }

                })
                {
                    IsBackground = true
                };
                myThread.Start();
                Thread mThread = new Thread(() =>
                {
                    if (talker.SendFile(EzoneFile))
                        richTextBox1.Text += "{sys}：\r\n文件发送成功\r\n";
                    else
                    {
                        richTextBox1.Text += "{sys}：\r\n文件发送失败\r\n";
                    }

                    isTran = false;
                });
                mThread.Start();


            }
        }

        private void setProgressBar(string type)
        {
            if (type.Equals("send"))
            {
                while ((1 - talker.getfileSendProgress()) > 0.001)
                {
                    progressBar1.Value = (int) (talker.getfileSendProgress() * 100);
                    Text = talker.SendFileSpeed().ToString() + "KB/S";

                }
               
            }
               
            
               
            
            else if (type.Equals("received"))
            {
                while ((1 - talker.getfileReceiverProgress()) > 0.001)
                {
                    progressBar1.Value = (int) (talker.getfileReceiverProgress() * 100);
                    Text = talker.ReceivedFileSpeed().ToString() + "KB/S";
                }
                
            }
           
        }

         
        

        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form form2=new Form2();
           // form2.MdiParent = this;
            form2.Owner = this;
            form2.Show();
            
        }

        public void Connect()
        {
            if (talker.ConnectByHost(host, port))
            {
                richTextBox1.Text +=
                    String.Format("System[{0}]：\r\n已成功连接至远程\r\n", DateTime.Now);
                talker.IsConnect = true;
            }
        }

        public void sethost(string str)
        {
            host = str;
        }
    }
}



