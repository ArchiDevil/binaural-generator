namespace AudioCore
{
    internal static class UtilFuncs
    {
        internal static double TwoPi = 2 * 3.14159265;
    }

    public class BasicSignalModel
    {
        public double difference = 0.0;
        public bool enabled = false;
        public double frequency = 0.0;
        public float gain = 100.0f;
    }

    public class BasicNoiseModel
    {
        public bool enabled = false;
        public float gain = 100.0f;
        public double smoothness = 0.0;
    }
}
