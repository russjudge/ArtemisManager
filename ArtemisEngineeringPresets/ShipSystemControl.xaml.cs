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

namespace ArtemisEngineeringPresets
{
    /// <summary>
    /// Interaction logic for ShipSystemControl.xaml
    /// </summary>
    public partial class ShipSystemControl : UserControl
    {
        public ShipSystemControl()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty LevelsProperty =
            DependencyProperty.Register("Levels", typeof(SystemLevel),
            typeof(ShipSystemControl));

        public SystemLevel Levels
        {
            get
            {
                return (SystemLevel)this.GetValue(LevelsProperty);
            }
            set
            {
                this.SetValue(LevelsProperty, value);
            }
        }
    }
}
