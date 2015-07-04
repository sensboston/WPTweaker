using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.ComponentModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Info;
using Windows.System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.Activation;
using WPTweaker.Resources;

namespace WPTweaker
{
    public partial class MainPage : PhoneApplicationPage
    {
        AppSettings _settings = new AppSettings();
        List<Tweak> _tweaks = new List<Tweak>();
        bool _isSamsung = false;
        int _rebootCounter = 0;
        Uri _tweakListUri = new Uri("https://raw.githubusercontent.com/sensboston/WPTweaker/master/WPTweaker/Tweaks.xml");

        public MainPage()
        {
            ParseTheme();
            InitializeComponent();

            App.Current.UnhandledException += (object sender, ApplicationUnhandledExceptionEventArgs e) =>
            {
                MessageBox.Show(e.ExceptionObject.Message, "Exception", MessageBoxButton.OK);
                e.Handled = true;
            };

            BackKeyPress += (object sender, CancelEventArgs e) =>
            {
                App.Current.UnhandledException -= null;
                if (_rebootCounter > 0)
                {
                    var message = string.Format("You've applied registry tweak(s) that require a phone reboot.\n\n{0}", 
                        _isSamsung ? "Would you like to reboot now?" : "Don't forget to reboot your phone now!");
                    if (MessageBox.Show(message, "Warning", _isSamsung ? MessageBoxButton.OKCancel : MessageBoxButton.OK) == MessageBoxResult.OK && _isSamsung)
                    {
#if ARM
                        uint retCode;
                        RPCComponent.CRPCComponent.System_Reboot(out retCode);
                        App.Current.Terminate();
#endif
                    }
                }
            };

            PhoneApplicationService.Current.ContractActivated +=  Application_ContractActivated;
            _isSamsung = DeviceStatus.DeviceManufacturer.ToLower().Contains("samsung");

            (ApplicationBar.MenuItems[2] as ApplicationBarMenuItem).Text = string.Format("{0}  sort tweaks", (_settings.SortTweaks ? _checkBoxChar[1] : _checkBoxChar[0]));
            (ApplicationBar.MenuItems[3] as ApplicationBarMenuItem).Text = string.Format("{0}  auto-check tweaks update", (_settings.CheckTweaks ? _checkBoxChar[1] : _checkBoxChar[0]));
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                if (!RegistryEntry.IsInteropUnlocked())
                {
                    MessageBox.Show("RPC component is failed to initialize!\n" +
                                    "Sorry but this application is designed to work with Nokia/Microsoft and Samsung handsets ONLY...\n" +
                                    "Press <OK> to terminate application", "Error", MessageBoxButton.OK);
                    App.Current.Terminate();
                }

                if (_settings.CheckTweaks) CheckTweakListUpdate();
                ParseTweaksXml();
                BuildUI();
            }
        }

        void ParseTweaksXml()
        {
            _tweaks.Clear();
#if !DEBUG
            var xmlDoc = string.IsNullOrEmpty(_settings.XmlTweaks) ? XDocument.Load("Tweaks.xml") : XDocument.Parse(_settings.XmlTweaks);
#else
            var xmlDoc = XDocument.Load("Tweaks.xml");
#endif
            if (xmlDoc != null)
            {
                // Check custom app theme
                if (xmlDoc.Descendants("theme").FirstOrDefault() != null)
                {
                    _settings.Theme = xmlDoc.Descendants("theme").FirstOrDefault().ToString();
                }

                // Load list contributors
                AboutPage.Contributors = xmlDoc.Descendants("contributor").Select(d => d.Value).ToList();

                var tweaks = xmlDoc.Descendants("tweak").ToList();
                if (tweaks != null)
                {
                    string tweakName = string.Empty;
                    foreach (var xmlTweak in tweaks)
                    {
                        tweakName = string.Empty;
                        try
                        {
                            if (xmlTweak.Element("name") != null) tweakName = xmlTweak.Element("name").Value;
                            _tweaks.Add(new Tweak(xmlTweak));
                            _tweaks.Last().ValueChanged += TweakValueChanged;
                        }
                        catch (Exception error)
                        {
                            MessageBox.Show(string.Format("Error adding tweak \"{0}\"\n\n{1}", tweakName, error.Message));
                        }
                    }
                }
            }
            LayoutRoot.Title = string.Format("WPTweaker: {0} tweak{1} available", _tweaks.Count, _tweaks.Count == 1 ? "" : "s");
        }

