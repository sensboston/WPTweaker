namespace System.Windows.Media
{
    public static class ColorExtensions
    {
        public static int ToArgb(this Color color)
        {
            int argb = color.A << 24;
            argb += color.R << 16;
            argb += color.G << 8;
            argb += color.B;
            return argb;
        }
        public static Color FromArgb(int intValue)
        {
            return Color.FromArgb(0xFF, (byte)(intValue & 0xFF), (byte)((intValue & 0xFF00) >> 8), (byte)((intValue & 0xFF0000) >> 16));
        }
    }
}