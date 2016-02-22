using NAudio.Wave;

namespace AudioCore.SampleProviders
{
    internal abstract class SampleProvider : ISampleProvider
    {
        readonly protected WaveFormat _waveFormat;

        protected int _nSample = 0;
        protected double _time = 0.0; // time in seconds

        internal float Gain { get; set; } = 1.0f;

        internal SampleProvider()
        {
            _waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate: 44100, channels: 2);
        }

        public WaveFormat WaveFormat
        {
            get { return _waveFormat; }
        }
        
        public abstract int Read(float[] buffer, int offset, int count);
    }
}
