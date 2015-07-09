using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
#if ARM
using OEMSharedFolderAccessLib;
using RPCComponent;
using DevProgram;
#endif

namespace WPTweaker
{
    public enum RegDataType
    {
        REG_SZ = 1,
        REG_BINARY = 3,
        REG_DWORD = 4,
        REG_MULTI_SZ = 7,
        REG_QWORD = 11,
        REG_UNKNOWN = 255
    }

    public enum RegistryHive
    {
        HKEY_CLASSES_ROOT = 0,
        HKEY_LOCAL_MACHINE = 1,
        HKEY_CURRENT_USER = 2,
        HKEY_CURRENT_CONFIG = 5
    }

    public enum Hives : uint
    {
        HKCR = 2147483648,
        HKCU = 2147483649,
        HKLM = 2147483650,
        HCCC = 2147483653,
        HKU = 2147483651
    }

    public class RegistryEntry
    {
        private static bool _useDevProgram = false;
#if ARM
        private static COEMSharedFolder _rpc = null;
#else
        private AppSettings _appSettings = new AppSettings();
#endif
        public RegistryHive Hive { get; set; }

        private static Dictionary<RegistryHive, Hives> _hives = new Dictionary<RegistryHive, Hives>
        {
            { RegistryHive.HKEY_CLASSES_ROOT, Hives.HKCR },
            { RegistryHive.HKEY_LOCAL_MACHINE, Hives.HKLM },
            { RegistryHive.HKEY_CURRENT_USER, Hives.HKCU },
            { RegistryHive.HKEY_CURRENT_CONFIG, Hives.HCCC }
        };
        public uint RegHive { get { return (uint) _hives[Hive]; } }
        public string KeyPath { get; set; }
        public string KeyName { get; set; }
        public RegDataType DataType { get; set; }
        public dynamic DefaultValue { get; set; }
        public string Comparer { get; set; }
        public Int64 Min { get; set; }
        public Int64 Max { get; set; }
        private bool _firstTime = true;
        private dynamic _originalValue;
        public static bool IsInteropUnlocked()
        {
#if ARM
            if (_rpc == null && _useDevProgram == false)
            {
                _rpc = new COEMSharedFolder();
                try
                {
                    var retCode = _rpc.RPC_Init();
                    if (retCode != 1) _rpc = null;
                }
                catch
                {
                    _rpc = null;
                }

                if (_rpc == null)
                {
                    CRPCComponent.Initialize();
                    if (DevProgramReg.IsDeviceUnlocked())
                    {
                        return _useDevProgram = true;
                    }
                }
                else return true;
            }
            return (_rpc != null) || (_useDevProgram);
#else
            return true;
#endif
        }
        public RegistryEntry(string fullPath, string keyName, RegDataType dataType, object defaultValue, string comparer, Int64 min = Int64.MinValue, Int64 max = Int64.MaxValue)
        {
            if (fullPath.StartsWith("HKEY_CLASSES_ROOT") || fullPath.StartsWith("HKCR")) Hive = RegistryHive.HKEY_CLASSES_ROOT;
            else if (fullPath.StartsWith("HKEY_LOCAL_MACHINE") || fullPath.StartsWith("HKLM")) Hive = RegistryHive.HKEY_LOCAL_MACHINE;
            else if (fullPath.StartsWith("HKEY_CURRENT_USER") || fullPath.StartsWith("HKCU")) Hive = RegistryHive.HKEY_CURRENT_USER;
            else if (fullPath.Contains("HKEY_CURRENT_CONFIG") || fullPath.Contains("HKCC")) Hive = RegistryHive.HKEY_CURRENT_CONFIG;
            KeyPath = fullPath.Substring(fullPath.IndexOf('\\') + 1);
            KeyName = keyName;
            DataType = dataType;
            DefaultValue = defaultValue;
            Comparer = comparer;
            Min = min;
            Max = max;
            IsInteropUnlocked();
        }

        public bool IsSet 
        { 
            get 
            { 
                // Default comparer is "!="
                if (string.IsNullOrEmpty(Comparer)) return !Value.Equals(DefaultValue);
                else if (Comparer.Equals(">")) return Value > DefaultValue;
                else if (Comparer.Equals("<")) return Value < DefaultValue;
                else if (Comparer.Equals("==")) return Value == DefaultValue;
                else return !Value.Equals(DefaultValue);
            } 
        }

        public bool IsChanged 
        { 
            get 
            { 
                return Value != _originalValue; 
            } 
        }

        public void Reset() { Value = DefaultValue; }

