using AMCommunicator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            SettingsData = SettingsAction.Current;
            InitializeComponent();
            //isLoading = false;
        }
        //private bool isLoading = true;

        public static readonly DependencyProperty PopupMessageProperty =
         DependencyProperty.Register(nameof(PopupMessage), typeof(string),
             typeof(SettingsWindow));

        public string PopupMessage
        {
            get
            {
                return (string)this.GetValue(PopupMessageProperty);

            }
            set
            {
                this.SetValue(PopupMessageProperty, value);

            }
        }



        public static readonly DependencyProperty SettingsDataProperty =
          DependencyProperty.Register(nameof(SettingsData), typeof(SettingsAction),
         typeof(SettingsWindow));

        public SettingsAction SettingsData
        {
            get
            {
                return (SettingsAction)this.GetValue(SettingsDataProperty);

            }
            set
            {
                this.SetValue(SettingsDataProperty, value);
            }
        }



        private void OnEngineeringPresets(object sender, RoutedEventArgs e)
        {
            EngineeringPresetEditWindow win = new();
            win.Show();
        }

        private void OnSaveNetworkSettings(object sender, RoutedEventArgs e)
        {
            SettingsData.Save();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            SettingsData.Save();
        }

        private void OnBackupControlsIni(object sender, RoutedEventArgs e)
        {
            ArtemisManagerAction.ArtemisManager.SaveControlsINIFile(System.IO.Path.Combine(ArtemisManagerAction.ModItem.ActivatedFolder, ArtemisManagerAction.ArtemisManager.controlsINI));
            PopupMessage = "Backup of controls.ini complete.";
        }

        private void OnRestoreControlsIni(object sender, RoutedEventArgs e)
        {
            var controlsINI = ArtemisManagerAction.ArtemisManager.GetControlsINIFileList();
            if (controlsINI.Length > 0)
            {
                System.IO.File.Copy(System.IO.Path.Combine(ArtemisManagerAction.ArtemisManager.ControlsINIFolder, controlsINI[0]),
                    System.IO.Path.Combine(ArtemisManagerAction.ModItem.ActivatedFolder, ArtemisManagerAction.ArtemisManager.controlsINI), true);
                PopupMessage = "controls.ini file restored.";
            }
            else
            {
                MessageBox.Show("No backup of controls.ini found.");
            }
        }

        private void OnBackupDMXCommands(object sender, RoutedEventArgs e)
        {
            ArtemisManagerAction.ArtemisManager.SaveDMXCommandsFile(System.IO.Path.Combine(ArtemisManagerAction.ModItem.ActivatedFolder, ArtemisManagerAction.ArtemisManager.ArtemisDATSubfolder, ArtemisManagerAction.ArtemisManager.DMXCommands));
            PopupMessage = "Backup of DMXcommands.xml complete.";

        }

        private void OnRestoreDMXCommands(object sender, RoutedEventArgs e)
        {
            var DMXCommands = ArtemisManagerAction.ArtemisManager.GetDMXCommandsFileList();
            if (DMXCommands.Length > 0)
            {
                System.IO.File.Copy(System.IO.Path.Combine(ArtemisManagerAction.ArtemisManager.DMXCommandsFolder, DMXCommands[0]),
                    System.IO.Path.Combine(ArtemisManagerAction.ModItem.ActivatedFolder, ArtemisManagerAction.ArtemisManager.ArtemisDATSubfolder, ArtemisManagerAction.ArtemisManager.DMXCommands), true);
                PopupMessage = "DMXcommands.xml file restored.";
            }
            else
            {
                MessageBox.Show("No backup of DMXcommands.xml found.");
            }
        }
    }
}
