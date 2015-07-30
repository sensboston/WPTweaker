using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

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
            if (!string.IsNullOrEmpty(_tweak.InputHelper))
            {
                DefaultButton.Content = "Browse";
            }
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
                if (!Int64.TryParse(ValueInput.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result))
                {
                    MessageBox.Show("Value must be numerical, in the hex format");
                    ValueInput.Focus();
                    return;
                }
                if (result < _tweak.Min || result > _tweak.Max)
                {
                    MessageBox.Show(string.Format("Value must be in range {0} to {1} (hex)", _tweak.Min.ToString("X"), _tweak.Max.ToString("X")));
                    ValueInput.Focus();
                    return;
                }
            }
            _tweak.Value = ValueInput.Text;
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_tweak.InputHelper))
            {
                if (_tweak.InputHelper.Contains("picture"))
                {
                    var photoChooser = new PhotoChooserTask();
                    photoChooser.Completed += (object _, PhotoResult result) =>
                    {
                        if (result.ChosenPhoto != null)
                        {
                            ValueInput.Text = result.OriginalFileName;
                        }
                    };
                    photoChooser.Show();
                }
            }
            else
            {
                string value = _tweak.DefaultValueToString;
                if (!string.IsNullOrEmpty(value)) ValueInput.SetValue(TextBox.TextProperty, _tweak.DefaultValueToString);
            }
        }
    }
}
