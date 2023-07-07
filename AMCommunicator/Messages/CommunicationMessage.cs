using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.Communication)]
    internal class CommunicationMessage : NetworkMessageAbstract
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CommunicationMessage(byte[] data) : base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CommunicationMessage() : base()
        {
            Message = new();
        }
        public CommunicationMessage(string message) : base()
        {
            Message = new MessageString(message);
        }
        [NetworkMessage(Sequence = 4)]
        public MessageString Message { get; set; }

        protected override void SetCommand()
        {
            Command = MessageCommand.Communication;
            MessageVersion = ThisVersion;
        }

    }
}
