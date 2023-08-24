using ArtemisManagerAction;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Forms;
using AMCommunicator;
using System.Net;

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

            RestoreOriginalToolTip = string.Format("Restore Original artemis.ini file ({0}) to defaults.",  ArtemisManager.GetOriginalArtemisINIFile(ModItem.ActivatedFolder));
            
            InitializeComponent();
            InitializeFileList();
        }
        void InitializeFileList()
        {
            ArtemisSettingsFiles.Clear();
            
            if (IsRemote)
            {
                if (fsw != null)
                {
                    fsw.EnableRaisingEvents = false;
                    fsw.Created -= OnINIFileCreated;
                    fsw.Deleted -= OnINIFileDeleted;
                    fsw.Changed -= Fsw_Changed;
                    fsw.Renamed -= Fsw_Renamed;

                    fsw.Dispose();
                    fsw = null;
                }
                //Need to send request for: list of artemisINI files
                //Need to send request for: list of available resolutions.
                if (TargetClient != null)
                {
                    if (Network.Current != null)
                    {
                        Network.Current.InfoReceived += OnInfoReceived;
                    }
                    Network.Current?.SendRequestInformation(TargetClient, RequestInformationType.ListOfArtemisINIFiles);
                    
                }
            }
            else
            {
                foreach (var ini in ArtemisManager.GetArtemisINIFileList())
                {
                    ArtemisSettingsFiles.Add(new ArtemisINIFileListItem(ini.Substring(0, ini.Length - 4)));
                }
                fsw = new FileSystemWatcher(ArtemisManager.ArtemisINIFolder);
                fsw.Created += OnINIFileCreated;
                fsw.Deleted += OnINIFileDeleted;

                fsw.Changed += Fsw_Changed;
                fsw.Renamed += Fsw_Renamed;
                fsw.EnableRaisingEvents = true;
            }
        }

        private void Fsw_Renamed(object sender, RenamedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(e.Name))
                {
                    foreach (var file in ArtemisSettingsFiles)
                    {
                        if (file.OriginalName == e.OldName)
                        {
                            file.Name = e.Name;
                            file.OriginalName = e.Name;
                            if (file.INIFile != null)
                            {
                                file.INIFile.SaveFile = e.Name;
                            }
                        }
                    }
                }
                Network.Current?.SendInformation(TakeAction.AllConnections, RequestInformationType.ListOfArtemisINIFiles, string.Empty, ArtemisManager.GetArtemisINIFileList());
            });
        }

        private void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (SelectedArtemisSettingsFile != null && SelectedArtemisSettingsFile.INIFile?.SaveFile == e.Name)
                {
                    SelectedArtemisSettingsFile.INIFile = new ArtemisINI(e.FullPath);
                }
                else
                {
                    foreach (var file in ArtemisSettingsFiles)
                    {
                        if (file.INIFile?.SaveFile == e.Name)
                        {
                            file.INIFile = new(e.FullPath);
                        }
                    }
                }
            });
        }

        private void OnInfoReceived(object? sender, InformationEventArgs e)
        {
            if (!e.Handled)
            {
                Network.Current?.RaiseStatusUpdate("ArtemisINIManagerControl InfoReceive processing...");
                Dispatcher.Invoke(() =>
                {
                    if (e.Source != null && TargetClient != null)
                    {
                        if (e.Source.ToString() == TargetClient.ToString())
                        {
                            switch (e.RequestType)
                            {
                                case RequestInformationType.ListOfArtemisINIFiles:
                                    ProcessListOfArtemisINIFiles(e.Data);
                                    Network.Current?.RaiseStatusUpdate("ArtemisINIManagerControl InfoReceive - ListOfArtemisINIFiles...");
                                    e.Handled = true;
                                    break;
                                case RequestInformationType.SpecificArtemisINIFile:
                                    if (e.Data.Length > 0)
                                    {
                                        ProcessSpecificArtemisINIFile(e.Identifier, e.Data[0]);
                                    }
                                    Network.Current?.RaiseStatusUpdate("ArtemisINIManagerControl InfoReceive - SpecificArtemisINIFile...");
                                    e.Handled = true;
                                    break;
                            }
                        }
                    }
                });
            }
            else
            {
                Network.Current?.RaiseStatusUpdate("ArtemisINIManagerControl InfoReceive processing---already handled, nothing done.");
            }
        }
        private void ProcessListOfArtemisINIFiles(string[] names)
        {
            ArtemisSettingsFiles.Clear();
            foreach (var nm in names)
            {
                string newnm = nm;
                if (nm.Contains('.'))
                {
                    newnm = nm.Substring(0, nm.Length - 4);
                }
                ArtemisSettingsFiles.Add(new ArtemisINIFileListItem(newnm));
            }
        }
        private void ProcessSpecificArtemisINIFile(string filename, string data)
        {
            foreach (var item in ArtemisSettingsFiles)
            {
                if ( item.Name == filename)
                {
                    var ini = new ArtemisINI();
                    ini.Deserialize(data);
                    ini.SaveFile = filename + ArtemisManager.INIFileExtension;
                    item.SettingsFile = ini;
                    item.INIFile = ini;
                }
            }
        }
        FileSystemWatcher? fsw = null;
        string? selectedININame = null;
        private void OnINIFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Name) && e.Name.EndsWith(ArtemisManager.INIFileExtension))
            {
                this.Dispatcher.Invoke(() =>
                {
                    string match = e.Name.Substring(0, e.Name.Length - 4);
                    ArtemisINIFileListItem? remover = null;
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
                        if (SelectedArtemisSettingsFile != null && SelectedArtemisSettingsFile == remover)
                        {
                            selectedININame = SelectedArtemisSettingsFile.Name;
                            SelectedArtemisSettingsFile = null;
                        }
                    }
                });
            }
            Network.Current?.SendInformation(TakeAction.AllConnections, RequestInformationType.ListOfArtemisINIFiles, string.Empty, ArtemisManager.GetArtemisINIFileList());
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
                        ArtemisINIFileListItem newINI = new(match);
                        ArtemisSettingsFiles.Add(newINI);
                        if (match == selectedININame)
                        {
                            SelectedArtemisSettingsFile = newINI;

                        }
                    }
                });
            }
            selectedININame = null;
            Network.Current?.SendInformation(TakeAction.AllConnections, RequestInformationType.ListOfArtemisINIFiles, string.Empty, ArtemisManager.GetArtemisINIFileList());
        }
        public static readonly DependencyProperty IsRemoteProperty =
           DependencyProperty.Register(nameof(IsRemote), typeof(bool),
           typeof(ArtemisINIManagerControl), new PropertyMetadata(OnIsRemoteChanged));

        private static void OnIsRemoteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArtemisINIManagerControl control)
            {
                control.InitializeFileList();
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
           typeof(ArtemisINIManagerControl));

    
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
            DependencyProperty.Register(nameof(ArtemisSettingsFiles), typeof(ObservableCollection<ArtemisINIFileListItem>),
                typeof(ArtemisINIManagerControl));

        public ObservableCollection<ArtemisINIFileListItem> ArtemisSettingsFiles
        {
            get
            {
                return (ObservableCollection<ArtemisINIFileListItem>)this.GetValue(ArtemisSettingsFilesProperty);

            }
            set
            {
                this.SetValue(ArtemisSettingsFilesProperty, value);

            }
        }
        public static readonly DependencyProperty SelectedArtemisSettingsFileProperty =
            DependencyProperty.Register(nameof(SelectedArtemisSettingsFile), typeof(ArtemisINIFileListItem),
             typeof(ArtemisINIManagerControl), new PropertyMetadata(OnSelectedArtemisSettingsFileChanged));

        private static void OnSelectedArtemisSettingsFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArtemisINIManagerControl me && me.SelectedArtemisSettingsFile != null)
            {
                
                if (me.IsRemote)
                {
                    if (me.TargetClient != null)
                    {
                        Network.Current?.SendRequestInformation(me.TargetClient, RequestInformationType.SpecificArtemisINIFile, me.SelectedArtemisSettingsFile.Name);
                    }
                }
                else
                {
                    me.SelectedArtemisSettingsFile.INIFile = ArtemisManager.GetArtemisINI(me.SelectedArtemisSettingsFile.Name);
                }
                
            }
        }

        public ArtemisINIFileListItem? SelectedArtemisSettingsFile
        {
            get
            {
                return (ArtemisINIFileListItem?)this.GetValue(SelectedArtemisSettingsFileProperty);

            }
            set
            {
                this.SetValue(SelectedArtemisSettingsFileProperty, value);

            }
        }
        
        //public static readonly DependencyProperty SelectedArtemisSettingsProperty =
        //    DependencyProperty.Register(nameof(SelectedArtemisSettings), typeof(ArtemisINI),
        //     typeof(ArtemisINIManagerControl));

        //public ArtemisINI? SelectedArtemisSettings
        //{
        //    get
        //    {
        //        return (ArtemisINI?)this.GetValue(SelectedArtemisSettingsProperty);

        //    }
        //    set
        //    {
        //        this.SetValue(SelectedArtemisSettingsProperty, value);

        //    }
        //}
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
        string getNewFileName()
        {
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
            return newFile;
        }

        private void OnAddSettingsFile(object sender, RoutedEventArgs e)
        {
            if (IsRemote)
            {
                if (TargetClient != null)
                {
                    ArtemisINI ini = new ArtemisINI();
                    ini.Deserialize(Properties.Resources.artemis);

                    ini.SaveFile=getNewFileName();
                    string filedata = ini.GetSerializedString();
                    Network.Current?.SendArtemisAction(TargetClient, AMCommunicator.Messages.ArtemisActions.InstallArtemisINI, Guid.Empty, filedata, ini.SaveFile);
                }
            }
            else
            {
                ModManager.CreateFolder(ArtemisManagerAction.ArtemisManager.ArtemisINIFolder);
                string newFile = getNewFileName();
                string target = System.IO.Path.Combine(ArtemisManager.ArtemisINIFolder, newFile + ArtemisManager.INIFileExtension);
                ArtemisINI f = new();
                f.Deserialize(Properties.Resources.artemis);
                f.SaveFile = target;
                f.Save();
            }
        }

        private void OnRestoreToDefault(object sender, RoutedEventArgs e)
        {
            if (IsRemote)
            {
                if (TargetClient != null)
                {
                    Network.Current?.SendArtemisAction(TargetClient,
                        AMCommunicator.Messages.ArtemisActions.RestoreArtemisINIToDefault,
                        Guid.Empty, Properties.Resources.artemis);
                }
            }
            else
            {
                ArtemisManager.RestoreArtemisINIToDefault(Properties.Resources.artemis);
                //string originalINI = ArtemisManager.GetOriginalArtemisINIFile(ModItem.ActivatedFolder);
                //if (!string.IsNullOrEmpty(originalINI) && File.Exists(originalINI))
                //{
                //    File.Copy(originalINI, ArtemisManager.ArtemisINIFolder);
                //}
            }
        }



        private void OnImportFile(object sender, RoutedEventArgs e)
        {
            //Not valid for remote.
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

                FileInfo source = new(ofd.FileName);

                string target = System.IO.Path.Combine(ArtemisManager.ArtemisINIFolder, source.Name);
                int i = 0;
                while (File.Exists(target))
                {
                    int j = source.Name.IndexOf('.');
                    if (j < 0)
                    {
                        j = source.Name.Length;
                    }
                    target = System.IO.Path.Combine(ArtemisManager.ArtemisINIFolder,
                        string.Format("{0}({1})", source.Name.Substring(0, j), (++i).ToString()) + ArtemisManager.INIFileExtension);
                }
                source.CopyTo(target, true);

            }
        }


        private void OnSendFileRequest(object sender, FileRequestRoutedEventArgs e)
        {
            e.File = SelectedArtemisSettingsFile?.SettingsFile;
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
                    bool isOkay = true;
                    foreach (var file in this.ArtemisSettingsFiles)
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
                                    AMCommunicator.Messages.ArtemisActions.RenameArtemisINIFile,
                                    Guid.Empty,
                                    selectedFile.OriginalName + ":" + selectedFile.Name);
                            }
                        }
                        else
                        {
                            if (!ArtemisManager.RenameArtemisINIFile(selectedFile.OriginalName, selectedFile.Name))
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

        private void OnExportSettingsFile(object sender, RoutedEventArgs e)
        {
            if (!IsRemote)
            {
                if (sender is MenuItem m)
                {
                    if (m.CommandParameter is ArtemisINIFileListItem presetsFile)
                    {
                        Microsoft.Win32.SaveFileDialog dialg = new()
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
                            if (OkayToSave && presetsFile.INIFile != null)
                            {
                                File.Copy(presetsFile.INIFile.SaveFile, dialg.FileName, true);
                            }
                        }
                    }
                }
            }
        }

        private void OnDeleteSettingsFile(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is ArtemisINIFileListItem selectedFile)
                {
                    if (!string.IsNullOrEmpty(selectedFile.Name))
                    {
                        if (IsRemote)
                        {
                            if (TargetClient != null)
                            {
                                Network.Current?.SendArtemisAction(TargetClient, AMCommunicator.Messages.ArtemisActions.DeleteArtemisINIFile, Guid.Empty, selectedFile.Name);
                            }
                        }
                        else
                        {
                            if (ArtemisManager.DeleteArtemisINIFile(selectedFile.Name))
                            {
                               
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
                        if (IsRemote)
                        {
                            if (TargetClient != null)
                            {
                                Network.Current?.SendArtemisAction(TargetClient, AMCommunicator.Messages.ArtemisActions.ActivateArtemisINIFile, Guid.Empty, selectedFile.Name);
                            }
                        }
                        else
                        {
                            if (ArtemisManager.ActivateArtemisINIFile(selectedFile.Name))
                            {
                                PopupMessage = "Artemis INI file activated.";
                            }
                        }
                    }
                }
            }
        }

        private void OnSavingSettings(object sender, RoutedEventArgs e)
        {
            if (!IsRemote && fsw != null)
            {
                this.fsw.EnableRaisingEvents = false;
            }
        }

        private void OnSettingsSaved(object sender, RoutedEventArgs e)
        {
            if (!IsRemote && fsw != null)
            {
                fsw.EnableRaisingEvents = true;
            }
        }

        private void OnTest(object sender, RoutedEventArgs e)
        {

        }
    }
}