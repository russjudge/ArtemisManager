using ArtemisEngineeringPresets;
using ArtemisManagerAction;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for ArtemisINIManagerControl.xaml
    /// </summary>
    public partial class ArtemisINIManagerControl : System.Windows.Controls.UserControl
    {
        public ArtemisINIManagerControl()
        {
            ArtemisSettingsFiles = new(ArtemisManager.GetArtemisINIFileList());

            RestoreOriginalToolTip = string.Format("Restore Original artemis.ini file ({0}) to defaults.",  ArtemisManager.GetOriginalArtemisINIFile(ModItem.ActivatedFolder));
            InitializeFSW();
            InitializeComponent();
        }

        void InitializeFSW()
        {
            fsw = new FileSystemWatcher(ArtemisManager.ArtemisINIFolder);
            fsw.Created += OnINIFileCreated;
            fsw.Deleted += OnINIFileDeleted;
            fsw.EnableRaisingEvents = true;
        }
        FileSystemWatcher? fsw = null;
        public static readonly DependencyProperty IsRemoteProperty =
            DependencyProperty.Register(nameof(IsRemote), typeof(bool),
            typeof(ArtemisINIManagerControl), new PropertyMetadata(OnIsRemoteChanged));

        private static void OnIsRemoteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArtemisINIManagerControl control)
            {
                if (control.IsRemote)
                {
                    if (control.fsw != null)
                    {
                        control.fsw.EnableRaisingEvents = false;
                        control.fsw.Created -= control.OnINIFileCreated;
                        control.fsw.Deleted -= control.OnINIFileDeleted;
                        control.fsw.Dispose();
                        control.fsw = null;
                    }
                }
                else
                {
                    control.InitializeFSW();
                }
            }
        }

        private void OnINIFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Name) && e.Name.EndsWith(ArtemisManager.INIFileExtension))
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (ArtemisSettingsFiles.Contains(e.Name))
                    {
                        ArtemisSettingsFiles.Remove(e.Name);
                    }
                });
            }
        }

        private void OnINIFileCreated(object sender, FileSystemEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Name) && e.Name.EndsWith(ArtemisManager.INIFileExtension))
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (!ArtemisSettingsFiles.Contains(e.Name))
                    {
                        ArtemisSettingsFiles.Add(e.Name);
                    }
                });
            }
        }

        public bool IsRemote
        {
            get
            {
                return (bool)this.GetValue(IsRemoteProperty);

            }
            set
            {
                this.SetValue(IsRemoteProperty, value);

            }
        }


        public static readonly DependencyProperty PopupMessageProperty =
         DependencyProperty.Register(nameof(PopupMessage), typeof(string),
             typeof(ArtemisINIManagerControl));

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


        public static readonly DependencyProperty ArtemisSettingsFilesProperty =
            DependencyProperty.Register(nameof(ArtemisSettingsFiles), typeof(ObservableCollection<string>),
                typeof(ArtemisINIManagerControl));

        public ObservableCollection<string> ArtemisSettingsFiles
        {
            get
            {
                return (ObservableCollection<string>)this.GetValue(ArtemisSettingsFilesProperty);

            }
            set
            {
                this.SetValue(ArtemisSettingsFilesProperty, value);

            }
        }
        public static readonly DependencyProperty SelectedArtemisSettingsFileProperty =
            DependencyProperty.Register(nameof(SelectedArtemisSettingsFile), typeof(string),
             typeof(ArtemisINIManagerControl), new PropertyMetadata(OnSelectedArtemisSettingsFileChanged));

        private static void OnSelectedArtemisSettingsFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArtemisINIManagerControl me)
            {
                string source = System.IO.Path.Combine(ArtemisManager.ArtemisINIFolder, me.SelectedArtemisSettingsFile);
                if (!string.IsNullOrEmpty(me.SelectedArtemisSettingsFile) && File.Exists(source))
                {
                    me.SelectedArtemisSettings = new ArtemisINI(source);
                }
            }
        }

        public string SelectedArtemisSettingsFile
        {
            get
            {
                return (string)this.GetValue(SelectedArtemisSettingsFileProperty);

            }
            set
            {
                this.SetValue(SelectedArtemisSettingsFileProperty, value);

            }
        }
        public static readonly DependencyProperty SelectedArtemisSettingsProperty =
            DependencyProperty.Register(nameof(SelectedArtemisSettings), typeof(ArtemisINI),
             typeof(ArtemisINIManagerControl));

        public ArtemisINI SelectedArtemisSettings
        {
            get
            {
                return (ArtemisINI)this.GetValue(SelectedArtemisSettingsProperty);

            }
            set
            {
                this.SetValue(SelectedArtemisSettingsProperty, value);

            }
        }
        public static readonly DependencyProperty RestoreOriginalToolTipProperty =
           DependencyProperty.Register(nameof(RestoreOriginalToolTip), typeof(string),
            typeof(ArtemisINIManagerControl));

        public string RestoreOriginalToolTip
        {
            get
            {
                return (string)this.GetValue(RestoreOriginalToolTipProperty);

            }
            set
            {
                this.SetValue(RestoreOriginalToolTipProperty, value);

            }
        }
        

        private void OnAddSettingsFile(object sender, RoutedEventArgs e)
        {
            ModManager.CreateFolder(ArtemisManagerAction.ArtemisManager.ArtemisINIFolder);
            Microsoft.Win32.SaveFileDialog dialg = new()
            {
                CheckPathExists = true,
                Filter = "Artemis.ini file (*" + ArtemisManager.INIFileExtension + ")|*" + ArtemisManager.INIFileExtension,
                DefaultExt = "dat",
                InitialDirectory = ArtemisManagerAction.ArtemisManager.ArtemisINIFolder,
                OverwritePrompt = true,
                //dialg.FileName = System.IO.Path.Combine(ArtemisManagerAction.ArtemisManager.ArtemisINIFolder)
                Title = "Select new filename"
            };

            if (dialg.ShowDialog() == true)
            {
                string? source = ArtemisManager.GetOriginalArtemisINIFile(ModItem.ActivatedFolder);
                if (!string.IsNullOrEmpty(source) && File.Exists(source))
                {
                    File.Copy(source, dialg.FileName, true);
                }
                else
                {
                    ArtemisINI ini = new()
                    {
                        SaveFile = dialg.FileName
                    };
                    ini.Save();
                }
                SelectedArtemisSettingsFile = new FileInfo(dialg.FileName).Name;

            }
        }

        private void OnRestoreToDefault(object sender, RoutedEventArgs e)
        {
            string originalINI = ArtemisManager.GetOriginalArtemisINIFile(ModItem.ActivatedFolder);
            if (!string.IsNullOrEmpty(originalINI) && File.Exists(originalINI))
            {
                File.Copy(originalINI, ArtemisManager.ArtemisINIFolder);
            }
        }

       

        private void OnImportFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new()
            {
                Title = "Select Artemis.ini file to import",
                DefaultExt = "ini",
                Filter = "INI Files (*" + ArtemisManager.INIFileExtension + ")|*" + ArtemisManager.INIFileExtension,
                Multiselect = false,
                AddExtension = true,
                CheckFileExists = true
            };
            if (ofd.ShowDialog() == true)
            {
                FileInfo source =new(ofd.FileName);
                string target = System.IO.Path.Combine(ArtemisManager.ArtemisINIFolder, source.Name);
                int i = 0;
                while (File.Exists(target))
                {
                    target = System.IO.Path.Combine(ArtemisManager.ArtemisINIFolder, string.Format("{0} ({1})", source.Name, (++i).ToString()));
                }
                source.CopyTo(target, true);
            }
        }

        private void OnExportFile(object sender, RoutedEventArgs e)
        {
            if (SelectedArtemisSettingsFile != null)
            {
                string source = System.IO.Path.Combine(ArtemisManager.ArtemisINIFolder, SelectedArtemisSettingsFile);
                if (File.Exists(source)) 
                {
                    Microsoft.Win32.SaveFileDialog ofd = new()
                    {
                        Title = "Select Artemis.ini file to export",
                        DefaultExt = "ini",
                        Filter = "INI Files (*" + ArtemisManager.INIFileExtension + ")|*" + ArtemisManager.INIFileExtension,
                        AddExtension = true,
                        CheckFileExists = false,
                        CheckPathExists = true
                    };
                    if (ofd.ShowDialog() == true)
                    {
                        bool OkayToSave = true;
                        if (File.Exists(ofd.FileName))
                        {
                            OkayToSave = (System.Windows.MessageBox.Show("Overwrite " + ofd.FileName + "?", "Export settings file", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
                        }       
                        if (OkayToSave)
                        {
                            File.Copy(source, ofd.FileName, true);
                        }
                    }
                }
            }
        }

        private void OnSendFileRequest(object sender, FileRequestRoutedEventArgs e)
        {
            e.File = SelectedArtemisSettings;
        }

        private void OnTransmissionCompleted(object sender, RoutedEventArgs e)
        {
            PopupMessage = "Transmission completed.";
        }
    }
}
