using AMCommunicator;
using ArtemisManagerAction;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for PeerInfoControl.xaml
    /// </summary>
    public partial class PeerInfoControl : UserControl
    {
        public PeerInfoControl()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty ConnectedPCsProperty =
          DependencyProperty.Register(nameof(ConnectedPCs), typeof(ObservableCollection<PCItem>),
              typeof(PeerInfoControl));

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
        public static readonly DependencyProperty ItemDataProperty =
        DependencyProperty.Register(nameof(ItemData), typeof(PCItem),
            typeof(PeerInfoControl));

        public PCItem ItemData
        {
            get
            {
                return (PCItem)this.GetValue(ItemDataProperty);

            }
            set
            {
                this.SetValue(ItemDataProperty, value);
            }
        }
        public static readonly DependencyProperty ChatMessageProperty =
            DependencyProperty.Register(nameof(ChatMessage), typeof(string),
           typeof(PeerInfoControl));

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

        private void OnDeactivateMods(object sender, RoutedEventArgs e)
        {
            if (ItemData?.IP != null)
            {
                AMCommunicator.Network.Current?.SendArtemisAction(ItemData.IP, AMCommunicator.Messages.ArtemisActions.ResetToVanilla, Guid.Empty, string.Empty);
            }
        }

        private void OnRemoteRemoveManagerFromStartup(object sender, RoutedEventArgs e)
        {
            if (ItemData?.IP != null)
            {
                AMCommunicator.Network.Current?.SendPCAction(ItemData.IP, PCActions.RemoveAppFromStartup, true);
            }
        }

        private void OnRemoteAddManagerFromStartup(object sender, RoutedEventArgs e)
        {
            if (ItemData?.IP != null)
            {
                AMCommunicator.Network.Current?.SendPCAction(ItemData.IP, PCActions.AddAppToStartup, true);
            }
        }

       

        private void OnSetConnectOnStart(object sender, RoutedEventArgs e)
        {
            if (ItemData?.IP != null)
            {
                AMCommunicator.Network.Current?.SendChangeSetting(ItemData.IP, "ConnectOnStart", this.ItemData.ConnectOnstart.ToString());
                
            }
        }

        void RaiseSendMessageEvent()
        {
            RoutedEventArgs args = new RoutedEventArgs(SendMessageEvent);
            RaiseEvent(args);
        }

        public static readonly RoutedEvent SendMessageEvent = EventManager.RegisterRoutedEvent(
            name: nameof(SendMessage),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(PeerInfoControl));
        public event RoutedEventHandler SendMessage
        {
            add { AddHandler(SendMessageEvent, value); }
            remove {  RemoveHandler(SendMessageEvent, value); }
        }

        private void OnSendMessage(object sender, RoutedEventArgs e)
        {
            RaiseSendMessageEvent();
        }

        private void OnDisconnect(object sender, RoutedEventArgs e)
        {
            PCItem? item = ItemData;
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
                                AMCommunicator.Network.Current?.Halt(pcItem.IP);
                            }
                        }
                    }
                }
                else
                {

                    AMCommunicator.Network.Current?.Halt(item.IP);
                }
            }
        }

        private void OnPing(object sender, RoutedEventArgs e)
        {
            PCItem? item = ItemData;
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
                                AMCommunicator.Network.Current?.SendPing(pcItem.IP);
                            }
                        }
                    }
                }
                else
                {
                    AMCommunicator.Network.Current?.SendPing(item.IP);
                }
            }
        }

        private void OnRestart(object sender, RoutedEventArgs e)
        {
            PCItem? item = ItemData;
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
                                AMCommunicator.Network.Current?.SendPCAction(item.IP, PCActions.RestartPC, true);
                            }
                        }
                    }
                }
                else
                {
                    AMCommunicator.Network.Current?.SendPCAction(item.IP, PCActions.RestartPC, true);
                }
            }
        }

        private void OnShutdown(object sender, RoutedEventArgs e)
        {
            PCItem? item = ItemData;
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
                                AMCommunicator.Network.Current?.SendPCAction(item.IP, PCActions.ShutdownPC, true);
                            }
                        }
                    }
                }
                else
                {
                    AMCommunicator.Network.Current?.SendPCAction(item.IP, PCActions.ShutdownPC, true);
                }
            }
        }
        private void OnUpdateCheck(object sender, RoutedEventArgs e)
        {
            PCItem? item = ItemData;
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
                                AMCommunicator.Network.Current?.SendPCAction(item.IP, PCActions.CheckForUpdate, true);
                            }
                        }
                    }
                }
                else
                {
                    AMCommunicator.Network.Current?.SendPCAction(item.IP, PCActions.CheckForUpdate, true);
                }
            }
        }
        private void OnCloseApp(object sender, RoutedEventArgs e)
        {
            PCItem? item = ItemData;
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
                                AMCommunicator.Network.Current?.SendPCAction(item.IP, PCActions.CloseApp, true);
                            }
                        }
                    }
                }
                else
                {
                    AMCommunicator.Network.Current?.SendPCAction(item.IP, PCActions.CloseApp, true);
                }
            }
        }

        private void OnCloseAndRestartApp(object sender, RoutedEventArgs e)
        {
            PCItem? item = ItemData;
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
                                AMCommunicator.Network.Current?.SendPCAction(item.IP, PCActions.RestartApp, true);
                            }
                        }
                    }
                }
                else
                {
                    AMCommunicator.Network.Current?.SendPCAction(item.IP, PCActions.RestartApp, true);
                }
            }
        }
        private void OnRemoteInstallMod(object sender, RoutedEventArgs e)
        {
            //Can't work because mod not available here.  Would need to prompt for mod.
            PCItem? item = ItemData;
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
            PCItem? item =ItemData;
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
                                AMCommunicator.Network.Current?.SendArtemisAction(pcItem.IP, AMCommunicator.Messages.ArtemisActions.StartArtemis, Guid.Empty, string.Empty);
                            }
                        }
                    }
                }
                else
                {
                    AMCommunicator.Network.Current?.SendArtemisAction(item.IP, AMCommunicator.Messages.ArtemisActions.StartArtemis, Guid.Empty, string.Empty);
                }
            }
        }

        private void OnStopArtemisRemote(object sender, RoutedEventArgs e)
        {
            PCItem? item = ItemData;
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
                                AMCommunicator.Network.Current?.SendArtemisAction(pcItem.IP, AMCommunicator.Messages.ArtemisActions.StopArtemis, Guid.Empty, string.Empty);
                            }
                        }
                    }
                }
                else
                {
                    AMCommunicator.Network.Current?.SendArtemisAction(item.IP, AMCommunicator.Messages.ArtemisActions.StopArtemis, Guid.Empty, string.Empty);
                }
            }
        }

        bool isDragging = false;
        private void OnModDragEnter(object sender, DragEventArgs e)
        {
            if (sender is ListBox me)
            {
                if (e.Data.GetDataPresent(typeof(ModItemControl)))
                {
                    isDragging = true;
                }
            }
        }

        private void OnModDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(typeof(ModItemControl)))
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
            if (sender is ListBox me)
            {
                if (e.Data.GetDataPresent(typeof(ModItemControl)))
                {
                    PCItem? item = ItemData;

                    var ctl = (ModItemControl)e.Data.GetData(typeof(ModItemControl));
                    if (!ctl.IsRemote && item != null && item.IP != null)
                    {
                        isDragging = false;
                        

                        if (item.IP.ToString() == IPAddress.Any.ToString())
                        {
                            foreach (var pcItem in ConnectedPCs)
                            {
                                if (pcItem.IP != null)
                                {
                                    if (pcItem.IP.ToString() != IPAddress.Any.ToString())
                                    {
                                        TakeAction.FulfillModPackageRequest(pcItem.IP, ctl.Mod.PackageFile, ctl.Mod);
                                    }
                                }
                            }
                        }
                        else
                        {
                            TakeAction.FulfillModPackageRequest(item.IP, ctl.Mod.PackageFile, ctl.Mod);
                        }
                    }
                }
            }
        }
    }
}
