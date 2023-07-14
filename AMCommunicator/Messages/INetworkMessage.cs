using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    internal interface INetworkMessage
    {
        public string? Source { get; set; }
        public short MessageVersion { get; set; }
        public byte[] Unspecified { get; set; }
    }
}
