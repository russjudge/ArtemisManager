using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    internal interface INetworkMessage
    {
        [NetworkMessage(Sequence = NetworkMessage.LengthSequence)]
        public int Length { get; set; }

        [NetworkMessage(Sequence = NetworkMessage.CommandSequence)]
        public MessageCommand Command { get; set; }

        [NetworkMessage(Sequence = NetworkMessage.MessageVersionSequence)]
        public short MessageVersion { get; set; }

        [NetworkMessage(Sequence = NetworkMessage.MaxSequence)]
        public byte[] Unspecified { get; set; }
    }
}
