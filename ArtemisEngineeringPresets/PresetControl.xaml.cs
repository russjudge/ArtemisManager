﻿using System;
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

namespace ArtemisEngineeringPresets
{
    /// <summary>
    /// Interaction logic for PresetControl.xaml
    /// </summary>
    public partial class PresetControl : UserControl
    {
        public PresetControl()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty PresetItemProperty =
            DependencyProperty.Register("PresetItem", typeof(Preset),
            typeof(PresetControl));

        public Preset PresetItem
        {
            get
            {
                return (Preset)this.GetValue(PresetItemProperty);
            }
            set
            {
                this.SetValue(PresetItemProperty, value);
            }
        }
        private void AutoAdjust_Click(object sender, RoutedEventArgs e)
        {
            int MaxCoolant = 8;
            for (int i = 0; i < 8; i++)
            {
                PresetItem.SystemLevels[i].CoolantLevel = 0;
            }
            int MostNeed;
            do
            {
                MostNeed = -1;
                for (int i = 0; i < 8; i++)
                {
                    if (PresetItem.SystemLevels[i].CoolantNeed > 0 && PresetItem.SystemLevels[i].CoolantLevel == 0
                        && (MostNeed < 0 || PresetItem.SystemLevels[i].CoolantNeed > PresetItem.SystemLevels[MostNeed].CoolantNeed))
                    {
                        MostNeed = i;
                    }
                }
                if (MostNeed >= 0)
                {
                    int j = PresetItem.SystemLevels[MostNeed].CoolantNeed;
                    if (MaxCoolant - j < 0)
                    {
                        j = MaxCoolant;
                    }
                    PresetItem.SystemLevels[MostNeed].CoolantLevel = j;
                    MaxCoolant -= j;
                }
            } while (MostNeed > -1 && MaxCoolant > 0);
        }
    }
}
