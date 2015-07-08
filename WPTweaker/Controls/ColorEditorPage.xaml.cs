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
        public ObservableCollection<SolidColorBrush> Colors = new ObservableCollection<SolidColorBrush>();

        private const string AccentThemePath = "HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Control Panel\\Theme\\Themes\\{0}\\Accents\\{1}";
        private uint[] _defaultAccentColors = new uint[] { 4288988160, 4284524823, 4278225408, 4278234025, 4280000994, 4282279423, 4285137151, 4289331455, 4294210256, 4292345971, 4288806949, 4293202944, 4294600704, 4293960458, 4293117952, 4286732844, 4285368164, 4284774023, 4285948042, 4287068494 };
        private uint[] _defaultComplementaryColors = new uint[] { 4284524823, 4278225408, 4278234025, 4280000994, 4282279423, 4285137151, 4289331455, 4294210256, 4292345971, 4288806949, 4293202944, 4294600704, 4293960458, 4293117952, 4293960458, 4287068494, 4287068494, 4285948042, 4284774023, 4286732844 };

        private int _numColors;
        private RegistryEntry[,] _colorEntries;
        private bool _doUpdate = false;
        private string _keyName = "";

        public ColorEditorPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            PhoneApplicationService.Current.State["LastPage"] = "ColorEditorPage";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                _keyName = "Color";
                if (NavigationContext.QueryString.ContainsKey("keyName")) _keyName = NavigationContext.QueryString["keyName"];
                
                _numColors = _defaultAccentColors.Length;
                _colorEntries = new RegistryEntry[2, _numColors];
                for (int i = 0; i < 2; i++)
                    for (int j = 0; j < _defaultAccentColors.Length; j++)
                        _colorEntries[i, j] = new RegistryEntry(string.Format(AccentThemePath, i, j), _keyName, RegDataType.REG_DWORD, _keyName.Equals("Color") ? _defaultAccentColors[j] : _defaultComplementaryColors[j], "", 0, 255);

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
            Colors.Clear();
            for (int j=0; j<_defaultAccentColors.Length; j++)
            {
                Colors.Add(new SolidColorBrush(ColorExtensions.FromArgb(_colorEntries[0, j].Value)));
            }
            ColorsList.ItemsSource = Colors;
            ColorsList.SelectedIndex = 0;
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i=0; i<2; i++)
                for (int j = 0; j < _defaultAccentColors.Length; j++)
                    _colorEntries[i, j].Value = Colors[j].Color.ToArgb();
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            Colors.Clear();
            foreach (uint color in _keyName.Equals("Color") ? _defaultAccentColors : _defaultComplementaryColors)
                Colors.Add(new SolidColorBrush(ColorExtensions.FromArgb(color)));
            ColorsList.ItemsSource = Colors;
            ColorsList.SelectedIndex = 0;
        }

        private void ColorsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = ColorsList.SelectedIndex;
            if (i >= 0)
            {
                _doUpdate = false;
                RedSlider.Value = Colors[i].Color.R;
                GreenSlider.Value = Colors[i].Color.G;
                BlueSlider.Value = Colors[i].Color.B;
                _doUpdate = true;
            }
        }

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int i = ColorsList.SelectedIndex;
            if (i >= 0 && _doUpdate)
            {
                Colors[i] = new SolidColorBrush(Color.FromArgb(0xFF, (byte)RedSlider.Value, (byte)GreenSlider.Value, (byte)BlueSlider.Value));
                ColorsList.ItemsSource = Colors;
                ColorsList.SelectedIndex = i;
            }
        }
    }
}