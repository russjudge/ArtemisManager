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
    /// Interaction logic for ArtemisINISettingControl.xaml
    /// </summary>
    public partial class ArtemisINISettingControl : UserControl
    {
        public ArtemisINISettingControl()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty ForLocalSettingsProperty =
            DependencyProperty.Register(nameof(ForLocalSettings), typeof(bool),
            typeof(ArtemisINISettingControl));

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

        public static readonly DependencyProperty SettingProperty =
           DependencyProperty.Register(nameof(Setting), typeof(ArtemisINISetting),
           typeof(ArtemisINISettingControl));

        public ArtemisINISetting Setting
        {
            get
            {
                return (ArtemisINISetting)this.GetValue(SettingProperty);

            }
            set
            {
                this.SetValue(SettingProperty, value);
            }
        }


        public static readonly DependencyProperty IsStringProperty =
           DependencyProperty.Register(nameof(IsString), typeof(bool),
           typeof(ArtemisINISettingControl));

        public bool IsString
        {
            get
            {
                return (bool)this.GetValue(IsStringProperty);

            }
            set
            {
                this.SetValue(IsStringProperty, value);
            }
        }

        public static readonly DependencyProperty IsIntProperty =
           DependencyProperty.Register(nameof(IsInt), typeof(bool),
           typeof(ArtemisINISettingControl));

        public bool IsInt
        {
            get
            {
                return (bool)this.GetValue(IsIntProperty);

            }
            set
            {
                this.SetValue(IsIntProperty, value);
            }
        }
        public static readonly DependencyProperty IntMaxValueProperty =
          DependencyProperty.Register(nameof(IntMaxValue), typeof(int),
          typeof(ArtemisINISettingControl), new PropertyMetadata(int.MaxValue));

        public int IntMaxValue
        {
            get
            {
                return (int)this.GetValue(IntMaxValueProperty);

            }
            set
            {
                this.SetValue(IntMaxValueProperty, value);
            }
        }
        public static readonly DependencyProperty IntMinValueProperty =
          DependencyProperty.Register(nameof(IntMinValue), typeof(int),
          typeof(ArtemisINISettingControl), new PropertyMetadata(int.MinValue));

        public int IntMinValue
        {
            get
            {
                return (int)this.GetValue(IntMinValueProperty);

            }
            set
            {
                this.SetValue(IntMinValueProperty, value);
            }
        }

        public static readonly DependencyProperty IsDoubleProperty =
           DependencyProperty.Register(nameof(IsDouble), typeof(bool),
           typeof(ArtemisINISettingControl));

        public bool IsDouble
        {
            get
            {
                return (bool)this.GetValue(IsDoubleProperty);

            }
            set
            {
                this.SetValue(IsDoubleProperty, value);
            }
        }

        public static readonly DependencyProperty IsBoolProperty =
           DependencyProperty.Register(nameof(IsBool), typeof(bool),
           typeof(ArtemisINISettingControl));

        public bool IsBool
        {
            get
            {
                return (bool)this.GetValue(IsBoolProperty);

            }
            set
            {
                this.SetValue(IsBoolProperty, value);
            }
        }
    }
}
