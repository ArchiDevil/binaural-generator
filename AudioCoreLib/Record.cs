using NAudio.Wave;
using System;
using NetworkLayer.Protocol;

namespace AudioCoreLib
{
    public class Record
    {
        WaveIn input = new WaveIn();
        WaveFormat format = new WaveFormat(8000, 16, 1);
        WaveFileWriter writer = null;

        ClientProtocol protocol = null;

        public Record()
        {
            input.WaveFormat = format;
            input.DataAvailable += RecordInput;
            input.RecordingStopped += RecordingStopped;
        }

        public Record(ClientProtocol argProtocol)
        {
            protocol = argProtocol;
            input.WaveFormat = format;
            input.DataAvailable += RecordInput;
        }

        public void StartRecording(string outputFileName)
        {
            writer = new WaveFileWriter(outputFileName, format);
            input.StartRecording();
        }

        public void StartRecording()
        {
            input.StartRecording();
        }

        public void StopRecording()
        {
            input.StopRecording();
        }

        private void RecordInput(object sender, WaveInEventArgs e)
        {
            if (writer != null)
            {
                writer.Write(e.Buffer, 0, e.BytesRecorded);
            }
            else
            {
                protocol.SendVoiceWindow(e.Buffer);
            }
        }

        private void RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (writer != null)
            {
                writer.Close();
                writer = null;
            }
        }
    }
}
