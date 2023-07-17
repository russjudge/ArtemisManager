using AMCommunicator;
using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private void OnDeactivateMods(object sender, RoutedEventArgs e)
        {

        }

        private void OnRemoteRemoveManagerFromStartup(object sender, RoutedEventArgs e)
        {
            if (ItemData?.IP != null)
            {
                Network.Current?.SendPCAction(ItemData.IP, PCActions.RemoveAppFromStartup, true);
            }
        }

        private void OnRemoteAddManagerFromStartup(object sender, RoutedEventArgs e)
        {
            if (ItemData?.IP != null)
            {
                Network.Current?.SendPCAction(ItemData.IP, PCActions.AddAppToStartup, true);
            }
        }

        private void OnConnectOnStart(object sender, RoutedEventArgs e)
        {
            if (ItemData?.IP != null)
            {
                Network.Current?.SendPCAction(ItemData.IP, PCActions.ConnectOnStartup, true);
            }
        }

        private void OnNotConnectOnStart(object sender, RoutedEventArgs e)
        {
            if (ItemData?.IP != null)
            {
                Network.Current?.SendPCAction(ItemData.IP, PCActions.NotConnectOnStartup, true);
            }
        }
    }
}
