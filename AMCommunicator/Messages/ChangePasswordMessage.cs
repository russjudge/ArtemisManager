using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{

    [NetworkMessageCommand(MessageCommand.ChangePassword)]
    internal class ChangePasswordMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        public ChangePasswordMessage() : base()
        {
            NewPassword = Network.Password;
        }
        public ChangePasswordMessage(string newPassword) : base()
        {
            NewPassword = newPassword;
        }
        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
        public string NewPassword { get; set; }
      
    }
}
