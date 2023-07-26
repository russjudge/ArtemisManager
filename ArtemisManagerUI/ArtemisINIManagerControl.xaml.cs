using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    /// Interaction logic for ArtemisINIManagerControl.xaml
    /// </summary>
    public partial class ArtemisINIManagerControl : UserControl
    {
        public ArtemisINIManagerControl()
        {
            InitializeComponent();
        }

        FileSystemWatcher? fsw = null;
        public static readonly DependencyProperty IsRemoteProperty =
         DependencyProperty.Register(nameof(IsRemote), typeof(bool),
             typeof(ArtemisINIManagerControl), new PropertyMetadata(OnIsRemoteChanged));

        private static void OnIsRemoteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArtemisINIManagerControl control)
            {
                if (control.IsRemote)
                {
                    if (control.fsw != null)
                    {
                        control.fsw.EnableRaisingEvents = false;
                        control.fsw.Created -= control.OnINIFileCreated;
                        control.fsw.Deleted -= control.OnINIFileDeleted;
                        control.fsw.Dispose();
                        control.fsw = null;
                    }
                }
                else
                {
                    control.fsw = new FileSystemWatcher(ArtemisManager.ArtemisINIFolder);
                    control.fsw.Created += control.OnINIFileCreated;
                    control.fsw.Deleted += control.OnINIFileDeleted;
                    control.fsw.EnableRaisingEvents = true;
                }
            }
        }

        private void OnINIFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Name) && e.Name.EndsWith(ArtemisManager.INIFileExtension))
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (ArtemisSettingsFiles.Contains(e.Name))
                    {
                        ArtemisSettingsFiles.Remove(e.Name);
                    }
                });
            }
        }

        private void OnINIFileCreated(object sender, FileSystemEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Name) && e.Name.EndsWith(ArtemisManager.INIFileExtension))
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (!ArtemisSettingsFiles.Contains(e.Name))
                    {
                        ArtemisSettingsFiles.Add(e.Name);
                    }
                });
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


        public static readonly DependencyProperty PopupMessageProperty =
         DependencyProperty.Register(nameof(PopupMessage), typeof(string),
             typeof(ArtemisINIManagerControl));

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


        public static readonly DependencyProperty ArtemisSettingsFilesProperty =
            DependencyProperty.Register(nameof(ArtemisSettingsFiles), typeof(ObservableCollection<string>),
                typeof(ArtemisINIManagerControl));

        public ObservableCollection<string> ArtemisSettingsFiles
        {
            get
            {
                return (ObservableCollection<string>)this.GetValue(ArtemisSettingsFilesProperty);

            }
            set
            {
                this.SetValue(ArtemisSettingsFilesProperty, value);

            }
        }
        public static readonly DependencyProperty SelectedArtemisSettingsFileProperty =
            DependencyProperty.Register(nameof(SelectedArtemisSettingsFile), typeof(string),
             typeof(ArtemisINIManagerControl));

        public string SelectedArtemisSettingsFile
        {
            get
            {
                return (string)this.GetValue(SelectedArtemisSettingsFileProperty);

            }
            set
            {
                this.SetValue(SelectedArtemisSettingsFileProperty, value);

            }
        }
        public static readonly DependencyProperty SelectedArtemisSettingsProperty =
            DependencyProperty.Register(nameof(SelectedArtemisSettings), typeof(ArtemisINI),
             typeof(ArtemisINIManagerControl));

        public ArtemisINI SelectedArtemisSettings
        {
            get
            {
                return (ArtemisINI)this.GetValue(SelectedArtemisSettingsProperty);

            }
            set
            {
                this.SetValue(SelectedArtemisSettingsProperty, value);

            }
        }

        private void OnAddSettingsFile(object sender, RoutedEventArgs e)
        {

        }

        private void OnRestoreToDefault(object sender, RoutedEventArgs e)
        {

        }

        private void OnSendSelectedFile(object sender, RoutedEventArgs e)
        {

        }

        private void OnImportFile(object sender, RoutedEventArgs e)
        {

        }

        private void OnExportFile(object sender, RoutedEventArgs e)
        {

        }
    }
}
