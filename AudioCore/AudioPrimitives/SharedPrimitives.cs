namespace AudioCore.AudioPrimitives
{
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
