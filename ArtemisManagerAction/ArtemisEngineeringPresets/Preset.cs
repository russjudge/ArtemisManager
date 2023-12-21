using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ArtemisManagerAction.ArtemisEngineeringPresets
{
    public class Preset : INotifyPropertyChanged
    {
        public const int MaxStations = 8;
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
                for (int i = 0; i < MaxStations; i++)
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


            TotalCoolantLevel = 0;
            for (int i = 0; i < MaxStations; i++)
            {
                SystemLevel s = new(100, 0)
                {
                    Index = (Systems)i
                };
                s.EnergyChanged += S_EnergyChanged;
                s.CoolantChanged += S_CoolantChanged;

                SystemLevels[i] = s;
            }

        }

        private void S_CoolantChanged(object? sender, EventArgs e)
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

        private void S_EnergyChanged(object? sender, EventArgs e)
        {
            lock (lockObject)
            {
                Changed = true;
            }
        }

        readonly object lockObject = new();



        public void AcceptChanges()
        {
            Changed = false;

        }


        int _index = 0;

        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
                DoChanged();
            }
        }

        bool _changed = false;

        public bool Changed
        {
            get
            {
                return _changed;
            }
            set
            {
                _changed = value;
                DoChanged();
            }
        }

        public SystemLevel[] SystemLevels { get; set; } = new SystemLevel[MaxStations];
        int _totalCoolant = 0;

        public int TotalCoolantLevel
        {
            get
            {
                return _totalCoolant;
            }
            set
            {
                _totalCoolant = value;
                DoChanged();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
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
