using System;

namespace SharedContent
{
    public static class SharedFuncs
    {
        public const double TwoPi = 2 * Math.PI;

        public static double Lerp(double value1, double value2, double amount)
        {
            return value1 + (value2 - value1) * amount;
        }
    }
}
