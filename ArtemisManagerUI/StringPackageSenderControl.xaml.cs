﻿using AMCommunicator;
using ArtemisEngineeringPresets;
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
           typeof(StringPackageSenderControl), new PropertyMetadata(OnIsForReceiveChanged));

        private static void OnIsForReceiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StringPackageSenderControl me)
            {
                //if (me.IsForReceive)
                //{
                //    me.SelectedTargetPC = me.ConnectedPCs[0];
                //}
                //else
                //{
                //    me.SelectedTargetPC = me.ConnectedPCs[1];
                //}
            }
        }

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
        public static readonly DependencyProperty SourcePCProperty =
        DependencyProperty.Register(nameof(SourcePC), typeof(PCItem),
        typeof(StringPackageSenderControl));

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



        public static readonly RoutedEvent FileRequestEvent = EventManager.RegisterRoutedEvent(
            name: nameof(FileRequest),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(EventHandler<FileRequestRoutedEventArgs>),
            ownerType: typeof(StringPackageSenderControl));

        public event EventHandler<FileRequestRoutedEventArgs> FileRequest
        {
            add { AddHandler(FileRequestEvent, value); }
            remove { RemoveHandler(FileRequestEvent, value); }
        }


        private void OnSendSelectedFile(object sender, RoutedEventArgs e)
        {
            FileRequestRoutedEventArgs eFileRequest = new(FileRequestEvent);
            RaiseEvent(eFileRequest);
            if (eFileRequest.File != null && SelectedTargetPC != null && SelectedTargetPC.IP != null && !string.IsNullOrEmpty(eFileRequest.File.SaveFile))
            {
                if (SelectedTargetPC.IP.ToString() == IPAddress.Any.ToString())
                {
                    foreach (var pcItem in ConnectedPCs)
                    {
                        if (pcItem.IP != null && pcItem.IP.ToString() != IPAddress.Any.ToString())
                        {
                            Network.Current?.SendStringPackageFile(pcItem.IP, eFileRequest.File.GetSerializedString(), eFileRequest.File.FileType, eFileRequest.File.SaveFile);
                        }
                    }
                }
                else
                {
                    Network.Current?.SendStringPackageFile(SelectedTargetPC.IP, eFileRequest.File.GetSerializedString(), eFileRequest.File.FileType, eFileRequest.File.SaveFile);
                }
                RaiseEvent(new RoutedEventArgs(TransmissionCompletedEvent));
            }
        }

        private void OnReceiveSelectedFile(object sender, RoutedEventArgs e)
        {

        }
    }
}
