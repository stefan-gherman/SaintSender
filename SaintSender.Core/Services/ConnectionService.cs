using NETWORKLIST;
using SaintSender.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace SaintSender.Core.Services
{
    public class ConnectionService : IWebConnectionService
    {
        public bool ConnectionCheck(string url)
        {
            try
            {
                using (WebClient client = new WebClient() { Proxy = null})
                {
                    using (client.OpenRead(url))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public bool NLMAPICheck()
        {
            INetworkListManager networkListManager = new NetworkListManager();
            return networkListManager.IsConnectedToInternet;
        }

        public bool PingChek(string url)
        {
            try {
                Ping myPing = new Ping();
                PingReply reply = myPing.Send(url, 2000);
                if(reply != null)
                {

                    return true; 
                }
                return false;
            } 
            catch(Exception)
            {
                return false;
            }
            
        }
    }
}
