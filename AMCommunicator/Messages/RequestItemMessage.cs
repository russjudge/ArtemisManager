using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.RequestItem)]
    internal class RequestItemMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        public RequestItemMessage() : base()
        {
            ItemIdentifier = string.Empty;
        }
        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
        public string ItemIdentifier { get; set; }
    }
}
