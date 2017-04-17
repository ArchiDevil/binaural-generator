namespace AudioCore.AudioPrimitives
{
    internal static class UtilFuncs
    {
        internal static double TwoPi = 2 * 3.14159265;
    }

    public class BasicSignalModel
    {
        public double Difference { get; set; } = 0.0;
        public bool Enabled { get; set; } = false;
        public double Frequency { get; set; } = 0.0;
        public double Gain { get; set; } = 100.0f;
    }

    public class BasicNoiseModel
    {
        public bool Enabled { get; set; } = false;
        public double Gain { get; set; } = 100.0f;
        public double Smoothness { get; set; } = 0.0;
    }
}
