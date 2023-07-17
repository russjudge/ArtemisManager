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
using System.Windows.Shapes;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for ModInstallWindow.xaml
    /// </summary>
    public partial class ModInstallWindow : Window
    {
        public ModInstallWindow()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty ModProperty =
           DependencyProperty.Register(nameof(Mod), typeof(ModItem),
           typeof(ModInstallWindow));
        
        public ModItem Mod
        {
            get
            {

                return (ModItem)GetValue(ModProperty);
            }
            set
            {
                this.SetValue(ModProperty, value);
            }
        }
        public static readonly DependencyProperty PackageFileProperty =
          DependencyProperty.Register(nameof(PackageFile), typeof(string),
          typeof(ModInstallWindow));

        public string PackageFile
        {
            get
            {

                return (string)GetValue(PackageFileProperty);
            }
            set
            {
                this.SetValue(PackageFileProperty, value);
            }
        }
        
        private void OnInstallMod(object sender, RoutedEventArgs e)
        {
            
            ModManager.InstallMod(PackageFile, Mod);
            this.DialogResult = true;
            this.Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            
            this.DialogResult = false;
            this.Close();
        }
    }
}
