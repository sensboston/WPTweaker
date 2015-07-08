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
        const string HtmlTemplate =
@"<html>
<head>
    <meta name='viewport' content='width=device-width, height=device-height, initial-scale=1.0, maximum-scale=1.0, user-scalable=no;' />
    <script type='text/javascript'>
        function setContent(content) { document.getElementById('editor').innerText = content; }
        function getContent() { return document.getElementById('editor').innerText; }
        function setFontSize(size) { document.getElementById('editor').style.fontSize = size+'%'; }
    </script>
</head>
<body>
    <div style='width:2000px; overflow-x:scroll; overflow-y:scroll; font-family:monospace; font-size:80%' id='editor' contenteditable='true' ></div>
</body>
</html>";

        AppSettings _settings = new AppSettings();
        string _originalXML = string.Empty;
        int _fontSize = 80;

        public XmlEditorPage()
        {
            InitializeComponent();
            BackKeyPress += XmlEditorPage_BackKeyPress;
            _originalXML = _settings.XmlTweaks;
            webBrowser.Navigated += (_,__) => { webBrowser.InvokeScript("setContent", _originalXML); };
            webBrowser.NavigateToString(HtmlTemplate);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            bool doReload = !_settings.XmlTweaks.Equals(_originalXML);
            PhoneApplicationService.Current.State["reload"] = doReload.ToString();
        }

        void XmlEditorPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string xml = (string) webBrowser.InvokeScript("getContent");
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
            webBrowser.InvokeScript("setContent", _settings.XmlTweaks);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string xml = (string) webBrowser.InvokeScript("getContent");
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

       private void IncreaseFontSizeButton_Click(object sender, EventArgs e)
        {
            if (_fontSize < 140)
            {
                _fontSize += 20;
                webBrowser.InvokeScript("setFontSize", _fontSize.ToString());
            }
        }

        private void DecreaseFontSizeButton_Click(object sender, EventArgs e)
        {
            if (_fontSize > 80)
            {
                _fontSize -= 20;
                webBrowser.InvokeScript("setFontSize", _fontSize.ToString());
            }
        }
     }
}