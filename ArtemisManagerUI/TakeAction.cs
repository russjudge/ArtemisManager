using AMCommunicator;
using System.Net;
using System.Windows;

namespace ArtemisManagerUI
{
    public static class TakeAction
    {
        public static void ChangeSetting(string settingName,string value)
        {
            switch(settingName)
            {
                case "ConnectOnStart":
                    Properties.Settings.Default.ConnectOnStart = bool.Parse(value);
                    break;
                case "ListeningPort":
                    Properties.Settings.Default.ListeningPort = int.Parse(value);
                    break;
            }
            Properties.Settings.Default.Save();
        }
        public static bool ProcessPCAction(ActionCommands action, bool force, IPAddress? source)
        {
            
            bool WasProcessed = false;
            if (!force)
            {
                if (MessageBox.Show("The follow action is being requested: " + action.ToString() + ".\r\nDo you wish to allow this?", "Action requested", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return false;
                }
            }
            switch (action)
            {
                case ActionCommands.CloseApp:
                    //Handled elsewhere.
                    WasProcessed = true;
                    break;
                case ActionCommands.RestartPC:

                    System.Diagnostics.Process.Start("shutdown /r /t 0");
                    WasProcessed = true;
                    break;
                case ActionCommands.UpdateCheck:
                    //TODO: Add process to check for update.
                    if (UpdateCheck(true, source))
                    {
                        //TODO: do the update.  No prompt.
                    }
                    WasProcessed = true;
                    break;
                case ActionCommands.ShutdownPC:
                    System.Diagnostics.Process.Start("shutdown /s /t 0");
                    WasProcessed = true;
                    break;
                default: 
                    WasProcessed = false;
                    break;

            }
            return WasProcessed;
        }
        /// <summary>
        /// Determines whether or not an update is available.
        /// </summary>
        /// <param name="AlertIfCannotCheck">send "true" to send an alert to the source that it could not access the website of the update to check.  Possible: have update transmitted from source.</param>
        /// <returns></returns>
        public static bool UpdateCheck(bool AlertIfCannotCheck, IPAddress? source = null)
        {
            
            return false;
        }
        
    }
}
