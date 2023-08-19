using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public  enum  MessageCommand : short
    {
        Handshake = 0,
        ClientInfo = 1,
        RequestModPackage = 2,
        ModPackage = 3,
        ChangePassword = 4,
        PCAction = 5,
        UpdateCheck = 6,
        AretmisAction = 7,
        Communication = 8,
        Ping = 9,
        ChangeAppSetting = 10,
        Alert = 11,
        StringPackage = 12,
        UndefinedPackage = 13,
        RequestInformation = 14,
        Information = 15,
    }
}
