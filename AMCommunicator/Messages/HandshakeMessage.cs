using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.Handshake)]
    internal class HandshakeMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.
        public HandshakeMessage() : base()
        {
            Password = Network.Password;
        }
        public string Password { get; set; }

        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
        public bool IsValid()
        {
            return Password == Network.Password;
        }
    }
}
