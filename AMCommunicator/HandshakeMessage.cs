using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    [NetworkMessageCommand(MessageCommand.Handshake)]
    public class HandshakeMessage : NetworkMessageAbstract
    {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public HandshakeMessage(byte[] data): base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public HandshakeMessage() : base()
        {
            Password = new MessageString(Network.Password);
            
        }
        [NetworkMessage(Sequence = 4)]
        public MessageString Password { get; set; }

        protected override void SetCommand()
        {
            Command = MessageCommand.Handshake;
        }
        public bool IsValid()
        {
            return Password.Message == Network.Password;
        }
    }
}
