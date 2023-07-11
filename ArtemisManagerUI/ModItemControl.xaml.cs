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
    /// Interaction logic for ModItemControl.xaml
    /// </summary>
    public partial class ModItemControl : UserControl
    {
        public ModItemControl()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty ModProperty =
         DependencyProperty.Register(nameof(Mod), typeof(ModItem),
             typeof(ModItemControl));

        public ModItem Mod
        {
            get
            {
                return (ModItem)this.GetValue(ModProperty);

            }
            set
            {
                this.SetValue(ModProperty, value);
            }
        }
    }
}