        void ChangeResource(string resName, string newValue)
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
                        try
                        {
                            var newColor = DataConverter.FromString(newValue);
                            Application.Current.Resources.Add(resName, new SolidColorBrush(newColor));
                        }
                        catch { }
                    }
                }
            }
        }

        void ParseTheme()
        {
            if (!string.IsNullOrEmpty(_settings.Theme))
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
        }

        void TweakValueChanged(object sender, string hashedKeys)
        {
            var senderTweak = sender as Tweak;
            if (senderTweak != null)
            {
                foreach (var tweak in _tweaks)
                {
                    if (senderTweak != tweak && tweak.CheckForUpdate(hashedKeys)) _rebootCounter += tweak.RequireReboot;
                }
            }
        }

        void BuildUI()
        {
            List<SolidColorBrush> brushes = new List<SolidColorBrush>();
            brushes.Add(Application.Current.Resources["TweakEvenBackgroundBrush"] as SolidColorBrush);
            brushes.Add(Application.Current.Resources["TweakOddBackgroundBrush"] as SolidColorBrush);

            LayoutRoot.Items.Clear();
            var categories = _tweaks.Select(t => t.Category).Distinct();
            if (_settings.SortTweaks) categories = categories.OrderBy(t => t);
            foreach (var category in categories)
            {
                var tweaksByCategory = _tweaks.Where(t => t.Category.Equals(category));
                if (_settings.SortTweaks) tweaksByCategory = tweaksByCategory.OrderBy(t => t.Name);
                var pivotItem = new PivotItem() { Header = category };
                var content = new StackPanel();
                int i = 0;
                foreach (var tweak in tweaksByCategory)
                {
                    tweak.Background = brushes[++i % 2];
                    dynamic tweakControl = null;
                    switch (tweak.Type)
                    {
                        case TweakType.Toggle: tweakControl = new ToggleTweak(tweak); break;
                        case TweakType.Enum: tweakControl = new EnumTweak(tweak); break;
                        case TweakType.Input: tweakControl = new InputTweak(tweak); break;
                    }
                    try
                    {
                        tweakControl.DataContext = tweak;
                        content.Children.Add(tweakControl);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, string.Format("Exception on adding tweak \"{0}\"", tweak.Name), MessageBoxButton.OK);
                    }
                }
                pivotItem.Content = new ScrollViewer() { Content = content };
                LayoutRoot.Items.Add(pivotItem);
            }
        }

        #region Command bar buttons commands

        private async void CheckTweakListUpdate()
        {
            var req = HttpWebRequest.Create(_tweakListUri);
            req.Method = "HEAD";
            WebResponse resp = await req.GetResponseAsync();
            if (resp.ContentLength > 0 && Math.Abs(resp.ContentLength - _settings.XmlTweaks.Length) > 20)
            {
                if (MessageBox.Show("Would you like to download new list?", "Tweak list update found", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    SyncButton_Click(this, null);
                }
            }
        }

        /// <summary>
        /// Download updated tweaks from the project's uri
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncButton_Click(object sender, EventArgs e)
        {
            var webClient = new WebClient();
            webClient.DownloadStringCompleted += (object _, DownloadStringCompletedEventArgs args) =>
                {
                    if (args.Error == null)
                    {
                        _settings.XmlTweaks = args.Result;
                        ParseTweaksXml();
                        BuildUI();
                    }
                    else
                    {
                        MessageBox.Show(args.Error.Message);
                    }
                };
            webClient.DownloadStringAsync(_tweakListUri);
        }

        /// <summary>
        /// Open tweaks from the phone's storage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadButton_Click(object sender, EventArgs e)
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".xml");
            openPicker.PickSingleFileAndContinue();
        }

        private void SaveTweaksButton_Click(object sender, EventArgs e)
        {
            var savePicker = new FileSavePicker()
            {
                DefaultFileExtension = ".xml",
                SuggestedFileName = "Tweaks.xml",
            };
            savePicker.FileTypeChoices.Add("XML file", new List<string>() { ".xml" });
            savePicker.PickSaveFileAndContinue();
        }

        private async void Application_ContractActivated(object sender, IActivatedEventArgs e)
        {
            var openArgs = e as FileOpenPickerContinuationEventArgs;
            if (openArgs != null)
            {
                if (openArgs.Files.Count > 0)
                {
                    try
                    {
                        _settings.XmlTweaks = await FileIO.ReadTextAsync(openArgs.Files.First());
                        ParseTweaksXml();
                        BuildUI();
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                    }
                }
            }
            else
            {
                var saveArgs = e as FileSavePickerContinuationEventArgs;
                if (saveArgs != null && saveArgs.File != null)
                {
                    try
                    {
                        CachedFileManager.DeferUpdates(saveArgs.File);
                        var xmlDoc = XDocument.Load("Tweaks.xml");
                        await FileIO.WriteTextAsync(saveArgs.File, xmlDoc.ToString());
                        await CachedFileManager.CompleteUpdatesAsync(saveArgs.File);
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                    }
                }
            }
        }

        private async void DonateButton_Click(object sender, EventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=DDGGVLR6LR72N"));
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void RestoreTweaksButton_Click(object sender, EventArgs e)
        {
            var xmlDoc = XDocument.Load("Tweaks.xml");
            _settings.XmlTweaks = xmlDoc.ToString();
            ParseTweaksXml();
            BuildUI();
        }

        static char[] _checkBoxChar = new char[] {'☐', '☑'};
        private void SortTweaks_Click(object sender, EventArgs e)
        {
            _settings.SortTweaks = !_settings.SortTweaks;
            (sender as ApplicationBarMenuItem).Text = string.Format("{0}  sort tweaks", (_settings.SortTweaks ? _checkBoxChar[1] : _checkBoxChar[0]));
            ParseTweaksXml();
            BuildUI();
        }

        private void AutoCheckTweaks_Click(object sender, EventArgs e)
        {
            _settings.CheckTweaks = !_settings.CheckTweaks;
            (sender as ApplicationBarMenuItem).Text = string.Format("{0}  auto-check tweaks update", (_settings.CheckTweaks ? _checkBoxChar[1] : _checkBoxChar[0]));
            if (_settings.CheckTweaks) CheckTweakListUpdate();
        }

        #endregion
    }
}