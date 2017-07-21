using System;
using System.Globalization;
using System.Windows.Data;
using PID_HSV.Util;

namespace PID_HSV.Converter
{
    public class RGBToHSVConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert((byte[])value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack((byte[])value);
        }

        public static byte[] Convert(byte[] bytes)
        {
            var res = new byte[3];
            var b = bytes[0] / 255d;
            var g = bytes[1] / 255d;
            var r = bytes[2] / 255d;

            var min = MathUtil.Min(r, g, b);
            var max = MathUtil.Max(r, g, b);

            var v = max;
            double s;
            double h;
            var delta = max - min;

            if (max != min)
            {
                s = delta / max;
            }
            else
            {
                res[0] = 0;
                res[1] = 0;
                res[2] = (byte)(v * 255);
                return res;
            }

            if (r == max)
                h = (g - b) / delta;
            else if (g == max)
                h = 2 + (b - r) / delta;
            else
                h = 4 + (r - g) / delta;

            h *= 60;

            if (h < 0)
                h += 360;

            res[0] = (byte)(h / 360 * 255);
            res[1] = (byte)(s * 255);
            res[2] = (byte)(v * 255);

            return res;
        }

        public static byte[] ConvertBack(byte[] bytes)
        {
            var h = bytes[0] / 255d * 359;
            var s = bytes[1] / 255d;
            var v = bytes[2] / 255d;

            double r;
            double g;
            double b;

            var c = v * s;
            var hl = h / 60;
            var x = c * (1 - Math.Abs(hl % 2 - 1));

            if (hl >= 0 && hl <= 1)
            {
                r = c;
                g = x;
                b = 0;
            }
            else if (hl >= 1 && hl <= 2)
            {
                r = x;
                g = c;
                b = 0;
            }
            else if (hl >= 2 && hl <= 3)
            {
                r = 0;
                g = c;
                b = x;
            }
            else if (hl >= 3 && hl <= 4)
            {
                r = 0;
                g = x;
                b = c;
            }
            else if (hl >= 4 && hl <= 5)
            {
                r = x;
                g = 0;
                b = c;
            }
            else if (hl >= 5 && hl <= 6)
            {
                r = c;
                g = 0;
                b = x;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }

            var m = v - c;

            r += m;
            g += m;
            b += m;

            r *= 255;
            g *= 255;
            b *= 255;

            return new[] { MathUtil.ClampByte(b), MathUtil.ClampByte(g), MathUtil.ClampByte(r) };
        }
    }
}