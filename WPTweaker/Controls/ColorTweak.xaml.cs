using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;


namespace WPTweaker
{
    public partial class ColorTweak : UserControl
    {
        private Tweak _tweak;
        public ColorTweak(Tweak tweak)
        {
            InitializeComponent();
            _tweak = tweak;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/Controls/ColorEditorPage.xaml", UriKind.Relative));
        }
    }
}
