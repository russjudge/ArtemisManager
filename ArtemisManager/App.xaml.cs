using ArtemisManagerUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArtemisManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool ProbablyUnattended { get; private set; }
        private void OnStartup(object sender, StartupEventArgs e)
        {
            //TODO: Use Mutex to prevent multiple copies running simulateously.
            

            foreach (var arg in e.Args)
            {
                if (arg.Equals("FROMSTARTUPFOLDER",  StringComparison.InvariantCultureIgnoreCase))
                {
                    ProbablyUnattended = true;
                    break;
                }
            }
            //TODO: Check for Update here.
            if (TakeAction.UpdateCheck(false))
            {
                //TODO: do update.  Prompt user to download.
            }

        }

        private void OnError(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //TODO: Handle error here.
            MessageBox.Show("FATAL: " + e.Exception.ToString());
        }
    }
}
