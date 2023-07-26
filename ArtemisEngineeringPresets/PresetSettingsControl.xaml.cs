﻿using System;
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

namespace ArtemisEngineeringPresets
{
    /// <summary>
    /// Interaction logic for PresetSettingsControl.xaml
    /// </summary>
    public partial class PresetSettingsControl : UserControl
    {
        public PresetSettingsControl()
        {
           
            InitializeComponent();
        }


        public static readonly DependencyProperty SelectedIItemProperty =
           DependencyProperty.Register(nameof(SelectedIItem), typeof(Preset),
           typeof(PresetSettingsControl));

        public Preset SelectedIItem
        {
            get
            {
                return (Preset)this.GetValue(SelectedIItemProperty);
            }
            set
            {
                this.SetValue(SelectedIItemProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
           DependencyProperty.Register(nameof(SelectedIndex), typeof(int),
           typeof(PresetSettingsControl));

        public int SelectedIndex
        {
            get
            {
                return (int)this.GetValue(SelectedIndexProperty);
            }
            set
            {
                this.SetValue(SelectedIndexProperty, value);
            }
        }

        public static readonly DependencyProperty FileProperty =
           DependencyProperty.Register(nameof(File), typeof(PresetsFile),
           typeof(PresetSettingsControl), new PropertyMetadata(OnFileChanged));

        private static void OnFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PresetSettingsControl me)
            {
                //me.SelectedIndex = 0;
                me.SelectedIItem = me.File.Presets[0];
            }
        }

        public PresetsFile File
        {
            get
            {
                return (PresetsFile)this.GetValue(FileProperty);
            }
            set
            {
                this.SetValue(FileProperty, value);
            }
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            File.Save();
            RaiseSavedEvent();
        }

        void RaiseSavedEvent()
        {
            RaiseEvent(new RoutedEventArgs(SavedEvent, File));
        }

        public static readonly RoutedEvent SavedEvent = EventManager.RegisterRoutedEvent(
            name: nameof(Saved),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(PresetSettingsControl));
        public event RoutedEventHandler Saved
        {
            add { AddHandler(SavedEvent, value); }
            remove { RemoveHandler(SavedEvent, value); }
        }




        private void OnActivate(object sender, RoutedEventArgs e)
        {
            RaiseActivateEvent();
        }
        void RaiseActivateEvent()
        {
            RaiseEvent(new RoutedEventArgs(ActivateEvent, File));
        }

        public static readonly RoutedEvent ActivateEvent = EventManager.RegisterRoutedEvent(
            name: nameof(Activate),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(PresetSettingsControl));
        public event RoutedEventHandler Activate
        {
            add { AddHandler(ActivateEvent, value); }
            remove { RemoveHandler(ActivateEvent, value); }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            RaiseDeleteEvent();
        }
        void RaiseDeleteEvent()
        {
            RaiseEvent(new RoutedEventArgs(DeleteEvent, File));
        }

        public static readonly RoutedEvent DeleteEvent = EventManager.RegisterRoutedEvent(
            name: nameof(Delete),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(PresetSettingsControl));
        public event RoutedEventHandler Delete
        {
            add { AddHandler(DeleteEvent, value); }
            remove { RemoveHandler(DeleteEvent, value); }
        }
    }
}