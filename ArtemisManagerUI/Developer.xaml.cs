using AMCommunicator;
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
using System.Windows.Shapes;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for Developer.xaml
    /// </summary>
    public partial class Developer : Window
    {
        public Developer()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AMCommunicator.Network.OnNetworkDebug += OnNetwork;
        }
        void OnNetwork(object? sender, NetworkDebugEventArgs e)
        {

        }

    }
}
