using AMCommunicator;
using ArtemisManagerAction.ArtemisEngineeringPresets;
using System.ComponentModel;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ArtemisManagerAction
{
    public static class TakeArtemisAction
    {
        public static event EventHandler<CommunicationMessageEventArgs>? CommunicationMessageReceived;
        public static ModItem? StagedModItemToActivateOnceInstalled { get; set; } = null;
       
        public static Tuple<bool, ModItem?> ProcessArtemisAction(IPAddress? target, AMCommunicator.Messages.ArtemisActions action, string? modJSON, string? saveName = null)
        {
            bool WasProcessed = false;
            ModItem? mod = null;
            switch (action)
            {
                case AMCommunicator.Messages.ArtemisActions.StopArtemis:
                    ArtemisManager.StopArtemis();
                    WasProcessed = true;
                    break;
                case AMCommunicator.Messages.ArtemisActions.StartArtemis:
                    ArtemisManager.StartArtemis();
                    WasProcessed = true;
                    break;
                case AMCommunicator.Messages.ArtemisActions.ResetToVanilla:
                    var baseArtemis = ArtemisManager.ClearActiveFolder();
                    baseArtemis?.Activate();
                    mod = baseArtemis;
                    WasProcessed = true;
                    break;
                case AMCommunicator.Messages.ArtemisActions.InstallMod:
                    //Check that mod isn't already installed. If not.
                    WasProcessed = false;
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        var receivedMod = ModItem.GetModItem(modJSON);
                        if (receivedMod != null)
                        {
                            WasProcessed = RequestInstallMod(receivedMod, target);
                            if (WasProcessed)
                            {
                                mod = receivedMod;
                            }
                        }
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.UninstallMod:
                    WasProcessed = false;
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        var receivedMod = ModItem.GetModItem(modJSON);
                        if (receivedMod != null)
                        {
                            WasProcessed = receivedMod.Uninstall();
                            if (WasProcessed)
                            {
                                mod = receivedMod;
                            }
                        }
                    }
                    if (!WasProcessed && target != null)
                    {
                        Network.Current?.SendAlert(target, AMCommunicator.Messages.AlertItems.Uninstall_Failure, "Unable to uninstall requested mod.  Cannot uninstall active mods.");
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.ActivateMod:
                    //Is Mod already activitated?  skip if it is.
                    WasProcessed = false;
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        var receivedMod = ModItem.GetModItem(modJSON);
                        if (receivedMod != null)
                        {
                            if (!ArtemisManager.IsModActive(receivedMod))
                            {
                                if (!RequestInstallMod(receivedMod, target))
                                {
                                    //Means that Mod is not installed and was not installed, but the install package was requested.
                                    //Therefore we need to set a flag to activate the mod as soon as we get it installed.
                                    StagedModItemToActivateOnceInstalled = receivedMod;
                                }
                                else
                                {
                                    receivedMod.Activate();
                                    mod = receivedMod;
                                    WasProcessed = true;
                                }
                            }
                        }
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.ActivateArtemisINIFile:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        WasProcessed = ArtemisManager.ActivateArtemisINIFile(modJSON);
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.ActivateControlsINI:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        WasProcessed = ArtemisManager.ActivateOtherSettingsFile(AMCommunicator.Messages.SendableStringPackageFile.controlsINI, modJSON);
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.ActivateMission:
                    break;
                case AMCommunicator.Messages.ArtemisActions.ActivateDMXCommands:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        WasProcessed = ArtemisManager.ActivateOtherSettingsFile(AMCommunicator.Messages.SendableStringPackageFile.DMXCommandsXML, modJSON);
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.ActivateEngineeringPresetsFile:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        WasProcessed = ArtemisManager.ActivateEngineeringPresetFile(modJSON);
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.DeleteArtemisINIFile:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        WasProcessed = ArtemisManager.DeleteArtemisINIFile(modJSON);
                        if (WasProcessed)
                        {
                            CommunicationMessageReceived?.Invoke(null, new CommunicationMessageEventArgs(null, "Artemis INI " + modJSON + " Deleted"));
                            
                        }
                        else
                        {
                            //string tar = ArtemisManager.ResolveFilename(ArtemisManager.ArtemisINIFolder, modJSON, ArtemisManager.INIFileExtension);
                            //CommunicationMessageReceived?.Invoke(null, new CommunicationMessageEventArgs(null, "Request to delete Artemis INI Failed: file=" +tar));
                        }
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.DeleteControlsINI:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        WasProcessed = ArtemisManager.DeleteOtherSettingsFile(AMCommunicator.Messages.SendableStringPackageFile.controlsINI, modJSON);
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.DeleteDMXCommands:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        WasProcessed = ArtemisManager.DeleteOtherSettingsFile(AMCommunicator.Messages.SendableStringPackageFile.DMXCommandsXML, modJSON);
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.DeleteEngineeringPresetsFile:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        WasProcessed = ArtemisManager.DeleteEngineeringPresetsFile(modJSON);
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.InstallMission:
                    break;
                case AMCommunicator.Messages.ArtemisActions.InstallArtemisINI:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        ArtemisINI? ini = new();
                        ini.Deserialize(modJSON);
                        if (saveName != null)
                        {
                            ini.SaveFile = saveName;
                        }
                        ini.Save();

                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.InstallEngineeringPresets:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        PresetsFile ini = new();
                        /*
                        ini.Deserialize(modJSON);
                        ini.Save();
                        */
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.InstallControlsINI:
                    break;
                case AMCommunicator.Messages.ArtemisActions.InstallDMXCommands:
                    break;
                case AMCommunicator.Messages.ArtemisActions.RenameArtemisINIFile:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        string[] names = modJSON.Split(':');
                        if (names.Length > 1)
                        {
                            WasProcessed = ArtemisManager.RenameArtemisINIFile(names[0], names[1]);
                        }
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.RenameControlsINI:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        string[] names = modJSON.Split(':');
                        if (names.Length > 1)
                        {
                            WasProcessed = ArtemisManager.RenameOtherSettingsFile(AMCommunicator.Messages.SendableStringPackageFile.controlsINI, names[0], names[1]);
                        }
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.RenameDMXCommands:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        string[] names = modJSON.Split(':');
                        if (names.Length > 1)
                        {
                            WasProcessed = ArtemisManager.RenameOtherSettingsFile(AMCommunicator.Messages.SendableStringPackageFile.DMXCommandsXML, names[0], names[1]);
                        }
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.RenameEngineeringPresetsFile:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        string[] names = modJSON.Split(':');
                        if (names.Length > 1)
                        {
                            WasProcessed = ArtemisManager.RenameEngineeringPresetsFile(names[0], names[1]);
                        }
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.RestoreArtemisINIToDefault:
                    if (modJSON != null)
                    {
                        WasProcessed = ArtemisManager.RestoreArtemisINIToDefault(modJSON);
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.RestoreControlINIToDefault:
                    if (modJSON != null)
                    {
                        WasProcessed = ArtemisManager.RestoreDefaultOtherSettingsFile(AMCommunicator.Messages.SendableStringPackageFile.controlsINI, modJSON);
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.RestoreDMXCommandsToDefault:
                    if (modJSON != null)
                    {
                        WasProcessed = ArtemisManager.RestoreDefaultOtherSettingsFile(AMCommunicator.Messages.SendableStringPackageFile.DMXCommandsXML, modJSON);
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.RestoreEngineeringPresetsToDefault:
                    WasProcessed = ArtemisManager.RestoreEngineeringPresetsToDefault();
                    break;
            }
            return new Tuple<bool, ModItem?> (WasProcessed, mod);
        }

        private static bool RequestInstallMod(ModItem receivedMod, IPAddress? target)
        {
            bool ActionCompleted = false;
            if (!ModManager.IsModInstalled(receivedMod))
            {
                if (!string.IsNullOrEmpty(receivedMod.PackageFile))
                {
                    if (File.Exists(Path.Combine(ModManager.ModArchiveFolder, receivedMod.PackageFile)))
                    {
                        receivedMod.Unpack();
                        ModManager.InstallMod(Path.Combine(ModManager.ModArchiveFolder, receivedMod.PackageFile), receivedMod);
                        ActionCompleted = true;
                    }
                    else
                    {
                        if (target != null)
                        {
                            Network.Current?.SendModPackageRequest(target, receivedMod.GetJSON(), receivedMod.PackageFile);
                        }
                    }
                }
            }
            else
            {
                ActionCompleted = true;
            }
            return ActionCompleted;
        }
        
    }
}