using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using Microsoft.Phone.Net.NetworkInformation;

namespace WPTweaker
{
    public class ThemeManager
    {
        AppSettings _settings = new AppSettings();

        public ThemeManager()
        {
            ParseTheme();
            DownloadThemes();
        }

        public void ParseTheme()
        {
            if (!string.IsNullOrEmpty(_settings.Theme))
            {
                try
                {
                    var theme = XElement.Parse(_settings.Theme);
                    if (theme != null)
                    {
                        foreach (var key in Application.Current.Resources.Keys)
                        {
                            if (theme.Element(key.ToString()) != null)
                            {
                                ChangeResource(key.ToString(), theme.Element(key.ToString()).Value);
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private void ChangeResource(string resName, string newValue)
        {
            if (!string.IsNullOrEmpty(newValue))
            {
                if (Application.Current.Resources.Contains(resName)) Application.Current.Resources.Remove(resName);
                if (resName.Contains("Size"))
                {
                    int newSize = 0;
                    if (int.TryParse(newValue, out newSize)) Application.Current.Resources.Add(resName, newSize);
                }
                else
                {
                    // first, check for the system resources
                    if (App.Current.Resources.Contains(newValue))
                    {
                        Application.Current.Resources.Add(resName, Application.Current.Resources[newValue]);
                    }
                    // try to parse color
                    else
                    {
                        try { Application.Current.Resources.Add(resName, new SolidColorBrush(ColorExtensions.FromString(newValue))); }
                        catch { }
                    }
                }
            }
        }

        private void DownloadThemes()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                var webClient = new WebClient();
                webClient.DownloadStringCompleted += (object _, DownloadStringCompletedEventArgs args) =>
                {
                    if (args.Error == null)
                        try
                        {
                            XDocument tmp = XDocument.Parse(args.Result);
                            _settings.XmlThemes = tmp.ToString();
                        }
                        catch { }
                };
                webClient.DownloadStringAsync(new Uri("https://raw.githubusercontent.com/sensboston/WPTweaker/master/WPTweaker/Themes.xml"));
            }
        }

    }
}
