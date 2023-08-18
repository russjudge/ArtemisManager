using AMCommunicator;
using ArtemisManagerAction;
using ArtemisManagerAction.ArtemisEngineeringPresets;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for EngineeringPresetEditControl.xaml
    /// </summary>
    public partial class EngineeringPresetEditControl : UserControl
    {
        public EngineeringPresetEditControl()
        {
            PresetFiles = new ObservableCollection<EngineeringPresetFileListItem>();
            foreach (var name in ArtemisManagerAction.ArtemisManager.GetEngineeringPresetFiles())
            {
                PresetFiles.Add(new EngineeringPresetFileListItem(name.Substring(0, name.Length - 4)));
            }
            InitializeComponent();
            ModManager.CreateFolder(ArtemisManager.EngineeringPresetsFolder);
            Initialize();
        }
        void Initialize()
        {
            if (IsRemote)
            {
                if (watcher != null)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Created -= Watcher_Created;
                    watcher.Deleted -= Watcher_Deleted;
                    watcher.Renamed -= Watcher_Renamed;
                    watcher.Changed -= Watcher_Changed;

                    watcher.Dispose();
                    watcher = null;
                }
            }
            else
            {
                if (watcher != null)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                    watcher = null;
                }
                watcher = new FileSystemWatcher(ArtemisManager.EngineeringPresetsFolder);
                watcher.Created += Watcher_Created;
                watcher.Deleted += Watcher_Deleted;
                watcher.Renamed += Watcher_Renamed;
                watcher.Changed += Watcher_Changed;
                watcher.EnableRaisingEvents = true;
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Name) && e.Name.EndsWith(ArtemisManager.DATFileExtension))
            {
                Dispatcher.Invoke(() =>
                {
                    string match = e.Name.Substring(0, e.Name.Length - 4);
                    EngineeringPresetFileListItem? remover = null;
                    foreach (var item in PresetFiles)
                    {
                        if (item.Name == match)
                        {
                            remover = item;
                            break;
                        }
                    }
                    if (remover != null)
                    {
                        remover.INIFile = new(e.FullPath);
                    }
                });
            }
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Name) && e.Name.EndsWith(ArtemisManager.DATFileExtension))
            {
                Dispatcher.Invoke(() =>
                {
                    string match = e.Name.Substring(0, e.Name.Length - 4);
                    EngineeringPresetFileListItem? remover = null;
                    foreach (var item in PresetFiles)
                    {
                        if (item.Name == match)
                        {
                            remover = item;
                            break;
                        }
                    }
                    if (remover != null && remover.SettingsFile != null)
                    {
                        remover.SettingsFile.SaveFile = e.FullPath;
                        
                    }
                });
            }
        }

        public static readonly DependencyProperty IsRemoteProperty =
           DependencyProperty.Register(nameof(IsRemote), typeof(bool),
           typeof(EngineeringPresetEditControl), new PropertyMetadata(OnIsRemoteChanged));

        private static void OnIsRemoteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EngineeringPresetEditControl me)
            {
                me.Initialize();
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


        public static readonly DependencyProperty TargetClientProperty =
           DependencyProperty.Register(nameof(TargetClient), typeof(IPAddress),
           typeof(EngineeringPresetEditControl));


        public IPAddress? TargetClient
        {
            get
            {
                return (IPAddress?)this.GetValue(TargetClientProperty);

            }
            set
            {
                this.SetValue(TargetClientProperty, value);

            }
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Name) && e.Name.EndsWith(ArtemisManager.DATFileExtension))
            {
                Dispatcher.Invoke(() =>
                {
                    string match = e.Name.Substring(0, e.Name.Length - 4);
                    EngineeringPresetFileListItem? remover = null;
                    foreach (var item in PresetFiles)
                    {
                        if (item.Name == match)
                        {
                            remover = item;
                            break;
                        }
                    }
                    if (remover != null)
                    {
                        PresetFiles.Remove(remover);
                    }
                });
            }
        }

        public static readonly DependencyProperty PopupMessageProperty =
         DependencyProperty.Register(nameof(PopupMessage), typeof(string),
             typeof(EngineeringPresetEditControl));

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
        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Name) && e.Name.EndsWith(ArtemisManager.DATFileExtension))
            {
                Dispatcher.Invoke(() =>
                {
                    string match = e.Name.Substring(0, e.Name.Length - 4);
                    FileListItem? remover = null;
                    foreach (var item in PresetFiles)
                    {
                        if (item.Name == match)
                        {
                            remover = item;
                            break;
                        }
                    }
                    if (remover == null)
                    {
                        PresetFiles.Add(new(match));
                    }
                    
                });
            }
        }

        FileSystemWatcher? watcher;


        public static readonly DependencyProperty PresetFilesProperty =
          DependencyProperty.Register(nameof(PresetFiles), typeof(ObservableCollection<EngineeringPresetFileListItem>),
          typeof(EngineeringPresetEditControl));

        public ObservableCollection<EngineeringPresetFileListItem> PresetFiles
        {
            get
            {
                return (ObservableCollection<EngineeringPresetFileListItem>)this.GetValue(PresetFilesProperty);
            }
            set
            {
                this.SetValue(PresetFilesProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedPresetFileProperty =
            DependencyProperty.Register(nameof(SelectedPresetFile), typeof(EngineeringPresetFileListItem),
            typeof(EngineeringPresetEditControl), new PropertyMetadata(OnSelectedPresetFileChanged));

        private static void OnSelectedPresetFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EngineeringPresetEditControl me)
            {
                if (me.SelectedPresetFile != null && !string.IsNullOrEmpty(me.SelectedPresetFile.Name)
                    && System.IO.File.Exists(System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, me.SelectedPresetFile.Name + ArtemisManager.DATFileExtension)))
                {
                    bool IsOkay = true;

                    if (me.SelectedPresetFile != null && me.SelectedPresetFile.INIFile != null && me.SelectedPresetFile.INIFile.HasChanges())
                    {
                        switch (System.Windows.MessageBox.Show("Save changes to current Presets File?", "Presets File changed", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                        {
                            case MessageBoxResult.Yes:
                                if (me.IsRemote)
                                {
                                    if (me.TargetClient != null)
                                    {
                                        Network.Current?.SendArtemisAction(me.TargetClient, AMCommunicator.Messages.ArtemisActions.InstallEngineeringPresets, Guid.Empty, me.SelectedPresetFile.INIFile.GetSerializedString());
                                    }
                                }
                                else
                                {
                                    me.SelectedPresetFile.INIFile.Save();
                                }
                                IsOkay = true;
                                break;
                            case MessageBoxResult.No:
                                IsOkay = true;
                                break;
                            case MessageBoxResult.Cancel:
                                IsOkay = false;
                                break;
                        }
                    }
                    if (IsOkay)
                    {
                        //me.SelectedPresetFile = new EngineeringPresetFileListItem()
                        if (me.SelectedPresetFile != null)
                        {
                            if (me.IsRemote)
                            {
                                
                            }
                            else
                            {
                                me.SelectedPresetFile.INIFile = new(System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, me.SelectedPresetFile.Name + ArtemisManager.DATFileExtension));
                            }
                        }
                    }
                    else
                    {
                        if (me.SelectedPresetFile != null && me.SelectedPresetFile.INIFile != null && !string.IsNullOrEmpty(me.SelectedPresetFile.INIFile.SaveFile))
                        {
                            if (me.IsRemote)
                            {
                               
                            }
                            else
                            {
                                var fle = new System.IO.FileInfo(me.SelectedPresetFile.INIFile.SaveFile);
                                me.SelectedPresetFile = new(fle.Name.Substring(0, fle.Name.Length - 4));
                            }
                        }
                    }
                }
            }
        }

        public EngineeringPresetFileListItem? SelectedPresetFile
        {
            get
            {
                return (EngineeringPresetFileListItem?)this.GetValue(SelectedPresetFileProperty);
            }
            set
            {
                this.SetValue(SelectedPresetFileProperty, value);
            }
        }
        string GetNewFilename()
        {
            string newFile = "NewPresets";
            string baseName = newFile;
            int i = 0;
            bool isOkay = true;
            do
            {
                isOkay = true;
                foreach (var preset in PresetFiles)
                {
                    if (preset.Name == newFile)
                    {
                        isOkay = false;
                        newFile = baseName + "(" + (++i).ToString() + ")";
                    }
                }
            } while (!isOkay);
            return newFile;
        }

        private void OnAddPresetFile(object sender, RoutedEventArgs e)
        {
            string newFile = GetNewFilename();
            PresetsFile f = new()
            {
                SaveFile = System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, newFile + ArtemisManager.DATFileExtension)
            };
            if (IsRemote)
            {
                if (TargetClient != null)
                {
                    Network.Current?.SendArtemisAction(TargetClient, AMCommunicator.Messages.ArtemisActions.InstallEngineeringPresets, Guid.Empty, f.GetSerializedString());
                }
            }
            else
            {
                f.Save();
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {

            if (e.OriginalSource is PresetsFile presetsFile && !string.IsNullOrEmpty(presetsFile?.SaveFile) && System.IO.File.Exists(presetsFile?.SaveFile))
            {
                if (IsRemote)
                {
                    if (TargetClient != null)
                    {
                        Network.Current?.SendArtemisAction(TargetClient, AMCommunicator.Messages.ArtemisActions.DeleteEngineeringPresetsFile, Guid.Empty, new FileInfo(presetsFile.SaveFile).Name);
                    }
                }
                else
                {
                    string n = new FileInfo(presetsFile.SaveFile).Name;
                    string nm = n.Substring(0, n.Length - 4);

                    string fullname = presetsFile.SaveFile;
                    if (System.Windows.MessageBox.Show(string.Format("Are you sure you want to delete {0}", nm), "Delete Presets file",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        presetsFile.Delete();

                        //EngineeringPresetFileListItem? remover = null;
                        //foreach (var item in PresetFiles)
                        //{
                        //    if (item.Name == nm)
                        //    {
                        //        remover = item;
                        //        break;
                        //    }
                        //}
                        //if (remover != null)
                        //{
                        //    PresetFiles.Remove(remover);
                        //}
                        //if (SelectedPresetFile?.Name == nm)
                        //{
                        //    SelectedPresetFile = null;
                        //}
                    }
                }
            }
        }

        private void OnActivate(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is PresetsFile presetsFile && !string.IsNullOrEmpty(presetsFile?.SaveFile) && System.IO.File.Exists(presetsFile?.SaveFile))
            {
                if (IsRemote)
                {
                    if (TargetClient != null)
                    {
                        Network.Current?.SendArtemisAction(TargetClient, AMCommunicator.Messages.ArtemisActions.ActivateEngineeringPresetsFile, Guid.Empty, new FileInfo(presetsFile.SaveFile).Name);
                    }
                }
                else
                {
                    ArtemisManager.ActivateEngineeringPresetFile(presetsFile.SaveFile);
                    //File.Copy(presetsFile.SaveFile, System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisEngineeringFile), true);
                }
            }
        }

        private void OnRestoreToDefault(object sender, RoutedEventArgs e)
        {
            if (IsRemote)
            {
                if (TargetClient != null)
                {
                    Network.Current?.SendArtemisAction(TargetClient, AMCommunicator.Messages.ArtemisActions.RestoreEngineeringPresetsToDefault, Guid.Empty, string.Empty);
                }
            }
            else
            {
                ArtemisManager.RestoreEngineeringPresetsToDefault();
                System.Windows.MessageBox.Show("Engieering Presets restored to defaults.");
            }
        }

        private void OnExportPresets(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem m)
            {
                if (m.CommandParameter is EngineeringPresetFileListItem presetsFile)
                {
                    SaveFileDialog dialg = new()
                    {
                        CheckPathExists = true,
                        Filter = "Engineering Presets (*" + ArtemisManager.DATFileExtension + ")|*" + ArtemisManager.DATFileExtension,
                        DefaultExt = "dat",
                        InitialDirectory = ArtemisManagerAction.ArtemisManager.EngineeringPresetsFolder,
                        Title = "Select new presets filename"
                    };
                    if (dialg.ShowDialog() == true)
                    {
                        bool OkayToSave = true;
                        if (File.Exists(dialg.FileName))
                        {
                            OkayToSave = (System.Windows.MessageBox.Show("Do you want to overwrite " + new FileInfo(dialg.FileName).Name + "?",
                                "Export Presets File", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
                        }
                        if (OkayToSave && presetsFile.INIFile != null)
                        {
                            File.Copy(presetsFile.INIFile.SaveFile, dialg.FileName, true);
                        }
                    }
                }
            }
        }

        private void OnImportPresets(object sender, RoutedEventArgs e)
        {
            bool OkayToSave = true;
            OpenFileDialog dialg = new()
            {
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = "Engineering Presets (*" + ArtemisManager.DATFileExtension + ")|*" + ArtemisManager.DATFileExtension,
                DefaultExt = "dat",
                Title = "Select presets filename"
            };
            if (dialg.ShowDialog() == true)
            {
                FileInfo source = new(dialg.FileName);
                PresetsFile? work = null;
                try
                {
                    work = new PresetsFile(source.FullName);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        string.Format("Invalid Engineering Presets file:{1}{1}{0}", ex.Message, Environment.NewLine),
                        "Import Engineering Presets",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    OkayToSave = false;

                }
                if (OkayToSave)
                {
                    if (work == null)
                    {
                        System.Windows.MessageBox.Show(
                       "Invalid Engineering Presets file.  Unable to process.",
                       "Import Engineering Presets",
                       MessageBoxButton.OK,
                       MessageBoxImage.Error);
                    }
                    else
                    {
                        FileInfo target = new(System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, source.Name));
                        if (target.Exists)
                        {
                            OkayToSave = (System.Windows.MessageBox.Show("Do you want to overwrite " + source.Name + "?",
                                       "Import Presets File", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
                        }
                        if (OkayToSave)
                        {
                            source.CopyTo(target.FullName, true);
                            //SelectedPresetFile.INIFile = work;
                            
                        }
                    }
                }
            }
        }

        ////private void OnSaveSelectedPresets(object sender, RoutedEventArgs e)
        ////{
        ////    if (IsRemote)
        ////    {
        ////        //TODO: Remotely save the changes
        ////        if (TargetClient != null)
        ////        {
        ////            //Same as send file to target.
        ////            //                    Network.Current?.SendArtemisAction(TargetClient, AMCommunicator.Messages.ArtemisActions.InstallEngineeringPresets, Guid.Empty, );
        ////            Network.Current?.SendStringPackageFile(TargetClient, SelectedPresetFile.SettingsFile.GetSerializedString(), AMCommunicator.Messages.SendableStringPackageFile.EngineeringPreset, SelectedPresetFile.);
        ////        }
        ////    }
        ////    else
        ////    {
        ////        SelectedPresetFile?.INIFile?.Save();
        ////    }
        ////}

        private void OnActivateSelectedPresets(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is FileListItem selectedFile)
                {
                    if (!string.IsNullOrEmpty(selectedFile.Name))
                    {
                        if (IsRemote)
                        {
                            if (TargetClient != null )
                            {
                                Network.Current?.SendArtemisAction(TargetClient, AMCommunicator.Messages.ArtemisActions.ActivateEngineeringPresetsFile, Guid.Empty, selectedFile.Name );
                            }
                        }
                        else
                        {
                            string source = System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, selectedFile.Name + ArtemisManager.DATFileExtension);
                            if (System.IO.File.Exists(source))
                            {
                                File.Copy(source, System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisEngineeringFile), true);
                                PopupMessage = "Presets Activated.";
                            }
                        }
                    }
                }
            }
        }

        private void OnDeleteSelectedPresets(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is EngineeringPresetFileListItem selectedFile)
                {
                    if (!string.IsNullOrEmpty(selectedFile.Name))
                    {
                        if (IsRemote)
                        {
                            if (TargetClient != null)
                            {
                                Network.Current?.SendArtemisAction(TargetClient, AMCommunicator.Messages.ArtemisActions.DeleteEngineeringPresetsFile, Guid.Empty, selectedFile.Name);
                            }
                        }
                        else
                        {
                            ArtemisManager.DeleteEngineeringPresetsFile(selectedFile.Name);
                            //if (PresetFiles.Contains(selectedFile))
                            //{
                            //    PresetFiles.Remove(selectedFile);
                            //}
                            //if (SelectedPresetFile != null && SelectedPresetFile.INIFile != null && SelectedPresetFile.INIFile.SaveFile == selectedFile.Name + ArtemisManager.DATFileExtension)
                            //{
                            //    SelectedPresetFile = null;
                            //}
                            //if (SelectedPresetFile == selectedFile)
                            //{
                            //    SelectedPresetFile = null;
                            //}
                        }
                    }
                }
            }
        }

        private void OnSaved(object sender, RoutedEventArgs e)
        {
            PopupMessage = "Presets Saved.";
        }
        private void OnTransmissionCompleted(object sender, RoutedEventArgs e)
        {
            PopupMessage = "Transmission completed.";
        }

        private void OnPresetRename(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is FileListItem selectedFile)
                {
                    selectedFile.IsEditMode = true;
                }
            }
        }

        private void OnSelectedFilenameTextChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlockEditControl me)
            {
                if (me.Tag is FileListItem selectedFile)
                {
                    bool isOkay = true;
                    foreach (var file in this.PresetFiles)
                    {
                        if (selectedFile != file)
                        {
                            if (file.Name == selectedFile.Name)
                            {
                                isOkay = false;
                                break;
                            }
                        }
                    }
                    if (isOkay)
                    {
                        if (IsRemote)
                        {
                            if (TargetClient != null)
                            {
                                Network.Current?.SendArtemisAction(
                                    TargetClient,
                                    AMCommunicator.Messages.ArtemisActions.RenameEngineeringPresetsFile,
                                    Guid.Empty, selectedFile.OriginalName + ":" + selectedFile.Name);
                            }
                        }
                        else
                        {
                            if (!ArtemisManager.RenameEngineeringPresetsFile(selectedFile.OriginalName, selectedFile.Name))
                            {

                            }
                        }
                        selectedFile.OriginalName = selectedFile.Name;
                    }
                    else
                    {
                        selectedFile.Name = selectedFile.OriginalName;
                        System.Windows.MessageBox.Show("Cannot rename--new name already exists.", "Rename settings file", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
