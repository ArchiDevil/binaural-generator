using NAudio.Wave;

namespace AudioCore.SampleProviders
{
    internal abstract class SampleProvider : ISampleProvider
    {
        protected int _nSample = 0;
        protected double _time = 0.0; // time in seconds

        internal float Gain { get; set; } = 1.0f;

        internal SampleProvider()
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate: 44100, channels: 2);
        }

        public WaveFormat WaveFormat { get; }

        public abstract int Read(float[] buffer, int offset, int count);
    }
}
