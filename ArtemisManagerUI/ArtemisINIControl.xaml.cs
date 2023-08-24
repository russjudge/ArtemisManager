using AMCommunicator;
using ArtemisManagerAction;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for ArtemisINIControl.xaml
    /// </summary>
    public partial class ArtemisINIControl : UserControl
    {
        public ArtemisINIControl()
        {
            AvailableResolutions = new ObservableCollection<System.Drawing.Size>();
            ArtemisFolder = ModItem.ActivatedFolder;
            InitializeComponent();
            InitializeResolutions();
        }
        public static readonly DependencyProperty IsRemoteProperty =
            DependencyProperty.Register(nameof(IsRemote), typeof(bool),
            typeof(ArtemisINIControl), new PropertyMetadata(OnIsRemoteChanged));
        private void InitializeResolutions()
        {
            AvailableResolutions.Clear();
            if (IsRemote)
            {
                if (TargetClient != null)
                {
                    if (Network.Current != null)
                    {
                        Network.Current.InfoReceived += OnInfoReceived;
                    }
                    Network.Current?.SendRequestInformation(TargetClient, RequestInformationType.ListOfScreenResolutions);
                }
            }
            else
            {
                foreach (var resolution in TakeAction.GetAvailableScreenResolutions())
                {
                    AvailableResolutions.Add(resolution);
                }
            }
        }
        private static void OnIsRemoteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArtemisINIControl control)
            {
                control.InitializeResolutions();
            }
        }
        private void OnInfoReceived(object? sender, InformationEventArgs e)
        {
            if (!e.Handled)
            {
                Network.Current?.RaiseStatusUpdate("ArtemisINIControl InfoReceive processing...");
                Dispatcher.Invoke(() =>
                {
                    if (e.Source != null && TargetClient != null)
                    {
                        if (e.Source.ToString() == TargetClient.ToString())
                        {
                            switch (e.RequestType)
                            {
                                case RequestInformationType.ListOfScreenResolutions:
                                    foreach (var resolution in e.Data)
                                    {
                                        var sz = resolution.Split('x');
                                        if (sz.Length > 1)
                                        {
                                            int w = 0;
                                            int h = 0;
                                            if (int.TryParse(sz[0], out w))
                                            {
                                                if (int.TryParse(sz[1], out h))
                                                {
                                                    System.Drawing.Size size = new(w, h);
                                                    AvailableResolutions.Add(size);
                                                }
                                            }
                                        }
                                    }
                                    Network.Current?.RaiseStatusUpdate("ArtemisINIControl InfoReceive - ListOfScreenResolutions...");
                                    e.Handled = true;
                                    break;
                            }
                        }
                    }
                });
            }
            else
            {
                Network.Current?.RaiseStatusUpdate("ArtemisINIControl InfoReceive processing---already handled, nothing done.");
            }
        }

        public bool IsRemote
        {
            get
            {
                return (bool)this.GetValue(IsRemoteProperty);

            }
            set
            {
                this.SetValue(IsRemoteProperty, value);
            }
        }
        public static readonly DependencyProperty ArtemisFolderProperty =
         DependencyProperty.Register(nameof(ArtemisFolder), typeof(string),
        typeof(ArtemisINIControl));

        public string ArtemisFolder
        {
            get
            {
                return (string)this.GetValue(ArtemisFolderProperty);

            }
            set
            {
                this.SetValue(ArtemisFolderProperty, value);
            }
        }
        public static readonly DependencyProperty TargetClientProperty =
          DependencyProperty.Register(nameof(TargetClient), typeof(IPAddress),
          typeof(ArtemisINIControl));


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

        public static readonly DependencyProperty PopupMessageProperty =
          DependencyProperty.Register(nameof(PopupMessage), typeof(string),
         typeof(ArtemisINIControl));

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


        public static readonly DependencyProperty SettingsFileProperty =
          DependencyProperty.Register(nameof(SettingsFile), typeof(ArtemisINI),
         typeof(ArtemisINIControl), new PropertyMetadata(OnSettingsFileChanged));

        private static void OnSettingsFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArtemisINIControl me)
            {

                var x = me.SettingsFile;
            }
        }

        public ArtemisINI SettingsFile
        {
            get
            {
                return (ArtemisINI)this.GetValue(SettingsFileProperty);

            }
            set
            {
                this.SetValue(SettingsFileProperty, value);
            }
        }

        private void OnActivate(object sender, RoutedEventArgs e)
        {
            if (IsRemote)
            {
                if (TargetClient != null)
                {
                    Network.Current?.SendArtemisAction(TargetClient, AMCommunicator.Messages.ArtemisActions.ActivateArtemisINIFile, Guid.Empty, new System.IO.FileInfo(SettingsFile.SaveFile).Name);
                }
            }
            else
            {
                
                ArtemisManager.SetActiveLocalArtemisINISettings(SettingsFile.SaveFile);
                PopupMessage = "Settings file activated.";
            }
        }
        public static readonly DependencyProperty AvailableResolutionsProperty =
          DependencyProperty.Register(nameof(AvailableResolutions), typeof(ObservableCollection<System.Drawing.Size>),
           typeof(ArtemisINIControl));

        public ObservableCollection<System.Drawing.Size> AvailableResolutions
        {
            get
            {
                return (ObservableCollection<System.Drawing.Size>)this.GetValue(AvailableResolutionsProperty);

            }
            set
            {
                this.SetValue(AvailableResolutionsProperty, value);

            }
        }

        private void OnAllowOptionMouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        void RaiseSavingSettingsEvent()
        {
            RaiseEvent(new RoutedEventArgs(SavingSettingsEvent));
        }

        public static readonly RoutedEvent SavingSettingsEvent = EventManager.RegisterRoutedEvent(
            name: nameof(SavingSettings),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(PresetSettingsControl));
        public event RoutedEventHandler SavingSettings
        {
            add { AddHandler(SavingSettingsEvent, value); }
            remove { RemoveHandler(SavingSettingsEvent, value); }
        }
        void RaiseSettingsSavedEvent()
        {
            RaiseEvent(new RoutedEventArgs(SettingsSavedEvent));
        }

        public static readonly RoutedEvent SettingsSavedEvent = EventManager.RegisterRoutedEvent(
            name: nameof(SettingsSaved),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(PresetSettingsControl));
        public event RoutedEventHandler SettingsSaved
        {
            add { AddHandler(SettingsSavedEvent, value); }
            remove { RemoveHandler(SettingsSavedEvent, value); }
        }



        private void OnSave(object sender, RoutedEventArgs e)
        {
            if (IsRemote)
            {
                if (TargetClient != null)
                {
                    //Network.Current?.SendInformation(TargetClient, RequestInformationType.SaveSpecificArtemisINIFile, new System.IO.FileInfo(SettingsFile.SaveFile).Name, new string[] { SettingsFile.ToString() });
                    Network.Current?.SendStringPackageFile(TargetClient, SettingsFile.GetSerializedString(), AMCommunicator.Messages.SendableStringPackageFile.ArtemisINI, new System.IO.FileInfo(SettingsFile.SaveFile).Name);
                }
            }
            else
            {
                RaiseSavingSettingsEvent();
                SettingsFile.Save();
                PopupMessage = "Settings Saved.";
                RaiseSettingsSavedEvent();
            }
        }

        private void OnTest(object sender, RoutedEventArgs e)
        {

        }
    }
}
