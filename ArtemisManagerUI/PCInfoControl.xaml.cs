using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for PCInfoControl.xaml
    /// </summary>
    public partial class PCInfoControl : UserControl
    {
        public PCInfoControl()
        {
            InitializeComponent();
        }
        //public static readonly DependencyProperty ConnectedPCsProperty =
        //  DependencyProperty.Register(nameof(ConnectedPCs), typeof(ObservableCollection<PCItem>),
        //      typeof(PCInfoControl));

        //public ObservableCollection<PCItem> ConnectedPCs
        //{
        //    get
        //    {
        //        return (ObservableCollection<PCItem>)this.GetValue(ConnectedPCsProperty);

        //    }
        //    set
        //    {
        //        this.SetValue(ConnectedPCsProperty, value);

        //    }
        //}
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

        }

        private void OnIsMasterSet(object sender, RoutedEventArgs e)
        {

        }

        private void OnRemoteAddManagerFromStartup(object sender, RoutedEventArgs e)
        {

        }

        private void OnRemoteRemoveManagerFromStartup(object sender, RoutedEventArgs e)
        {

        }

        private void OnStartArtemisRemote(object sender, RoutedEventArgs e)
        {

        }

        private void OnStopArtemisRemote(object sender, RoutedEventArgs e)
        {

        }

        private void OnDisconnect(object sender, RoutedEventArgs e)
        {

        }

        private void OnCloseApp(object sender, RoutedEventArgs e)
        {

        }

        private void OnCloseAndRestartApp(object sender, RoutedEventArgs e)
        {

        }

        private void OnPing(object sender, RoutedEventArgs e)
        {

        }

        private void OnRestart(object sender, RoutedEventArgs e)
        {

        }

        private void OnShutdown(object sender, RoutedEventArgs e)
        {

        }

        private void OnUpdateCheck(object sender, RoutedEventArgs e)
        {

        }
    }
}
