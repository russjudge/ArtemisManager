using AMCommunicator;
using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WPFFolderBrowser;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        public static bool ProbablyUnattended { get; private set; }
        public Main()
        {
            try
            {
                SettingsAction.Touch();
                ArtemisUpgradeLinks = new();
                ExternalToolsLinks = new();


                Status = new ObservableCollection<string>();

                if (TakeAction.ConnectedPCs != null)
                {
                    ConnectedPCs = TakeAction.ConnectedPCs;
                }


                this.InWindowsStartupFolder = TakeAction.IsThisAppInStartup();

                IsArtemisRunning = ArtemisManager.IsArtemisRunning();
                IsUsingThisAppControlledArtemis = ArtemisManager.IsRunningArtemisUnderMyControl();
                InstalledMods = new(ArtemisManager.GetInstalledMods());
                InstalledMissions = new(ArtemisManager.GetInstalledMissions());
                AppVersion = TakeAction.GetAppVersion();

                IsMaster = SettingsAction.Current.IsMaster;

                var drives = DriveInfo.GetDrives();

                foreach (var drive in drives)
                {
                    if (drive.Name.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)[..1]))
                    {
                        FreeSpaceOnAppDrive = drive.AvailableFreeSpace;
                    }
                }
                ConnectOnStart = SettingsAction.Current.ConnectOnStart;
            }
            catch (Exception ex)
            {
                UpdateStatus("Error starting up: " + ex.Message);
            }


            InitializeComponent();

            Task.Run(async () =>
            {
                var links = await ArtemisManager.GetArtemisUpgradeLinks();
                Dispatcher.Invoke(() =>
                {
                    foreach (var link in links)
                    {
                        ArtemisUpgradeLinks.Add(link);
                    }
                });
            });
            Task.Run(async () =>
            {
                var links = await ArtemisManager.GetExternaToolsLinks();
                Dispatcher.Invoke(() =>
                {
                    foreach (var link in links)
                    {
                        MenuItem item = new()
                        {
                            Header = link.Item1
                        };
                        MenuItem sub = new()
                        {
                            Header = "Download",
                            CommandParameter = link,

                            Icon = new System.Windows.Controls.Image
                            {
                                Source = new BitmapImage(new Uri("/ArtemisManagerUI;component/Resources/download (1).png", UriKind.Relative))
                            }
                        };
                        sub.Click += OnDownload;

                        item.Items.Add(sub);
                        sub = new()
                        {
                            Header = "Visit Owner website",
                            CommandParameter = link.Item3,
                            Icon = new System.Windows.Controls.Image
                            {
                                Source = new BitmapImage(new Uri("/ArtemisManagerUI;component/Resources/www.png", UriKind.Relative))
                            }
                        };
                        sub.Click += OnGotoOwnerWebsite;

                        item.Items.Add(sub);
                        ExternalToolsLinks.Add(item);
                    }
                });
            });
            timer = new DispatcherTimer(DispatcherPriority.Background, this.Dispatcher);
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Tick += Timer_Tick;

            timer.Start();

        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            CheckArtemisSBSStatus();
        }

        DispatcherTimer timer;
        void CheckArtemisSBSStatus()
        {
            IsArtemisRunning = ArtemisManager.IsArtemisRunning();
            IsUsingThisAppControlledArtemis = ArtemisManager.IsRunningArtemisUnderMyControl();
        }
        private void OnGotoOwnerWebsite(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me && me.CommandParameter is string url)
            {
                ProcessStartInfo processStartInfo = new(url)
                {
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }
        }
        private void OnDownload(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is Tuple<string, string, string> item)
                {
                    WPFFolderBrowserDialog dialog = new();

                    string targetFolder = string.Empty;
                    dialog.Title = "Select folder to save download to";
                    if (dialog.ShowDialog() == true)
                    {
                        targetFolder = dialog.FileName;

                        Task.Run(async () =>
                        {
                            using HttpClient client = new();
                            try
                            {

                                using var stream = await client.GetStreamAsync(ArtemisManager.ExternalToolsURLFolder + item.Item2);
                                if (stream != null)
                                {
                                    byte[] buffer = new byte[32768];
                                    int bytesRead = 0;
                                    if (stream != null)
                                    {
                                        string target = System.IO.Path.Combine(targetFolder, item.Item2);
                                        using (FileStream fs = new(target, FileMode.Create))
                                        {
                                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                                            {
                                                fs.Write(buffer, 0, bytesRead);
                                            }
                                        }
                                        Dispatcher.Invoke(() =>
                                        {
                                            if (System.Windows.MessageBox.Show(
                                                string.Format("{0} downloaded successfully.{1}{1}Do you want to run the install?",
                                                item.Item1, Environment.NewLine),
                                                item.Item1 + " download", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                            {
                                                ProcessStartInfo startInfo = new(target)
                                                {
                                                    UseShellExecute = true
                                                };
                                                Process.Start(startInfo);
                                            }
                                        });
                                    }
                                    else
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            System.Windows.MessageBox.Show("Error downloading " + item.Item1,
                                            item.Item1 + " download", MessageBoxButton.OK, MessageBoxImage.Error);
                                        });

                                    }
                                }
                                else
                                {
                                    Dispatcher.Invoke(() =>
                                    {

                                        System.Windows.MessageBox.Show("Error downloading " + item.Item1,
                                            item.Item1 + " download", MessageBoxButton.OK, MessageBoxImage.Error);

                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Windows.MessageBox.Show(string.Format("Error downloading {0}:{2}{2}{1}", item.Item1, ex.Message, Environment.NewLine),
                                                   item.Item1 + " download", MessageBoxButton.OK, MessageBoxImage.Error);
                            }

                        });
                    }

                }
            }
        }

        bool isLoading = true;
        readonly Network MyNetwork = Network.GetNetwork();
        FileSystemWatcher? watcher = null;
        /*
         * 
         *
         *Settings to set up:
         *ModInstallFolder
         *ServerIPOrName
         *ConnectOnStart
         *IsMaster
         *NetworkPassword
         *ArtemisInstallFolder (after finding actual install folder, make copy to here).
         *ArtemisServer
         *ArtemisPort
        */
        private void OnLoaded(object sender, RoutedEventArgs? e)
        {
            if (isLoading)
            {
                if (TakeAction.MustExit)
                {
                    this.Close();
                }
                else
                {
                    TakeAction.MainWindow = this;

                    ArtemisInstallFolder = Properties.Settings.Default.ArtemisInstallFolder;
                    if (string.IsNullOrEmpty(ArtemisInstallFolder) || !System.IO.File.Exists(System.IO.Path.Combine(ArtemisInstallFolder, ArtemisManager.ArtemisEXE)))
                    {
                        ArtemisInstallFolder = ArtemisManager.AutoDetectArtemisInstallPath();
                        Properties.Settings.Default.ArtemisInstallFolder = ArtemisInstallFolder;
                        Properties.Settings.Default.Save();
                    }

                    if (!string.IsNullOrEmpty(ArtemisInstallFolder))
                    {
                        if (ArtemisManager.CheckIfArtemisSnapshotNeeded(ArtemisInstallFolder))
                        {
                            ArtemisChanged = true;
                            string? version = ArtemisManager.GetArtemisVersion(ArtemisInstallFolder);
                            if (version != null && System.Windows.MessageBox.Show(
                                string.Format("New Version of Artemis SBS detected (Version {0}).{1}{1}Do you wish to make a new snapshot?", version, Environment.NewLine),
                                "New Artemis Version", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                SnapshotArtemis();
                            }
                        }
                    }


                    isLoading = false;
                    if (ConnectOnStart)
                    {
                        DoStartServer();
                    }

                    foreach (var arg in Environment.GetCommandLineArgs())
                    {
                        if (arg.Equals("FROMSTARTUPFOLDER", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ProbablyUnattended = true;
                            break;
                        }
                    }
                    if (ProbablyUnattended)
                    {
                        this.WindowState = WindowState.Minimized;
                    }
                    ArtemisManager.VerifyArtemisINIBackup();
                }
            }
        }



        public static readonly DependencyProperty IsUsingThisAppControlledArtemisProperty =
            DependencyProperty.Register(nameof(IsUsingThisAppControlledArtemis), typeof(bool),
           typeof(Main));

        public bool IsUsingThisAppControlledArtemis
        {
            get
            {
                return (bool)this.GetValue(IsUsingThisAppControlledArtemisProperty);

            }
            set
            {
                this.SetValue(IsUsingThisAppControlledArtemisProperty, value);

            }
        }

        public static readonly DependencyProperty FreeSpaceOnAppDriveProperty =
            DependencyProperty.Register(nameof(FreeSpaceOnAppDrive), typeof(long),
           typeof(Main));

        public long FreeSpaceOnAppDrive
        {
            get
            {
                return (long)this.GetValue(FreeSpaceOnAppDriveProperty);

            }
            set
            {
                this.SetValue(FreeSpaceOnAppDriveProperty, value);

            }
        }

        public static readonly DependencyProperty ChatAlertProperty =
        DependencyProperty.Register(nameof(ChatAlert), typeof(bool),
            typeof(Main));

        public bool ChatAlert
        {
            get
            {
                return (bool)this.GetValue(ChatAlertProperty);

            }
            set
            {
                this.SetValue(ChatAlertProperty, value);

            }
        }

        public static readonly DependencyProperty PopupMessageProperty =
         DependencyProperty.Register(nameof(PopupMessage), typeof(string),
             typeof(Main));

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

        public static readonly DependencyProperty AppVersionProperty =
          DependencyProperty.Register(nameof(AppVersion), typeof(string),
              typeof(Main));

        public string AppVersion
        {
            get
            {
                return (string)this.GetValue(AppVersionProperty);

            }
            set
            {
                this.SetValue(AppVersionProperty, value);

            }
        }
        public static readonly DependencyProperty IsStartedProperty =
          DependencyProperty.Register(nameof(IsStarted), typeof(bool),
              typeof(Main));

        /// <summary>
        /// Identifies whether or not this app has completed loading and is running.
        /// </summary>
        public bool IsStarted
        {
            get
            {
                return (bool)this.GetValue(IsStartedProperty);

            }
            set
            {
                this.SetValue(IsStartedProperty, value);

            }
        }
        public static readonly DependencyProperty IsWindowsProperty =
         DependencyProperty.Register(nameof(IsWindows), typeof(bool),
             typeof(Main), new PropertyMetadata(OperatingSystem.IsWindows()));

        public bool IsWindows
        {
            get
            {
                return (bool)this.GetValue(IsWindowsProperty);

            }
            set
            {
                this.SetValue(IsWindowsProperty, value);

            }
        }

        public static readonly DependencyProperty ConnectOnStartProperty =
         DependencyProperty.Register(nameof(ConnectOnStart), typeof(bool),
             typeof(Main), new PropertyMetadata(OnAutoStartChanged));
        static void UpdateAutoStart(bool value)
        {
            SettingsAction.Current.ConnectOnStart = value;
            SettingsAction.Current.Save();
        }
        private static void OnAutoStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Main me = (Main)d;
            if (me != null)
            {
                if (!me.isLoading)
                {

                    if (!TakeAction.DoChangeSetting(nameof(ConnectOnStart), me.ConnectOnStart.ToString(), true))
                    {
                        me.ConnectOnStart = SettingsAction.Current.ConnectOnStart;
                    }
                    else
                    {
                        UpdateAutoStart(me.ConnectOnStart);
                    }
                }
            }
        }

        public bool ConnectOnStart
        {
            get
            {
                return (bool)this.GetValue(ConnectOnStartProperty);

            }
            set
            {
                this.SetValue(ConnectOnStartProperty, value);

            }
        }

        public static readonly DependencyProperty HostnameProperty =
           DependencyProperty.Register(nameof(Hostname), typeof(string),
               typeof(Main), new PropertyMetadata(Dns.GetHostName()));

        public string Hostname
        {
            get
            {
                return (string)this.GetValue(HostnameProperty);

            }
            set
            {
                this.SetValue(HostnameProperty, value);

            }
        }


        public static readonly DependencyProperty IPProperty =
           DependencyProperty.Register(nameof(IP), typeof(string),
               typeof(Main), new PropertyMetadata(Network.GetMyIP()?.ToString()));

        public string IP
        {
            get
            {
                return (string)this.GetValue(IPProperty);

            }
            set
            {
                this.SetValue(IPProperty, value);

            }
        }

        public static readonly DependencyProperty InstalledMissionsProperty =
           DependencyProperty.Register(nameof(InstalledMissions), typeof(ObservableCollection<ModItem>),
               typeof(Main));

        public ObservableCollection<ModItem> InstalledMissions
        {
            get
            {
                return (ObservableCollection<ModItem>)this.GetValue(InstalledMissionsProperty);

            }
            set
            {
                this.SetValue(InstalledMissionsProperty, value);

            }
        }

        public static readonly DependencyProperty InstalledModsProperty =
           DependencyProperty.Register(nameof(InstalledMods), typeof(ObservableCollection<ModItem>),
               typeof(Main));

        public ObservableCollection<ModItem> InstalledMods
        {
            get
            {
                return (ObservableCollection<ModItem>)this.GetValue(InstalledModsProperty);

            }
            set
            {
                this.SetValue(InstalledModsProperty, value);

            }
        }
        public static readonly DependencyProperty ConnectedPCsProperty =
           DependencyProperty.Register(nameof(ConnectedPCs), typeof(ObservableCollection<PCItem>),
               typeof(Main));

        public ObservableCollection<PCItem> ConnectedPCs
        {
            get
            {
                return (ObservableCollection<PCItem>)this.GetValue(ConnectedPCsProperty);

            }
            set
            {
                this.SetValue(ConnectedPCsProperty, value);

            }
        }

        public static readonly DependencyProperty StatusProperty =
           DependencyProperty.Register(nameof(Status), typeof(ObservableCollection<string>),
               typeof(Main));

        public ObservableCollection<string> Status
        {
            get
            {
                return (ObservableCollection<string>)this.GetValue(StatusProperty);

            }
            set
            {
                this.SetValue(StatusProperty, value);

            }
        }
        public static readonly DependencyProperty LastStatusProperty =
           DependencyProperty.Register(nameof(LastStatus), typeof(string),
               typeof(Main));

        public string LastStatus
        {
            get
            {
                return (string)this.GetValue(LastStatusProperty);

            }
            set
            {
                this.SetValue(LastStatusProperty, value);

            }
        }
        public static readonly DependencyProperty InWindowsStartupFolderProperty =
          DependencyProperty.Register(nameof(InWindowsStartupFolder), typeof(bool),
              typeof(Main));

        public bool InWindowsStartupFolder
        {
            get
            {
                return (bool)this.GetValue(InWindowsStartupFolderProperty);

            }
            set
            {
                this.SetValue(InWindowsStartupFolderProperty, value);

            }
        }

        public static readonly DependencyProperty SelectedTabItemProperty =
         DependencyProperty.Register(nameof(SelectedTabItem), typeof(TabItem),
             typeof(Main), new PropertyMetadata(OnSelectedTabChanged));

        private static void OnSelectedTabChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Main me)
            {
                if (me.SelectedTabItem != null)
                {
                    if (me.SelectedTabItem.Tag?.ToString() == "Chat")
                    {
                        me.ChatAlert = false;
                    }
                }
            }
        }

        public TabItem SelectedTabItem
        {
            get
            {
                return (TabItem)this.GetValue(SelectedTabItemProperty);

            }
            set
            {
                this.SetValue(SelectedTabItemProperty, value);
            }
        }



        private void UpdateStatus(string message)
        {
            try
            {
                if (Thread.CurrentThread != this.Dispatcher.Thread)
                {
                    this.Dispatcher.Invoke(new Action<string>(UpdateStatus), message);
                }
                else
                {
                    LastStatus = message;
                    Status.Add(DateTime.Now.ToString("HH:mm:ss") + ": " + message);
                }
            }
            catch (ThreadInterruptedException)
            {

            }
        }

        public static readonly DependencyProperty ArtemisInstallFolderProperty =
          DependencyProperty.Register(nameof(ArtemisInstallFolder), typeof(string),
              typeof(Main), new PropertyMetadata(OnArtemisInstallFolderChanged));

        private static void OnArtemisInstallFolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Main me)
            {
                if (!string.IsNullOrEmpty(me.ArtemisInstallFolder))
                {
                    me.watcher = new FileSystemWatcher(me.ArtemisInstallFolder);
                    me.watcher.Created += me.Watcher_Changed;
                    me.watcher.Changed += me.Watcher_Changed;
                    me.watcher.EnableRaisingEvents = true;
                }
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(ArtemisInstallFolder))
                {
                    ArtemisChanged = ArtemisManager.CheckIfArtemisSnapshotNeeded(ArtemisInstallFolder);
                }
            });

        }


        public string? ArtemisInstallFolder
        {
            get
            {
                return (string?)this.GetValue(ArtemisInstallFolderProperty);

            }
            set
            {
                this.SetValue(ArtemisInstallFolderProperty, value);
            }
        }
        public static readonly DependencyProperty IsArtemisRunningProperty =
            DependencyProperty.Register(nameof(IsArtemisRunning), typeof(bool),
            typeof(Main));

        public bool IsArtemisRunning
        {
            get
            {
                return (bool)this.GetValue(IsArtemisRunningProperty);

            }
            set
            {
                this.SetValue(IsArtemisRunningProperty, value);
            }
        }
        void SubscribeToEvents()
        {
            TakeAction.StatusUpdate += MyNetwork_StatusUpdated;

            MyNetwork.ModPackageRequested += MyNetwork_ModPackageRequested;
            MyNetwork.ConnectionRequested += MyNetwork_ConnectionRequested;
            MyNetwork.ConnectionReceived += MyNetwork_ConnectionReceived;
            MyNetwork.StatusUpdated += MyNetwork_StatusUpdated;
            MyNetwork.FatalExceptionEncountered += MyNetwork_FatalExceptionEncountered;
            MyNetwork.ConnectionClosed += MyNetwork_ConnectionClosed;
            MyNetwork.ActionCommand += MyNetwork_ActionCommand;
            MyNetwork.PasswordChanged += MyNetwork_PasswordChanged;
            MyNetwork.ChangeSetting += MyNetwork_ChangeSetting;
            MyNetwork.AlertReceived += MyNetwork_AlertReceived;
            MyNetwork.ClientInfoReceived += MyNetwork_ClientInfoReceived;
            MyNetwork.ArtemisActionReceived += MyNetwork_ArtemisActionReceived;
            MyNetwork.ModPackageReceived += MyNetwork_ModPackageReceived;
            MyNetwork.PopupMessageEvent += MyNetwork_PopupMessageEvent;
            MyNetwork.PackageFileReceived += MyNetwork_PackageFileReceived;
            MyNetwork.InfoRequestReceived += MyNetwork_InfoRequestReceived;
            TakeArtemisAction.CommunicationMessageReceived += TakeArtemisAction_CommunicationMessageReceived;
        }

        private void TakeArtemisAction_CommunicationMessageReceived(object? sender, CommunicationMessageEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PopupMessage = e.Message;
            });
        }

        void DoStartServer()
        {
            UpdateStatus("Starting Connection Service");
            SubscribeToEvents();

            MyNetwork.Connect();
            IsStarted = true;
        }

        private void MyNetwork_InfoRequestReceived(object? sender, InformationRequestEventArgs e)
        {
            //TODO: meet request requirements
            if (e.Source != null)
            {
                switch (e.RequestType)
                {
                    case RequestInformationType.ListOfArtemisINIFiles:
                        MyNetwork.SendInformation(e.Source, e.RequestType, e.Identifier, ArtemisManager.GetArtemisINIFileList());
                        break;
                    case RequestInformationType.SpecificArtemisINIFile:
                        string data = ArtemisManager.GetArtemisINIData(e.Identifier);
                        MyNetwork.SendInformation(e.Source, e.RequestType, e.Identifier, new string[] { data });
                        break;
                    case RequestInformationType.ListOfEngineeringPresets:
                        MyNetwork.SendInformation(e.Source, e.RequestType, e.Identifier, ArtemisManager.GetEngineeringPresetFiles());
                        break;
                    case RequestInformationType.SpecificEngineeringPreset:
                        //TODO: return specific engineerign preset
                        break;
                    case RequestInformationType.ListOfDMXCommandfiles:
                        MyNetwork.SendInformation(e.Source, e.RequestType, e.Identifier, ArtemisManager.GetDMXCommandsFileList());
                        break;
                    case RequestInformationType.SpecificDMXCommandFile:
                        //TODO: RETURN DMX file
                        break;
                    case RequestInformationType.ListOfControLINIFiles:
                        MyNetwork.SendInformation(e.Source, e.RequestType, e.Identifier, ArtemisManager.GetControlsINIFileList());
                        break;
                    case RequestInformationType.SpecificControlINIFile:
                        //TODO: RETURN controls.ini file.
                        break;
                    case RequestInformationType.ListOfScreenResolutions:
                        List<string> items = new();
                        foreach (var sz in TakeAction.GetAvailableScreenResolutions())
                        {
                            items.Add(sz.Width.ToString() + "x" + sz.Height.ToString());
                        }
                        MyNetwork.SendInformation(e.Source, e.RequestType, e.Identifier, items.ToArray());
                        break;
                }
            }
        }

        private void MyNetwork_PackageFileReceived(object? sender, PackageFileEventArgs e)
        {
            switch (e.FileType)
            {
                case AMCommunicator.Messages.SendableStringPackageFile.EngineeringPreset:
                    TakeAction.SaveEngineeringPreset(e.Filename, e.SerializedString);
                    break;
                case AMCommunicator.Messages.SendableStringPackageFile.ArtemisINI:
                    TakeAction.SaveArtemisINISettingsFile(e.Filename, e.SerializedString);
                    break;
            }
        }

        private void MyNetwork_PopupMessageEvent(object? sender, StatusUpdateEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.PopupMessage += Environment.NewLine + Environment.NewLine + e.Message;

            });
        }

        private void MyNetwork_ModPackageReceived(object? sender, ModPackageEventArgs e)
        {
            ModItem? item = ModItem.GetModItem(e.Mod);
            if (item != null)
            {
                //1. confirm not already installed.
                if (!ModManager.IsModInstalled(item))
                {
                    //2. Write the file to a package in Archive.
                    //3. Unzip the package to the mod Install folder.
                    item.StoreAndUnpack(e.Data);

                    //4. Save modItem and add to list.
                    item.Save();
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (item.IsMission)
                        {
                            InstalledMissions.Add(item);
                        }
                        else
                        {
                            InstalledMods.Add(item);
                        }
                    }));

                }

                //5. check TakeArtemisAction.StagedModItemToActivateOnceInstalled and if matches and not null, activate and set to null.
                if (TakeArtemisAction.StagedModItemToActivateOnceInstalled != null && TakeArtemisAction.StagedModItemToActivateOnceInstalled.Equals(item))
                {
                    item.Activate();
                    TakeArtemisAction.StagedModItemToActivateOnceInstalled = null;
                }
                TakeAction.SendClientInfo(IPAddress.Any);
            }
        }

        private void MyNetwork_ArtemisActionReceived(object? sender, ArtemisActionEventArgs e)
        {
            var wasProcessed = TakeArtemisAction.ProcessArtemisAction(e.Source, e.Action, e.Mod, e.SaveName);
            if (wasProcessed.Item1)
            {
                switch (e.Action)
                {
                    case AMCommunicator.Messages.ArtemisActions.ResetToVanilla:
                        this.Dispatcher.Invoke(new Action<Guid?>(DeactivateAllButBase), wasProcessed.Item2?.LocalIdentifier);
                        break;
                    case AMCommunicator.Messages.ArtemisActions.StopArtemis:
                        this.Dispatcher.Invoke(new Action(() => { this.IsArtemisRunning = false; }));
                        break;
                    case AMCommunicator.Messages.ArtemisActions.StartArtemis:
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.IsArtemisRunning = wasProcessed.Item1;
                            IsUsingThisAppControlledArtemis = ArtemisManager.IsRunningArtemisUnderMyControl();
                        }));

                        break;
                    case AMCommunicator.Messages.ArtemisActions.InstallMod:

                        if (wasProcessed.Item1)  //Will only be processed if we already have the package file in the archive folder.
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                if (wasProcessed.Item2 != null)
                                {
                                    if (wasProcessed.Item2.IsMission)
                                    {
                                        InstalledMissions.Add(wasProcessed.Item2);
                                    }
                                    else
                                    {
                                        InstalledMods.Add(wasProcessed.Item2);
                                    }
                                }
                            }));
                        }

                        break;
                    case AMCommunicator.Messages.ArtemisActions.UninstallMod:
                        if (wasProcessed.Item1)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                if (wasProcessed.Item2 != null)
                                {
                                    if (wasProcessed.Item2.IsMission)
                                    {
                                        this.InstalledMissions.Clear();
                                        foreach (var mod in ArtemisManager.GetInstalledMissions())
                                        {
                                            InstalledMissions.Add(mod);
                                        }
                                    }
                                    else
                                    {
                                        this.InstalledMods.Clear();
                                        foreach (var mod in ArtemisManager.GetInstalledMods())
                                        {
                                            InstalledMods.Add(mod);
                                        }
                                    }
                                }
                            }));
                        }

                        break;
                    case AMCommunicator.Messages.ArtemisActions.ActivateMod:

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            if (wasProcessed.Item2 != null)
                            {
                                if (wasProcessed.Item2.IsMission)
                                {
                                    foreach (var mod in InstalledMissions)
                                    {
                                        if (mod.LocalIdentifier == e.Identifier)
                                        {
                                            mod.IsActive = true;
                                        }
                                    }
                                }
                                else
                                {
                                    this.InstalledMods.Clear();

                                    foreach (var mod in ArtemisManager.GetInstalledMods())
                                    {
                                        InstalledMods.Add(mod);
                                    }
                                }
                            }
                        }));

                        break;
                }
            }
            UpdateStatus("Sending Updated Client info");
            TakeAction.SendClientInfo(IPAddress.Any);
        }
        
        void LoadClientInfoData(ClientInfoEventArgs e)
        {
            if (Thread.CurrentThread != this.Dispatcher.Thread)
            {
                this.Dispatcher.Invoke(new Action<ClientInfoEventArgs>(LoadClientInfoData), e);
            }
            else
            {
                bool isMasterFound = false;

                foreach (var item in ConnectedPCs)
                {
                    if (item.IP != null && !TakeAction.IsBroadcast(item.IP))
                    {
                        if (e.Source != null)
                        {
                            if (item.IsMaster)
                            {
                                isMasterFound = true;
                            }
                            if (item.IP.ToString().Equals(e.Source.ToString()))
                            {
                                item.LoadClientInfoData(e);
                                
                            }
                        }
                    }
                }
                if (!SettingsAction.Current.IsMaster)
                {
                    if (isMasterFound)
                    {
                        if (IsMaster)
                        {
                            isLoading = true;
                            IsMaster = false;
                            isLoading = false;
                        }
                    }
                    else
                    {
                        if (!IsMaster)
                        {
                            isLoading = true;
                            IsMaster = true;
                            isLoading = false;
                        }
                    }
                }
            }
        }
        private void MyNetwork_ClientInfoReceived(object? sender, ClientInfoEventArgs e)
        {
            LoadClientInfoData(e);
        }

        private void MyNetwork_AlertReceived(object? sender, AlertEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                PopupMessage = e.AlertItem.ToString() + Environment.NewLine + e.RelatedData;

                System.Windows.MessageBox.Show("Alert Recieved: " + e.AlertItem.ToString() + "--" + e.RelatedData);
            }));
        }

        private void MyNetwork_ChangeSetting(object? sender, ChangeSettingEventArgs e)
        {
            SettingsAction.Current.SynchronizeEnabled = false;
            SettingsAction.Current.ChangeSetting(e.SettingName, e.SettingValue);
            SettingsAction.Current.Save();

            switch (e.SettingName)
            {
                case "ConnectOnStart":
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.isLoading = true;
                        this.ConnectOnStart = bool.Parse(e.SettingValue);
                        UpdateAutoStart(this.ConnectOnStart);
                        TakeAction.thisMachine.ConnectOnstart = this.ConnectOnStart;
                        this.isLoading = false;
                    }));
                    break;

                case "IsMaster":
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.IsMaster = bool.Parse(e.SettingValue);
                        TakeAction.thisMachine.IsMaster = this.IsMaster;
                        PopupMessage = "Is Master changed to: " + e.SettingValue;
                        
                    }));
                    break;
            }
            TakeAction.SendClientInfo(IPAddress.Any);
        }

        private void OnStartServer(object sender, RoutedEventArgs? e)
        {
            DoStartServer();
        }

        private void MyNetwork_PasswordChanged(object? sender, CommunicationMessageEventArgs e)
        {
            SettingsAction.Current.SynchronizeEnabled = false;
            SettingsAction.Current.ChangeSetting(nameof(SettingsAction.NetworkPassword), e.Message);
            SettingsAction.Current.Save();
        }



        void ConsiderClosing(bool force, IPAddress? source)
        {
            if (source != null)
            {
                if (!force)
                {
                    if (System.Windows.MessageBox.Show(string.Format("Peer {0} has request this app to close.  Should it?", source.ToString()), "Close Artemis Manager?", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                this.Close();
            }

        }
        private void MyNetwork_ActionCommand(object? sender, ActionCommandEventArgs e)
        {
            if (e.Action == PCActions.RestartApp)
            {

                string file = string.Empty;
                List<string> args = new();
                foreach (var arg in Environment.GetCommandLineArgs())
                {
                    if (string.IsNullOrEmpty(file))
                    {
                        file = arg;
                    }
                    else
                    {
                        args.Add(arg);
                    }
                }

                ProcessStartInfo startInfo = new(file.Replace(".dll", ".exe"), string.Join(" ", args.ToArray()));
                Process.Start(startInfo);
                this.Dispatcher.Invoke(new Action<bool, IPAddress?>(ConsiderClosing), e.Force, e.Source);
                return;
            }
            else if (e.Action == PCActions.CloseApp)
            {
                this.Dispatcher.Invoke(new Action<bool, IPAddress?>(ConsiderClosing), e.Force, e.Source);
                return;
            }
            else
            {
                var result = TakeAction.ProcessPCAction(e.Action, e.Force, e.Source);
                if (result)
                {
                    switch (e.Action)
                    {
                        case PCActions.AddAppToStartup:
                        case PCActions.RemoveAppFromStartup:
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.InWindowsStartupFolder = TakeAction.IsThisAppInStartup();
                                
                            }));
                            break;
                        //case PCActions.SetAsMainScreenServer:
                        //case PCActions.RemoveAsMainScreenServer:
                        //    this.Dispatcher.Invoke(new Action(() =>
                        //    {
                                
                        //    }));
                        //    break;
                    }
                }
            }
        }

        private void MyNetwork_ConnectionClosed(object? sender, ConnectionRequestEventArgs e)
        {
            UpdateStatus(string.Format("Connection to: {0} - {1} CLOSED", e.Address.ToString(), e.Hostname));
            RemovePC(e.Address);
        }
        void RemovePC(IPAddress? address)
        {
            if (this.Dispatcher != System.Windows.Threading.Dispatcher.CurrentDispatcher)
            {
                this.Dispatcher.Invoke(new Action<IPAddress>(RemovePC), address);
            }
            else
            {
                try
                {
                    PCItem? remover = null;
                    foreach (var pc in ConnectedPCs)
                    {
                        if (pc != null && pc.IP != null && pc.IP.Equals(address))
                        {
                            remover = pc;
                            break;
                        }
                    }
                    if (remover != null)
                    {
                        TakeAction.RemoveConnection(remover);
                    }
                }
                catch
                {

                }
            }
        }
        private void ThrowFatal(Exception e)
        {
            //if (this.Dispatcher != System.Windows.Threading.Dispatcher.CurrentDispatcher)
            //{
            //    this.Dispatcher.Invoke(new Action<Exception>(ThrowFatal), e);
            //}
            //else
            //{
                System.Windows.MessageBox.Show("FATAL Exception:\r\n" + Environment.NewLine + e.Message, "FATAL ERROR!!!", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            //}
        }
        private void MyNetwork_FatalExceptionEncountered(object? sender, FatalExceptionEncounteredEventArgs e)
        {
            ThrowFatal(e.Exception);
        }

        private void MyNetwork_StatusUpdated(object? sender, StatusUpdateEventArgs e)
        {
            UpdateStatus(e.Message);
        }
        void AddPC(PCItem pcItem)
        {
            if (this.Dispatcher != System.Windows.Threading.Dispatcher.CurrentDispatcher)
            {
                this.Dispatcher.Invoke(new Action<PCItem>(AddPC), pcItem);
            }
            else
            {
                TakeAction.AddConnection(pcItem);
                //ConnectedPCs.Add(pcItem);
            }
        }

        private void MyNetwork_ConnectionReceived(object? sender, ConnectionRequestEventArgs e)
        {
            UpdateStatus(string.Format("Connection Received from: {0} - {1}", e.Address.ToString(), e.Hostname));
            var pcItem = new PCItem(e.Hostname, e.Address);
            AddPC(pcItem);
        }

        private void MyNetwork_ConnectionRequested(object? sender, ConnectionRequestEventArgs e)
        {
            UpdateStatus(string.Format("Connection Requested from: {0} - {1}", e.Address.ToString(), e.Hostname));
        }

        private void MyNetwork_ModPackageRequested(object? sender, ModPackageRequestEventArgs? e)
        {
            if (e != null)
            {
                TakeAction.FulfillModPackageRequest(e.Target, e.ItemRequestedIdentifier, ModItem.GetModItem(e.ModItem));
            }
        }

        private void OnRebroadcast(object sender, RoutedEventArgs? e)
        {
            MyNetwork.BroadcastMe();
        }

        private void OnClosed(object sender, EventArgs? e)
        {
            MyNetwork.HaltAll();
        }

        public static readonly DependencyProperty ExternalToolsLinksProperty =
            DependencyProperty.Register(nameof(ExternalToolsLinks), typeof(ObservableCollection<MenuItem>),
            typeof(Main));

        public ObservableCollection<MenuItem> ExternalToolsLinks
        {
            get
            {
                return (ObservableCollection<MenuItem>)this.GetValue(ExternalToolsLinksProperty);

            }
            set
            {
                this.SetValue(ExternalToolsLinksProperty, value);
            }
        }


        public static readonly DependencyProperty ArtemisUpgradeLinksProperty =
            DependencyProperty.Register(nameof(ArtemisUpgradeLinks), typeof(ObservableCollection<KeyValuePair<string, string>>),
            typeof(Main));

        public ObservableCollection<KeyValuePair<string, string>> ArtemisUpgradeLinks
        {
            get
            {
                return (ObservableCollection<KeyValuePair<string, string>>)this.GetValue(ArtemisUpgradeLinksProperty);

            }
            set
            {
                this.SetValue(ArtemisUpgradeLinksProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedPeerProperty =
          DependencyProperty.Register(nameof(SelectedPeer), typeof(PCItem),
         typeof(Main));

        public PCItem SelectedPeer
        {
            get
            {
                return (PCItem)this.GetValue(SelectedPeerProperty);

            }
            set
            {
                this.SetValue(SelectedPeerProperty, value);
            }
        }

        public static readonly DependencyProperty ChatMessageProperty =
           DependencyProperty.Register(nameof(ChatMessage), typeof(string),
          typeof(Main));

        public string ChatMessage
        {
            get
            {
                return (string)this.GetValue(ChatMessageProperty);

            }
            set
            {
                this.SetValue(ChatMessageProperty, value);
            }
        }
        public static readonly DependencyProperty IsMasterProperty =
           DependencyProperty.Register(nameof(IsMaster), typeof(bool),
          typeof(Main));


        public bool IsMaster
        {
            get
            {
                return (bool)this.GetValue(IsMasterProperty);

            }
            set
            {
                this.SetValue(IsMasterProperty, value);
            }
        }


        private void OnPutInStartup(object sender, RoutedEventArgs e)
        {
            TakeAction.CreateShortcutInStartup();
            this.InWindowsStartupFolder = TakeAction.IsThisAppInStartup();

        }
        private void OnRemoveFromStartup(object sender, RoutedEventArgs e)
        {
            TakeAction.RemoveShortcutFromStartup();
            this.InWindowsStartupFolder = TakeAction.IsThisAppInStartup();
        }

        private void OnBrowseForArtemis(object sender, RoutedEventArgs e)
        {
            BrowseForArtemis();
        }
        void BrowseForArtemis()
        {
            bool isOkay = false;
            while (!isOkay)
            {
                WPFFolderBrowserDialog dialog = new("Browse for Artemis executable");

                if (dialog.ShowDialog() == true)
                {
                    string candidate = System.IO.Path.Combine(dialog.FileName, ArtemisManager.ArtemisEXE);
                    if (System.IO.File.Exists(candidate))
                    {
                        ArtemisInstallFolder = dialog.FileName;
                        Properties.Settings.Default.ArtemisInstallFolder = ArtemisInstallFolder;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Invalid folder selected:{0}{0}Artemis executable not found.{0}Please select another folder.", Environment.NewLine),
                            "Invalid Artemis install location.", MessageBoxButton.OK, MessageBoxImage.Hand);
                    }
                }
                else
                {
                    isOkay = true;
                }
            }
        }
        public static readonly DependencyProperty ArtemisChangedProperty =
            DependencyProperty.Register(nameof(ArtemisChanged), typeof(bool),
            typeof(Main));

        public bool ArtemisChanged
        {
            get
            {
                return (bool)this.GetValue(ArtemisChangedProperty);

            }
            set
            {
                this.SetValue(ArtemisChangedProperty, value);
            }
        }
        private void OnSnapshotAretmis(object sender, RoutedEventArgs e)
        {
            //Mod Install.  Install folder naming convention:
            //ArtemisV#.##
            SnapshotArtemis();
        }
        void SnapshotArtemis()
        {
            if (string.IsNullOrEmpty(ArtemisInstallFolder) || !System.IO.File.Exists(System.IO.Path.Combine(ArtemisInstallFolder, ArtemisManagerAction.ArtemisManager.ArtemisEXE)))
            {
                BrowseForArtemis();
            }
            if (string.IsNullOrEmpty(ArtemisInstallFolder) || !System.IO.File.Exists(System.IO.Path.Combine(ArtemisInstallFolder, ArtemisManagerAction.ArtemisManager.ArtemisEXE)))
            {
                return;
            }
            else
            {
                ModItem item = ArtemisManagerAction.ArtemisManager.SnapshotInstalledArtemisVersion(ArtemisInstallFolder);
                item.Activate();
                Dispatcher.Invoke(new Action(() =>
                {
                    InstalledMods.Clear();
                    foreach (var mod in ArtemisManager.GetInstalledMods())
                    {
                        InstalledMods.Add(mod);
                    }
                    ArtemisChanged = false;
                }));
                TakeAction.SendClientInfo(IPAddress.Any);
            }
        }
        //@@@@


        private void OnStartArtemisSBS(object sender, RoutedEventArgs e)
        {
            ArtemisManager.StartArtemis();
        }

        private void OnStopArtemisSBS(object sender, RoutedEventArgs e)
        {
            ArtemisManager.StopArtemis();
        }

        private void DeactivateAllButBase(Guid? activeIdentifier)
        {
            foreach (var mod in InstalledMods)
            {
                if (mod.LocalIdentifier == activeIdentifier)
                {
                    mod.IsActive = true;
                }
                else
                {
                    mod.IsActive = false;
                }
            }
        }
        private void OnDeactivateMods(object sender, RoutedEventArgs e)
        {
            var baseItem = ArtemisManager.ClearActiveFolder();
            DeactivateAllButBase(baseItem?.LocalIdentifier);

            baseItem?.Activate();
            TakeAction.SendClientInfo(IPAddress.Any);
        }

        private void OnInstallMod(object sender, RoutedEventArgs e)
        {
            ModInstallWindow win = new()
            {
                ForInstall = true
            };
            if (win.ShowDialog() == true)
            {
                InstalledMods.Add(win.Mod);
            }
        }

        private void OnAbout(object sender, RoutedEventArgs e)
        {
            AboutWindow win = new();
            win.Show();
        }

        private void OnOpenArtemisRunFolder(object sender, RoutedEventArgs e)
        {
            ModManager.CreateFolder(ModItem.ActivatedFolder);
            ProcessStartInfo startInfo = new("explorer", ModItem.ActivatedFolder);
            Process.Start(startInfo);
        }

        private void OnGenerateMod(object sender, RoutedEventArgs e)
        {
            ModInstallWindow win = new()
            {
                ForInstall = false
            };
            if (win.ShowDialog() == true)
            {
                InstalledMods.Add(win.Mod);
            }
        }

        private void OnLocalUpdateCheck(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Checking for update...");
            Task.Run(async () =>
            {
                var result = TakeAction.UpdateCheck(true);
                if (result.Result.Item1)
                {
                    var newResult = await TakeAction.DoUpdate(true, result.Result.Item2);
                    if (newResult)
                    {
                        this.Dispatcher.Invoke(new Action(() => { this.Close(); }));
                    }
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show("You are running the current version of Artemis Manager.", "Update Check", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                }
                UpdateStatus("Update check complete.");
            });
        }
        private void OnTest(object sender, RoutedEventArgs e)
        {
            /*  Popup Test */
            /*
            PopupMessage = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            ShowPopup = true;
            */

            /*  Scroll to bottom of listbox on adding entry test */
            /*
            Chat.Add(new ChatMessage("local", (++testCounter).ToString() + ") Lorem ipsum dolor sit amet."));
            Status.Add((testCounter).ToString() + ") Lorem ipsum dolor sit amet.");
            */


            //PopupMessage = "This is a test.  It works on my machine, so all is good.";

            /* Screen resolutions test */
            //var sizes = TakeAction.GetAvailableScreenResolutions();
            //StringBuilder sb = new();
            //foreach (var sz in sizes)
            //{
            //    sb.AppendLine(sz.Width.ToString() + "x" + sz.Height.ToString());
            //}
            //PopupMessage = sb.ToString();
            //TestClientManagerWindow win = new TestClientManagerWindow();
            //win.Show();
        }



        bool isDragging = false;
        private void OnModDragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                isDragging = true;
            }
            else if (e.Data.GetDataPresent(typeof(ModItemControl)))
            {
                isDragging = true;
            }

        }

        private void OnModDragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = System.Windows.DragDropEffects.None;
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(typeof(ModItemControl)))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
        }

        private void OnModDragLeave(object sender, System.Windows.DragEventArgs e)
        {
            isDragging = false;
        }

        private void OnModDrop(object sender, System.Windows.DragEventArgs e)
        {

            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                var file = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                foreach (var f in file)
                {
                    ModInstallWindow win = new()
                    {
                        ForInstall = true,
                        PackageFile = f
                    };
                    if (win.ShowDialog() == true)
                    {
                        InstalledMods.Add(win.Mod);
                    }
                }
                isDragging = false;
            }
            else if (e.Data.GetDataPresent(typeof(ModItemControl)))
            {

                var ctl = (ModItemControl)e.Data.GetData(typeof(ModItemControl));
                isDragging = false;
                if (ctl.IsRemote && ctl.Source != null)
                {
                    MyNetwork.SendModPackageRequest(ctl.Source, ctl.Mod.GetJSON(), ctl.Mod.PackageFile);
                }
            }

        }

        private void OnMissionDragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                isDragging = true;
            }
            else if (e.Data.GetDataPresent(typeof(ModItemControl)))
            {
                isDragging = true;
            }

        }

        private void OnMissionDragLeave(object sender, System.Windows.DragEventArgs e)
        {

        }

        private void OnMissionDragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = System.Windows.DragDropEffects.None;
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(typeof(ModItemControl)))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
        }

        private void OnMissionDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                var file = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                foreach (var f in file)
                {
                    ModInstallWindow win = new()
                    {
                        ForInstall = true,
                        PackageFile = f
                    };
                    win.Mod.IsMission = true;
                    win.Title = "Install Mission";
                    if (win.ShowDialog() == true)
                    {
                        InstalledMissions.Add(win.Mod);
                    }
                }
            }
            else if (e.Data.GetDataPresent(typeof(ModItemControl)))
            {
                var ctl = (ModItemControl)e.Data.GetData(typeof(ModItemControl));

                if (ctl.IsRemote && ctl.Source != null)
                {
                    MyNetwork.SendModPackageRequest(ctl.Source, ctl.Mod.GetJSON(), ctl.Mod.PackageFile);
                }
            }
        }

        private void OnModUninstalled(object sender, RoutedEventArgs e)
        {
            if (sender is ModItemControl ctl)
            {
                ctl.Dispatcher.Invoke(() =>
                {
                    if (ctl.DataContext is ModItem mod)
                    {
                        InstalledMods.Remove(mod);
                    }
                });
            }
        }

        private void OnMissionUninstalled(object sender, RoutedEventArgs e)
        {
            if (sender is ModItemControl ctl)
            {
                ctl.Dispatcher.Invoke(() =>
                {
                    if (ctl.DataContext is ModItem mod)
                    {
                        InstalledMissions.Remove(mod);
                    }
                });
            }
        }

        private void OnRemoteInstallMod(object sender, RoutedEventArgs e)
        {
            if (SelectedPeer != null && SelectedPeer.IP != null && e.OriginalSource is ModItem Mod)
            {
                TakeAction.FulfillModPackageRequest(SelectedPeer.IP, Mod.PackageFile, Mod);
            }
        }

        private void OnModActivated(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                InstalledMods.Clear();
                foreach (var mod in ArtemisManager.GetInstalledMods())
                {
                    InstalledMods.Add(mod);
                }
            });
        }

        private void OnDownloadArtemisUpgrade(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is string file)
                {
                    WPFFolderBrowserDialog dialog = new();

                    string targetFolder = string.Empty;
                    dialog.Title = "Select folder to save download to";
                    if (dialog.ShowDialog() == true)
                    {
                        targetFolder = dialog.FileName;

                        Task.Run(async () =>
                        {
                            using HttpClient client = new();
                            try
                            {

                                using var stream = await client.GetStreamAsync(ArtemisManager.ArtemisUpgradesURLFolder + file);
                                if (stream != null)
                                {
                                    byte[] buffer = new byte[32768];
                                    int bytesRead = 0;
                                    if (stream != null)
                                    {
                                        string target = System.IO.Path.Combine(targetFolder, file);
                                        using (FileStream fs = new(target, FileMode.Create))
                                        {
                                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                                            {
                                                fs.Write(buffer, 0, bytesRead);
                                            }
                                        }
                                        Dispatcher.Invoke(() =>
                                        {
                                            if (MessageBox.Show(
                                                string.Format("Artemis SBS Upgrade downloaded successfully.{0}{0}Do you want to run the install?", Environment.NewLine),
                                                    "Artemis SBS Upgrade download", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                            {
                                                ProcessStartInfo startInfo = new(target)
                                                {
                                                    UseShellExecute = true
                                                };
                                                Process.Start(startInfo);

                                            }
                                        });
                                    }
                                    else
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            MessageBox.Show("Error downloading " + file,
                                                "Artmis SBS Upgrade file download", MessageBoxButton.OK, MessageBoxImage.Error);
                                        });

                                    }
                                }
                                else
                                {
                                    Dispatcher.Invoke(() =>
                                    {

                                        MessageBox.Show("Error downloading " + file,
                                            "Artmis SBS Upgrade file download", MessageBoxButton.OK, MessageBoxImage.Error);

                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(string.Format("Error downloading {0}:{2}{2}{1}", file, ex.Message, Environment.NewLine),
                                    "Artmis SBS Upgrade file download", MessageBoxButton.OK, MessageBoxImage.Error);
                            }

                        });
                    }

                }
            }
        }


        private void OnEngineeringPresets(object sender, RoutedEventArgs e)
        {
            EngineeringPresetEditWindow win = new();
            win.Show();
        }


        private void OnSettings(object sender, RoutedEventArgs e)
        {
            SettingsWindow win = new();

            win.ShowDialog();
            ConnectOnStart = SettingsAction.Current.ConnectOnStart;
        }

        private void OnChatReceived(object sender, RoutedEventArgs e)
        {
            if (SelectedTabItem != null)
            {
                if (SelectedTabItem.Tag?.ToString() != "Chat")
                {
                    ChatAlert = true;
                }
            }
        }
    }
}