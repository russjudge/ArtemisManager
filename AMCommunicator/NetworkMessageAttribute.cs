using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    internal class NetworkMessageAttribute : Attribute
    {
        public NetworkMessageAttribute() { }
        public int Sequence { get; set; }
        
    }
}
