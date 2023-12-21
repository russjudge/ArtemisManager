using AMCommunicator;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : UserControl
    {
        public ChatControl()
        {
            Chat = [];
            InitializeComponent();
            if (Network.Current != null)
            {
                Network.Current.CommunicationMessageReceived += MyNetwork_CommunicationMessageReceived;
            }
        }

        public static readonly DependencyProperty ConnectedPCsProperty =
         DependencyProperty.Register(nameof(ConnectedPCs), typeof(ObservableCollection<PCItem>),
             typeof(ChatControl));

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
        public static readonly DependencyProperty SelectedTargetPCProperty =
        DependencyProperty.Register(nameof(SelectedTargetPC), typeof(PCItem),
            typeof(ChatControl));

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
        public static readonly DependencyProperty ChatMessageProperty =
           DependencyProperty.Register(nameof(ChatMessage), typeof(string),
          typeof(ChatControl));

        public string ChatMessage
        {
            get
            {
                return (string)this.GetValue(ChatMessageProperty);

            }
            set
            {
                this.SetValue(ChatMessageProperty, value);
            }
        }
        public static readonly DependencyProperty ChatsProperty =
         DependencyProperty.Register(nameof(Chat), typeof(ObservableCollection<ChatMessage>),
             typeof(ChatControl));

        public ObservableCollection<ChatMessage> Chat
        {
            get
            {
                return (ObservableCollection<ChatMessage>)this.GetValue(ChatsProperty);

            }
            set
            {
                this.SetValue(ChatsProperty, value);
            }
        }

        private void MyNetwork_CommunicationMessageReceived(object? sender, CommunicationMessageEventArgs e)
        {
            if (e.Host != null)
            {
                AddChatLine(e.Host, e.Message);
            }
        }
        public void AddChatLine(string source, string message)
        {
            if (this.Dispatcher != System.Windows.Threading.Dispatcher.CurrentDispatcher)
            {
                this.Dispatcher.Invoke(new Action<string, string>(AddChatLine), source, message);
            }
            else
            {
                Chat.Add(new ChatMessage(source.ToString(), message));
                RaiseChatReceivedEvent();
                //if (SelectedTabItem == null || SelectedTabItem.Tag?.ToString() != "Chat")
                //{
                //    ChatAlert = true;
                //}
            }
        }
        void RaiseChatReceivedEvent()
        {
            RaiseEvent(new RoutedEventArgs(ChatReceivedEvent));
        }

        public static readonly RoutedEvent ChatReceivedEvent = EventManager.RegisterRoutedEvent(
            name: nameof(ChatReceived),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(PresetSettingsControl));
        public event RoutedEventHandler ChatReceived
        {
            add { AddHandler(ChatReceivedEvent, value); }
            remove { RemoveHandler(ChatReceivedEvent, value); }
        }


        private void OnSendMessage(object sender, RoutedEventArgs e)
        {
            string? msg = ChatMessage;
            ChatMessage = string.Empty;

            if (SelectedTargetPC.IP?.ToString() == IPAddress.Any.ToString())
            {
                foreach (var pcItem in ConnectedPCs)
                {
                    if (pcItem.IP?.ToString() != IPAddress.Any.ToString() && msg != null)
                    {
                        if (pcItem.IP != null)
                        {
                            AddChatLine("LOCALHOST -> " + pcItem.Hostname, msg);
                            Network.Current?.SendMessage(pcItem.IP, msg);
                        }
                    }
                }
            }
            else
            {
                if (msg != null && SelectedTargetPC.IP != null)
                {
                    AddChatLine("LOCALHOST -> " + SelectedTargetPC.Hostname, msg);
                    Network.Current?.SendMessage(SelectedTargetPC.IP, msg);
                }
            }
        }
        bool hasLoaded = false;
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!hasLoaded)
            {
                try
                {
                    SelectedTargetPC = ConnectedPCs[TakeAction.AllConnectionsElement];
                    hasLoaded = true;
                }
                catch { }
            }
        }
    }
}
