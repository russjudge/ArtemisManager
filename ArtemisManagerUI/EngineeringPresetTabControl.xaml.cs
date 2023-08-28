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
            ConnectedPCs = new();
            if (TakeAction.ConnectedPCs != null)
            {
                foreach (var PC in TakeAction.ConnectedPCs)
                {
                    if (PC.IP != null && !TakeAction.IsBroadcast(PC.IP))
                    {
                        ConnectedPCs.Add(new(PC));
                    }

                }
            }
            TakeAction.ConnectionAdded += TakeAction_ConnectionAdded;
            TakeAction.ConnectionRemoved += TakeAction_ConnectionRemoved;
            if (TakeAction.SourcePC != null)
            {
                SourcePC = TakeAction.SourcePC;
            }
           
            InitializeComponent();
        }

        private void TakeAction_ConnectionRemoved(object? sender, ConnectionEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                EngineeringPresetsPCItem? remover = null;
                foreach (var item in ConnectedPCs)
                {
                    if (item.Connection?.IP?.ToString() == e.Connection?.IP?.ToString())
                    {
                        remover = item;
                        break;
                    }
                }
                if (remover != null)
                {
                    ConnectedPCs.Remove(remover);
                }
            });
        }

        private void TakeAction_ConnectionAdded(object? sender, ConnectionEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ConnectedPCs.Add(new(e.Connection));
            });
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
        DependencyProperty.Register(nameof(SelectedTargetPC), typeof(EngineeringPresetsPCItem),
        typeof(EngineeringPresetTabControl));

        public EngineeringPresetsPCItem SelectedTargetPC
        {
            get
            {
                return (EngineeringPresetsPCItem)this.GetValue(SelectedTargetPCProperty);
            }
            set
            {
                this.SetValue(SelectedTargetPCProperty, value);
            }
        }

        public static readonly DependencyProperty ConnectedPCsProperty =
          DependencyProperty.Register(nameof(ConnectedPCs), typeof(ObservableCollection<EngineeringPresetsPCItem>),
          typeof(EngineeringPresetTabControl));

        public ObservableCollection<EngineeringPresetsPCItem> ConnectedPCs
        {
            get
            {
                return (ObservableCollection<EngineeringPresetsPCItem>)this.GetValue(ConnectedPCsProperty);
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
