using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Info;
using Windows.System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.Activation;
using WPTweaker.Resources;
using System.Reflection;
using Microsoft.Phone.Net.NetworkInformation;

namespace WPTweaker
{
    public partial class MainPage : PhoneApplicationPage
    {
        AppSettings _settings = new AppSettings();
        List<Tweak> _tweaks = new List<Tweak>();
        bool _isSamsung = false;
        int _rebootCounter = 0;
        Uri _tweakListUri = new Uri("https://raw.githubusercontent.com/sensboston/WPTweaker/master/WPTweaker/Tweaks.xml");
        ThemeManager _themeManager = new ThemeManager();

        public MainPage()
        {
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
        }

        /// <summary>
        /// Run storage cleanup task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_settings.CleanupStorage)
            {
                Task.Run(() =>
                {
                    string[] exts = { ".wma", ".wav", ".mp3" };
                    using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        string[] files = isoStore.GetFileNames();
                        foreach (var fileName in files)
                        {
                            if (exts.Contains(Path.GetExtension(fileName)))
                            {
                                try { isoStore.DeleteFile(fileName); }
                                catch { }
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
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

                if (_settings.RunCount++ == 5)
                {
                    if (MessageBox.Show("Would you like to support this project by installing and rating \"5 stars\" my apps from the store? "+
                                        "It will take less than 5 minutes of your time...\n\n" +
                                        "I hope you will like these apps ☺\n\n" +
                                        "Press [OK] to open store or [Cancel] to igonre this note", "Developer's note", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        await Launcher.LaunchUriAsync(new Uri("ms-windows-store:search?keyword=senssoft"));
                    }
                }

                if (_settings.CheckTweaks) CheckTweakListUpdate();
                ParseTweaksXml();
                BuildUI();
            }
            else if (e.NavigationMode == NavigationMode.Back && PhoneApplicationService.Current.State.ContainsKey("reload") && PhoneApplicationService.Current.State["reload"].ToString().Equals("True"))
            {
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
                // Save settings
                if (string.IsNullOrEmpty(_settings.XmlTweaks)) _settings.XmlTweaks = xmlDoc.ToString();

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

        void TweakValueChanged(object sender, string hashedKeys)
        {
            var senderTweak = sender as Tweak;
            if (senderTweak != null)
            {
                foreach (var tweak in _tweaks)
                {
                    if (senderTweak != tweak) tweak.CheckForUpdate(hashedKeys);
                    else _rebootCounter += tweak.RequireReboot * (tweak.IsChanged ? 1 : -1);
                }
            }
        }

        void BuildUI()
        {
            List<SolidColorBrush> brushes = new List<SolidColorBrush>();
            brushes.Add(Application.Current.Resources["TweakEvenBackgroundBrush"] as SolidColorBrush);
            brushes.Add(Application.Current.Resources["TweakOddBackgroundBrush"] as SolidColorBrush);
            SystemTray.ProgressIndicator.IsVisible = true;

            LayoutRoot.Items.Clear();
            var categories = _tweaks.Select(t => t.Category).Distinct().ToList();
            categories.Add("Sounds");
            if (_settings.SortTweaks) categories = categories.OrderBy(t => t).ToList();
            foreach (var category in categories)
            {
                var pivotItem = new PivotItem() { Header = category };
                var content = new StackPanel();

                // Special event notification sounds tweaks
                if (category.Equals("Sounds"))
                {
#if ARM
                    string[] subKeyNames = null;
                    if (Registry.NativeRegistry.GetSubKeyNames(Registry.RegistryHive.HKLM, "Software\\Microsoft\\EventSounds\\Sounds", out subKeyNames))
#else
                    string[] subKeyNames = EmulatorData.NotificationEventValues.Keys.ToArray();
#endif
                    {
                        if (subKeyNames != null)
                        {
                            foreach (var str in subKeyNames)
                            {
#if ARM
                                string readStr = string.Empty;
                                if (Registry.NativeRegistry.ReadString(Registry.RegistryHive.HKLM, string.Concat("SOFTWARE\\Microsoft\\EventSounds\\Sounds\\", str), "Sound", out readStr))
                                {
#else
                                    string readStr = EmulatorData.NotificationEventValues[str];
#endif
                                    var btn = new Button()
                                    {
                                        Content = str,
                                        Tag = str,
                                        Margin = new Thickness(4, 2, 4, 2),
                                        Height = 90
                                    };
                                    btn.Click += button_Click;
                                    btn.IsEnabled = readStr.Length > 3;
                                    content.Children.Add(btn);
#if ARM
                                }
#endif
                            }
                        }
                    }
                }
                else
                {
                    var tweaksByCategory = _tweaks.Where(t => t.Category.Equals(category));
                    if (_settings.SortTweaks) tweaksByCategory = tweaksByCategory.OrderBy(t => t.Name);
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
                            case TweakType.Color: tweakControl = new ColorTweak(tweak); break;
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
                }
                pivotItem.Content = new ScrollViewer() { Content = content };
                LayoutRoot.Items.Add(pivotItem);
            }
            SystemTray.ProgressIndicator.IsVisible = false;
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
#if ARM
                string str = string.Empty;
                if (Registry.NativeRegistry.ReadString(Registry.RegistryHive.HKLM, string.Concat("SOFTWARE\\Microsoft\\EventSounds\\Sounds\\", ((Button)sender).Tag), "Sound", out str))
#else
                string str = EmulatorData.NotificationEventValues[((Button)sender).Tag as string];
#endif
                {
                    var ringtoneChooser = new RingtoneChooser() { SelectedRingtone = str };
                    var msgBox = new CustomMessageBox()
                    {
                        Tag = ((Button)sender).Tag,
                        Caption = string.Format("choose sound notification\nfor event \"{0}\"", ((Button)sender).Tag),
                        Content = ringtoneChooser,
                        LeftButtonContent = "choose",
                        RightButtonContent = "cancel",
                        IsFullScreen = true,
                    };

                    // Set background only for custom theme (not for default)
                    if ((Application.Current.Resources["PageHeaderBackgroundBrush"] as SolidColorBrush) !=
                        (Application.Current.Resources["PhoneBackgroundColor"] as SolidColorBrush))
                    {
                        msgBox.Background = Application.Current.Resources["PageHeaderBackgroundBrush"] as SolidColorBrush;
                    }

                    msgBox.Dismissed += (object boxSender, DismissedEventArgs ea) =>
                        {
                            if (ea.Result == 0)
                            {
                                var tag = ((CustomMessageBox)boxSender).Tag as string;
                                var value = ((RingtoneChooser)((CustomMessageBox)boxSender).Content).SelectedRingtone;
                                var regEntry = new RegistryEntry(@"HKLM\SOFTWARE\Microsoft\EventSounds\Sounds\"+tag, "Sound", RegDataType.REG_SZ);
                                regEntry.Value = value.Equals("none") ? string.Empty : value;
                            }
                        };

                    msgBox.Show();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        #region Command bar buttons commands

        private async void CheckTweakListUpdate()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                var req = HttpWebRequest.Create(_tweakListUri);
                req.Method = "HEAD";
                try
                {
                    WebResponse resp = await req.GetResponseAsync();
                    // 196 bytes difference it's a headers size. Let's add a few more bytes for CR/LF, tabs etc. (non-significant changes)
                    if (resp.ContentLength > 0 && Math.Abs(resp.ContentLength - _settings.XmlTweaks.Length) > 210)
                    {
                        if (MessageBox.Show("Would you like to download new list?", "Tweak list update found", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            SyncButton_Click(this, null);
                        }
                    }
                }
                // Lets be silent in case of exception...
                catch { }
            }
        }

        /// <summary>
        /// Download updated tweaks from the project's uri
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncButton_Click(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
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
                        SystemTray.ProgressIndicator.IsVisible = false;
                    };
                webClient.DownloadStringAsync(_tweakListUri);
                SystemTray.ProgressIndicator.IsVisible = true;
            }
            else MessageBox.Show("Network is not available!", "Error", MessageBoxButton.OK);
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

        private void EditButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Controls/XmlEditorPage.xaml", UriKind.Relative));
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
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
        #endregion
    }
}