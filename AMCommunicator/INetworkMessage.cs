using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    internal interface INetworkMessage
    {

      

        [NetworkMessage(Sequence = 0)]
        public int Length { get; set; }

        [NetworkMessage(Sequence = 1)]
        public MessageCommand Command { get; set; }

        [NetworkMessage(Sequence = 99999)]
        public byte[] Unspecified { get; set; }
    }
}
