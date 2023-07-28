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

        public static readonly DependencyProperty SettingsFileProperty =
          DependencyProperty.Register(nameof(SettingsFile), typeof(ArtemisINI),
         typeof(ArtemisINIControl));

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
        }
        public static readonly DependencyProperty AvailableResolutionsProperty =
          DependencyProperty.Register(nameof(AvailableResolutions), typeof(ObservableCollection<Size>),
           typeof(ArtemisINIControl));

        public ObservableCollection<Size> AvailableResolutions
        {
            get
            {
                return (ObservableCollection<Size>)this.GetValue(AvailableResolutionsProperty);

            }
            set
            {
                this.SetValue(AvailableResolutionsProperty, value);

            }
        }
    }
}
