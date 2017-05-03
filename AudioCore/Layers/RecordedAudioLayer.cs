using AudioCore.AudioPrimitives;

namespace AudioCore.Layers
{
    public class RecordedAudioLayer : LocalAudioLayer
    {
        internal Record _recorder = null;

        public RecordedAudioLayer(int rate, int bits, int channels)
            : base()
        {
            _recorder = new Record(rate, bits, channels);
        }

        public RecordedAudioLayer(int rate, int bits, int channels, int bufferLength)
            : base()
        {
            _recorder = new Record(rate, bits, channels, bufferLength);
        }

        public bool RecordingEnabled
        {
            get { return _recorder.Enabled; }
            set { _recorder.Enabled = value; }
        }

        public int AudioInDevicesCount
        {
            get { return _recorder.DevicesCount; }
        }
    }
}
