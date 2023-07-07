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
    internal class PingMessage : NetworkMessageAbstract
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PingMessage(byte[] data) : base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PingMessage() : base() { }

        public PingMessage(bool acknowledge) : base()
        {
            Acknowledge = acknowledge;
        }
        [NetworkMessage(Sequence = 4)]
        public bool Acknowledge { get; set; }

        protected override void SetCommand()
        {
            Command = MessageCommand.Ping;
            MessageVersion = ThisVersion;
        }

    }
}
