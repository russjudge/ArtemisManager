using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    internal class ConnectionTracker
    {
        public ConnectionTracker(string hostname, IPAddress address, NetworkStream? stream, Socket socket, Thread? thread)
        {
            Hostname = hostname;
            Address = address;
            Stream = stream;
            Socket = socket;
            Thread = thread;
        }
        public string Hostname { get; private set; }
        public IPAddress Address { get; private set; }
        public NetworkStream? Stream { get; private set; }
        public Socket Socket { get; private set; }
        public Thread? Thread { get; private set; }

    }
}
