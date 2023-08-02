using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for ArtemisINITabControl.xaml
    /// </summary>
    public partial class ArtemisINITabControl : UserControl
    {
        public ArtemisINITabControl()
        {
            
            ConnectedPCs = new();
            if (TakeAction.ConnectedPCs != null)
            {
                foreach (var PC in TakeAction.ConnectedPCs)
                {
                    if (PC.IP?.ToString() != TakeAction.AllConnections.ToString())
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
                ArtemisINIPCItem? remover = null;
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
             typeof(ArtemisINITabControl));

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
            typeof(ArtemisINITabControl));


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
        typeof(ArtemisINITabControl));

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
        DependencyProperty.Register(nameof(SelectedTargetPC), typeof(ArtemisINIPCItem),
        typeof(ArtemisINITabControl));

        public ArtemisINIPCItem SelectedTargetPC
        {
            get
            {
                return (ArtemisINIPCItem)this.GetValue(SelectedTargetPCProperty);
            }
            set
            {
                this.SetValue(SelectedTargetPCProperty, value);
            }
        }

        public static readonly DependencyProperty ConnectedPCsProperty =
          DependencyProperty.Register(nameof(ConnectedPCs), typeof(ObservableCollection<ArtemisINIPCItem>),
          typeof(ArtemisINITabControl));

        public ObservableCollection<ArtemisINIPCItem> ConnectedPCs
        {
            get
            {
                return (ObservableCollection<ArtemisINIPCItem>)this.GetValue(ConnectedPCsProperty);
            }
            set
            {
                this.SetValue(ConnectedPCsProperty, value);
            }
        }

        private void OnSendFileRequest(object sender, FileRequestRoutedEventArgs e)
        {
            if (sender is StringPackageSenderControl ctl)
            {
                if (ctl.Tag is ArtemisINIPCItem data)
                {
                    e.File = data.SelectedSettingsFile.SettingsFile;
                }
            }
        }

        private void OnTransmissionCompleted(object sender, RoutedEventArgs e)
        {
            PopupMessage = "Settings file transmitted.";
        }
    }
}
