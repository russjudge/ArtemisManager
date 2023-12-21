using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    internal class NetworkMessageCommandAttribute : Attribute
    {
        private NetworkMessageCommandAttribute() { }
        public NetworkMessageCommandAttribute(MessageCommand command) { Command = command; }
        public MessageCommand Command { get; set; }
    }
}
