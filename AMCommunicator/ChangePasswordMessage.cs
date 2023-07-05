using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{

    [NetworkMessageCommand(MessageCommand.ChangePassword)]
    public class ChangePasswordMessage : NetworkMessageAbstract
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ChangePasswordMessage(byte[] data) : base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public ChangePasswordMessage() : base()
        {
            NewPassword = new MessageString(Network.Password);
        }
        protected override void SetCommand()
        {
            Command = MessageCommand.ChangePassword;
        }
        public MessageString NewPassword { get; set; }
        /*
         *  public const int ChangePassword = 4;
        public const int PCAction = 5;
        public const int UpdateCheck = 6;
        public const int AretmisAction = 7;
        public const int SetClientInfo = 8;
         * */
    }
}
