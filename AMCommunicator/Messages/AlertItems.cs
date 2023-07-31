using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    public enum AlertItems : short
    {
        MessageVersionMismatch,
        UpdateCheckFail,
        UpdateCheckSuccess,
        Uninstall_Failure,
        UndefinedMessageReceived
    }
}
