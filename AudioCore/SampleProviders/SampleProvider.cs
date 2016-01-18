using NAudio.Wave;

namespace AudioCore.SampleProviders
{
    public abstract class SampleProvider : ISampleProvider
    {
        readonly protected WaveFormat _waveFormat;

        protected int _nSample = 0;
        protected double _time = 0.0; // time in seconds

        public WaveFormat WaveFormat
        {
            get { return _waveFormat; }
        }

        public float Gain { get; set; } = 1.0f;

        public SampleProvider()
        {
            _waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate: 44100, channels: 2);
        }

        public abstract int Read(float[] buffer, int offset, int count);
        public virtual void AddVoiceSamples() { }
    }
}
