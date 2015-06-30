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

        // The default value of our settings
        private const string XmlTweaksSettingDefault = "";

        public AppSettings()
        {
            try
            {
                localSettings = IsolatedStorageSettings.ApplicationSettings;
            }
            catch (Exception)
            {
            }
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
    }
}
