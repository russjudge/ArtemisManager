using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.JsonPackage)]
    public class JsonPackageMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.
        public JsonPackageMessage() : base()
        {
            JsonData = string.Empty;
            FileType = JsonPackageFile.Generic;
            FileName = string.Empty;
        }

        public JsonPackageMessage(string jsonData, JsonPackageFile fileType, string fileName) : base()
        {
            JsonData = jsonData;
            FileType = fileType;
            FileName = fileName;
        }
        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }

        public string JsonData { get; set; }

        public JsonPackageFile FileType { get; set; }
        public string FileName { get; set; }


    }
}
