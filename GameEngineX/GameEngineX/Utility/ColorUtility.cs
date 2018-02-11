using System.Drawing;

namespace GameEngineX.Utility {
    public static class ColorUtility {
        public static Color LerpColors(Color c1, Color c2, float t) {
            float a = c1.A * (1 - t) + c2.A * t;
            float r = c1.R * (1 - t) + c2.R * t;
            float g = c1.G * (1 - t) + c2.G * t;
            float b = c1.B * (1 - t) + c2.B * t;

            return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
        }

        public static string ColorToHex(Color color) {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2") + color.A.ToString("X2");
        }

        public static Color HexToColor(string hex) {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            byte a = 255;
            if (hex.Length == 8)
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }
    }
}