        public dynamic Value
        {
            get
            {
                string result = string.Empty;
                uint retVal = 0;
                try
                {
#if ARM
                    // unlocked Samsungs
                    if (_useDevProgram)
                    {
                        switch (DataType)
                        {
                            case RegDataType.REG_DWORD:
                                CRPCComponent.Registry_GetDWORD(RegHive, KeyPath, KeyName, out retVal);
                                result = retVal.ToString("X");
                                break;

                            case RegDataType.REG_SZ:
                                result = CRPCComponent.Registry_GetString(RegHive, KeyPath, KeyName, out retVal);
                                break;

                            case RegDataType.REG_QWORD:
                            case RegDataType.REG_MULTI_SZ:
                            case RegDataType.REG_BINARY:
                                throw new NotSupportedException();
                        }
                    }
                    // unlocked Lumias
                    else
                    {
                        if (_rpc != null) result = _rpc.rget((uint)Hive, KeyPath, KeyName, (uint)DataType);
                    }
#else
                    // Use AppSettings to emulate registry changes (if running on phone emulator)
                    switch (DataType)
                    {
                        case RegDataType.REG_DWORD:
                            retVal = _appSettings.GetValueOrDefault(GetHashCode().ToString(), retVal);
                            result = retVal.ToString("X");
                            break;

                        case RegDataType.REG_SZ:
                            result = _appSettings.GetValueOrDefault(GetHashCode().ToString(), result);
                            break;

                        case RegDataType.REG_QWORD:
                            UInt64 retVal64 = 0;
                            retVal64 = _appSettings.GetValueOrDefault(GetHashCode().ToString(), retVal64);
                            result = retVal64.ToString("X");
                            break;

                        case RegDataType.REG_MULTI_SZ:
                            List<string> listResult = new List<string>();
                            listResult = _appSettings.GetValueOrDefault(GetHashCode().ToString(), listResult);
                            result = String.Join(";", listResult);
                            break;

                        case RegDataType.REG_BINARY:
                            byte[] byteResult = new byte[0];
                            byteResult = _appSettings.GetValueOrDefault(GetHashCode().ToString(), byteResult);
                            result = BitConverter.ToString(byteResult).Replace("-", "");
                            break;
                    }
#endif
                }
                finally
                {
                    Debug.WriteLine(string.Format("Reading registry key {0}\non path {1}\nreturned {2}\n", KeyName, string.Concat(Hive, "\\", KeyPath), result));
                }

                if (_firstTime)
                {
                    _firstTime = false;
                    _originalValue = DataConverter.FromString(result, DataType);
                }

                return DataConverter.FromString(result, DataType);
            }

            set
            {
                if (value == null) return;
#if ARM
                if (_rpc == null && _useDevProgram == false) return;
                uint retVal = 0;
#endif
                string buffer = string.Empty;

                switch (DataType)
                {
                    case RegDataType.REG_SZ:
                        if (!(value is string)) throw new InvalidArgumentTypeException("Value must be a string");
                        buffer = value;
#if ARM
                        if (_useDevProgram) CRPCComponent.Registry_SetString(RegHive, KeyPath, KeyName, value, out retVal);
#endif
                        break;

                    case RegDataType.REG_MULTI_SZ:
                        if (!(value is string[] || value is List<string>)) throw new InvalidArgumentTypeException("Value must be a semicolon separated string or List<string> type");
                        if (_useDevProgram) throw new NotSupportedException();
                        else buffer = (value is List<string>) ? String.Join(";", value) : value;
                        break;

                    case RegDataType.REG_DWORD:
                        try { buffer = ((UInt32)value).ToString("X8"); }
                        catch { throw new InvalidArgumentTypeException("Value must be an unsigned integer type"); }
#if ARM
                        if (_useDevProgram) CRPCComponent.Registry_SetDWORD(RegHive, KeyPath, KeyName, value, out retVal);
#endif
                        break;

                    case RegDataType.REG_QWORD:
                        try { buffer = ((UInt64)value).ToString("X16"); }
                        catch { throw new InvalidArgumentTypeException("Value must be an unsigned integer type"); }
                        if (_useDevProgram) throw new NotSupportedException();
                        break;

                    case RegDataType.REG_BINARY:
                        buffer = BitConverter.ToString(value).Replace("-", "");
                        if (_useDevProgram) throw new NotSupportedException();
                        break;
                }
#if ARM
                if (!_useDevProgram) _rpc.rset((uint)Hive, KeyPath, KeyName, (uint)DataType, buffer, 0);
#else
                _appSettings.AddOrUpdateValue(GetHashCode().ToString(), value);
#endif
                Debug.WriteLine(string.Format("Value {0} of type {1}\nis written to the registry key {2}\non path {3}\n", buffer, DataType, KeyName, string.Concat(Hive, "\\", KeyPath)));
            }
        }

        public static RegDataType DataTypeFromString(string dataType)
        {
            string type = dataType.ToLower();
            if (type.Equals("string")) return RegDataType.REG_SZ;
            else if (type.Equals("strings")) return RegDataType.REG_MULTI_SZ;
            else if (type.Equals("binary")) return RegDataType.REG_BINARY;
            else if (type.Equals("dword")) return RegDataType.REG_DWORD;
            else if (type.Equals("qword")) return RegDataType.REG_QWORD;
            else return RegDataType.REG_UNKNOWN;
        }

        public override int GetHashCode()
        {
            return string.Concat(Hive.ToString(), KeyPath, KeyName).GetHashCode();
        }
    }
}
