namespace HomeController.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Windows.UI;
    using Q42.HueApi;

    internal static class ColorGradients
    {
        internal static readonly Dictionary<string, ColorGradient> Gradients = new Dictionary
            <string, ColorGradient>
        {
            { "Crimson", new ColorGradient("FF5E3A", "FF2A68") },
            { "Orange", new ColorGradient("FF9500", "FF5E3A") },
            { "Sunshine", new ColorGradient("FFDB4C", "FFCD02") },
            { "Forest", new ColorGradient("87FC70", "0BD318") },
            { "Waves", new ColorGradient("52EDC7", "5AC8FB") },
            { "Sky", new ColorGradient("1AD6FD", "1D62F0") },
            { "Sleek", new ColorGradient("4A4A4A", "2B2B2B") },
            { "Aqua", new ColorGradient("55EFCB", "5BCAFF") },
            { "Purple", new ColorGradient("C644FC", "5856D6") },
            { "Pink", new ColorGradient("EF4DB6", "C643FC") },
            { "Snow", new ColorGradient("DBDDDE", "898C90") }
        };
    }

    internal class ColorGradient
    {
        internal ColorGradient(string startColorHex, string endColorHex)
        {
            this.StartColorHex = startColorHex;
            this.EndColorHex = endColorHex;
        }

        internal string StartColorHex { get; set; }

        internal string EndColorHex { get; set; }
    }

    internal static class ColorGradientExtensions
    {
        internal static IList<RGBColor> GetColorsForLights(this ColorGradient gradient, int colorsCount)
        {
            var colors = new List<RGBColor>();

            var startColor = GetColorFromHexColorCode(gradient.StartColorHex);
            var endColor = GetColorFromHexColorCode(gradient.EndColorHex);

            // First convert normalize to double and then divide by the amount of colors we need to output
            var redChange = (double)(endColor.R - startColor.R) / byte.MaxValue / colorsCount;
            var greenChange = (double)(endColor.G - startColor.G) / byte.MaxValue / colorsCount;
            var blueChange = (double)(endColor.B - startColor.B) / byte.MaxValue / colorsCount;

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

        private static Color GetColorFromHexColorCode(string colorCode)
        {
            int argb = Int32.Parse(colorCode, NumberStyles.HexNumber);
            return Color.FromArgb(0, (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
        }
    }
}
