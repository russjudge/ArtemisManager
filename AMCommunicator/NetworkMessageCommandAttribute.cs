using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{

    internal class NetworkMessageCommandAttribute : Attribute
    {
        public NetworkMessageCommandAttribute(MessageCommand command) { Command = command; }
        public MessageCommand Command { get; set; }
    }
}
