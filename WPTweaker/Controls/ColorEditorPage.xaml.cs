using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.Diagnostics;

namespace WPTweaker
{
    public partial class ColorEditorPage : PhoneApplicationPage
    {
        public ObservableCollection<SolidColorBrush> Accents = new ObservableCollection<SolidColorBrush>();

        private const string AccentThemePath = "HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Control Panel\\Theme\\Themes\\{0}\\Accents\\{1}";
        private uint[] _defaultAccentColors = new uint[] { 4288988160, 4284524823, 4278225408, 4278234025, 4280000994, 4282279423, 4285137151, 4289331455, 4294210256, 4292345971, 4288806949, 4293202944, 4294600704, 4293960458, 4293117952, 4286732844, 4285368164, 4284774023, 4285948042, 4287068494 };
        private uint[] _defaultComplementaryColors = new uint[] { 4284524823, 4278225408, 4278234025, 4280000994, 4282279423, 4285137151, 4289331455, 4294210256, 4292345971, 4288806949, 4293202944, 4294600704, 4293960458, 4293117952, 4293960458, 4287068494, 4287068494, 4285948042, 4284774023, 4286732844 };

        private int _numAccents;
        private RegistryEntry[,] _accentEntries;
        private bool _doUpdate = false;
        private string _keyName = "";

        public ColorEditorPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                _keyName = "Color";
                _numAccents = _defaultAccentColors.Length;
                _accentEntries = new RegistryEntry[2, _numAccents];
                for (int i = 0; i < 2; i++)
                    for (int j = 0; j < _defaultAccentColors.Length; j++)
                        _accentEntries[i, j] = new RegistryEntry(string.Format(AccentThemePath, i, j), _keyName, RegDataType.REG_DWORD, _defaultAccentColors[j], "", 0, 255);

#if false
                for (int i = 0; i < 2; i++)
                {
                    string s = string.Empty;
                    for (int j = 0; j < _defaultAccentColors.Length; j++)
                    {
                        uint v = _accentEntries[i, j].Value;
                        s += string.Format("{0}, ", v);
                    }
                    Debug.WriteLine(s);
                }
#endif

                ReadButton_Click(this, null);
            }
        }

        private void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            Accents.Clear();
            for (int j=0; j<_defaultAccentColors.Length; j++)
            {
                var color = (uint) _accentEntries[0, j].Value;
                Accents.Add(new SolidColorBrush(Color.FromArgb(0xFF, (byte)(color & 0xFF), (byte)((color & 0xFF00) >> 8), (byte)((color & 0xFF0000) >> 16))));
            }
            ColorsList.ItemsSource = Accents;
            ColorsList.SelectedIndex = 0;
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i=0; i<2; i++)
                for (int j = 0; j < _defaultAccentColors.Length; j++)
                    _accentEntries[i, j].Value = Accents[j];
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            Accents.Clear();
            foreach (uint color in _defaultAccentColors)
                Accents.Add(new SolidColorBrush(Color.FromArgb(0xFF, (byte)(color & 0xFF), (byte)((color & 0xFF00) >> 8), (byte)((color & 0xFF0000) >> 16))));
            ColorsList.ItemsSource = Accents;
            ColorsList.SelectedIndex = 0;
        }

        private void ColorsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = ColorsList.SelectedIndex;
            if (i >= 0)
            {
                _doUpdate = false;
                RedSlider.Value = Accents[i].Color.R;
                GreenSlider.Value = Accents[i].Color.G;
                BlueSlider.Value = Accents[i].Color.B;
                _doUpdate = true;
            }
        }

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int i = ColorsList.SelectedIndex;
            if (i >= 0 && _doUpdate)
            {
                Accents[i] = new SolidColorBrush(Color.FromArgb(0xFF, (byte)RedSlider.Value, (byte)GreenSlider.Value, (byte)BlueSlider.Value));
                ColorsList.ItemsSource = Accents;
                ColorsList.SelectedIndex = i;
            }
        }
    }
}