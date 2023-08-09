using AMCommunicator.Messages;
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
    /// Interaction logic for TextFileTabControl.xaml
    /// </summary>
    public partial class TextFileTabControl : UserControl
    {
        public TextFileTabControl()
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
        public static readonly DependencyProperty FileTypeProperty =
           DependencyProperty.Register(nameof(FileType), typeof(SendableStringPackageFile),
           typeof(TextFileTabControl));


        public SendableStringPackageFile FileType
        {
            get
            {
                return (SendableStringPackageFile)this.GetValue(FileTypeProperty);

            }
            set
            {
                this.SetValue(FileTypeProperty, value);

            }
        }
        private void TakeAction_ConnectionRemoved(object? sender, ConnectionEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                TextDataPCItem? remover = null;
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
             typeof(TextFileTabControl));

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
            typeof(TextFileTabControl));


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
        typeof(TextFileTabControl));

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
        DependencyProperty.Register(nameof(SelectedTargetPC), typeof(TextDataPCItem),
        typeof(TextFileTabControl));

        public TextDataPCItem SelectedTargetPC
        {
            get
            {
                return (TextDataPCItem)this.GetValue(SelectedTargetPCProperty);
            }
            set
            {
                this.SetValue(SelectedTargetPCProperty, value);
            }
        }

        public static readonly DependencyProperty ConnectedPCsProperty =
          DependencyProperty.Register(nameof(ConnectedPCs), typeof(ObservableCollection<TextDataPCItem>),
          typeof(TextFileTabControl));

        public ObservableCollection<TextDataPCItem> ConnectedPCs
        {
            get
            {
                return (ObservableCollection<TextDataPCItem>)this.GetValue(ConnectedPCsProperty);
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
                if (ctl.Tag is TextDataPCItem data)
                {
                    //e.File = data.SelectedSettingsFile.SettingsFile;
                }
            }
        }

        private void OnTransmissionCompleted(object sender, RoutedEventArgs e)
        {
            PopupMessage = "Settings file transmitted.";
        }
    }
}
