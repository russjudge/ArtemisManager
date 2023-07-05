using AMCommunicator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        public Main()
        {
            MyNetwork = Network.GetNetwork("");
            Status = new ObservableCollection<string>();
            ConnectedPCs = new();
            ConnectedPCs.Add(new PCItem("All Connections", IPAddress.Any));
            Password = Network.Password;
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
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
            
            Network.ConnectionPort = Properties.Settings.Default.ListeningPort;
            Network.Password = Properties.Settings.Default.NetworkPassword;
            isLoading = false;
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



                    switch (MessageBox.Show("Update all connected Managers with new password?", "Update Managers", MessageBoxButton.YesNoCancel))
                    {
                        case MessageBoxResult.Yes:
                            Network.Password = me.Password;
                            Properties.Settings.Default.NetworkPassword = me.Password;
                            Properties.Settings.Default.Save();
                            //TODO: Send notice of update to all PCs
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
              typeof(Main), new PropertyMetadata(Properties.Settings.Default.ListeningPort, OnPortChanged));

        private static void OnPortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Properties.Settings.Default.ListeningPort = (int)e.NewValue;
            Properties.Settings.Default.Save();
            Network.ConnectionPort = Properties.Settings.Default.ListeningPort;
            //TODO: Change listener to use new connection port, if active.
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
                    Status.Add(message);
                    StatusList.ScrollIntoView(Status.Count - 1);
                }
            }
            catch (ThreadInterruptedException)
            {

            }
        }

        private void OnStartServer(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Starting Connection Service");
         
            MyNetwork.ItemRequested += MyNetwork_ItemRequested;
            MyNetwork.ConnectionRequested += MyNetwork_ConnectionRequested;
            MyNetwork.ConnectionReceived += MyNetwork_ConnectionReceived;
            MyNetwork.StatusUpdated += MyNetwork_StatusUpdated;
            MyNetwork.FatalExceptionEncountered += MyNetwork_FatalExceptionEncountered;
            MyNetwork.ConnectionClosed += MyNetwork_ConnectionClosed;
            MyNetwork.Connect();
        }

        private void MyNetwork_ConnectionClosed(object? sender, ConnectionRequestEventArgs e)
        {
            UpdateStatus(string.Format("Connection to: {0} - {1} CLOSED", e.Address.ToString(), e.Hostname));
            RemovePC(e.Address);
        }
        void RemovePC(IPAddress address)
        {
            if (this.Dispatcher != System.Windows.Threading.Dispatcher.CurrentDispatcher)
            {
                this.Dispatcher.Invoke(new Action<PCItem>(AddPC), address);
            }
            else
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
        }
        private void MyNetwork_FatalExceptionEncountered(object? sender, FatalExceptionEncounteredEventArgs e)
        {
            MessageBox.Show("Fatal exeception occurred: " + e.ToString());
        }

        private void MyNetwork_StatusUpdated(object? sender, StatusUpdate e)
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

        private void MyNetwork_ItemRequested(object? sender, ItemRequestEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void OnRebroadcast(object sender, RoutedEventArgs e)
        {
            MyNetwork.BroadcastMe();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            MyNetwork.HaltAll();
        }
    }
}
