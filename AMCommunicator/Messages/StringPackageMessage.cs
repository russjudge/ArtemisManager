using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.StringPackage)]
    public class StringPackageMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.
        public StringPackageMessage() : base()
        {
            SerializedString = string.Empty;
            FileType = SendableStringPackageFile.Generic;
            FileName = string.Empty;
        }

        public StringPackageMessage(string serializedString, SendableStringPackageFile fileType, string fileName) : base()
        {
            SerializedString = serializedString;
            FileType = fileType;
            FileName = fileName;
        }
        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }

        public string SerializedString { get; set; }

        public SendableStringPackageFile FileType { get; set; }
        public string FileName { get; set; }


    }
}
