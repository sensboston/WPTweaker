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
    public sealed partial class InputTweak : UserControl
    {
        private Tweak _tweak;
        public InputTweak(Tweak tweak)
        {
            this.InitializeComponent();
            _tweak = tweak;
            _tweak.ValueChanged += ValueChanged;
        }

        void ValueChanged(object sender, string hashedKeys)
        {
            if (string.IsNullOrEmpty(hashedKeys))
                ValueInput.SetValue(TextBox.TextProperty, _tweak.ValueToString);
        }

        private void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            ValueInput.SetValue(TextBox.TextProperty, _tweak.ValueToString);
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            _tweak.Value = ValueInput.Text;
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            string value = _tweak.DefaultValueToString;
            if (!string.IsNullOrEmpty(value)) ValueInput.SetValue(TextBox.TextProperty, _tweak.DefaultValueToString);
        }
    }
}
