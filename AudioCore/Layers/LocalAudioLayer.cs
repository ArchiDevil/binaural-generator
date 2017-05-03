using AudioCore.SampleProviders;
using AudioCore.AudioPrimitives;

namespace AudioCore.Layers
{
    public class LocalAudioLayer
    {
        internal Playback _playback = null;

        internal ConstantSampleProvider _constantProvider = new ConstantSampleProvider();
        internal MixerProvider _playbackProvider = new MixerProvider();

        public LocalAudioLayer()
        {
            _playback = new Playback(_playbackProvider);
            _playbackProvider.AddProvider(_constantProvider);
        }

        public virtual void SetSignalSettings(BasicSignalModel[] channelSignals, BasicNoiseModel noiseSignal)
        {
            _constantProvider.SetSignals(channelSignals, noiseSignal);
        }

        public bool PlaybackEnabled
        {
            get { return _playback.Enabled; }
            set { _playback.Enabled = value; }
        }
    }
}
