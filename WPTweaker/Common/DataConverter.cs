using System;
using System.Linq;
using System.Windows.Media;
using System.Globalization;

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
                    return Convert.ToUInt32(string.IsNullOrEmpty(data)?"0" : data, 16);

                case RegDataType.REG_QWORD:
                    return Convert.ToUInt64(string.IsNullOrEmpty(data) ? "0" : data, 16);

                case RegDataType.REG_BINARY:
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
                    return string.Empty; // Enumerable.Range(0, data.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(data.Substring(x, 2), 16)).ToArray();

                default:
                    return string.Empty;
            }
        }

        public static Color FromString(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Replace("#", "");
            byte a = 255, r, g , b;
            int start = 0;

            // handle ARGB strings (8 characters long)
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                start = 2;
            }

            //convert RGB characters to bytes
            r = byte.Parse(hex.Substring(start, 2), NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(start + 2, 2), NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(start + 4, 2), NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }
    }
}
