using AMCommunicator;
using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
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
            Chat = new();
            MyNetwork = Network.GetNetwork("");
            Status = new ObservableCollection<string>();
            ConnectedPCs = new()
            {
                new PCItem("All Connections", IPAddress.Any)
            };
            this.InWindowsStartupFolder = TakeAction.IsThisAppInStartup();
            ArtemisInstallFolder = ArtemisManager.AutoDetectArtemisInstallPath();
            IsArtemisRunning = ArtemisManager.IsArtemisRunning();
            InitializeComponent();

        }
        bool isLoading = true;
        Network MyNetwork;
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
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
            
            Network.ConnectionPort = Properties.Settings.Default.ListeningPort;
            Network.Password = Properties.Settings.Default.NetworkPassword;
            Password = Network.Password;
            AutoStartServer = Properties.Settings.Default.ConnectOnStart;
            Port = Properties.Settings.Default.ListeningPort;

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

        public static readonly DependencyProperty AutoStartServerProperty =
         DependencyProperty.Register(nameof(AutoStartServer), typeof(bool),
             typeof(Main), new PropertyMetadata(OnAutoStartChanged));

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
                            Properties.Settings.Default.ConnectOnStart = me.AutoStartServer;
                            Properties.Settings.Default.Save();
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

        private static void OnPortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Main me = (Main)d;
            if (me != null)
            {
                if (!me.isLoading)
                {
                    if (!me.IsStarted || MessageBox.Show("Are you sure you wish to change the listening port for this application?\r\nThis will change the port on all connected computers and only on the connected computers.\r\n\r\nYou will need to restart the application for the new port to take effect.", "Change Listening Port", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Properties.Settings.Default.ListeningPort = (int)e.NewValue;
                        Properties.Settings.Default.Save();
                        Network.ConnectionPort = Properties.Settings.Default.ListeningPort;
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
                    Status.Add(DateTime.Now.ToString("HH:mm:ss") + ": "  + message);
                    StatusList.ScrollIntoView(Status.Count - 1);
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

            MyNetwork.ItemRequested += MyNetwork_ItemRequested;
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
            MyNetwork.Connect();
            IsStarted = true;
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
            MessageBox.Show("Alert Recieved: " + e.AlertItem.ToString() + "--" + e.RelatedData);
            //TODO:  Offer option of attempting remote update of client, with warning that if not updated, some functionality may not work.
        }

        private void MyNetwork_ChangeSetting(object? sender, ChangeSettingEventArgs e)
        {
            TakeAction.ChangeSetting(e.SettingName, e.SettingValue);
            switch (e.SettingName)
            {
                case "ConnectOnStart":
                    this.AutoStartServer = bool.Parse(e.SettingValue);
                    break;
                case "ListeningPort":
                    this.Port = int.Parse(e.SettingValue);
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

        private void MyNetwork_ActionCommand(object? sender, ActionCommandEventArgs e)
        {
            if (e.Action == ActionCommands.CloseApp)
            {
                this.Close();
                return;
            }
            else
            {
                if (!TakeAction.ProcessPCAction(e.Action, e.Force, e.Source))
                {
                    TakeArtemisAction.ProcessArtemisAction(e.Action);
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

        private void MyNetwork_ItemRequested(object? sender, ItemRequestEventArgs? e)
        {
            //throw new NotImplementedException();
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
    }
}
