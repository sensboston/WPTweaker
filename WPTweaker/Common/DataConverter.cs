using System;
using System.Linq;

namespace WPTweaker
{
    public class DataConverter
    {
        public static dynamic FromString(string data, RegDataType dataType)
        {
            switch (dataType)
            {
                case RegDataType.REG_SZ:
                case RegDataType.REG_MULTI_SZ:
                    return data;

                case RegDataType.REG_DWORD:
                    return Convert.ToUInt32(string.IsNullOrEmpty(data) ? "0" : data, 16);

                case RegDataType.REG_QWORD:
                    return Convert.ToUInt64(string.IsNullOrEmpty(data) ? "0" : data, 16);

                case RegDataType.REG_BINARY:
                    data = data.Replace("-", "");
                    return Enumerable.Range(0, data.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(data.Substring(x, 2), 16)).ToArray();

                default:
                    return 0;
            }
        }

        public static string ToString(object data, RegDataType dataType)
        {
            switch (dataType)
            {
                case RegDataType.REG_SZ:
                    return data as string;

                case RegDataType.REG_MULTI_SZ:
                    return data as string;

                case RegDataType.REG_DWORD:
                    return String.Format("{0:X}", data);

                case RegDataType.REG_QWORD:
                    return String.Format("{0:X}", data);

                case RegDataType.REG_BINARY:
                    return BitConverter.ToString(data as byte[], 0);

                default:
                    return string.Empty;
            }
        }
    }
}