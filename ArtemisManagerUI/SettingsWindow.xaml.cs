using AMCommunicator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        [GeneratedRegex("[^0-9.-]+")]
        private static partial Regex MyRegex();

        public SettingsWindow()
        {
            SettingsData = SettingsAction.Current;
            InitializeComponent();
            isLoading = false;
        }
        private bool isLoading = true;

       
        public static readonly DependencyProperty SettingsDataProperty =
          DependencyProperty.Register(nameof(SettingsData), typeof(SettingsAction),
         typeof(SettingsWindow));

        public SettingsAction SettingsData
        {
            get
            {
                return (SettingsAction)this.GetValue(SettingsDataProperty);

            }
            set
            {
                this.SetValue(SettingsDataProperty, value);
            }
        }

        private void OnPreviewPortInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = MyRegex();
            e.Handled = regex.IsMatch(e.Text);
        }

        private void OnEngineeringPresets(object sender, RoutedEventArgs e)
        {

        }
    }
}
