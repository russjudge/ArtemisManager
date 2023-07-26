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
    public partial class NumericTextBox : UserControl
    {
        [GeneratedRegex("[^0-9.-]+")]
        private static partial Regex MyRegex();
        public NumericTextBox()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int),
            typeof(NumericTextBox));

        public int Value
        {
            get
            {
                return (int)this.GetValue(ValueProperty);

            }
            set
            {
                this.SetValue(ValueProperty, value);
            }
        }
        public static readonly DependencyProperty MaxValueProperty =
           DependencyProperty.Register(nameof(MaxValue), typeof(int),
           typeof(NumericTextBox), new PropertyMetadata(int.MaxValue));

        public int MaxValue
        {
            get
            {
                return (int)this.GetValue(MaxValueProperty);

            }
            set
            {
                this.SetValue(MaxValueProperty, value);
            }
        }
        public static readonly DependencyProperty MinValueProperty =
           DependencyProperty.Register(nameof(MinValue), typeof(int),
           typeof(NumericTextBox), new PropertyMetadata(int.MinValue));

        public int MinValue
        {
            get
            {
                return (int)this.GetValue(MinValueProperty);

            }
            set
            {
                this.SetValue(MinValueProperty, value);
            }
        }
        private void OnDown(object sender, RoutedEventArgs e)
        {
            if (Value > MinValue)
            {
                Value--;
            }
        }

        private void OnUp(object sender, RoutedEventArgs e)
        {
            if (Value < MaxValue)
            {
                Value++;
            }
        }

        private void OnPreviewPortInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = MyRegex();
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
