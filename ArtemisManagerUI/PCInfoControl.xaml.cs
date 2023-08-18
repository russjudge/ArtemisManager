using AMCommunicator;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
    /// Interaction logic for PCInfoControl.xaml
    /// </summary>
    public partial class PCInfoControl : UserControl
    {
        public PCInfoControl()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty SelectedTargetPCProperty =
        DependencyProperty.Register(nameof(SelectedTargetPC), typeof(PCItem),
            typeof(PCInfoControl));

        public PCItem SelectedTargetPC
        {
            get
            {
                return (PCItem)this.GetValue(SelectedTargetPCProperty);

            }
            set
            {
                this.SetValue(SelectedTargetPCProperty, value);
            }
        }

        public static readonly DependencyProperty IsMasterProperty =
           DependencyProperty.Register(nameof(IsMaster), typeof(bool),
          typeof(PCInfoControl));

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

        private void OnSetConnectOnStart(object sender, RoutedEventArgs e)
        {
            TakeAction.SendChangeSetting(SelectedTargetPC?.IP, "ConnectOnStart", SelectedTargetPC?.ConnectOnstart?.ToString());
        }


        private void OnIsMasterSet(object sender, RoutedEventArgs e)
        {
            TakeAction.SendChangeSetting(SelectedTargetPC?.IP, "IsMaster", SelectedTargetPC?.ConnectOnstart?.ToString());
        }

        private void OnStartArtemisRemote(object sender, RoutedEventArgs e)
        {
            TakeAction.SendArtemisAction(SelectedTargetPC?.IP, AMCommunicator.Messages.ArtemisActions.StartArtemis, Guid.Empty, string.Empty);
        }

        private void OnStopArtemisRemote(object sender, RoutedEventArgs e)
        {
            TakeAction.SendArtemisAction(SelectedTargetPC?.IP, AMCommunicator.Messages.ArtemisActions.StopArtemis, Guid.Empty, string.Empty);
        }

        private void OnDisconnect(object sender, RoutedEventArgs e)
        {
            TakeAction.SendPCAction(SelectedTargetPC?.IP, PCActions.DisconnectThisConnection);
        }

        private void OnCloseApp(object sender, RoutedEventArgs e)
        {
            TakeAction.SendPCAction(SelectedTargetPC?.IP, PCActions.CloseApp);
        }

        private void OnCloseAndRestartApp(object sender, RoutedEventArgs e)
        {
            TakeAction.SendPCAction(SelectedTargetPC?.IP, PCActions.RestartApp);
        }

        private void OnPing(object sender, RoutedEventArgs e)
        {
            TakeAction.SendPing(SelectedTargetPC?.IP);
        }

        private void OnRestart(object sender, RoutedEventArgs e)
        {
            TakeAction.SendPCAction(SelectedTargetPC?.IP, PCActions.RestartPC);
        }

        private void OnShutdown(object sender, RoutedEventArgs e)
        {
            TakeAction.SendPCAction(SelectedTargetPC?.IP, PCActions.ShutdownPC);
        }

        private void OnUpdateCheck(object sender, RoutedEventArgs e)
        {
            TakeAction.SendPCAction(SelectedTargetPC?.IP, PCActions.CheckForUpdate);
        }


        private void OnSetManagerInStartup(object sender, RoutedEventArgs e)
        {
            if (SelectedTargetPC?.AppInStartFolder == true)
            {
                TakeAction.SendPCAction(SelectedTargetPC?.IP, PCActions.AddAppToStartup);
            }
            else
            {
                TakeAction.SendPCAction(SelectedTargetPC?.IP, PCActions.RemoveAppFromStartup);
            }
        }

        private void OnSetIsMainScreenServer(object sender, RoutedEventArgs e)
        {
            if (SelectedTargetPC?.IsMainScreenServer == true)
            {
                TakeAction.SendPCAction(SelectedTargetPC?.IP, PCActions.SetAsMainScreenServer);
            }
            else
            {
                TakeAction.SendPCAction(SelectedTargetPC?.IP, PCActions.RemoveAsMainScreenServer);
            }
        }
    }
}
