using AMCommunicator;
using AMCommunicator.Messages;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for JsonPackageSenderControl.xaml
    /// </summary>
    public partial class StringPackageSenderControl : UserControl
    {
        public StringPackageSenderControl()
        {
            if (TakeAction.ConnectedPCs != null)
            {
                ConnectedPCs = TakeAction.ConnectedPCs;
            }
            SelectedTargetPC = ConnectedPCs[TakeAction.AllConnectionsElement];
            InitializeComponent();
        }
        public static readonly DependencyProperty IsForReceiveProperty =
           DependencyProperty.Register(nameof(IsForReceive), typeof(bool),
           typeof(StringPackageSenderControl));

       
        public bool IsForReceive
        {
            get
            {
                return (bool)this.GetValue(IsForReceiveProperty);
            }
            set
            {
                this.SetValue(IsForReceiveProperty, value);
            }
        }


        public static readonly DependencyProperty PromptProperty =
            DependencyProperty.Register(nameof(Prompt), typeof(string),
            typeof(StringPackageSenderControl));

        public string Prompt
        {
            get
            {
                return (string)this.GetValue(PromptProperty);
            }
            set
            {
                this.SetValue(PromptProperty, value);
            }
        }
       
        //ISendableStringFile
        public static readonly DependencyProperty SelectedFileProperty =
            DependencyProperty.Register(nameof(SelectedFile), typeof(ISendableStringFile),
            typeof(StringPackageSenderControl));

        public ISendableStringFile? SelectedFile
        {
            get
            {
                return (ISendableStringFile?)this.GetValue(SelectedFileProperty);
            }
            set
            {
                this.SetValue(SelectedFileProperty, value);
            }
        }


        public static readonly DependencyProperty SelectedTargetPCProperty =
        DependencyProperty.Register(nameof(SelectedTargetPC), typeof(PCItem),
        typeof(StringPackageSenderControl));

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

        public static readonly DependencyProperty TargetClientProperty =
        DependencyProperty.Register(nameof(TargetClient), typeof(IPAddress),
        typeof(StringPackageSenderControl));

        public IPAddress? TargetClient
        {
            get
            {
                return (IPAddress?)this.GetValue(TargetClientProperty);
            }
            set
            {
                this.SetValue(TargetClientProperty, value);
            }
        }



        public static readonly DependencyProperty ConnectedPCsProperty =
          DependencyProperty.Register(nameof(ConnectedPCs), typeof(ObservableCollection<PCItem>),
          typeof(StringPackageSenderControl));

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


        public static readonly RoutedEvent TransmissionCompletedEvent = EventManager.RegisterRoutedEvent(
            name: nameof(TransmissionCompleted),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(StringPackageSenderControl));

        public event RoutedEventHandler TransmissionCompleted
        {
            add { AddHandler(TransmissionCompletedEvent, value); }
            remove { RemoveHandler(TransmissionCompletedEvent, value); }
        }



        public static readonly RoutedEvent SaveLocalEvent = EventManager.RegisterRoutedEvent(
            name: nameof(SaveLocal),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(StringPackageSenderControl));

        public event RoutedEventHandler SaveLocal
        {
            add { AddHandler(SaveLocalEvent, value); }
            remove { RemoveHandler(SaveLocalEvent, value); }
        }

        void DoSendPackage(IPAddress address, string data, SendableStringPackageFile fileType, string filename, string packageFile)
        {
            switch (fileType)
            {
                case SendableStringPackageFile.Mission:
                case SendableStringPackageFile.Mod:
                    if (TakeAction.IsLoopback(address))  //Nothing should happen as this should be an already installed local mod
                    {
                    }
                    else
                    {
                        TakeAction.FulfillModPackageRequest(address, packageFile, data);
                    }
                    break;
                default:
                    if (TakeAction.IsLoopback(address)) //Nothing should happen as this will be the source
                    {

                    }
                    else
                    {
                        Network.Current?.SendStringPackageFile(address, data, fileType, filename);
                    }
                    break;
            }
        }
        private void OnSendSelectedFile(object sender, RoutedEventArgs e)
        {
            /*
            FileRequestRoutedEventArgs eFileRequest = new(FileRequestEvent);
            RaiseEvent(eFileRequest);
            */
            if (SelectedFile != null && SelectedTargetPC != null && SelectedTargetPC.IP != null && !string.IsNullOrEmpty(SelectedFile.SaveFile))
            {
                if (SelectedTargetPC.IP.ToString() == IPAddress.Any.ToString())
                {
                    foreach (var pcItem in ConnectedPCs)
                    {
                        if (pcItem.IP != null && pcItem.IP.ToString() != IPAddress.Any.ToString())
                        {
                            DoSendPackage(pcItem.IP, SelectedFile.GetSerializedString(), SelectedFile.FileType, SelectedFile.SaveFile, SelectedFile.PackageFile);
                        }
                    }
                }
                else
                {
                    DoSendPackage(SelectedTargetPC.IP, SelectedFile.GetSerializedString(), SelectedFile.FileType, SelectedFile.SaveFile, SelectedFile.PackageFile);
                }
                RaiseEvent(new RoutedEventArgs(TransmissionCompletedEvent));
            }
        }

        private void OnReceiveSelectedFile(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null && !string.IsNullOrEmpty(SelectedFile.SaveFile))
            {
                RaiseEvent(new RoutedEventArgs(SaveLocalEvent));
            }
        }
    }
}
