using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public enum ActionCommands
    {
        RestartPC,
        ShutdownPC,
        CloseApp,
        UpdateCheck,
        ClientInformationRequested,
        AddAppToStartup,
        RemoveAppFromStartup
    }
}
