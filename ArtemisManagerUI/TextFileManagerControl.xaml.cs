﻿using AMCommunicator;
using AMCommunicator.Messages;
using ArtemisManagerAction;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for TextFileManagerControl.xaml
    /// </summary>
    public partial class TextFileManagerControl : UserControl
    {
        public TextFileManagerControl()
        {
            DataFileList = new ObservableCollection<TextDataFile>();
            InitializeComponent();
        }
        FileSystemWatcher? fsw = null;
        void LoadControlsINIFiles()
        {
            LoadFiles(ArtemisManager.GetControlsINIFileList());
        }
        void LoadDMXCommandsFiles()
        {
            LoadFiles(ArtemisManager.GetDMXCommandsFileList());
        }
        void LoadFiles(string[] files)
        {
            foreach (var file in files)
            {
                FileInfo fle = new(file);
                TextDataFile f = new(FileType, GetNameOfFile(fle.Name), file);
                DataFileList.Add(f);
            }
        }
        void InitializeFileList()
        {
            DataFileList.Clear();
            if (IsRemote)
            {
                if (fsw != null)
                {
                    fsw.EnableRaisingEvents = false;
                    fsw.Changed -= Fsw_Changed;
                    fsw.Created -= Fsw_Created;
                    fsw.Deleted -= Fsw_Deleted;
                    fsw.Renamed -= Fsw_Renamed;
                    fsw.Dispose();
                    fsw = null;
                }
                if (Network.Current != null)
                {
                    Network.Current.InfoReceived += OnInfoReceived;
                }
                if (TargetClient != null)
                {
                    switch (FileType)
                    {
                        case SendableStringPackageFile.controlsINI:
                            Network.Current?.SendRequestInformation(TargetClient, RequestInformationType.ListOfControLINIFiles);
                            break;
                        case SendableStringPackageFile.DMXCommandsXML:
                            Network.Current?.SendRequestInformation(TargetClient, RequestInformationType.ListOfDMXCommandfiles);
                            break;
                    }
                }
            }
            else
            {
                switch (FileType)
                {
                    case SendableStringPackageFile.controlsINI:
                        LoadControlsINIFiles();
                        break;
                    case SendableStringPackageFile.DMXCommandsXML:
                        LoadDMXCommandsFiles();
                        break;
                }
                if (fsw != null)
                {
                    fsw.EnableRaisingEvents = false;
                    fsw.Dispose();
                    fsw = null;
                }
                fsw = new FileSystemWatcher(GetTargetFolder());

                fsw.Changed += Fsw_Changed;
                fsw.Created += Fsw_Created;
                fsw.Deleted += Fsw_Deleted;
                fsw.Renamed += Fsw_Renamed;
                fsw.EnableRaisingEvents = true;
            }
        }
        private void OnInfoReceived(object? sender, InformationEventArgs e)
        {
            if (!e.Handled)
            {
                Network.Current?.RaiseStatusUpdate("TextFileManagerControl InfoReceive processing...");
                Dispatcher.Invoke(() =>
                {
                    if (e.Source != null && TargetClient != null)
                    {
                        if (e.Source.ToString() == TargetClient.ToString())
                        {
                            switch (e.RequestType)
                            {
                                case RequestInformationType.ListOfControLINIFiles:
                                    ProcessListOfArtemisINIFiles(e.Data);
                                    Network.Current?.RaiseStatusUpdate("TextFileManagerControl InfoReceive - {0}...", e.RequestType.ToString());
                                    e.Handled = true;
                                    //Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfControLINIFiles, string.Empty, ArtemisManager.GetControlsINIFileList());
                                    break;
                                case RequestInformationType.ListOfDMXCommandfiles:
                                    ProcessListOfArtemisINIFiles(e.Data);
                                    Network.Current?.RaiseStatusUpdate("TextFileManagerControl InfoReceive - {0}...", e.RequestType.ToString());
                                    e.Handled = true;
                                    //Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfDMXCommandfiles, string.Empty, ArtemisManager.GetDMXCommandsFileList());
                                    break;
                                case RequestInformationType.SpecificControlINIFile:
                                case RequestInformationType.SpecificDMXCommandFile:
                                    if (e.Data.Length > 0)
                                    {
                                        ProcessSpecificArtemisINIFile(e.Identifier, e.Data[0]);
                                    }
                                    Network.Current?.RaiseStatusUpdate("TextFileManagerControl InfoReceive - {0}...", e.RequestType.ToString());
                                    e.Handled = true;
                                    break;
                            }
                        }
                    }
                });
            }
            else
            {
                Network.Current?.RaiseStatusUpdate("TextFileManagerControl InfoReceive processing---already handled, nothing done.");
            }
        }
        private void ProcessListOfArtemisINIFiles(string[] names)
        {
            DataFileList.Clear();
            foreach (var nm in names)
            {

                string ext = string.Empty;
                string folder = string.Empty;
                switch (FileType)
                {
                    case SendableStringPackageFile.DMXCommandsXML:
                        folder = ArtemisManager.DMXCommandsFolder;
                        ext = ArtemisManager.XMLFileExtension;
                        break;
                    case SendableStringPackageFile.controlsINI:
                        folder = ArtemisManager.ControlsINIFolder;
                        ext = ArtemisManager.INIFileExtension;
                        break;
                }
                DataFileList.Add(new TextDataFile(FileType, nm, ArtemisManager.ResolveFilename(folder, nm, ext)));
            }
        }
        private void ProcessSpecificArtemisINIFile(string filename, string data)
        {
            foreach (var item in DataFileList)
            {
                if (item.Name == filename)
                {
                    item.Data = data;
                }
            }
        }
        private void Fsw_Renamed(object sender, RenamedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                foreach (var f in DataFileList)
                {
                    if (f.SaveFile == e.OldFullPath)
                    {
                        f.SaveFile = e.FullPath;
                        if (e.Name != null)
                        {
                            int i = e.Name.IndexOf('.');
                            if (i < 0)
                            {
                                i = e.Name.Length;
                            }
                            f.Name = e.Name.Substring(0, i);
                        }
                        break;
                    }
                }
                switch (FileType)
                {
                    case SendableStringPackageFile.controlsINI:
                        Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfControLINIFiles, string.Empty, ArtemisManager.GetControlsINIFileList());
                        break;
                    case SendableStringPackageFile.DMXCommandsXML:
                        Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfDMXCommandfiles, string.Empty, ArtemisManager.GetDMXCommandsFileList());
                        break;
                }
            });
        }

        private void Fsw_Deleted(object sender, FileSystemEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                TextDataFile? remover = null;
                foreach (var f in DataFileList)
                {
                    if (f.SaveFile == e.FullPath)
                    {
                        remover = f;
                        break;
                    }
                }
                if (remover != null)
                {
                    DataFileList.Remove(remover);
                }

                switch (FileType)
                {
                    case SendableStringPackageFile.controlsINI:
                        Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfControLINIFiles, string.Empty, ArtemisManager.GetControlsINIFileList());
                        break;
                    case SendableStringPackageFile.DMXCommandsXML:
                        Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfDMXCommandfiles, string.Empty, ArtemisManager.GetDMXCommandsFileList());
                        break;
                }
            });
        }
        private string GetNewFilename(string baseName)
        {
            int i = 0;
            bool matched = false;
            string workName = baseName;
            do
            {
                matched = false;
                foreach (var name in DataFileList)
                {
                    if (name.Name == baseName)
                    {
                        matched = true;
                        break;
                    }
                }
                if (matched)
                {
                    workName = baseName + "(" + (++i).ToString() + ")";
                    matched = false;
                }

            } while (matched);
            return workName;
        }


        private void OnAddSettingsFile(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (IsRemote)
                {
                    if (TargetClient != null)
                    {
                        switch (FileType)
                        {
                            case SendableStringPackageFile.controlsINI:
                                Network.Current?.SendStringPackageFile(TargetClient, Properties.Resources.controls, FileType, GetNewFilename(FileType.ToString()) + ArtemisManager.INIFileExtension);
                                break;
                            case SendableStringPackageFile.DMXCommandsXML:
                                Network.Current?.SendStringPackageFile(TargetClient, Properties.Resources.DMXcommands, FileType, GetNewFilename(FileType.ToString()) + ArtemisManager.XMLFileExtension);
                                break;
                        }

                    }
                }
                else
                {
                    string targetFile = string.Empty;
                    string source = string.Empty;
                    System.IO.FileInfo? fle = null;
                    switch (FileType)
                    {
                        case SendableStringPackageFile.controlsINI:
                            fle = GetTarget(System.IO.Path.Combine(GetTargetFolder(), ArtemisManager.controlsINI));
                            source = System.IO.Path.Combine(ArtemisManagerAction.ModItem.ActivatedFolder, ArtemisManagerAction.ArtemisManager.controlsINI);

                            break;
                        case SendableStringPackageFile.DMXCommandsXML:
                            fle = GetTarget(System.IO.Path.Combine(GetTargetFolder(), ArtemisManager.DMXCommands));
                            source = System.IO.Path.Combine(ArtemisManagerAction.ModItem.ActivatedFolder, ArtemisManagerAction.ArtemisManager.ArtemisDATSubfolder, ArtemisManagerAction.ArtemisManager.DMXCommands);
                            break;
                    }
                    if (fle != null)
                    {
                        if (!string.IsNullOrEmpty(source) && System.IO.File.Exists(source))
                        {
                            System.IO.File.Copy(source, fle.FullName, true);
                        }
                        else
                        {

                            using (StreamWriter sw = new(fle.FullName))
                            {
                                switch (FileType)
                                {
                                    case SendableStringPackageFile.controlsINI:
                                        sw.WriteLine(Properties.Resources.controls);
                                        break;
                                    case SendableStringPackageFile.DMXCommandsXML:
                                        sw.WriteLine(Properties.Resources.DMXcommands);
                                        break;
                                    default:
                                        sw.WriteLine(Properties.Resources.DMXcommands);
                                        break;
                                }
                            }
                        }
                        SelectedDataFile = new(FileType, GetNameOfFile(fle.Name), fle.FullName);
                    }
                }
            });
        }
        string GetNameOfFile(string namewithExtension)
        {
            string nm = namewithExtension;
            if (nm.Contains("."))
            {
                nm = nm.Substring(0, nm.IndexOf('.'));
            }
            return nm;
        }
        private void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (!string.IsNullOrEmpty(e.Name))
                {
                    TextDataFile txt = new TextDataFile(FileType, GetNameOfFile(e.Name), e.FullPath);
                    DataFileList.Add(txt);
                }

                switch (FileType)
                {
                    case SendableStringPackageFile.controlsINI:
                        Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfControLINIFiles, string.Empty, ArtemisManager.GetControlsINIFileList());
                        break;
                    case SendableStringPackageFile.DMXCommandsXML:
                        Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfDMXCommandfiles, string.Empty, ArtemisManager.GetDMXCommandsFileList());
                        break;
                }
            });
        }

        private void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                foreach (var f in DataFileList)
                {
                    if (f.SaveFile == e.FullPath)
                    {
                        using (StreamReader sr = new StreamReader(e.FullPath))
                        {
                            f.Data = sr.ReadToEnd();
                        }
                        break;
                    }
                }
            });

        }

        public static readonly DependencyProperty FileTypeProperty =
        DependencyProperty.Register(nameof(FileType), typeof(SendableStringPackageFile),
            typeof(TextFileManagerControl), new PropertyMetadata(OnFileTypeChanged));

        private static void OnFileTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextFileManagerControl me)
            {
                me.InitializeFileList();
            }
        }

        public SendableStringPackageFile FileType
        {
            get
            {
                return (SendableStringPackageFile)this.GetValue(FileTypeProperty);

            }
            set
            {
                this.SetValue(FileTypeProperty, value);

            }
        }


        public static readonly DependencyProperty PopupMessageProperty =
         DependencyProperty.Register(nameof(PopupMessage), typeof(string),
             typeof(TextFileManagerControl));

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
        public static readonly DependencyProperty IsRemoteProperty =
            DependencyProperty.Register(nameof(IsRemote), typeof(bool),
            typeof(TextFileManagerControl), new PropertyMetadata(OnIsRemoteChanged));

        private static void OnIsRemoteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextFileManagerControl control)
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
           typeof(TextFileManagerControl));


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

        public static readonly DependencyProperty DataFileListProperty =
            DependencyProperty.Register(nameof(DataFileList), typeof(ObservableCollection<TextDataFile>),
                typeof(TextFileManagerControl));

        public ObservableCollection<TextDataFile> DataFileList
        {
            get
            {
                return (ObservableCollection<TextDataFile>)this.GetValue(DataFileListProperty);

            }
            set
            {
                this.SetValue(DataFileListProperty, value);

            }
        }
        public static readonly DependencyProperty SelectedDataFileProperty =
            DependencyProperty.Register(nameof(SelectedDataFile), typeof(TextDataFile),
             typeof(TextFileManagerControl), new PropertyMetadata(OnSelectedSettingsFileChanged));

        private static void OnSelectedSettingsFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextFileManagerControl me && me.SelectedDataFile != null)
            {
                if (string.IsNullOrEmpty(me.SelectedDataFile.Data))
                {
                    if (me.IsRemote)
                    {
                        if (me.TargetClient != null)
                        {
                            RequestInformationType request;
                            switch (me.FileType)
                            {
                                case SendableStringPackageFile.controlsINI:
                                    request = RequestInformationType.SpecificControlINIFile;
                                    break;
                                case SendableStringPackageFile.DMXCommandsXML:
                                    request = RequestInformationType.SpecificDMXCommandFile;
                                    break;
                                default:
                                    request = RequestInformationType.None;
                                    break;
                            }
                            Network.Current?.SendRequestInformation(me.TargetClient, request, me.SelectedDataFile.Name);
                        }
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(me.SelectedDataFile.SaveFile))
                        {
                            me.SelectedDataFile.Data = sr.ReadToEnd();
                        }
                    }
                }

                //me.SelectedArtemisSettingsFile.INIFile = ArtemisManager.GetArtemisINI(me.SelectedArtemisSettingsFile.Name);
            }
        }

        public TextDataFile? SelectedDataFile
        {
            get
            {
                return (TextDataFile?)this.GetValue(SelectedDataFileProperty);

            }
            set
            {
                this.SetValue(SelectedDataFileProperty, value);

            }
        }
        private System.IO.FileInfo GetTarget(string file)
        {
            int i = 0;
            System.IO.FileInfo fle = new(file);
            string baseName = fle.Name.Substring(0, fle.Name.Length - fle.Extension.Length);

            while (fle.Exists && !string.IsNullOrEmpty(fle.DirectoryName))
            {
                i++;
                fle = new(System.IO.Path.Combine(fle.DirectoryName, baseName + "(" + i.ToString() + ")" + fle.Extension));
            }
            return fle;
        }


        private void OnRestoreToDefault(object sender, RoutedEventArgs e)
        {
            if (IsRemote)
            {
                if (TargetClient != null)
                {
                    switch (FileType)
                    {
                        case SendableStringPackageFile.controlsINI:
                            Network.Current?.SendArtemisAction(TargetClient, ArtemisActions.RestoreControlINIToDefault, Guid.Empty, Properties.Resources.controls);
                            break;
                        case SendableStringPackageFile.DMXCommandsXML:
                            Network.Current?.SendArtemisAction(TargetClient, ArtemisActions.RestoreDMXCommandsToDefault, Guid.Empty, Properties.Resources.DMXcommands);
                            break;
                    }
                }
            }
            else
            {
                string data = string.Empty;
                switch (FileType)
                {
                    case SendableStringPackageFile.controlsINI:
                        data = Properties.Resources.controls;
                        break;
                    case SendableStringPackageFile.DMXCommandsXML:
                        data = Properties.Resources.DMXcommands;
                        break;
                }
                ArtemisManager.RestoreDefaultOtherSettingsFile(FileType, data);

            }
        }
        private string GetExtension()
        {
            switch (FileType)
            {
                case SendableStringPackageFile.controlsINI:
                    return "ini";

                case SendableStringPackageFile.DMXCommandsXML:
                    return "xml";
                default:
                    return "xml";
            }
        }
        private string GetTargetFolder()
        {
            switch (FileType)
            {
                case SendableStringPackageFile.controlsINI:
                    return ArtemisManager.ControlsINIFolder;
                case SendableStringPackageFile.DMXCommandsXML:
                    return ArtemisManager.DMXCommandsFolder;
                default:
                    return ArtemisManager.DMXCommandsFolder;
            }
        }
        private string GetFilename()
        {
            switch (FileType)
            {
                case SendableStringPackageFile.controlsINI:
                    return ArtemisManager.controlsINI;
                case SendableStringPackageFile.DMXCommandsXML:
                    return ArtemisManager.DMXCommands;
                default:
                    return ArtemisManager.DMXCommands;
            }
        }
        private string GetActivatedFilename()
        {
            switch (FileType)
            {
                case SendableStringPackageFile.controlsINI:
                    return System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.controlsINI);
                case SendableStringPackageFile.DMXCommandsXML:
                    return System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisDATSubfolder, ArtemisManager.DMXCommands);
                default:
                    return System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisDATSubfolder, ArtemisManager.DMXCommands);
            }
        }
        private void OnImportFile(object sender, RoutedEventArgs e)
        {
            //For local only
            string ext = GetExtension();
            string targetFolder = GetTargetFolder();
            Microsoft.Win32.OpenFileDialog ofd = new()
            {
                Title = "Select " + GetFilename() + " file to import",
                DefaultExt = ext,
                Filter = ext.ToUpperInvariant() + " Files (*." + ext + ")|*." + ext,
                Multiselect = false,
                AddExtension = true,
                CheckFileExists = true
            };
            if (ofd.ShowDialog() == true)
            {

                FileInfo source = new(ofd.FileName);

                string target = System.IO.Path.Combine(targetFolder, source.Name);
                bool OkayToSave = true;
                if (File.Exists(target))
                {
                    OkayToSave = (System.Windows.MessageBox.Show("Do you want to overwrite " + source.Name + "?",
                              "Import File", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
                }
                if (OkayToSave)
                {
                    source.CopyTo(target, true);
                }
            }

        }


        private void OnActivateSettingsFile(object sender, RoutedEventArgs e)
        {
            if (SelectedDataFile != null)
            {
                if (IsRemote)
                {
                    if (TargetClient != null)
                    {
                        switch (FileType)
                        {
                            case SendableStringPackageFile.controlsINI:
                                Network.Current?.SendArtemisAction(TargetClient, ArtemisActions.ActivateControlsINI, Guid.Empty, new FileInfo(SelectedDataFile.SaveFile).Name);
                                break;
                            case SendableStringPackageFile.DMXCommandsXML:
                                Network.Current?.SendArtemisAction(TargetClient, ArtemisActions.ActivateDMXCommands, Guid.Empty, new FileInfo(SelectedDataFile.SaveFile).Name);
                                break;
                        }
                    }
                }
                else
                {
                    System.IO.File.Copy(SelectedDataFile.SaveFile, GetActivatedFilename());
                }
            }
        }

        private void OnSettingsFileRename(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is TextDataFile selectedFile)
                {
                    selectedFile.IsEditMode = true;
                }
            }
        }

        private void OnDeleteSettingsFile(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is TextDataFile selectedFile)
                {
                    if (!string.IsNullOrEmpty(selectedFile.Name))
                    {
                        if (IsRemote)
                        {
                            if (TargetClient != null)
                            {
                                switch (FileType)
                                {
                                    case SendableStringPackageFile.controlsINI:
                                        Network.Current?.SendArtemisAction(TargetClient, ArtemisActions.DeleteControlsINI, Guid.Empty, selectedFile.Name);
                                        break;
                                    case SendableStringPackageFile.DMXCommandsXML:
                                        Network.Current?.SendArtemisAction(TargetClient, ArtemisActions.DeleteDMXCommands, Guid.Empty, selectedFile.Name);
                                        break;
                                }
                            }
                        }
                        else
                        {

                            if (File.Exists(selectedFile.SaveFile))
                            {
                                File.Delete(selectedFile.SaveFile);
                                /*
                                if (DataFileList.Contains(selectedFile))
                                {
                                    DataFileList.Remove(selectedFile);
                                }
                                if (DataFileList != null && SelectedDataFile?.SaveFile == System.IO.Path.Combine(GetTargetFolder(), GetFilename()))
                                {
                                    SelectedDataFile = null;
                                }
                                if (SelectedDataFile == selectedFile)
                                {
                                    SelectedDataFile = null;
                                }
                                */
                            }
                        }
                    }
                }
            }
        }

        private void OnExportSettingsFile(object sender, RoutedEventArgs e)
        {
            //Cannot export a remote file--must first make it local.
            if (sender is MenuItem m)
            {
                if (m.CommandParameter is TextDataFile presetsFile)
                {
                    Microsoft.Win32.SaveFileDialog dialg = new()
                    {
                        CheckPathExists = true,
                        Filter = GetExtension() + " settings files (*." + GetExtension() + ")|*." + GetExtension(),
                        DefaultExt = GetExtension(),
                        InitialDirectory = GetTargetFolder(),
                        Title = "Select new settings filename"
                    };
                    if (dialg.ShowDialog() == true)
                    {
                        bool OkayToSave = true;
                        if (File.Exists(dialg.FileName))
                        {
                            OkayToSave = (System.Windows.MessageBox.Show("Do you want to overwrite " + new FileInfo(dialg.FileName).Name + "?",
                                "Settings", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
                        }
                        if (OkayToSave)
                        {
                            File.Copy(presetsFile.SaveFile, dialg.FileName, true);
                        }
                    }
                }
            }
        }

        private void OnSettingsFilenameChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlockEditControl me)
            {
                if (me.Tag is TextDataFile selectedFile)
                {
                    bool isOkay = true;
                    foreach (var file in this.DataFileList)
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
                                switch (FileType)
                                {
                                    case SendableStringPackageFile.controlsINI:
                                        Network.Current?.SendArtemisAction(
                                            TargetClient,
                                            ArtemisActions.RenameControlsINI,
                                            Guid.Empty,
                                            selectedFile.OriginalName + ":" + selectedFile.Name);
                                        break;
                                    case SendableStringPackageFile.DMXCommandsXML:
                                        Network.Current?.SendArtemisAction(
                                            TargetClient,
                                            ArtemisActions.RenameDMXCommands,
                                            Guid.Empty,
                                            selectedFile.OriginalName + ":" + selectedFile.Name);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            ArtemisManager.RenameOtherSettingsFile(FileType, selectedFile.OriginalName, selectedFile.Name);
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

        private void OnTransmissionCompleted(object sender, RoutedEventArgs e)
        {
            PopupMessage = "Saved";
        }

        private void OnSaveLocal(object sender, RoutedEventArgs e)
        {
            if (SelectedDataFile != null)
            {
                using (StreamWriter sw = new(SelectedDataFile.SaveFile))
                {
                    sw.Write(SelectedDataFile.Data);
                }

                RequestInformationType tp = RequestInformationType.None;
                switch (SelectedDataFile.FileType)
                {
                    case AMCommunicator.Messages.SendableStringPackageFile.DMXCommandsXML:
                        tp = RequestInformationType.SpecificDMXCommandFile;
                        break;
                    case AMCommunicator.Messages.SendableStringPackageFile.controlsINI:
                        tp = RequestInformationType.SpecificControlINIFile;
                        break;
                }
                Network.Current?.SendInformation(IPAddress.Any, tp, SelectedDataFile.Name, new string[] { SelectedDataFile.Data });
                PopupMessage = "Saved";
            }
        }
    }
}
