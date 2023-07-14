using AMCommunicator;
using System.Net;

namespace ArtemisManagerAction
{
    public static class TakeArtemisAction
    {
        
        public static Tuple<bool, ModItem?> ProcessArtemisAction(IPAddress? target, AMCommunicator.Messages.ArtemisActions action, Guid identifier, string? modJSON)
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
                    if (baseArtemis != null)
                    {
                        baseArtemis.Activate();
                    }
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
                            bool found = ModManager.IsModInstalled(receivedMod);

                            if (!found)
                            {
                                ModItem? newMod = ModItem.GetModItem(modJSON);
                                if (newMod != null && !string.IsNullOrEmpty(newMod.PackageFile))
                                {
                                    if (File.Exists(Path.Combine(ModManager.ModArchiveFolder, newMod.PackageFile)))
                                    {
                                        ModManager.InstallMod(Path.Combine(ModManager.ModArchiveFolder, newMod.PackageFile), newMod);
                                        mod = newMod;
                                        WasProcessed = true;
                                    }
                                    else
                                    {
                                        if (target != null)
                                        {
                                            Network.Current?.SendModPackageRequest(target, modJSON, newMod.PackageFile);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case AMCommunicator.Messages.ArtemisActions.ActivateMod:
                    break;
            }
            return new Tuple<bool, ModItem?> (WasProcessed, mod);
        }

       
    }
}