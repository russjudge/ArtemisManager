using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerUI
{
    public class PCItem
    {
        public PCItem(string hostname, IPAddress? ip)
        {
            Hostname = hostname;
            IP = ip;
        }
        public string Hostname { get; private set; }
        public IPAddress? IP { get; private set; }
    }
}
