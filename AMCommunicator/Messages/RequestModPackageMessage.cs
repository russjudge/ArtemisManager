using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.RequestModPackage)]
    internal class RequestModPackageMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        public RequestModPackageMessage() : base()
        {
            Mod = string.Empty;
            ItemRequestedIdentifier = string.Empty;
        }
        public RequestModPackageMessage(string mod, string itemRequestedItentifier) : base()
        {
            Mod = mod;
            ItemRequestedIdentifier = itemRequestedItentifier;
        }
        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
        public string Mod { get; set; }
        public string ItemRequestedIdentifier { get; set; }
        
        //send mod package item.
    }
}
