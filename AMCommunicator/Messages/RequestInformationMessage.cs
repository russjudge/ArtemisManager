using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.RequestInformation)]
    public class RequestInformationMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        public RequestInformationMessage() : base() { }
        public RequestInformationMessage(RequestInformationType requestType, string identifier = "") : base() 
        {
            RequestType = requestType;
            Identifier = identifier;
        }

        public RequestInformationType RequestType { get; set; }
        public string Identifier { get; set; } = string.Empty;
        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
    }
}
