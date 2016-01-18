using AudioCore.SampleProviders;

namespace AudioCore
{
    public class LocalAudioLayer
    {
        internal Record _recorder = null;
        internal Playback _playback = null;
        internal ConstantSampleProvider _sampleProvider = null;

        public LocalAudioLayer()
        {
            _recorder = new Record(44100, 16, 1);
            _sampleProvider = new ConstantSampleProvider();
            _playback = new Playback(_sampleProvider);
        }

        public virtual void SetSignalSettings(BasicSignalModel[] channelSignals, BasicNoiseModel noiseSignal)
        {
            _sampleProvider.SetSignals(channelSignals, noiseSignal);
        }

        public bool PlaybackEnabled
        {
            get { return _playback.Enabled; }
            set { _playback.Enabled = value; }
        }

        public bool RecordingEnabled
        {
            get { return _recorder.Enabled; }
            set { _recorder.Enabled = value; }
        }
    }
}
