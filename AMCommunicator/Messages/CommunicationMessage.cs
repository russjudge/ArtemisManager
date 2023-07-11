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
    internal class CommunicationMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        public CommunicationMessage() : base()
        {
            Message = string.Empty;
        }
        public CommunicationMessage(string message) : base()
        {
            Message = message;
        }
        public string Message { get; protected set; }

        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }

    }
}
