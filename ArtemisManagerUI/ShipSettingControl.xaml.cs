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
    /// Interaction logic for VideoScreenSettingControl.xaml
    /// </summary>
    public partial class ShipSettingControl : UserControl
    {
        public ShipSettingControl()
        {
            AvailableShips =
            [
                new(Ship.None, Ship.None.ToString()),
                new(Ship.Artemis, Ship.Artemis.ToString()),
                new(Ship.Intrepid, Ship.Intrepid.ToString()),
                new(Ship.Aegis, Ship.Aegis.ToString()),
                new(Ship.Horatio, Ship.Horatio.ToString()),
                new(Ship.Excalibur, Ship.Excalibur.ToString()),
                new(Ship.Hera, Ship.Hera.ToString()),
                new(Ship.Ceres, Ship.Ceres.ToString()),
                new(Ship.Diana, Ship.Diana.ToString())
            ];
            AvailableResolutions = [];
            InitializeComponent();
        }
        public static readonly DependencyProperty SettingsFileProperty =
            DependencyProperty.Register(nameof(SettingsFile), typeof(ArtemisINI),
             typeof(ShipSettingControl));

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

        public static readonly DependencyProperty AvailableResolutionsProperty =
           DependencyProperty.Register(nameof(AvailableResolutions), typeof(ObservableCollection<Size>),
            typeof(ShipSettingControl));

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


        public static readonly DependencyProperty AvailableShipsProperty =
           DependencyProperty.Register(nameof(AvailableShips), typeof(ObservableCollection<KeyValuePair<Ship, string>>),
            typeof(ShipSettingControl));

        public ObservableCollection<KeyValuePair<Ship, string>> AvailableShips
        {
            get
            {
                return (ObservableCollection<KeyValuePair<Ship, string>>)this.GetValue(AvailableShipsProperty);

            }
            set
            {
                this.SetValue(AvailableShipsProperty, value);
            }
        }
    }
}
