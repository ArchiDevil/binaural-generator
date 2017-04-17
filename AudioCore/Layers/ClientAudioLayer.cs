using System;
using System.Linq;
using AudioCore.AudioPrimitives;
using AudioCore.SampleProviders;
using NetworkLayer;
using NetworkLayer.ProtocolShared;

namespace AudioCore.Layers
{
    public class ClientAudioLayer : RecordedAudioLayer
    {
        private ClientProtocol _protocol = null;
        private BufferedProvider _buffererProvider = null;

        public ClientAudioLayer(ClientProtocol protocol) 
            : base()
        {
            _buffererProvider = new BufferedProvider(44100);
            _protocol = protocol ?? throw new ArgumentNullException("protocol");
            _protocol.VoiceWindowReceived += Protocol_VoiceWindowReceive;
            _playbackProvider.AddProvider(_buffererProvider);
            _recorder.RecorderInput += Recorder_RecorderInput;
        }

        private void Recorder_RecorderInput(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            //UNDONE: buffer is not completely full!
            _protocol.SendVoiceWindow(e.Buffer);
        }

        private void Protocol_VoiceWindowReceive(object sender, VoiceWindowDataEventArgs e)
        {
            float[] buffer = new float[e.data.Length / 4]; // 4 bytes per each float
            for (int i = 0; i < buffer.Length; ++i)
                buffer[i] = BitConverter.ToSingle(e.data, i * 4);

            _buffererProvider.AddSamples(buffer, e.samplingRate);
        }

        public bool SendSignalSettings(BasicSignalModel[] channelSignals, BasicNoiseModel noiseSignal)
        {
            int enabledSignals = channelSignals.Count((x) => x.Enabled == true);
            ChannelDescription[] signals = new ChannelDescription[enabledSignals];
            for (int i = 0; i < signals.Length; ++i)
            {
                signals[i] = new ChannelDescription()
                {
                    carrierFrequency = channelSignals[i].Frequency,
                    differenceFrequency = channelSignals[i].Difference,
                    volume = channelSignals[i].Gain
                };
            }

            NoiseDescription noise = new NoiseDescription();
            if (noiseSignal.Enabled)
            {
                noise.smoothness = noiseSignal.Smoothness;
                noise.volume = noiseSignal.Gain;
            }
            else
            {
                noise.volume = 0.0;
            }
            return _protocol.SendSignalSettings(signals, noise);
        }
    }
}
