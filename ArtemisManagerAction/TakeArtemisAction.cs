using AMCommunicator;
using System.Net;

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
                        }
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.UninstallMod:
                    if (!string.IsNullOrEmpty(modJSON))
                    {
                        var receivedMod = ModItem.GetModItem(modJSON);
                        receivedMod?.Uninstall();
                    }
                    WasProcessed = true;

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
                                }
                            }
                        }
                    }
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