using ArtemisManagerAction;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for NumericTextBox.xaml
    /// </summary>
    public partial class NumericDoubleTextBox : UserControl
    {
        //[GeneratedRegex("[^0-9.-]+")]
        //private static Regex MyRegex = MyRegex1();
        public NumericDoubleTextBox()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double),
            typeof(NumericDoubleTextBox), new PropertyMetadata(OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericDoubleTextBox me)
            {
                if (me.Value > me.MaxValue)
                {
                    me.Value= me.MaxValue;
                }
                if (me.Value< me.MinValue)
                {
                    me.Value= me.MinValue;
                }
            }
        }

        public double Value
        {
            get
            {
                return (double)this.GetValue(ValueProperty);

            }
            set
            {
                this.SetValue(ValueProperty, value);
            }
        }
        public static readonly DependencyProperty MaxValueProperty =
           DependencyProperty.Register(nameof(MaxValue), typeof(double),
           typeof(NumericDoubleTextBox), new PropertyMetadata(double.MaxValue));

        public double MaxValue
        {
            get
            {
                return (double)this.GetValue(MaxValueProperty);

            }
            set
            {
                this.SetValue(MaxValueProperty, value);
            }
        }
        public static readonly DependencyProperty MinValueProperty =
           DependencyProperty.Register(nameof(MinValue), typeof(double),
           typeof(NumericDoubleTextBox), new PropertyMetadata(double.MinValue));

        public double MinValue
        {
            get
            {
                return (double)this.GetValue(MinValueProperty);

            }
            set
            {
                this.SetValue(MinValueProperty, value);
            }
        }
       

        private void OnPreviewPortInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = MyRegex();
            e.Handled = regex.IsMatch(e.Text);
        }

        [GeneratedRegex("[^0-9.-];+")]
        private static partial Regex MyRegex();
    }
}
