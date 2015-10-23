using System;
using System.Linq;

namespace SharedLibrary
{
    public static class SharedFuncs
    {
        public const double TwoPi = 2 * Math.PI;

        public static double Lerp(double value1, double value2, double amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static void RemoveFromArrayByValue<T>(ref T[] array, T elementToRemove)
        {
            int numIndex = Array.IndexOf(array, elementToRemove);
            array = array.Where((val, idx) => idx != numIndex).ToArray();
        }

        public static void RemoveAllFromArrayByValue(ref IComparable[] array, IComparable elementToRemove)
        {
            array = array.Where((val, idx) => val != elementToRemove).ToArray();
        }

        public static void RemoveFromArrayByIndex<T>(ref T[] array, int indexToRemove)
        {
            array = array.Where((val, idx) => idx != indexToRemove).ToArray();
        }
    }
}
