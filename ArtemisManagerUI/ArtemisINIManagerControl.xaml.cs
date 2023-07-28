using ArtemisEngineeringPresets;
using ArtemisManagerAction;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for ArtemisINIManagerControl.xaml
    /// </summary>
    public partial class ArtemisINIManagerControl : System.Windows.Controls.UserControl
    {
        public ArtemisINIManagerControl()
        {
            ArtemisSettingsFiles = new();
            foreach (var ini in ArtemisManager.GetArtemisINIFileList())
            {
                ArtemisSettingsFiles.Add(new FileListItem(ini.Substring(0, ini.Length - 4)));
            }
            AvailableResolutions = new(TakeAction.GetAvailableScreenResolutions());
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

                    string match = e.Name.Substring(0, e.Name.Length - 4);
                    FileListItem? remover = null;
                    foreach (var item in ArtemisSettingsFiles)
                    {
                        if (item.Name == match)
                        {
                            remover = item;
                            break;
                        }
                    }
                    if (remover != null)
                    {
                        ArtemisSettingsFiles.Remove(remover);
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
                    string match = e.Name.Substring(0, e.Name.Length - 4);
                    FileListItem? remover = null;
                    foreach (var item in ArtemisSettingsFiles)
                    {
                        if (item.Name == match)
                        {
                            remover = item;
                            break;
                        }
                    }
                    if (remover == null)
                    {
                        ArtemisSettingsFiles.Add(new(match));
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
            DependencyProperty.Register(nameof(ArtemisSettingsFiles), typeof(ObservableCollection<FileListItem>),
                typeof(ArtemisINIManagerControl));

        public ObservableCollection<FileListItem> ArtemisSettingsFiles
        {
            get
            {
                return (ObservableCollection<FileListItem>)this.GetValue(ArtemisSettingsFilesProperty);

            }
            set
            {
                this.SetValue(ArtemisSettingsFilesProperty, value);

            }
        }
        public static readonly DependencyProperty SelectedArtemisSettingsFileProperty =
            DependencyProperty.Register(nameof(SelectedArtemisSettingsFile), typeof(FileListItem),
             typeof(ArtemisINIManagerControl), new PropertyMetadata(OnSelectedArtemisSettingsFileChanged));

        private static void OnSelectedArtemisSettingsFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArtemisINIManagerControl me && me.SelectedArtemisSettingsFile != null)
            {
                string source = System.IO.Path.Combine(ArtemisManager.ArtemisINIFolder, me.SelectedArtemisSettingsFile.Name + ArtemisManager.INIFileExtension);
                if (!string.IsNullOrEmpty(me.SelectedArtemisSettingsFile.Name) && File.Exists(source))
                {
                    me.SelectedArtemisSettings = new ArtemisINI(source);
                }
            }
        }

        public FileListItem? SelectedArtemisSettingsFile
        {
            get
            {
                return (FileListItem?)this.GetValue(SelectedArtemisSettingsFileProperty);

            }
            set
            {
                this.SetValue(SelectedArtemisSettingsFileProperty, value);

            }
        }
        public static readonly DependencyProperty AvailableResolutionsProperty =
          DependencyProperty.Register(nameof(AvailableResolutions), typeof(ObservableCollection<System.Drawing.Size>),
           typeof(ArtemisINIManagerControl));

        public ObservableCollection<System.Drawing.Size> AvailableResolutions
        {
            get
            {
                return (ObservableCollection<System.Drawing.Size>)this.GetValue(AvailableResolutionsProperty);

            }
            set
            {
                this.SetValue(AvailableResolutionsProperty, value);

            }
        }
        public static readonly DependencyProperty SelectedArtemisSettingsProperty =
            DependencyProperty.Register(nameof(SelectedArtemisSettings), typeof(ArtemisINI),
             typeof(ArtemisINIManagerControl));

        public ArtemisINI? SelectedArtemisSettings
        {
            get
            {
                return (ArtemisINI?)this.GetValue(SelectedArtemisSettingsProperty);

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
            string newFile = "NewArtemisSettings";
            int i = 0;
            bool mustretry = true;
            while (mustretry)
            {
                mustretry = false;
                foreach (var item in ArtemisSettingsFiles)
                {
                    if (item.Name == newFile)
                    {
                        newFile = "NewArtemisSettings(" + (++i).ToString() + ")";
                        mustretry = true;
                        break;
                    }
                }
            }

            string? source = ArtemisManager.GetOriginalArtemisINIFile(ModItem.ActivatedFolder);
            string target = System.IO.Path.Combine(ArtemisManager.ArtemisINIFolder, newFile + ArtemisManager.INIFileExtension);
            if (!string.IsNullOrEmpty(source) && File.Exists(source))
            {
                File.Copy(source, target, true);
            }
            else
            {
                ArtemisINI f = new()
                {
                    SaveFile = target
                };
                f.Save();
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


        private void OnSendFileRequest(object sender, FileRequestRoutedEventArgs e)
        {
            e.File = SelectedArtemisSettings;
        }

        private void OnTransmissionCompleted(object sender, RoutedEventArgs e)
        {
            PopupMessage = "Transmission completed.";
        }

        private void OnSettingsFilenameChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlockEditControl me)
            {
                if (me.Tag is FileListItem selectedFile)
                {
                    if (!string.IsNullOrEmpty(selectedFile.Name) && !string.IsNullOrEmpty(selectedFile.OriginalName) && selectedFile.Name != selectedFile.OriginalName)
                    {
                        string source = System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, selectedFile.OriginalName + ArtemisManager.INIFileExtension);
                        string target = System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, selectedFile.Name + ArtemisManager.INIFileExtension);
                        if (File.Exists(target))
                        {
                            MessageBox.Show("Cannot rename--new name already exists.", "Rename settings file", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            File.Move(source, target, true);
                            selectedFile.OriginalName = selectedFile.Name;

                            if (SelectedArtemisSettings != null && SelectedArtemisSettings.SaveFile == selectedFile.OriginalName + ArtemisManager.INIFileExtension)
                            {
                                SelectedArtemisSettings.SaveFile = selectedFile.Name + ArtemisManager.INIFileExtension;
                            }
                        }
                    }
                }
            }
        }

        private void OnExportSettingsFile(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is ArtemisINI presetsFile)
            {
                SaveFileDialog dialg = new()
                {
                    CheckPathExists = true,
                    Filter = "Artemis INI settings files (*" + ArtemisManager.INIFileExtension + ")|*" + ArtemisManager.INIFileExtension,
                    DefaultExt = "ini",
                    InitialDirectory = ArtemisManager.ArtemisINIFolder,
                    Title = "Select new Artemis settings filename"
                };
                if (dialg.ShowDialog() == true)
                {
                    bool OkayToSave = true;
                    if (File.Exists(dialg.FileName))
                    {
                        OkayToSave = (System.Windows.MessageBox.Show("Do you want to overwrite " + new FileInfo(dialg.FileName).Name + "?",
                            "Artemis INI Settings", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
                    }
                    if (OkayToSave)
                    {
                        File.Copy(presetsFile.SaveFile, dialg.FileName, true);
                    }
                }
            }
        }

        private void OnDeleteSettingsFile(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is FileListItem selectedFile)
                {
                    if (!string.IsNullOrEmpty(selectedFile.Name))
                    {
                        string target = System.IO.Path.Combine(ArtemisManager.ArtemisINIFolder, selectedFile.Name + ArtemisManager.INIFileExtension);
                        if (File.Exists(target))
                        {
                            if (ArtemisSettingsFiles.Contains(selectedFile))
                            {
                                ArtemisSettingsFiles.Remove(selectedFile);
                            }
                            if (SelectedArtemisSettings != null && SelectedArtemisSettings.SaveFile == selectedFile.Name + ArtemisManager.INIFileExtension)
                            {
                                SelectedArtemisSettings = null;
                            }
                            if (SelectedArtemisSettingsFile == selectedFile)
                            {
                                SelectedArtemisSettingsFile = null;
                            }
                        }
                    }
                }
            }
        }

        private void OnSettingsFileRename(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is FileListItem selectedFile)
                {
                    selectedFile.IsEditMode = true;
                }
            }
        }

        private void OnActivateSettingsFile(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is FileListItem selectedFile)
                {
                    if (!string.IsNullOrEmpty(selectedFile.Name))
                    {
                        string source = System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, selectedFile.Name + ArtemisManager.INIFileExtension);
                        if (System.IO.File.Exists(source))
                        {
                            File.Copy(source, System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisINI), true);
                            PopupMessage = "Artemis INI file activated.";
                        }
                    }
                }
            }
        }
    }
}