using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WPTweaker
{
    public sealed partial class ToggleTweak : UserControl
    {
        private Tweak _tweak;
        public ToggleTweak(Tweak tweak)
        {
            this.InitializeComponent();
            _tweak = tweak;
            _tweak.ValueChanged += ValueChanged;
        }

        void ValueChanged(object sender, string hashedKeys)
        {
            if (string.IsNullOrEmpty(hashedKeys)) 
                Toggle.SetValue(ToggleSwitch.IsCheckedProperty, _tweak.Value);
        }
    }
}
