using NAudio.Wave;

namespace AudioCore.SampleProviders
{
    public abstract class SampleProvider : ISampleProvider
    {
        internal float Gain { get; set; } = 1.0f;

        internal SampleProvider()
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate: 44100, channels: 2);
        }

        public WaveFormat WaveFormat { get; }

        public abstract int Read(float[] buffer, int offset, int count);
    }
}
