using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LANTran
{
    class LANSearch
    {
        public List<string> IPList;

        public  LANSearch()
        {
          IPList=new List<string>();
        }

        public bool Search()
        {
            try
            {
                Ping myPing;

                myPing = new Ping();
                var tasks = new List<Task>();
                for (int i = 101; i <= 103; i++)
                {



                    string pingIP = "192.168.1." + i.ToString();
                    var task = PingAndUpdateNodeAsync(myPing, pingIP);
                   
                    tasks.Add(task);
                }
                
                Task.WhenAll(tasks);
            }
            catch
            {
                return false;
            }
            return true;
        }
        private async Task PingAndUpdateNodeAsync(Ping ping, string pingIP)
        {
            var reply = await ping.SendPingAsync(pingIP, 50);
            if (reply.Status == IPStatus.Success)
            {
                IPList.Add(pingIP);

            }
        }
    }
}
