using System.Linq;

namespace PID_HSV.Util
{
    public static class MathUtil
    {
        public static double Max(params double[] values)
        {
            return values.Max();
        }

        public static double Min(params double[] values)
        {
            return values.Min();
        }

        public static byte ClampByte(double b)
        {
            if (b < 0)
                return 0;
            if (b > 255)
                return 255;

            return (byte)b;
        }
    }
}