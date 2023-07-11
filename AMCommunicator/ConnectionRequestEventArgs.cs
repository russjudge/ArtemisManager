using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class ConnectionRequestEventArgs : EventArgs
    {
        public ConnectionRequestEventArgs(IPAddress? address, string? hostname)
        {
            if (address == null)
            {
                Address = new IPAddress(0);
            }
            else
            {
                Address = address;
            }
            if (hostname == null)
            {
                Hostname = "Unknown";
            }
            else
            {
                Hostname = hostname;
            }
        }
        public IPAddress Address { get; private set; }
        public string Hostname { get; private set; }
    }
}
