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
    /// Interaction logic for EngineeringPresetTabControl.xaml
    /// </summary>
    public partial class EngineeringPresetTabControl : UserControl
    {
        public EngineeringPresetTabControl()
        {
            if (TakeAction.ConnectedPCs != null)
            {
                ConnectedPCs = TakeAction.ConnectedPCs;
            }
            InitializeComponent();
        }
        public static readonly DependencyProperty PopupMessageProperty =
        DependencyProperty.Register(nameof(PopupMessage), typeof(string),
            typeof(EngineeringPresetTabControl));

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
        public static readonly DependencyProperty IsMasterProperty =
            DependencyProperty.Register(nameof(IsMaster), typeof(bool),
            typeof(EngineeringPresetTabControl));


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
        public static readonly DependencyProperty SourcePCProperty =
        DependencyProperty.Register(nameof(SourcePC), typeof(PCItem),
        typeof(EngineeringPresetTabControl));

        public PCItem SourcePC
        {
            get
            {
                return (PCItem)this.GetValue(SourcePCProperty);
            }
            set
            {
                this.SetValue(SourcePCProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedTargetPCProperty =
        DependencyProperty.Register(nameof(SelectedTargetPC), typeof(PCItem),
        typeof(EngineeringPresetTabControl));

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

        public static readonly DependencyProperty ConnectedPCsProperty =
          DependencyProperty.Register(nameof(ConnectedPCs), typeof(ObservableCollection<PCItem>),
          typeof(EngineeringPresetTabControl));

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
        private void OnSendFileRequest(object sender, FileRequestRoutedEventArgs e)
        {

        }

        private void OnTransmissionCompleted(object sender, RoutedEventArgs e)
        {

        }
    }
}
