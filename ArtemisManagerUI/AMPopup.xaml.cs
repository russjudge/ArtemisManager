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
    /// Interaction logic for AMPopup.xaml
    /// </summary>
    public partial class AMPopup : UserControl
    {
        public AMPopup()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ShowPopupProperty =
         DependencyProperty.Register(nameof(ShowPopup), typeof(bool),
             typeof(AMPopup), new PropertyMetadata(OnShowPopup));

        private static void OnShowPopup(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AMPopup me)
            {
                if (me.ShowPopup)
                {
                    System.Timers.Timer timer = new()
                    {
                        Interval = 5000
                    };
                    timer.Elapsed += me.Timer_Elapsed;
                    timer.Start();
                }
            }
        }
        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                ShowPopup = false;
                PopupMessage = string.Empty;
            }));
            if (sender is System.Timers.Timer tmr)
            {
                tmr.Stop();
                tmr.Dispose();

            }
        }
        void DoPopup(string message)
        {
            PopupMessage = message;
            ShowPopup = true;
        }
        public bool ShowPopup
        {
            get
            {
                return (bool)this.GetValue(ShowPopupProperty);

            }
            set
            {
                this.SetValue(ShowPopupProperty, value);

            }
        }
        public static readonly DependencyProperty PopupMessageProperty =
         DependencyProperty.Register(nameof(PopupMessage), typeof(string),
             typeof(AMPopup), new PropertyMetadata(OnPopupMessageChanged));

        private static void OnPopupMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AMPopup me)
            {
                if (!string.IsNullOrEmpty(me.PopupMessage))
                {
                    me.ShowPopup = true;
                }
            }
        }

        public string PopupMessage
        {
            get
            {
                return (string)this.GetValue(PopupMessageProperty);

            }
            set
            {
                this.SetValue(PopupMessageProperty, value);

            }
        }

        private void OnPopupMouseUp(object sender, MouseButtonEventArgs e)
        {
            ShowPopup = false;
        }
    }
}
