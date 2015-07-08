using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace WPTweaker
{
    class UriMapper : UriMapperBase
    {
        public override Uri MapUri(Uri uri)
        {
            string url = System.Net.HttpUtility.UrlDecode(uri.ToString());
            if (url.Contains("wptweaker:"))
            {
                // Map Uri to the MainPage.xaml
                return new Uri("/MainPage.xaml?" + url.Substring(url.IndexOf("wptweaker:") + "wptweaker:".Length), UriKind.Relative);
            }
            // Otherwise perform normal launch.
            return uri;
        }
    }
}
