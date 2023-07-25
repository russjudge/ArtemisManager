using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArtemisEngineeringPresets
{
    public class SystemLevel : UIElement
    {
        public SystemLevel()
        {
            EnergyLevel = 100;
            CoolantLevel = 0;
        }
        public SystemLevel(int energy, int coolant)
        {
            EnergyLevel = energy;
            CoolantLevel = coolant;
        }

        public void AcceptChanges()
        {
            Changed = false;
        }

        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent(
            "ValueChanged", RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(SystemLevel));

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }


        static void OnChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is SystemLevel me)
            {
                if (me.Changed)
                {

                }
            }
        }
        public static readonly DependencyProperty ChangedProperty =
         DependencyProperty.Register("Changed", typeof(bool),
         typeof(SystemLevel), new PropertyMetadata(OnChanged));

        public bool Changed
        {
            get
            {
                return (bool)this.GetValue(ChangedProperty);
            }
            private set
            {
                this.SetValue(ChangedProperty, value);
            }
        }
        static readonly string[] systems = { "Beam", "Torpedo", "Sensors", "Maneuvering", "Impulse", "Warp/Jump", "Front Shield", "Rear Shield" };
        public string SystemName
        {
            get
            {
                return systems[(int)Index];
            }
        }
        public static readonly DependencyProperty IndexProperty =
        DependencyProperty.Register("Index", typeof(Systems),
        typeof(SystemLevel));

        public Systems Index
        {
            get
            {
                return (Systems)this.GetValue(IndexProperty);
            }
            set
            {
                this.SetValue(IndexProperty, value);
            }
        }

        static void OnEnergyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is SystemLevel me)
            {
                var startNeed = me.CoolantNeed;
                me.CoolantNeed = Preset.CoolantLevelToPreventOverheat(me.EnergyLevel);
                if (me.CoolantNeed < startNeed && me.CoolantNeed < me.CoolantLevel)
                {
                    me.CoolantLevel = me.CoolantNeed;
                }
                me.Changed = true;
            }
        }

        public static readonly DependencyProperty EnergyLevelProperty =
         DependencyProperty.Register("EnergyLevel", typeof(int),
         typeof(SystemLevel), new PropertyMetadata(OnEnergyChanged));

        public int EnergyLevel
        {
            get
            {
                return (int)this.GetValue(EnergyLevelProperty);
            }
            set
            {
                this.SetValue(EnergyLevelProperty, value);
            }
        }
        static void OnCoolantChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is SystemLevel me)
            {
                me.Changed = true;
                me.RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
            }
        }

        public static readonly DependencyProperty CoolantLevelProperty =
         DependencyProperty.Register("CoolantLevel", typeof(int),
         typeof(SystemLevel), new PropertyMetadata(OnCoolantChanged));

        public int CoolantLevel
        {
            get
            {
                return (int)this.GetValue(CoolantLevelProperty);
            }
            set
            {
                this.SetValue(CoolantLevelProperty, value);
            }
        }

        public static readonly DependencyProperty CoolantNeedProperty =
         DependencyProperty.Register("CoolantNeed", typeof(int),
         typeof(SystemLevel));

        public int CoolantNeed
        {
            get
            {
                return (int)this.GetValue(CoolantNeedProperty);
            }
            private set
            {
                this.SetValue(CoolantNeedProperty, value);
            }
        }
    }
}
