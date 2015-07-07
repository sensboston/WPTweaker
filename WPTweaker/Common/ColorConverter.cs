using System;
using System.Windows.Media;
using System.Globalization;

namespace System.Windows.Media
{
    public static class ColorExtensions
    {
        public static uint ToArgb(this Color color)
        {
            return (uint) (color.A << 24 | color.R << 16 | color.G << 8 | color.B);
        }

        public static Color FromArgb(uint value)
        {
            return Color.FromArgb(0xFF, (byte)((value & 0xFF0000) >> 16), (byte)((value & 0xFF00) >> 8), (byte)(value & 0xFF));
        }

        public static Color FromString(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Replace("#", "");
            return FromArgb(Convert.ToUInt32(hex, 16));
        }
    }
}