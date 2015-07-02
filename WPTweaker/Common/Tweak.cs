using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;

namespace WPTweaker
{
    public class TweakNames { public static string[] TweakTypeNames = { "toggle", "input", "enum", "onetime" }; }

    public enum TweakType
    {
        Toggle = 0,
        Input = 1,
        Enum = 2,
    }

    public class RegValue
    {
        public dynamic Value { set; get; }
        public string DisplayName { get; set; }
        public RegValue (object value, string displayName) { Value = value; DisplayName = displayName; }
    }

    public class TweakEntry
    {
        public RegistryEntry RegEntry { get; set;}
        public List<RegValue> Values = new List<RegValue>();
        public TweakEntry(RegistryEntry regEntry, List<RegValue> values) { RegEntry = regEntry; Values = values; }
    }

    public class Tweak
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TweakType Type { get; set; }
        private List<TweakEntry> Entries = new List<TweakEntry>();
        public int RequireReboot { get; private set; }

        public delegate void ValueChangedHandler(object sender, string hashedKeys);
        public event ValueChangedHandler ValueChanged;

        public Tweak(XElement xml)
        {
            if (xml.Attribute("category") == null) throw new AttributeMissingException("category", "tweak");
            if (xml.Attribute("name") == null) throw new AttributeMissingException("name", "tweak");
            if (xml.Attribute("type") == null) throw new AttributeMissingException("type","tweak");

            Category = xml.Attribute("category").Value;
            Name = xml.Attribute("name").Value;
            try { Type = (TweakType)Array.IndexOf(TweakNames.TweakTypeNames, xml.Attribute("type").Value); }
            catch { throw new Exception(string.Format("Error: unknown \"type\" value for the tweak \"{0}\"", Name)); }
            Description = xml.Attribute("description") != null ? xml.Attribute("description").Value : string.Empty;
            RequireReboot = (xml.Attribute("reboot") != null && xml.Attribute("reboot").Value.Equals("true")) ? 1 : 0;

            var xmlEntries = xml.Descendants("entry");
            if (xmlEntries == null || xmlEntries.Count() == 0) throw new Exception(string.Format("Error: no registry entries found for the tweak \"{0}\"", Name));

            foreach (var xmlEntry in xmlEntries)
            {
                if (xmlEntry.Attribute("path") == null) throw new AttributeMissingException("path", "entry");
                if (xmlEntry.Attribute("name") == null) throw new AttributeMissingException("name", "entry");
                if (xmlEntry.Attribute("type") == null) throw new AttributeMissingException("type", "entry");

                string entryPath = xmlEntry.Attribute("path").Value;
                string entryName = xmlEntry.Attribute("name").Value;
                string entryType = xmlEntry.Attribute("type").Value;
                string entryDefault = xmlEntry.Attribute("default") != null ? xmlEntry.Attribute("default").Value : string.Empty;
                string entryComparer = xmlEntry.Attribute("comparer") != null ? xmlEntry.Attribute("comparer").Value : string.Empty;
                Int64 min = Int64.MinValue;
                Int64 max = Int64.MaxValue;
                if (Type == TweakType.Input)
                {
                    if (xmlEntry.Attribute("min") != null) Int64.TryParse(xmlEntry.Attribute("min").Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out min);
                    if (xmlEntry.Attribute("max") != null) Int64.TryParse(xmlEntry.Attribute("max").Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out max);
                }

                RegDataType dataType = RegistryEntry.DataTypeFromString(entryType);
                if (dataType == RegDataType.REG_UNKNOWN) throw new Exception(string.Format("Error, invalid data type \"{0}\" for the entry value", entryType));

                var regEntry = new RegistryEntry(entryPath, entryName, dataType, entryDefault, entryComparer, min, max);

                var values = new List<RegValue>();
                var xmlValues = xmlEntry.Descendants("value");
                if (xmlValues == null) throw new Exception("Error: no values found for the registry entry");
                else if (xmlValues.Count() > 1 && Type == TweakType.Input) throw new Exception(string.Format("Error: too many values for the tweak type {0}", Type));
                else
                {
                    foreach (var xmlValue in xmlValues)
                    {
                        string valueDisplayName = (xmlValue.Attribute("name") != null) ? xmlValue.Attribute("name").Value : string.Empty;
                        values.Add(new RegValue(DataConverter.FromString(xmlValue.Value, dataType), valueDisplayName));
                    }
                }

                Entries.Add(new TweakEntry(regEntry, values));
            }
        }

        public dynamic Value
        {
            get
            {
                switch (Type)
                {
                    case TweakType.Toggle: 
                        return Entries.Count(e => e.RegEntry.IsSet) == Entries.Count;

                    case TweakType.Enum:
                    case TweakType.Input:
                        return Entries.First().RegEntry.Value;
                }
                return false;
            }

            set
            {
                switch (Type)
                {
                    case TweakType.Toggle:
                        if (value is bool)
                            foreach (var entry in Entries) entry.RegEntry.Value = value ? entry.Values.First().Value : entry.RegEntry.DefaultValue;
                        break;

                    case TweakType.Enum:
                    case TweakType.Input:
                        Entries.First().RegEntry.Value = DataConverter.FromString(value is string ? value : value.ToString("X"), Entries.First().RegEntry.DataType);
                        break;
                }

                if (ValueChanged != null)
                {
                    ValueChanged(this, Entries.Select(i => i.RegEntry.GetHashCode().ToString()).Aggregate((i, j) => i + ';' + j));
                }
            }
        }

        public List<RegValue> EnumList 
        { 
            get 
            { 
                return Entries.First().Values; 
            } 
        }

        public dynamic DefaultValue
        {
            get
            {
                return Entries.First().RegEntry.DefaultValue;
            }
        }

        public string ValueToString 
        { 
            get 
            {
                return DataConverter.ToString(Value, Entries.First().RegEntry.DataType);
            }
        }

        public string DefaultValueToString
        {
            get
            {
                return DataConverter.ToString(DefaultValue, Entries.First().RegEntry.DataType);
            }
        }

        public bool CheckForUpdate(string hashes)
        {
            if (hashes != null)
            {
                var hashList = hashes.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var entry in Entries)
                    if (hashList.Contains(entry.RegEntry.GetHashCode().ToString()))
                    {
                        if (ValueChanged != null) ValueChanged(this, null);
                        return true;
                    }
            }
            return false;
        }

        public RegDataType RegistryDataType
        {
            get
            {
                return (Entries != null && Entries.Count > 0) ? Entries.First().RegEntry.DataType : RegDataType.REG_UNKNOWN;
            }
        }

        public Int64 Min
        {
            get
            {
                return (Entries != null && Entries.Count > 0) ? Entries.First().RegEntry.Min : Int64.MinValue;
            }
        }

        public Int64 Max
        {
            get
            {
                return (Entries != null && Entries.Count > 0) ? Entries.First().RegEntry.Max : Int64.MaxValue;
            }
        }
    }
}
