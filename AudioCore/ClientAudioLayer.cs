using System.Diagnostics.Contracts;
using System.Linq;
using NetworkLayer.Protocol;

namespace AudioCore
{
    public class ClientAudioLayer : LocalAudioLayer
    {
        private ClientProtocol _protocol = null;

        public ClientAudioLayer(ClientProtocol protocol) : base()
        {
            Contract.Requires(protocol != null, "protocol mustn't be null");
            this._protocol = protocol;
            protocol.VoiceWindowReceive += Protocol_VoiceWindowReceive;
            _recorder.RecorderInput += Recorder_RecorderInput;
        }

        private void Recorder_RecorderInput(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            _protocol.SendVoiceWindow(e.Buffer);
        }

        private void Protocol_VoiceWindowReceive(object sender, VoiceWindowDataEventArgs e)
        {
            _playback.AddSamples(e.data);
        }

        public override void SetSignalSettings(BasicSignalModel[] channelSignals, BasicNoiseModel noiseSignal)
        {
            int enabledSignals = channelSignals.Count((x) => x.enabled == true);
            ChannelDescription[] signals = new ChannelDescription[enabledSignals];
            for (int i = 0; i < signals.Length; ++i)
            {
                signals[i] = new ChannelDescription();
                signals[i].carrierFrequency = channelSignals[i].frequency;
                signals[i].differenceFrequency = channelSignals[i].difference;
                signals[i].volume = channelSignals[i].gain;
            }

            NoiseDescription noise = new NoiseDescription();
            if (noiseSignal.enabled)
            {
                noise.smoothness = noiseSignal.smoothness;
                noise.volume = noiseSignal.gain;
            }
            else
            {
                noise.volume = 0.0;
            }
            _protocol.SendSignalSettings(signals, noise);
        }
    }
}
