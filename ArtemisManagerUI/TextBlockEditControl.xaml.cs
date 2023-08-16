using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for TextBlockEditControl.xaml
    /// </summary>
    public partial class TextBlockEditControl : UserControl
    {
        public TextBlockEditControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register(nameof(EditMode), typeof(bool),
            typeof(TextBlockEditControl), new PropertyMetadata(OnEditModeChanged));

        private static void OnEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlockEditControl me)
            {
                if (me.EditMode)
                {
                    foreach (var pb in FindVisualChildren<TextBox>(me))
                    {
                        pb.Focus();
                        break;
                    }
                }
            }
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield return (T)Enumerable.Empty<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject ithChild = VisualTreeHelper.GetChild(depObj, i);
                if (ithChild == null) continue;
                if (ithChild is T t) yield return t;
                foreach (T childOfChild in FindVisualChildren<T>(ithChild)) yield return childOfChild;
            }
        }

        public bool EditMode
        {
            get
            {
                return (bool)this.GetValue(EditModeProperty);

            }
            set
            {
                this.SetValue(EditModeProperty, value);
            }
        }
        public static readonly DependencyProperty TextProperty =
           DependencyProperty.Register(nameof(Text), typeof(string),
           typeof(TextBlockEditControl), new PropertyMetadata(OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlockEditControl me)
            {
                me.RaiseTextChangedEvent();
            }
        }

        public string Text
        {
            get
            {
                return (string)this.GetValue(TextProperty);

            }
            set
            {
                this.SetValue(TextProperty, value);
            }
        }
        void RaiseTextChangedEvent()
        {
            RaiseEvent(new RoutedEventArgs(TextChangedEvent));
        }

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent(
            name: nameof(TextChanged),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(PresetSettingsControl));
        public event RoutedEventHandler TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            EditMode = false;
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is TextBox myTextBox)
                {
                    // If there is no other focusable element, you can choose to leave the focus on the TextBox.
                    if (myTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)))
                    {
                        e.Handled = true; // Set this to true to prevent further handling of the Enter key.
                    }
                    else
                    {
                        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(myTextBox), null);
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
