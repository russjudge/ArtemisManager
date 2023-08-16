using AMCommunicator;
using ArtemisManagerAction.ArtemisEngineeringPresets;
using System.Net;
using System.Text.Json.Nodes;

namespace ArtemisManagerAction
{
    public static class TakeArtemisAction
    {
        public static ModItem? StagedModItemToActivateOnceInstalled { get; set; } = null;
       
        public static Tuple<bool, ModItem?> ProcessArtemisAction(IPAddress? target, AMCommunicator.Messages.ArtemisActions action, string? modJSON)
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
                    break;
                case AMCommunicator.Messages.ArtemisActions.ActivateMission:
                    break;
                case AMCommunicator.Messages.ArtemisActions.ActivateDMXCommands:
                    break;
                case AMCommunicator.Messages.ArtemisActions.ActivateEngineeringPresetsFile:
                    break;
                case AMCommunicator.Messages.ArtemisActions.DeleteArtemisINIFile:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        WasProcessed = ArtemisManager.DeleteArtemisINIFile(modJSON);
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.DeleteControlsINI:
                    break;
                case AMCommunicator.Messages.ArtemisActions.DeleteDMXCommands:
                    break;
                case AMCommunicator.Messages.ArtemisActions.DeleteEngineeringPresetsFile:
                    break;
                case AMCommunicator.Messages.ArtemisActions.InstallMission:
                    break;
                case AMCommunicator.Messages.ArtemisActions.InstallArtemisINI:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        ArtemisINI ini = new();
                        ini.Deserialize(modJSON);
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
                    break;
                case AMCommunicator.Messages.ArtemisActions.RenameDMXCommands:
                    break;
                case AMCommunicator.Messages.ArtemisActions.RenameEngineeringPresetsFile:
                    break;
                case AMCommunicator.Messages.ArtemisActions.RestoreArtemisINIToDefault:
                    WasProcessed = ArtemisManager.RestoreArtemisINIToDefault();
                    break;
                case AMCommunicator.Messages.ArtemisActions.RestoreControlINIToDefault:
                    break;
                case AMCommunicator.Messages.ArtemisActions.RestoreDMXCommandsToDefault:
                    break;
                case AMCommunicator.Messages.ArtemisActions.RestoreEngineeringPresetsToDefault:
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