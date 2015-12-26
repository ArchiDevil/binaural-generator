using NAudio.Wave;

namespace AudioCore
{
    public class Record
    {
        private WaveIn input = new WaveIn();

        public Record(int rate, int bits, int channels)
        {
            input.WaveFormat = new WaveFormat(rate, bits, channels);
            input.DataAvailable += Input_DataAvailable;
        }

        public void StartRecording()
        {
            input.StartRecording();
        }

        public void StopRecording()
        {
            input.StopRecording();
        }

        private void Input_DataAvailable(object sender, WaveInEventArgs e)
        {
            RecorderInput(sender, e);
        }

        public delegate void RecordInputHandler(object sender, WaveInEventArgs e);
        public event RecordInputHandler RecorderInput = delegate { };
    }
}
