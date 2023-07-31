using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// Interaction logic for TestClientManagerWindow.xaml
    /// </summary>
    public partial class TestClientManagerWindow : Window
    {
        public TestClientManagerWindow()
        {
            SourcePC = new PCItem("this", IPAddress.Loopback);
            InitializeComponent();
        }
        public static readonly DependencyProperty SourcePCProperty =
       DependencyProperty.Register(nameof(SourcePC), typeof(PCItem),
       typeof(TestClientManagerWindow));

        public PCItem SourcePC
        {
            get
            {
                return (PCItem)this.GetValue(SourcePCProperty);
            }
            set
            {
                this.SetValue(SourcePCProperty, value);
            }
        }
    }
}
