// Used source code from https://github.com/jessenic/wph-tweaks/blob/master/HomebrewHelperWP/Filesystem/RingtoneChooser.xaml.cs
// by Jaxbot, by jessenic
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using Microsoft.Phone.Shell;

namespace WPTweaker
{
	public partial class RingtoneChooser : UserControl
    {
		public string SelectedRingtone { get; set; }

        IsolatedStorageFile _isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        string _isoRootPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;

		public RingtoneChooser()
		{
			this.InitializeComponent();
		}

        private void Image_Tap(object sender, GestureEventArgs e)
        {
            var file = (RingtoneListItem)((Image)sender).DataContext;
            if (file != null && !file.DisplayName.Contains("none"))
            {
#if ARM
                string fileName = Path.GetFileName(file.FullPath);
                try
                {
                    // Copy file to the isf
                    if (!_isoStore.FileExists(fileName))
                    {
                        SystemTray.ProgressIndicator.IsVisible = true;
                        File.Copy(file.FullPath, Path.Combine(_isoRootPath, fileName), true);
                    }
                    using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(fileName, FileMode.Open, _isoStore))
                    {
                        mediaElement.SetSource(isoStream);
                        mediaElement.Play();
                        SystemTray.ProgressIndicator.IsVisible = false;
                    }
                }
                catch { }
#else
                MessageBox.Show(file.FullPath);
#endif
            }
        }

        private void RingtoneChooser_Loaded(object sender, RoutedEventArgs e)
        {
            RingtoneListItem item = new RingtoneListItem("none");
            RingtoneList.Items.Add(item);
            if (this.SelectedRingtone != null && this.SelectedRingtone.ToLower().Equals("none"))
            {
                this.RingtoneList.SelectedItem = item;
            }
#if ARM
            string[] sourceDirs = new string[] { "C:\\Data\\Users\\Public\\Ringtones", "C:\\Programs\\CommonFiles\\Sounds" };
#else
            string[] sourceDirs = new string[] { "C:\\Programs\\CommonFiles\\Sounds" };
#endif
            foreach (var dir in sourceDirs)
            {
#if ARM
                string[] files = Directory.GetFiles(dir, "*.*");
#else
                string[] files = EmulatorData.SoundFiles.Select(f => Path.Combine(dir, f)).ToArray();
#endif
                foreach (var str in files)
                {
                    item = new RingtoneListItem(str);
                    RingtoneList.Items.Add(item);
                    if (this.SelectedRingtone != null && str.Equals(this.SelectedRingtone, StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.RingtoneList.SelectedItem = item;
                    }
                }
            }
        }

		private void RingtoneList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (RingtoneList.SelectedItem != null)
			{
				SelectedRingtone = ((RingtoneListItem)this.RingtoneList.SelectedItem).FullPath;
			}
		}

        public class RingtoneListItem
        {
            public string DisplayName { get; set; }
			public string FullPath { get; set; }
            public string ImageSource
            {
                get { return DisplayName.Contains("none") ? "/Assets/AppBar/none.png" : "/Assets/AppBar/play.png"; }
            }
            public RingtoneListItem(string fullPath) { DisplayName = Path.GetFileNameWithoutExtension(fullPath); FullPath = fullPath; }
        }
	}
}