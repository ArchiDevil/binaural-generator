using AudioCore.SampleProviders;

namespace AudioCore
{
    public class LocalAudioLayer
    {
        internal Record recorder = null;
        internal Playback playback = null;
        internal ConstantSampleProvider sampleProvider = null;

        public LocalAudioLayer()
        {
            recorder = new Record(44100, 16, 1);
            sampleProvider = new ConstantSampleProvider();
            playback = new Playback(sampleProvider);
        }

        public virtual void SetSignalSettings(BasicSignalModel[] channelSignals, BasicNoiseModel noiseSignal)
        {
            sampleProvider.SetSignals(channelSignals, noiseSignal);
        }

        public bool PlaybackEnabled
        {
            get { return playback.Enabled; }
            set { playback.Enabled = value; }
        }

        public bool RecordingEnabled
        {
            get { return recorder.Enabled; }
            set { recorder.Enabled = value; }
        }
    }
}
