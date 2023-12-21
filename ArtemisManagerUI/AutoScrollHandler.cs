using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ArtemisManagerUI
{
    public class AutoScrollHandler : DependencyObject, IDisposable
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(IEnumerable),
            typeof(AutoScrollHandler),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None,
                ItemsSourcePropertyChanged));

#pragma warning disable IDE0044 // Add readonly modifier
        private ListBox target;
        private bool disposedValue;
#pragma warning restore IDE0044 // Add readonly modifier

        public AutoScrollHandler(System.Windows.Controls.ListBox target)
        {
            this.target = target;
            var binding = new Binding("ItemsSource") { Source = this.target };
            BindingOperations.SetBinding(this, ItemsSourceProperty, binding);
        }



        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        private static void ItemsSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((AutoScrollHandler)o).ItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        private void ItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            var collection = oldValue as INotifyCollectionChanged;
            if (collection != null)
            {
                collection.CollectionChanged -= CollectionChangedEventHandler;
            }

            collection = newValue as INotifyCollectionChanged;
            if (collection != null)
            {
                collection.CollectionChanged += CollectionChangedEventHandler;
            }
        }

        private void CollectionChangedEventHandler(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems == null || e.NewItems.Count < 1)
            {
                return;
            }

            this.target.ScrollIntoView(e.NewItems[e.NewItems.Count - 1]);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    BindingOperations.ClearBinding(this, ItemsSourceProperty);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AutoScrollHandler()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}