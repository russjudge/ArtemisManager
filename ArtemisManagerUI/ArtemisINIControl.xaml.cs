using ArtemisManagerAction;
using System;
using System.Collections.Generic;
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
    }
}
