using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.ChangeAppSetting)]
    internal class ChangeAppSettingMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.
        public ChangeAppSettingMessage() :base()
        {
            SettingName = string.Empty;
            SettingValue = string.Empty;
        }
        public ChangeAppSettingMessage(string settingName, string settingValue) : base()
        {
            SettingName = settingName;
            SettingValue = settingValue;
        }
        public string SettingName { get; protected set; }
        public string SettingValue { get; protected set; }


        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
      
    }
}
