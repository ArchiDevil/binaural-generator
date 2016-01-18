using System;
using System.Diagnostics.Contracts;
using NetworkLayer.Protocol;

namespace AudioCore
{
    public class ServerAudioLayer : LocalAudioLayer
    {
        private ServerProtocol protocol = null;

        public ServerAudioLayer(ServerProtocol protocol) : base()
        {
            Contract.Requires(protocol != null, "protocol mustn't be null");
            this.protocol = protocol;
            protocol.VoiceWindowReceive += Protocol_VoiceWindowReceive;
            recorder.RecorderInput += Recorder_RecorderInput;
        }

        private void Protocol_VoiceWindowReceive(object sender, VoiceWindowDataEventArgs e)
        {
            playback.AddSamples(e.data);
        }

        private void Recorder_RecorderInput(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            protocol.SendVoiceWindow(e.Buffer);
        }

        public override void SetSignalSettings(BasicSignalModel[] channelSignals, BasicNoiseModel noiseSignal)
        {
            throw new Exception("Cannot be used on server");
        }
    }
}
