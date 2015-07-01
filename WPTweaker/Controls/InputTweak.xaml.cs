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
            // Validate user input first
            if (_tweak.RegistryDataType == RegDataType.REG_DWORD || _tweak.RegistryDataType == RegDataType.REG_QWORD)
            {
                long result = 0;
                if (!Int64.TryParse(ValueInput.Text, out result))
                {
                    MessageBox.Show("Value must be numerical");
                    ValueInput.Focus();
                    return;
                }
                if (result < _tweak.Min || result > _tweak.Max)
                {
                    MessageBox.Show(string.Format("Value must be in range {0} to {1}", _tweak.Min.ToString("X"), _tweak.Max.ToString("X")));
                    ValueInput.Focus();
                    return;
                }
            }
            _tweak.Value = ValueInput.Text;
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            string value = _tweak.DefaultValueToString;
            if (!string.IsNullOrEmpty(value)) ValueInput.SetValue(TextBox.TextProperty, _tweak.DefaultValueToString);
        }
    }
}
