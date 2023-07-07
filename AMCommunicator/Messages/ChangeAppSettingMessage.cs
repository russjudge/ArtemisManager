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
    internal class ChangeAppSettingMessage : NetworkMessageAbstract
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ChangeAppSettingMessage(byte[] data) : base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ChangeAppSettingMessage(string settingName, string settingValue) : base()
        {
            SettingName = new(settingName);
            SettingValue = new(settingValue);
        }
        [NetworkMessage(Sequence = 4)]
        public MessageString SettingName { get; set; }
        [NetworkMessage(Sequence = 5)]
        public MessageString SettingValue { get; set; }


        protected override void SetCommand()
        {
            Command = MessageCommand.ChangeAppSetting;
            MessageVersion = ThisVersion;
        }
      
    }
}
