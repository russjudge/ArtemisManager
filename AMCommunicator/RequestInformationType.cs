using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public enum RequestInformationType : int
    {
        None,
        ListOfArtemisINIFiles,
        SpecificArtemisINIFile,
        SaveSpecificArtemisINIFile,
        ListOfScreenResolutions,
        ListOfEngineeringPresets,
        SpecificEngineeringPreset,
        ListOfControLINIFiles,
        SpecificControlINIFile,
        ListOfDMXCommandfiles,
        SpecificDMXCommandFile
    }
}
