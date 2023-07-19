using AMCommunicator;
using ArtemisManagerAction;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
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
                ConnectedPCs = new()
                {
                    new PCItem("All Connections", IPAddress.Any)
                };
                
                this.InWindowsStartupFolder = TakeAction.IsThisAppInStartup();

                IsArtemisRunning = ArtemisManager.IsArtemisRunning();
                InstalledMods = new(ArtemisManager.GetInstalledMods());

                AppVersion = TakeAction.GetAppVersion();
            }
            catch (Exception ex)
            {
                UpdateStatus("Error starting up: " + ex.Message);
            }
            InitializeComponent();

        }
        bool isLoading = true;
        readonly Network MyNetwork = Network.GetNetwork("");
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
                            string? version = ArtemisManager.GetArtemisVersion(ArtemisInstallFolder);
                            if (version != null && MessageBox.Show(string.Format("New Version of Artemis SBS detected (Version {0}).\r\nDo you wish to make a new snapshot?", version), "New Artemis Version", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                SnapshotArtemis();
                            }
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
             typeof(Main));

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
              typeof(Main));

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
            //this.Dispatcher.Invoke(() =>
            //{
            //    this.PopupMessage = e.Message;
            //    this.ShowPopup = true;
            //});
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
                    InstalledMods.Add(item);
                }

                //5. check TakeArtemisAction.StagedModItemToActivateOnceInstalled and if matches and not null, activate and set to null.
                if (TakeArtemisAction.StagedModItemToActivateOnceInstalled != null && TakeArtemisAction.StagedModItemToActivateOnceInstalled.Equals(item))
                {
                    item.Activate();
                    TakeArtemisAction.StagedModItemToActivateOnceInstalled = null;
                }
            }
        }

        private void MyNetwork_ArtemisActionReceived(object? sender, ArtemisActionEventArgs e)
        {
            var wasProcessed = TakeArtemisAction.ProcessArtemisAction(e.Source, e.Action, e.Mod);

            switch (e.Action)
            {
                case AMCommunicator.Messages.ArtemisActions.ResetToVanilla:
                    this.Dispatcher.Invoke(new Action<Guid?>(DeactivateAllButBase), wasProcessed.Item2?.LocalIdentifier);
                    break;
                case AMCommunicator.Messages.ArtemisActions.StopArtemis:
                    this.Dispatcher.Invoke(new Action(() => { this.IsArtemisRunning = false; }));
                    break;
                case AMCommunicator.Messages.ArtemisActions.StartArtemis:
                    this.Dispatcher.Invoke(new Action(() => { this.IsArtemisRunning = wasProcessed.Item1; }));

                    break;
                case AMCommunicator.Messages.ArtemisActions.InstallMod:

                    if (wasProcessed.Item1)  //Will only be processed if we already have the package file in the archive folder.
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            if (wasProcessed.Item2 != null)
                            {
                                InstalledMods.Add(wasProcessed.Item2);
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
                                ModItem? modToRemove = null;
                                foreach (var mod in InstalledMods)
                                {
                                    if (mod.Equals(wasProcessed.Item2))
                                    {
                                        modToRemove = mod;
                                        break;
                                    }
                                }
                                if (modToRemove != null)
                                {
                                    InstalledMods.Remove(modToRemove);
                                }
                            }
                        }));
                    }

                    break;
                case AMCommunicator.Messages.ArtemisActions.ActivateMod:

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        foreach (var mod in InstalledMods)
                        {
                            if (mod.LocalIdentifier == e.Identifier)
                            {
                                mod.IsActive = true;
                            }
                        }
                    }));

                    break;
            }
        }

        void LoadClientInfoData(ClientInfoEventArgs e)
        {
            if (Thread.CurrentThread != this.Dispatcher.Thread)
            {
                this.Dispatcher.Invoke(new Action<ClientInfoEventArgs>(LoadClientInfoData), e);
            }
            else
            {

                foreach (var item in ConnectedPCs)
                {
                    if (item.IP != null && e.Source != null)
                    {
                        if (item.IP.ToString().Equals(e.Source.ToString()))
                        {
                            item.LoadClientInfoData(e);
                            break;
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
            }
        }

        private void OnStartServer(object sender, RoutedEventArgs? e)
        {
            DoStartServer();
        }

        private void MyNetwork_PasswordChanged(object? sender, CommunicationMessageEventArgs e)
        {
            isLoading = true;
            Password = e.Message;
            Network.Password = Password;
            Properties.Settings.Default.NetworkPassword = Password;
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
            if (e.Action == ActionCommands.CloseApp)
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
                        case ActionCommands.AddAppToStartup:
                        case ActionCommands.RemoveAppFromStartup:
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

        private void OnDisconnect(object sender, RoutedEventArgs? e)
        {
            PCItem? item = GetItem((ICommandSource)sender);
            if (item != null && item.IP != null)
            {
                if (item.IP.ToString() == IPAddress.Any.ToString())
                {
                    foreach (var pcItem in ConnectedPCs)
                    {
                        if (pcItem.IP != null)
                        {
                            if (pcItem.IP.ToString() != IPAddress.Any.ToString())
                            {
                                MyNetwork.Halt(pcItem.IP);
                            }
                        }
                    }
                }
                else
                {

                    MyNetwork.Halt(item.IP);
                }
            }

        }
        private static PCItem? GetItem(ICommandSource? commandSource)
        {
            if (commandSource != null)
            {
                return (PCItem)commandSource.CommandParameter;
            }
            else
            {
                return null;
            }

        }
        private void OnPing(object sender, RoutedEventArgs? e)
        {
            PCItem? item = GetItem((ICommandSource)sender);
            if (item != null && item.IP != null)
            {
                if (item.IP.ToString() == IPAddress.Any.ToString())
                {
                    foreach (var pcItem in ConnectedPCs)
                    {
                        if (pcItem.IP != null)
                        {
                            if (pcItem.IP.ToString() != IPAddress.Any.ToString())
                            {
                                MyNetwork.SendPing(pcItem.IP);
                            }
                        }
                    }
                }
                else
                {
                    MyNetwork.SendPing(item.IP);
                }
            }

        }

        private void OnSendMessage(object sender, RoutedEventArgs? e)
        {
            PCItem? item = GetItem((ICommandSource)sender);
            Button btn = (Button)sender;
            if (item != null)
            {
                string? msg = btn.Tag?.ToString();
                btn.Tag = "";

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
                InstalledMods.Add(item);
                //ArtemisManager.ClearActiveFolder();
                item.Activate();
            }

        }
        private void OnStartArtemisSBS(object sender, RoutedEventArgs e)
        {

            ArtemisManager.StartArtemis();
            System.Threading.Thread.Sleep(2000);
            IsArtemisRunning = ArtemisManager.IsArtemisRunning();
        }

        private void OnStopArtemisSBS(object sender, RoutedEventArgs e)
        {
            ArtemisManager.StopArtemis();
            System.Threading.Thread.Sleep(2000);
            IsArtemisRunning = ArtemisManager.IsArtemisRunning();
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

        /// <summary>
        /// Restart the connected Peer PC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRestart(object sender, RoutedEventArgs e)
        {
            PCItem? item = GetItem((ICommandSource)sender);
            if (item != null && item.IP != null)
            {
                if (item.IP.ToString() == IPAddress.Any.ToString())
                {
                    foreach (var pcItem in ConnectedPCs)
                    {
                        if (pcItem.IP != null)
                        {
                            if (pcItem.IP.ToString() != IPAddress.Any.ToString() && pcItem.IP.ToString() != IPAddress.None.ToString())
                            {
                                MyNetwork.SendPCAction(item.IP, PCActions.RestartPC, true);
                            }
                        }
                    }
                }
                else
                {
                    MyNetwork.SendPCAction(item.IP, PCActions.RestartPC, true);
                }
            }
        }

        private void OnShutdown(object sender, RoutedEventArgs e)
        {
            PCItem? item = GetItem((ICommandSource)sender);
            if (item != null && item.IP != null)
            {
                if (item.IP.ToString() == IPAddress.Any.ToString())
                {
                    foreach (var pcItem in ConnectedPCs)
                    {
                        if (pcItem.IP != null)
                        {
                            if (pcItem.IP.ToString() != IPAddress.Any.ToString() && pcItem.IP.ToString() != IPAddress.None.ToString())
                            {
                                MyNetwork.SendPCAction(item.IP, PCActions.ShutdownPC, true);
                            }
                        }
                    }
                }
                else
                {
                    MyNetwork.SendPCAction(item.IP, PCActions.ShutdownPC, true);
                }
            }
        }

        private void OnUpdateCheck(object sender, RoutedEventArgs e)
        {
            PCItem? item = GetItem((ICommandSource)sender);
            if (item != null && item.IP != null)
            {
                if (item.IP.ToString() == IPAddress.Any.ToString())
                {
                    foreach (var pcItem in ConnectedPCs)
                    {
                        if (pcItem.IP != null)
                        {
                            if (pcItem.IP.ToString() != IPAddress.Any.ToString() && pcItem.IP.ToString() != IPAddress.None.ToString())
                            {
                                MyNetwork.SendPCAction(item.IP, PCActions.CheckForUpdate, true);
                            }
                        }
                    }
                }
                else
                {
                    MyNetwork.SendPCAction(item.IP, PCActions.CheckForUpdate, true);
                }
            }
        }

        private void OnRemoteInstallMod(object sender, RoutedEventArgs e)
        {
            //Can't work because mod not available here.  Would need to prompt for mod.
            PCItem? item = GetItem((ICommandSource)sender);
            if (item != null && item.IP != null)
            {
                if (item.IP.ToString() == IPAddress.Any.ToString())
                {
                    foreach (var pcItem in ConnectedPCs)
                    {
                        if (pcItem.IP != null)
                        {
                            if (pcItem.IP.ToString() != IPAddress.Any.ToString() && pcItem.IP.ToString() != IPAddress.None.ToString())
                            {
                               //MyNetwork.SendArtemisAction(pcItem.IP, AMCommunicator.Messages.ArtemisActions.InstallMod, )
                            }
                        }
                    }
                }
                else
                {
                    
                }
            }
        }

        private void OnStartArtemisRemote(object sender, RoutedEventArgs e)
        {
            PCItem? item = GetItem((ICommandSource)sender);
            if (item != null && item.IP != null)
            {
                if (item.IP.ToString() == IPAddress.Any.ToString())
                {
                    foreach (var pcItem in ConnectedPCs)
                    {
                        if (pcItem.IP != null)
                        {
                            if (pcItem.IP.ToString() != IPAddress.Any.ToString())
                            {
                                MyNetwork.SendArtemisAction(pcItem.IP, AMCommunicator.Messages.ArtemisActions.StartArtemis, Guid.Empty, string.Empty);
                            }
                        }
                    }
                }
                else
                {
                    MyNetwork.SendArtemisAction(item.IP, AMCommunicator.Messages.ArtemisActions.StartArtemis, Guid.Empty, string.Empty);
                }
            }
        }

        private void OnStopArtemisRemote(object sender, RoutedEventArgs e)
        {
            PCItem? item = GetItem((ICommandSource)sender);
            if (item != null && item.IP != null)
            {
                if (item.IP.ToString() == IPAddress.Any.ToString())
                {
                    foreach (var pcItem in ConnectedPCs)
                    {
                        if (pcItem.IP != null)
                        {
                            if (pcItem.IP.ToString() != IPAddress.Any.ToString())
                            {
                                MyNetwork.SendArtemisAction(pcItem.IP, AMCommunicator.Messages.ArtemisActions.StopArtemis, Guid.Empty, string.Empty);
                            }
                        }
                    }
                }
                else
                {
                    MyNetwork.SendArtemisAction(item.IP, AMCommunicator.Messages.ArtemisActions.StopArtemis, Guid.Empty, string.Empty);
                }
            }
        }

        private void OnOpenArtemisRunFolder(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer " + ArtemisManagerAction.ModItem.ActivatedFolder);
        }

        private void OnGenerateMod(object sender, RoutedEventArgs e)
        {
            ModInstallWindow win = new();
            win.ForInstall = false;
            if (win.ShowDialog() == true)
            {
                InstalledMods.Add(win.Mod);
            }
        }

        private void OnLocalUpdateCheck(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Checking for update...");
            Task.Run( async () =>
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
    }
}