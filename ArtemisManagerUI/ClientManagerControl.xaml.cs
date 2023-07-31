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
    /// Interaction logic for ClientManagerControl.xaml
    /// </summary>
    public partial class ClientManagerControl : UserControl
    {
        public ClientManagerControl()
        {
            if (TakeAction.ConnectedPCs != null)
            {
                ConnectedPCs = TakeAction.ConnectedPCs;
            }
            InitializeComponent();
        }

        public static readonly DependencyProperty ChildContentProperty =
           DependencyProperty.Register("ChildContent",
               typeof(UIElement), typeof(ClientManagerControl),
               new PropertyMetadata(null));

        public UIElement ChildContent
        {
            get { return (UIElement)GetValue(ChildContentProperty); }
            set { SetValue(ChildContentProperty, value); }
        }


        public static readonly DependencyProperty SourcePCProperty =
        DependencyProperty.Register(nameof(SourcePC), typeof(PCItem),
        typeof(ClientManagerControl));

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
        typeof(ClientManagerControl));

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
          typeof(ClientManagerControl));

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
