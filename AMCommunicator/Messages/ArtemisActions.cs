using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    public enum ArtemisActions
    {
        StartArtemis,
        StopArtemis,

        ActivateMod,
        InstallMod,

        ResetToVanilla,

        InstallMission,
        ActivateMission,

        UninstallMod,

        ActivateArtemisINIFile,
        RenameArtemisINIFile,
        DeleteArtemisINIFile
            ,
        ActivateEngineeringPresetsFile,
        RenameEngineeringPresetsFile,
        DeleteEngineeringPresetsFile,

        InstallArtemisINI,
        InstallEngineeringPresets,

        InstallControlsINI,
        RenameControlsINI,
        ActivateControlsINI,
        DeleteControlsINI,

        InstallDMXCommands,
        RenameDMXCommands,
        ActivateDMXCommands,
        DeleteDMXCommands,

        RestoreArtemisINIToDefault,
        RestoreControlINIToDefault,
        RestoreDMXCommandsToDefault,
        RestoreEngineeringPresetsToDefault
    }
}
