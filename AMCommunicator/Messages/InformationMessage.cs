using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.Information)]
    public class InformationMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        public InformationMessage() : base() { Data = Array.Empty<string>(); }
        public InformationMessage(RequestInformationType requestType, string identifier = "", string[] data = null) : base()
        {
            RequestType = requestType;
            Data = data;
            Identifier = identifier;
        }

        public RequestInformationType RequestType { get; set; }
        public string Identifier { get; set; } = string.Empty;

        public string[] Data { get; set; }
        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
    }
}
