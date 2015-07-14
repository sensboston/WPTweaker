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
        /// <summary>
        ///  Decoration script from http://dema.ru/syntax/
        ///  Copyright (c) 2008, Dema (Dema.ru)
        /// </summary>
        const string HtmlTemplate =
@"<html>
<head>
    <meta name='viewport' content='width=device-width, height=device-height, initial-scale=1.0, maximum-scale=1.0, user-scalable=no;' />
    <style type='text/css'>
        .xml                        { font-family: monospace; color: #000000; font-weight: bold; }
        .xml .rem                   { color: #A0A0A0; }
        .xml .cdatao, .xml .cdatac  { color: #A0A0A0; }
        .xml .cdata                 { color: #000000; }
        .xml .tag                   { color: #0000FF; }
        .xml .name                  { color: #AA1515; font-weight: bold; }
        .xml .attr                  { color: #AA1515; }
        .xml .attr .name            { color: #FF0000; font-weight: normal; }
        .xml .attr .value           { color: #0000FF; }
    </style>
    <script type='text/javascript'>
        function DecorateXML (xml) 
        {
            decorator = decorators.XML;
            var all = [];
            return decorator.block(
                decorator.lines(
                    TrueTabs(correctRN(xml))
                    .replace(/<!--([\s\S]*?)-->/g, function (m, t)
                    { return '\0B' + push(all, multiline_comments('&lt;!--' + makeSafe(t) + '--&gt;', decorator.rem)) + '\0'; })
                    .replace(/<!\[CDATA\[([\s\S]*?)\]\]>/g, function (m, d)
                    { return '\0B' + push(all, decorator.cdatao() + multiline_comments(makeSafe(d), decorator.cdata) + decorator.cdatac()) + '\0'; })
                    .replace(/<(((\?)?([a-z][a-z0-9:_-]*)([^>]*)\3)|(\/([a-z][a-z0-9:_-]*)[\s\n]*))>/g, function (m, a, o, d, no, p, c, nc) {
                        if (d == '?')
                            return decorator.def(no, SyntaxXML_param(p, decorator));
                        if (nc != null && nc != '')
                            return decorator.tagc(nc);
                        if (p == null || p == '')
                            return decorator.tago(no, '', false);
                        if (p.substring(p.length - 1) == '/')
                            return decorator.tago(no, SyntaxXML_param(p.substring(0, p.length - 1), decorator), true);
                        return decorator.tago(no, SyntaxXML_param(p, decorator), false);
                    })
                    .replace(/\0B(\d+)\0/g, function (m, i)
                    { return all[i]; })
                    .split('\n')
                )
            );
        }

        var decorators = {
            XML: {
                rem: function (txt) { return '<span class=""rem"">'.concat(txt, '</span>'); },
                cdatao: function () { return '<span class=""cdatao"">&lt;![CDATA[</span>'; },
                cdata: function (txt) { return '<span class=""cdata"">'.concat(txt, '</span>'); },
                cdatac: function () { return '<span class=""cdatac"">]]&gt;</span>'; },
                def: function (n, p) { return '<span class=""tag"">&lt;?<span class=""name"">'.concat(n, '</span></span>', p, '<span class=""tag"">?&gt;</span>'); },
                tago: function (n, p, e) {
                    return '<span class=""tag"">&lt;<span class=""name"">'.concat(
                            n,
                            '</span></span>',
                            p,
                            e ? '<span class=""tag"">/&gt;</span>' : '<span class=""tag"">&gt;</span>');
                },
                tagc: function (n) { return '<span class=""tag"">&lt;/<span class=""name"">'.concat(n, '</span>&gt;</span>'); },
                param: function (n, v) { return '<span class=""attr""><span class=""name"">'.concat(n, '</span>=<span class=""value"">""', v, '""</span></span>'); },
                lines: function (lines) { return lines.join('<br/>'); },
                block: function (txt) { return '<div class=""xml"">'.concat(txt.replace(/  /g, '&nbsp;&nbsp;'), '</div>'); }
            }
        }

        var safe = { '<': '&lt;', '>': '&gt;', '&': '&amp;' };
        var htmlen = { '&lt;': '<', '&gt;': '>', '&amp;': '&', '&quot;': '""' };
        function getSafe(c) {
            return safe[c];
        }
        function getHTMLEn(m) {
            return htmlen[m];
        }
        function makeSafe(txt) {
            return txt.replace(/[<>&]/g, getSafe);
        }
        function correctRN(txt) {
            return txt.replace(/(\r\n|\r)/g, '\n');
        }
        function multiline_comments(txt, decorator) {
            txt = txt.split('\n');
            for (var i = 0; i < txt.length; i++)
                txt[i] = decorator(txt[i]);
            return txt.join('\n');
        }
        function push(arr, e) {
            arr.push(e);
            return arr.length - 1;
        }
        function SyntaxXML_param(txt, decorator) {
            return txt.replace(/([a-z][a-z0-9_-]*)[\s\n]*=[\s\n]*""([^""]*)""/g, function (m, n, v)
            { return decorator.param(n, makeSafe(v)); });
        }
        var truetabsstr = '                                                        ';
        var truetabsre2 = /^([^\t\n]*)(\t+)/gm;
        function GetTrueTabs(len) {
            while (len > truetabsstr.length) truetabsstr += truetabsstr;
            return truetabsstr.substring(0, len);
        }
        function TrueTabs(txt) {
            var mached = true;
            while (mached) {
                mached = false;
                txt = txt.replace(truetabsre2, function (m, text, tabs) {
                    mached = true;
                    return text + GetTrueTabs(tabs.length * 4 - text.length % 4);
                });
            }
            return txt;
        }
        function extend_run(arr, obj, syntax, decorator) {
            var i;
            syntax = obj[syntax];
            return arr.each(function () {
                i = $(this);
                i.html(syntax.call(obj, i.text(), decorator));
            });
        }

        function setContent(content) { 
            document.getElementById('editor').innerHTML = DecorateXML(content);
        }

        function getContent() { 
            return document.getElementById('editor').innerText; 
        }

        function setFontSize(size) { 
            document.getElementById('editor').style.fontSize = size+'%'; 
        }

        function decorate() {
            var element = document.getElementById('editor');
            element.innerHTML = DecorateXML(element.innerText);
        }
    </script>
</head>
<body>
    <div style='width:2000px; overflow-x:scroll; overflow-y:scroll; font-size:80%' id='editor' contenteditable='true' ></div>
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
            Windows.UI.ViewManagement.InputPane.GetForCurrentView().Hiding += (s, args) => { webBrowser.InvokeScript("decorate"); };
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