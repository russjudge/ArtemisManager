using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{

    [NetworkMessageCommand(MessageCommand.ChangePassword)]
    internal class ChangePasswordMessage : NetworkMessageAbstract
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ChangePasswordMessage(byte[] data) : base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public ChangePasswordMessage() : base()
        {
            NewPassword = new MessageString(Network.Password);
        }
        public ChangePasswordMessage(string newPassword) : base()
        {
            NewPassword = new(newPassword);
        }
        protected override void SetCommand()
        {
            Command = MessageCommand.ChangePassword;
            MessageVersion = ThisVersion;
        }
        public MessageString NewPassword { get; private set; }
      
    }
}
