using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ArtemisManagerAction.ArtemisEngineeringPresets
{
    public class SystemLevel : INotifyPropertyChanged
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
        static readonly string[] systems = ["Beam", "Torpedo", "Sensors", "Maneuvering", "Impulse", "Warp/Jump", "Front Shield", "Rear Shield"];
        public string SystemName
        {
            get
            {
                return systems[(int)Index];
            }
        }
        private Systems index;
        public Systems Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
                DoChanged();
            }
        }
        private int _energyLevel;
        public int EnergyLevel
        {
            get
            {
                return _energyLevel;
            }
            set
            {
                _energyLevel = value;
                DoChanged();
                EnergyChanged?.Invoke(this, EventArgs.Empty);
                var startNeed = CoolantNeed;
                CoolantNeed = CoolantLevelToPreventOverheat(EnergyLevel);
                if (CoolantNeed < startNeed && CoolantNeed < CoolantLevel)
                {
                    CoolantLevel = CoolantNeed;
                }
            }
        }
        private int _coolantLevel;
        public int CoolantLevel
        {
            get
            {
                return _coolantLevel;
            }
            set
            {
                _coolantLevel = value;
                DoChanged();
                CoolantChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private int _coolantNeed;
        public int CoolantNeed
        {
            get
            {
                return _coolantNeed;
            }
            set
            {
                _coolantNeed = value;
                DoChanged();
            }
        }

        public event EventHandler? EnergyChanged;
        public event EventHandler? CoolantChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
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

        bool changed = false;
        public bool Changed
        {
            get
            {
                return changed;
            }
            set
            {
                changed = value;
                DoChanged();
            }

        }
        protected void DoChanged([CallerMemberName] string property = "")
        {
            if (property != nameof(Changed))
            {
                Changed = true;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
