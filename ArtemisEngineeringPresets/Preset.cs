using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArtemisEngineeringPresets
{
    public class Preset : DependencyObject
    {
        public Preset()
        {
            Initialize(false);
        }
        public Preset(IList<int> energyLevels, IList<int> coolantLevels, int index)
        {
            Initialize(false);
            Index = index;
            if (energyLevels != null && coolantLevels != null)
            {
                for (int i = 0; i < PresetsFile.MaxStations; i++)
                {
                    SystemLevels[i].EnergyLevel = energyLevels[i];
                    SystemLevels[i].CoolantLevel = coolantLevels[i];
                    SystemLevels[i].Index = (Systems)i;
                }
            }
            AcceptChanges();
        }
        
        private Preset(bool isForOriginal)
        {
            Initialize(isForOriginal);
        }
        void Initialize(bool isForOriginal)
        {
            SystemLevels = new SystemLevel[PresetsFile.MaxStations];

            TotalCoolantLevel = 0;
            for (int i = 0; i < PresetsFile.MaxStations; i++)
            {
                SystemLevel s = new(100, 0)
                {
                    Index = (Systems)i
                };
                s.ValueChanged += new RoutedEventHandler(ValueChanged);

                SystemLevels[i] = s;
            }
            if (!isForOriginal)
            {
                Original = new Preset(true);
            }
            else
            {
                Original = null;
            }

        }
        readonly object lockObject = new();
        void ValueChanged(object sender, RoutedEventArgs e)
        {
            lock (lockObject)
            {
                Changed = true;
                TotalCoolantLevel = 0;
                foreach (SystemLevel coolant in SystemLevels)
                {
                    TotalCoolantLevel += coolant.CoolantLevel;
                }
            }

        }
        public static int CoolantLevelToPreventOverheat(int energyLevel)
        {

            if (energyLevel <= 100)
                return 0;
            else if (energyLevel <= 150)
                return 0 + (energyLevel - 100) / 25;
            else if (energyLevel <= 190.0)
                return 2 + (energyLevel - 150) / 20;
            else if (energyLevel <= 220.0)
                return 4 + (energyLevel - 190) / 15;
            else if (energyLevel <= 250)
                return 6 + (energyLevel - 220) / 15;
            else
                return 8;
        }

        void SetOriginal()
        {
            Original = new Preset(true);
            for (int i = 0; i < 8; i++)
            {
                Original.SystemLevels[i].EnergyLevel = this.SystemLevels[i].EnergyLevel;
                Original.SystemLevels[i].CoolantLevel = this.SystemLevels[i].CoolantLevel;

            }
        }
        public void AcceptChanges()
        {
            Changed = false;
            SetOriginal();
        }
        public void RejectChanges()
        {
            if (Original == null)
            {
                for (int i = 0; i < PresetsFile.MaxStations; i++)
                {
                    this.SystemLevels[i].EnergyLevel = 0;
                    this.SystemLevels[i].CoolantLevel = 0;
                }
            }
            else
            {
                for (int i = 0; i < PresetsFile.MaxStations; i++)
                {
                    this.SystemLevels[i].EnergyLevel = Original.SystemLevels[i].EnergyLevel;
                    this.SystemLevels[i].CoolantLevel = Original.SystemLevels[i].CoolantLevel;
                }
            }
            Changed = false;
        }
       

        public Preset? Original { get; private set; } = null;


        public static readonly DependencyProperty IndexProperty =
          DependencyProperty.Register(nameof(Index), typeof(int),
          typeof(Preset));

        public int Index
        {
            get
            {
                return (int)this.GetValue(IndexProperty);
            }
            set
            {
                this.SetValue(IndexProperty, value);
            }
        }

        public static readonly DependencyProperty ChangedProperty =
          DependencyProperty.Register("Changed", typeof(bool),
          typeof(Preset));

        public bool Changed
        {
            get
            {
                return (bool)this.GetValue(ChangedProperty);
            }
            set
            {
                this.SetValue(ChangedProperty, value);
            }
        }



        public static readonly DependencyProperty SystemLevelsProperty =
          DependencyProperty.Register(nameof(SystemLevels), typeof(SystemLevel[]),
          typeof(Preset));

        public SystemLevel[] SystemLevels
        {
            get
            {
                return (SystemLevel[])this.GetValue(SystemLevelsProperty);
            }
            private set
            {
                this.SetValue(SystemLevelsProperty, value);
            }
        }



        public static readonly DependencyProperty TotalCoolantLevelProperty =
            DependencyProperty.Register(nameof(TotalCoolantLevel), typeof(int),
            typeof(Preset));

        public int TotalCoolantLevel
        {
            get
            {
                return (int)this.GetValue(TotalCoolantLevelProperty);
            }
            set
            {
                this.SetValue(TotalCoolantLevelProperty, value);
            }
        }
    }
}
