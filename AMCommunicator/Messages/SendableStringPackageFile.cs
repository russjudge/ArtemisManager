using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    public enum SendableStringPackageFile
    {
        Generic,
        EngineeringPreset,
        ArtemisINI,
        controlsINI,
        vesselDataXML,
        DMXCommandsXML,
        Mod,
        Mission
    }
}
