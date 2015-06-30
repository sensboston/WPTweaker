using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.System;

namespace WPTweaker
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public static List<string> Contributors = new List<string>();
        public AboutPage()
        {
            InitializeComponent();
            ContributorsList.DataContext = this;
            ContributorsList.ItemsSource = Contributors;
        }

        private async void donateButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=DDGGVLR6LR72N"));
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:search?keyword=senssoft"));
        }
    }
}