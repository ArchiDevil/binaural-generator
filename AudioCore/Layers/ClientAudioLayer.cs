﻿using AudioCore.AudioPrimitives;
using AudioCore.SampleProviders;
using NetworkLayer.Protocol;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace AudioCore.Layers
{
    public class ClientAudioLayer : RecordedAudioLayer
    {
        private ClientProtocol _protocol = null;
        private BufferedProvider _buffererProvider = null;

        public ClientAudioLayer(ClientProtocol protocol) : base()
        {
            Contract.Requires(protocol != null, "protocol mustn't be null");

            _buffererProvider = new BufferedProvider(44100);
            _protocol = protocol;
            _protocol.VoiceWindowReceive += Protocol_VoiceWindowReceive;
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

        public override void SetSignalSettings(BasicSignalModel[] channelSignals, BasicNoiseModel noiseSignal)
        {
            int enabledSignals = channelSignals.Count((x) => x.enabled == true);
            ChannelDescription[] signals = new ChannelDescription[enabledSignals];
            for (int i = 0; i < signals.Length; ++i)
            {
                signals[i] = new ChannelDescription()
                {
                    carrierFrequency = channelSignals[i].frequency,
                    differenceFrequency = channelSignals[i].difference,
                    volume = channelSignals[i].gain
                };
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
