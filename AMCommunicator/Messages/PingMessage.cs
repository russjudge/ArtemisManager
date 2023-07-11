using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.Ping)]
    internal class PingMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.
        public PingMessage() : base() { }

        public PingMessage(bool acknowledge) : base()
        {
            Acknowledge = acknowledge;
        }
        public bool Acknowledge { get; set; }

        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }

    }
}
