using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WPTweaker
{
    public sealed partial class EnumTweak : UserControl
    {
        private Tweak _tweak;
        private bool _ignoreChange = false;

        public EnumTweak(Tweak tweak)
        {
            this.InitializeComponent();
            ValuesList.SetValue(ListPicker.ItemCountThresholdProperty, 20);
            _tweak = tweak;
            _tweak.ValueChanged += ValueChanged;
        }

        void ValueChanged(object sender, string hashedKeys)
        {
            if (string.IsNullOrEmpty(hashedKeys))
            {
                var val = _tweak.Value;
                var index = Array.FindIndex(_tweak.EnumList.ToArray(), t => t.Value == val);
                if (index >= 0)
                {
                    _ignoreChange = true;
                    ValuesList.SelectedIndex = index;
                }
            }
        }
        
        private void EnumControl_Loaded(object sender, RoutedEventArgs e)
        {
            ValueChanged(this, null);
            ValuesList.SelectionChanged += ValuesList_SelectionChanged;
        }

        void ValuesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_ignoreChange) _tweak.Value = (e.AddedItems[0] as RegValue).Value;
            _ignoreChange = false;
        }
    }
}
