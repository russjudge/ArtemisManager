using ArtemisManagerAction;
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
    /// Interaction logic for ArtemisINIControl.xaml
    /// </summary>
    public partial class ArtemisINIControl : UserControl
    {
        public ArtemisINIControl()
        {
            ArtemisFolder = ModItem.ActivatedFolder;
            InitializeComponent();
        }
        public static readonly DependencyProperty ForLocalSettingsProperty =
         DependencyProperty.Register(nameof(ForLocalSettings), typeof(bool),
        typeof(ArtemisINIControl));

        public bool ForLocalSettings
        {
            get
            {
                return (bool)this.GetValue(ForLocalSettingsProperty);

            }
            set
            {
                this.SetValue(ForLocalSettingsProperty, value);
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
            ArtemisManager.SetActiveLocalArtemisINISettings(SettingsFile.SaveFile);
            PopupMessage = "Settings file activated.";
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

        private void OnSave(object sender, RoutedEventArgs e)
        {
            SettingsFile.Save();
        }
    }
}
