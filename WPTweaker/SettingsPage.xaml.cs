using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Xml;
using System.Xml.Linq;

namespace WPTweaker
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        bool _sortTweaks;
        string _startTheme;
        AppSettings _settings = new AppSettings();
        IsolatedStorageFile _isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        string _isoRootPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        XDocument _xmlDoc = null;
        List<string> _themeNames = new List<string>();

        public SettingsPage()
        {
            InitializeComponent();

#if !DEBUG
            _xmlDoc = string.IsNullOrEmpty(_settings.XmlThemes) ? XDocument.Load("Themes.xml") : XDocument.Parse(_settings.XmlThemes);
#else
            _xmlDoc = XDocument.Load("Themes.xml");
#endif
            if (_xmlDoc != null)
            {
                if (string.IsNullOrEmpty(_settings.XmlThemes)) _settings.XmlThemes = _xmlDoc.ToString();

                _themeNames = _xmlDoc.Descendants("theme").Attributes("name").Select(a => a.Value).ToList();
                ThemeList.ItemsSource = _themeNames;
                ThemeList.SelectedItem = _startTheme = _settings.ThemeName;

                ThemeList.SelectionChanged += (_, __) =>
                {
                    string name = ThemeList.SelectedItem as string;
                    if (!string.IsNullOrEmpty(name) && _xmlDoc != null && name != _settings.ThemeName)
                    {
                        _settings.ThemeName = name;
                        var node = _xmlDoc.Descendants("theme").Where(d => d.Attribute("name").Value == name).FirstOrDefault();
                        if (node != null) _settings.Theme = node.ToString();
                    }
                };
            }
            bytesUsed.Text = string.Format("Used space: {0}", GetStoreUsedSize().ToSize());
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _sortTweaks = new AppSettings().SortTweaks;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (_startTheme != _settings.ThemeName)
            {
                if (MessageBox.Show("To apply new colors, you should relaunch the app.\n\nWould you like to exit now?", "You changed color theme", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    App.Current.Terminate();
                }
            }

            if (_sortTweaks != new AppSettings().SortTweaks) PhoneApplicationService.Current.State["reload"] = "True";
        }

        private long GetStoreUsedSize()
        {
            long size = 0; 
            var files = _isoStore.GetFileNames();
            foreach (var fileName in files)
            {
                try { size += new FileInfo(Path.Combine(_isoRootPath, fileName)).Length; }
                catch { }
            }
            return size;
        }

        private void CleanButton_Click(object sender, RoutedEventArgs e)
        {
            string[] exts = { ".wma", ".wav", ".mp3" };
            string[] files = _isoStore.GetFileNames();
            foreach (var fileName in files)
            {
                if (exts.Contains(Path.GetExtension(fileName)))
                {
                    try { _isoStore.DeleteFile(fileName); }
                    catch { }
                }
            }
            bytesUsed.Text = string.Format("Used space: {0}", GetStoreUsedSize().ToSize());
        }
    }

    public static class Extension
    {
        public enum SizeUnits { Bytes, KB, MB, GB }
        public static string ToSize(this Int64 value)
        {
            SizeUnits unit = SizeUnits.Bytes;
            if (value > 1024) unit = SizeUnits.KB; else if (value > 1024 * 1024) unit = SizeUnits.MB; else unit = SizeUnits.GB;
            return (value / (double)Math.Pow(1024, (Int64)unit)).ToString("0.00 ")+unit.ToString();
        }
    }
}