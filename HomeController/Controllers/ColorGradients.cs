namespace HomeController.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Windows.UI;
    using Q42.HueApi;

    public static class ColorGradients
    {
        public static readonly IList<ColorGradient> Gradients = new List
            <ColorGradient>
        {
            new ColorGradient("Crimson", 0xFF5E3A, 0xFF2A68),
            new ColorGradient("Orange", 0xFF9500, 0xFF5E3A),
            new ColorGradient("Sunshine", 0xFFDB4C, 0xFFCD02),
            new ColorGradient("Forest", 0x87FC70, 0x0BD318),
            new ColorGradient("Waves", 0x52EDC7, 0x5AC8FB),
            new ColorGradient("Sky", 0x1AD6FD, 0x1D62F0),
            new ColorGradient("Sleek", 0x4A4A4A, 0x2B2B2B),
            new ColorGradient("Aqua", 0x55EFCB, 0x5BCAFF),
            new ColorGradient("Purple", 0xC644FC, 0x5856D6),
            new ColorGradient("Pink", 0xEF4DB6, 0xC643FC),
            new ColorGradient("Snow", 0xDBDDDE, 0x898C90)
        };
    }

    public class ColorGradient
    {
        internal ColorGradient(string name, int startColor, int endColor)
        {
            this.Name = name;
            this.StartColor = startColor;
            this.EndColor = endColor;
        }

        public string Name { get; }

        internal int StartColor { get; set; }

        internal int EndColor { get; set; }
    }

    public static class ColorGradientExtensions
    {
        public static IList<RGBColor> GetColorsForLights(this ColorGradient gradient, int colorsCount)
        {
            var colors = new List<RGBColor>();

            var startColor = GetColorFromHexColorCode(gradient.StartColor);
            var endColor = GetColorFromHexColorCode(gradient.EndColor);

            // First normalize to double and then divide by the amount of colors we need to output - 1
            var redChange = (double)(endColor.R - startColor.R) / byte.MaxValue / (colorsCount - 1);
            var greenChange = (double)(endColor.G - startColor.G) / byte.MaxValue / (colorsCount - 1);
            var blueChange = (double)(endColor.B - startColor.B) / byte.MaxValue / (colorsCount - 1);

            for (var i = 0; i < colorsCount; i++)
            {
                colors.Add(new RGBColor(
                            (double)startColor.R / byte.MaxValue + redChange*i,
                            (double)startColor.G / byte.MaxValue + greenChange*i,
                            (double)startColor.B / byte.MaxValue + blueChange*i
                            ));
            }

            return colors;
        }

        internal static RGBColor ToRgbColor(this Color color)
        {
            return new RGBColor(
                (double)color.R / byte.MaxValue,
                (double)color.G / byte.MaxValue,
                (double)color.B / byte.MaxValue);
        }

        private static Color GetColorFromHexColorCode(int rgb)
        {
            return Color.FromArgb(0, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }
    }
}
