using AMCommunicator;
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
    /// Interaction logic for DriveInfoControl.xaml
    /// </summary>
    public partial class DriveInfoControl : UserControl
    {
        public DriveInfoControl()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty DriveProperty =
           DependencyProperty.Register(nameof(Drive), typeof(DriveData),
          typeof(DriveInfoControl));

        public DriveData Drive
        {
            get
            {
                return (DriveData)this.GetValue(DriveProperty);

            }
            set
            {
                this.SetValue(DriveProperty, value);
            }
        }
    }
}
