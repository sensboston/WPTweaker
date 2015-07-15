using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;

namespace WPTweaker
{
    public class AppSettings
    {
        IsolatedStorageSettings localSettings = null;

        // The isolated storage key names of our settings
        public const string XmlTweaksSettingKeyName = "XmlTweaks";
        public const string XmlThemesSettingKeyName = "XmlThemes";
        public const string ThemeSettingKeyName = "Theme";
        public const string SortTweaksSettingKeyName = "SortTweaks";
        public const string CheckTweaksSettingKeyName = "CheckTweaks";
        public const string RunCountSettingKeyName = "RunCount";
        public const string CleanupStorageSettingKeyName = "CleanupStorage";
        public const string ThemeNameSettingKeyName = "ThemeName";

        // The default value of our settings
        private const string XmlTweaksSettingDefault = "";
        private const string XmlThemesSettingDefault = "";
        private const string ThemeSettingDefault = "";
        private const bool SortTweaksSettingDefault = false;
        private const bool CheckTweaksSettingDefault = false;
        public const int RunCountSettingDefault = 0;
        private const bool CleanupStorageSettingDefault = false;
        public const string ThemeNameSettingDefault = "Default";

        public AppSettings()
        {
            try { localSettings = IsolatedStorageSettings.ApplicationSettings; }
            catch { }
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;
            try
            {
                // if new value is different, set the new value
                if (localSettings.Contains(Key))
                {
                    if (localSettings[Key] != value)
                    {
                        localSettings[Key] = value;
                        valueChanged = true;
                    }
                }
                else
                {
                    localSettings.Add(Key, value);
                    valueChanged = true;
                }
                localSettings.Save();
            }
            catch
            {
                localSettings.Add(Key, value);
                valueChanged = true;
            }

            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public valueType GetValueOrDefault<valueType>(string Key, valueType defaultValue)
        {
            valueType value;
            try { value = localSettings.Contains(Key) ? (valueType)localSettings[Key] : defaultValue; }
            catch {value = defaultValue; }
            return value;
        }

        public string XmlTweaks
        {
            get { return GetValueOrDefault<string>(XmlTweaksSettingKeyName, XmlTweaksSettingDefault); }
            set { AddOrUpdateValue(XmlTweaksSettingKeyName, value); }
        }

        public string XmlThemes
        {
            get { return GetValueOrDefault<string>(XmlThemesSettingKeyName, XmlThemesSettingDefault); }
            set { AddOrUpdateValue(XmlThemesSettingKeyName, value); }
        }

        public string Theme
        {
            get { return GetValueOrDefault<string>(ThemeSettingKeyName, ThemeSettingDefault); }
            set { AddOrUpdateValue(ThemeSettingKeyName, value); }
        }

        public bool SortTweaks
        {
            get { return GetValueOrDefault<bool>(SortTweaksSettingKeyName, SortTweaksSettingDefault); }
            set { AddOrUpdateValue(SortTweaksSettingKeyName, value); }
        }

        public bool CheckTweaks
        {
            get { return GetValueOrDefault<bool>(CheckTweaksSettingKeyName, CheckTweaksSettingDefault); }
            set { AddOrUpdateValue(CheckTweaksSettingKeyName, value); }
        }

        public int RunCount
        {
            get { return GetValueOrDefault<int>(RunCountSettingKeyName, RunCountSettingDefault); }
            set { AddOrUpdateValue(RunCountSettingKeyName, value); }
        }

        public bool CleanupStorage
        {
            get { return GetValueOrDefault<bool>(CleanupStorageSettingKeyName, CleanupStorageSettingDefault); }
            set { AddOrUpdateValue(CleanupStorageSettingKeyName, value); }
        }

        public string ThemeName
        {
            get { return GetValueOrDefault<string>(ThemeNameSettingKeyName, ThemeNameSettingDefault); }
            set { AddOrUpdateValue(ThemeNameSettingKeyName, value); }
        }
    }
}
