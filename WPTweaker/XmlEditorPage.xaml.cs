using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Xml;
using System.Xml.Linq;

namespace WPTweaker
{
    public partial class XmlEditorPage : PhoneApplicationPage
    {
        AppSettings _settings = new AppSettings();

        public XmlEditorPage()
        {
            InitializeComponent();
            BackKeyPress += XmlEditorPage_BackKeyPress;
            Editor.Text = _settings.XmlTweaks;
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            PhoneApplicationService.Current.State["LastPage"] = "XmlEditorPage";
        }

        void XmlEditorPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string xml = Editor.Text;
            if (!_settings.XmlTweaks.Equals(xml))
            {
                if (MessageBox.Show("Save changes?", "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    try
                    {
                        XDocument doc = XDocument.Parse(xml);
                        _settings.XmlTweaks = doc.ToString();
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                        e.Cancel = true;
                    }
                }
            }
        }

        private void ReloadButton_Click(object sender, EventArgs e)
        {
            Editor.Text = _settings.XmlTweaks;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string xml = Editor.Text;
            try
            {
                XDocument doc = XDocument.Parse(xml);
                _settings.XmlTweaks = doc.ToString();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }
    }
}