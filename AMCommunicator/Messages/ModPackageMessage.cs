using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.ModPackage)]
    internal class ModPackageMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        public ModPackageMessage() : base()
        {
            ModItem = string.Empty;
            Data = Array.Empty<byte>();
        }
        public ModPackageMessage(string modItem, byte[] data)
        {
            ModItem = modItem;
            Data = data;
        }
    
        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
        public string ModItem { get; set; }
        public byte[] Data { get; set; }
    }
}
