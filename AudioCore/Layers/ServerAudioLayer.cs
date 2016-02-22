using AudioCore.AudioPrimitives;
using AudioCore.SampleProviders;
using NetworkLayer.Protocol;
using System;
using System.Diagnostics.Contracts;

namespace AudioCore.Layers
{
    public class ServerAudioLayer : RecordedAudioLayer
    {
        private ServerProtocol _protocol = null;
        private BufferedProvider _buffererProvider = null;

        public ServerAudioLayer(ServerProtocol protocol) : base()
        {
            Contract.Requires(protocol != null, "protocol mustn't be null");

            _buffererProvider = new BufferedProvider(44100);
            _protocol = protocol;
            _protocol.VoiceWindowReceive += Protocol_VoiceWindowReceive;
            _playbackProvider.AddProvider(_buffererProvider);
            _recorder.RecorderInput += Recorder_RecorderInput;
        }

        public override void SetSignalSettings(BasicSignalModel[] channelSignals, BasicNoiseModel noiseSignal)
        {
            throw new Exception("Cannot be used on server");
        }

        private void Protocol_VoiceWindowReceive(object sender, VoiceWindowDataEventArgs e)
        {
            float[] buffer = new float[e.data.Length / 4]; // 4 bytes per each float
            for (int i = 0; i < buffer.Length; ++i)
                buffer[i] = BitConverter.ToSingle(e.data, i * 4);

            _buffererProvider.AddSamples(buffer, e.samplingRate);
        }

        private void Recorder_RecorderInput(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            //UNDONE: buffer is not completely full!
            _protocol.SendVoiceWindow(e.Buffer);
        }
    }
}
