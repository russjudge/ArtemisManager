using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    internal class NetworkMessageAttribute : Attribute
    {
        public NetworkMessageAttribute() { }
        public int Sequence { get; set; }

    }
}
