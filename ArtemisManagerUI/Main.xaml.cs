﻿using AMCommunicator;
using ArtemisManagerAction;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

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
                Chat = new();

                Status = new ObservableCollection<string>();
                ConnectedPCs = new();
                if (Properties.Settings.Default.IsAMaster)
                {
                    ConnectedPCs.Add(new PCItem("All Connections", IPAddress.Any));
                };
                TakeAction.ConnectedPCs = ConnectedPCs;
                this.InWindowsStartupFolder = TakeAction.IsThisAppInStartup();

                IsArtemisRunning = ArtemisManager.IsArtemisRunning();
                IsUsingThisAppControlledArtemis = ArtemisManager.IsRunningArtemisUnderMyControl();
                InstalledMods = new(ArtemisManager.GetInstalledMods());
                InstalledMissions = new(ArtemisManager.GetInstalledMissions());
                AppVersion = TakeAction.GetAppVersion();
                IsMaster = Properties.Settings.Default.IsAMaster;
                var drives = DriveInfo.GetDrives();
                //List<string> dd = new();
                foreach (var drive in drives)
                {
                    if (drive.Name.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)[..1]))
                    {
                        FreeSpaceOnAppDrive = drive.AvailableFreeSpace;
                    }
                    //if (drive.DriveType == DriveType.Fixed)
                    //{
                    //    dd.Add(drive.Name + "," + drive.DriveType.ToString() + "," + drive.TotalSize.ToString() + "," + drive.AvailableFreeSpace);
                    //}
                }
                
            }
            catch (Exception ex)
            {
                UpdateStatus("Error starting up: " + ex.Message);
            }
            InitializeComponent();
             

        }
        bool isLoading = true;
        readonly Network MyNetwork = Network.GetNetwork("");
        FileSystemWatcher watcher = null;
        /*
         * 
         *
         *Settings to set up:
         *ModInstallFolder
         *ServerIPOrName
         *ConnectOnStart
         *IsAMaster
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
                    if (Properties.Settings.Default.UpgradeRequired)
                    {
                        Properties.Settings.Default.Upgrade();
                        Properties.Settings.Default.UpgradeRequired = false;
                        Properties.Settings.Default.Save();
                    }
                    ArtemisInstallFolder = Properties.Settings.Default.ArtemisInstallFolder;
                    if (string.IsNullOrEmpty(ArtemisInstallFolder) || !System.IO.File.Exists(System.IO.Path.Combine(ArtemisInstallFolder, ArtemisManager.ArtemisEXE)))
                    {
                        ArtemisInstallFolder = ArtemisManager.AutoDetectArtemisInstallPath();
                        Properties.Settings.Default.ArtemisInstallFolder = ArtemisInstallFolder;
                        Properties.Settings.Default.Save();
                    }


                    Network.ConnectionPort = Properties.Settings.Default.ListeningPort;
                    Network.Password = Properties.Settings.Default.NetworkPassword;
                    Password = Network.Password;
                    AutoStartServer = Properties.Settings.Default.ConnectOnStart;
                    Port = Properties.Settings.Default.ListeningPort;
                    if (!string.IsNullOrEmpty(ArtemisInstallFolder))
                    {
                        if (ArtemisManager.CheckIfArtemisSnapshotNeeded(ArtemisInstallFolder))
                        {
                            ArtemisChanged= true;
                            string? version = ArtemisManager.GetArtemisVersion(ArtemisInstallFolder);
                            if (version != null && MessageBox.Show(string.Format("New Version of Artemis SBS detected (Version {0}).\r\nDo you wish to make a new snapshot?", version), "New Artemis Version", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                SnapshotArtemis();
                            }
                        }
                        else
                        {

                        }
                    }
                    isLoading = false;
                    if (AutoStartServer)
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


        public static readonly DependencyProperty ShowPopupProperty =
         DependencyProperty.Register(nameof(ShowPopup), typeof(bool),
             typeof(Main), new PropertyMetadata(OnShowPopup));

        private static void OnShowPopup(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Main me)
            {
                if (me.ShowPopup)
                {
                    System.Timers.Timer timer = new()
                    {
                        Interval = 5000
                    };
                    timer.Elapsed += me.Timer_Elapsed;
                    timer.Start();
                }
            }
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => {
                ShowPopup = false;
                PopupMessage = string.Empty;
            }));
            if (sender is System.Timers.Timer tmr)
            {
                tmr.Stop();
                tmr.Dispose();
            }
        }

        public bool ShowPopup
        {
            get
            {
                return (bool)this.GetValue(ShowPopupProperty);

            }
            set
            {
                this.SetValue(ShowPopupProperty, value);

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
        
        public static readonly DependencyProperty AutoStartServerProperty =
         DependencyProperty.Register(nameof(AutoStartServer), typeof(bool),
             typeof(Main), new PropertyMetadata(OnAutoStartChanged));
        void UpdateAutoStart(bool value)
        {
            Properties.Settings.Default.ConnectOnStart = value;
            Properties.Settings.Default.Save();

        }
        private static void OnAutoStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Main me = (Main)d;
            if (me != null)
            {
                if (!me.isLoading)
                {
                    switch (MessageBox.Show("Do you want to change this setting on all connected peers?", "Change auto start peer-to-peer network", MessageBoxButton.YesNoCancel))
                    {
                        case MessageBoxResult.Cancel:
                            me.AutoStartServer = Properties.Settings.Default.ConnectOnStart;
                            break;
                        case MessageBoxResult.Yes:
                            me.UpdateAutoStart(me.AutoStartServer);
                            foreach (var pc in me.ConnectedPCs)
                            {
                                if (pc.IP != null)
                                {
                                    me.MyNetwork.SendChangeSetting(pc.IP, "ConnectOnStart", me.AutoStartServer.ToString());
                                }
                            }
                            
                            break;
                        case MessageBoxResult.No:
                            Properties.Settings.Default.ConnectOnStart = me.AutoStartServer;
                            Properties.Settings.Default.Save();
                            break;
                    }
                }
            }
        }

        public bool AutoStartServer
        {
            get
            {
                return (bool)this.GetValue(AutoStartServerProperty);

            }
            set
            {
                this.SetValue(AutoStartServerProperty, value);

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


        public static readonly DependencyProperty PasswordProperty =
           DependencyProperty.Register(nameof(Password), typeof(string),
               typeof(Main), new PropertyMetadata(OnPasswordChanged));

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Main me = (Main)d;
            if (me != null)
            {
                if (!me.isLoading)
                {
                    if (me.IsStarted)
                    {
                        switch (MessageBox.Show("Update all connected peers with new password?\r\nWarning--if not changing the password on all connected peers will mean these peers will NOT be able to reconnect if they lose connection.\r\nThe current connection will be unaffected.", "Update Managers", MessageBoxButton.YesNoCancel))
                        {
                            case MessageBoxResult.Yes:
                                Network.Password = me.Password;
                                Properties.Settings.Default.NetworkPassword = me.Password;
                                Properties.Settings.Default.Save();
                                //TODO: Send notice of update to all PCs
                                foreach (var pc in me.ConnectedPCs)
                                {
                                    if (pc.IP != null)
                                    {
                                        me.MyNetwork.SendChangePassword(pc.IP, me.Password);
                                    }
                                }
                                break;
                            case MessageBoxResult.No:
                                Network.Password = me.Password;
                                Properties.Settings.Default.NetworkPassword = me.Password;
                                Properties.Settings.Default.Save();
                                break;
                            case MessageBoxResult.Cancel:
                                me.Password = Network.Password;
                                break;
                        }
                    }
                }
            }
        }

        public string Password
        {
            get
            {
                return (string)this.GetValue(PasswordProperty);

            }
            set
            {
                this.SetValue(PasswordProperty, value);

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

        public static readonly DependencyProperty PortProperty =
          DependencyProperty.Register(nameof(Port), typeof(int),
              typeof(Main), new PropertyMetadata(OnPortChanged));

        void UpdatePort(int port)
        {
            Properties.Settings.Default.ListeningPort = port;
            Properties.Settings.Default.Save();
            Network.ConnectionPort = Properties.Settings.Default.ListeningPort;
        }
        private static void OnPortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Main me = (Main)d;
            if (me != null)
            {
                if (!me.isLoading)
                {
                    if (int.TryParse(e.NewValue?.ToString(), out int val))
                    {
                        if (!me.IsStarted || MessageBox.Show("Are you sure you wish to change the listening port for this application?\r\nThis will change the port on all connected computers and only on the connected computers.\r\n\r\nYou will need to restart the application for the new port to take effect.", "Change Listening Port", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            me.UpdatePort((int)e.NewValue);
                            foreach (var pc in me.ConnectedPCs)
                            {
                                if (pc.IP != null)
                                {
                                    me.MyNetwork.SendChangeSetting(pc.IP, "ListeningPort", Properties.Settings.Default.ListeningPort.ToString());
                                }
                            }
                        }
                        else
                        {
                            me.Port = Properties.Settings.Default.ListeningPort;
                        }
                    }
                    else
                    {
                        if (int.TryParse(e.OldValue?.ToString(), out int result))
                        {
                            me.Port = result;
                        }
                    }
                }
            }
        }
        public int Port
        {
            get
            {
                return (int)this.GetValue(PortProperty);

            }
            set
            {
                this.SetValue(PortProperty, value);

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


        public static readonly DependencyProperty ChatsProperty =
          DependencyProperty.Register(nameof(Chat), typeof(ObservableCollection<ChatMessage>),
              typeof(Main));

        public ObservableCollection<ChatMessage> Chat
        {
            get
            {
                return (ObservableCollection<ChatMessage>)this.GetValue(ChatsProperty);

            }
            set
            {
                this.SetValue(ChatsProperty, value);
            }
        }

        public void AddChatLine(string source, string message)
        {
            if (this.Dispatcher != System.Windows.Threading.Dispatcher.CurrentDispatcher)
            {
                this.Dispatcher.Invoke(new Action<string, string>(AddChatLine), source, message);
            }
            else
            {
                Chat.Add(new ChatMessage(source.ToString(), message));
                if (SelectedTabItem == null || SelectedTabItem.Tag?.ToString() != "Chat")
                {
                    ChatAlert = true;
                }
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
                    Status.Add(DateTime.Now.ToString("HH:mm:ss") + ": "  + message);
                    //StatusList.ScrollIntoView(Status.Count - 1);
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
            Dispatcher.Invoke(() => {
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
        void DoStartServer()
        {
            UpdateStatus("Starting Connection Service");
            TakeAction.StatusUpdate += MyNetwork_StatusUpdated;


            MyNetwork.ModPackageRequested += MyNetwork_ModPackageRequested;
            MyNetwork.ConnectionRequested += MyNetwork_ConnectionRequested;
            MyNetwork.ConnectionReceived += MyNetwork_ConnectionReceived;
            MyNetwork.StatusUpdated += MyNetwork_StatusUpdated;
            MyNetwork.FatalExceptionEncountered += MyNetwork_FatalExceptionEncountered;
            MyNetwork.ConnectionClosed += MyNetwork_ConnectionClosed;
            MyNetwork.ActionCommand += MyNetwork_ActionCommand;
            MyNetwork.CommunicationMessageReceived += MyNetwork_CommunicationMessageReceived;
            MyNetwork.PasswordChanged += MyNetwork_PasswordChanged;
            MyNetwork.ChangeSetting += MyNetwork_ChangeSetting;
            MyNetwork.AlertReceived += MyNetwork_AlertReceived;
            MyNetwork.ClientInfoReceived += MyNetwork_ClientInfoReceived;
            MyNetwork.ArtemisActionReceived += MyNetwork_ArtemisActionReceived;
            MyNetwork.ModPackageReceived += MyNetwork_ModPackageReceived;
            MyNetwork.PopupMessageEvent += MyNetwork_PopupMessageEvent;
            MyNetwork.Connect();
            IsStarted = true;
        }

        private void MyNetwork_PopupMessageEvent(object? sender, StatusUpdateEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.PopupMessage += "\r\n\r\n" + e.Message;
                this.ShowPopup = true;
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
            var wasProcessed = TakeArtemisAction.ProcessArtemisAction(e.Source, e.Action, e.Mod);
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
                    
                    if (item.IP != null && e.Source != null)
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
                if (!Properties.Settings.Default.IsAMaster)
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
                PopupMessage = e.AlertItem.ToString() + "\r\n" + e.RelatedData;
                ShowPopup = true;
                MessageBox.Show("Alert Recieved: " + e.AlertItem.ToString() + "--" + e.RelatedData);
            }));
            
            //TODO:  Offer option of attempting remote update of client, with warning that if not updated, some functionality may not work.
        }

        private void MyNetwork_ChangeSetting(object? sender, ChangeSettingEventArgs e)
        {
            TakeAction.ChangeSetting(e.SettingName, e.SettingValue);
            switch (e.SettingName)
            {
                case "ConnectOnStart":
                    this.Dispatcher.Invoke(new Action(() => {
                        this.isLoading = true;
                        this.AutoStartServer = bool.Parse(e.SettingValue);
                        UpdateAutoStart(this.AutoStartServer);
                        this.isLoading = false;
                    }));
                    break;
                case "ListeningPort":
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.isLoading = true;
                        this.Port = int.Parse(e.SettingValue);
                        UpdatePort(Port);
                        this.isLoading = false;
                    }));
                    Network.ConnectionPort = int.Parse(e.SettingValue);
                    break;
                case "IsMaster":
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.IsMaster = bool.Parse(e.SettingValue);
                    }));
                    break;
            }
        }

        private void OnStartServer(object sender, RoutedEventArgs? e)
        {
            DoStartServer();
        }

        private void MyNetwork_PasswordChanged(object? sender, CommunicationMessageEventArgs e)
        {
            isLoading = true;
            this.Dispatcher.Invoke(new Action(() =>
            {
                Password = e.Message;
            }));
            
            Network.Password = e.Message;
            Properties.Settings.Default.NetworkPassword = e.Message;
            Properties.Settings.Default.Save();
            isLoading = false;
        }

        private void MyNetwork_CommunicationMessageReceived(object? sender, CommunicationMessageEventArgs e)
        {
            if (e.Host != null)
            {
                AddChatLine(e.Host, e.Message);
            }
        }

        void ConsiderClosing(bool force, IPAddress? source)
        {
            if (source != null)
            {
                if (!force)
                {
                    if (MessageBox.Show(string.Format("Peer {0} has request this app to close.  Should it?", source.ToString()), "Close Artemis Manager?", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
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
                        if (pc.IP == address)
                        {
                            remover = pc;
                            break;
                        }
                    }
                    if (remover != null)
                    {
                        ConnectedPCs.Remove(remover);
                    }
                }
                catch
                {

                }
            }
        }
        private void ThrowFatal(Exception e)
        {
            if (this.Dispatcher != System.Windows.Threading.Dispatcher.CurrentDispatcher )
            {
                this.Dispatcher.Invoke(new Action<Exception>(ThrowFatal), e);
            }
            else
            {
                MessageBox.Show("FATAL Exception:\r\n" + e.ToString());
                this.Close();
            }
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
                ConnectedPCs.Add(pcItem);
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
          typeof(Main), new PropertyMetadata(OnIsMasterChanged));

        private static void OnIsMasterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Main me) 
            {
                if (!me.isLoading)
                {
                    Properties.Settings.Default.IsAMaster = me.IsMaster;
                    Properties.Settings.Default.Save();
                }
            }
        }

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
        private void OnSendMessage(object sender, RoutedEventArgs e)
        {
            if (sender is PeerInfoControl me)
            {
                PCItem? item = me.ItemData;
                if (item != null)
                {
                    string? msg = me.ChatMessage;
                    me.ChatMessage = string.Empty;

                    if (item.IP?.ToString() == IPAddress.Any.ToString())
                    {
                        foreach (var pcItem in ConnectedPCs)
                        {
                            if (pcItem.IP?.ToString() != IPAddress.Any.ToString() && msg != null)
                            {
                                if (pcItem.IP != null)
                                {
                                    AddChatLine("LOCALHOST -> " + pcItem.Hostname, msg);
                                    MyNetwork.SendMessage(pcItem.IP, msg);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (msg != null && item.IP != null)
                        {
                            AddChatLine("LOCALHOST -> " + item.Hostname, msg);
                            MyNetwork.SendMessage(item.IP, msg);
                        }
                    }
                }
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
            WPFFolderBrowser.WPFFolderBrowserDialog dialog = new("Browse for Artemis executable");
            if (dialog.ShowDialog() == true)
            {
                ArtemisInstallFolder = dialog.FileName;
                Properties.Settings.Default.ArtemisInstallFolder = ArtemisInstallFolder;
                Properties.Settings.Default.Save();
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
                
                //ArtemisManager.ClearActiveFolder();
                item.Activate();
                Dispatcher.Invoke(new Action(() =>
                {
                    InstalledMods.Clear();
                    foreach(var mod in ArtemisManager.GetInstalledMods())
                    {
                        InstalledMods.Add(mod);
                    }
                    ArtemisChanged = false;
                }));
                TakeAction.SendClientInfo(IPAddress.Any);
            }

        }
        private void OnStartArtemisSBS(object sender, RoutedEventArgs e)
        {

            ArtemisManager.StartArtemis();
            System.Threading.Thread.Sleep(2000);
            IsArtemisRunning = ArtemisManager.IsArtemisRunning();
            IsUsingThisAppControlledArtemis = ArtemisManager.IsRunningArtemisUnderMyControl();
        }

        private void OnStopArtemisSBS(object sender, RoutedEventArgs e)
        {
            ArtemisManager.StopArtemis();
            System.Threading.Thread.Sleep(2000);
            IsArtemisRunning = ArtemisManager.IsArtemisRunning();
            IsUsingThisAppControlledArtemis = ArtemisManager.IsRunningArtemisUnderMyControl();
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
            ModInstallWindow win = new();
            win.ForInstall = true;
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
            
            ProcessStartInfo startInfo = new ProcessStartInfo("explorer", ArtemisManagerAction.ModItem.ActivatedFolder);
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
                    UpdateStatus("Update check complete.");
                });
            
        }
        private int testCounter = 0;
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


            PopupMessage = "This is a test.  It works on my machine, so all is good.";
            ShowPopup = true;
        }

        private void OnPopupMouseUp(object sender, MouseButtonEventArgs e)
        {
            ShowPopup = false;
        }

        private void OnPreviewPortInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9.-]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        bool isDragging = false;
        private void OnModDragEnter(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement me)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    isDragging = true;
                }
                else if (e.Data.GetDataPresent(typeof(ModItemControl)))
                {
                    isDragging = true;
                }
            }
        }

        private void OnModDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(typeof(ModItemControl)))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void OnModDragLeave(object sender, DragEventArgs e)
        {
            isDragging = false;
        }

        private void OnModDrop(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement me)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var file = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (var f in file)
                    {
                        ModInstallWindow win = new();
                        win.ForInstall = true;
                        win.PackageFile = f;
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
        }

        private void OnMissionDragEnter(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement me)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    isDragging = true;
                }
                else if (e.Data.GetDataPresent(typeof(ModItemControl)))
                {
                    isDragging = true;
                }
            }
        }

        private void OnMissionDragLeave(object sender, DragEventArgs e)
        {

        }

        private void OnMissionDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(typeof(ModItemControl)))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void OnMissionDrop(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement me)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var file = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (var f in file)
                    {
                        ModInstallWindow win = new();
                        win.ForInstall = true;
                        win.PackageFile = f;
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
            if (SelectedPeer!= null && SelectedPeer.IP != null && e.OriginalSource is ModItem Mod)
            {
                TakeAction.FulfillModPackageRequest(SelectedPeer.IP, Mod.PackageFile, Mod);
            }
        }

        private void OnModActivated(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                InstalledMods.Clear();
                foreach(var mod in ArtemisManager.GetInstalledMods())
                {
                    InstalledMods.Add(mod);
                }
            });
        }
    }
}