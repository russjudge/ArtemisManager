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
        private void OnStartup(object sender, StartupEventArgs e)
        {
            //TODO: Check for Update here.
            
            
        }

        private void OnError(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //TODO: Handle error here.
            MessageBox.Show("FATAL: " + e.Exception.ToString());
        }
    }
}
