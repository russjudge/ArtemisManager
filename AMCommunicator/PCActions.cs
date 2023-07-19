using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public enum PCActions : short
    {
        CloseApp,
        ShutdownPC,
        RestartPC,
        CheckForUpdate,
        DisconnectThisConnection,
        SendClientInformation,
        AddAppToStartup,
        RemoveAppFromStartup,
       
    }
}
