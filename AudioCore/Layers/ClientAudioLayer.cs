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
        private BufferedProvider _bufferedProvider = null;

        public ClientAudioLayer(ClientProtocol protocol)
            : base(8000, 16, 1, 50)
        {
            _bufferedProvider = new BufferedProvider(44100);
            _protocol = protocol ?? throw new ArgumentNullException("protocol");
            _protocol.VoiceWindowReceived += Protocol_VoiceWindowReceive;
            _playbackProvider.AddProvider(_bufferedProvider);
            _recorder.RecorderInput += Recorder_RecorderInput;
        }

        private void Recorder_RecorderInput(object sender, RecoderInputEventArgs e)
        {
            _protocol.SendVoiceWindow(e.Format.SampleRate, e.Format.BitsPerSample / 8, e.Buffer.Take(e.BytesRecorded).ToArray());
        }

        private void Protocol_VoiceWindowReceive(object sender, VoiceWindowDataEventArgs e)
        {
            int srcBytes = e.bytesPerSample;
            int dstBytes = _bufferedProvider.WaveFormat.BitsPerSample / 8;

            int size = SignalRateConverter.CalculateOutputBufferSize(e.data.Length,
                                                                     e.bytesPerSample,
                                                                     e.samplingRate,
                                                                     dstBytes,
                                                                     _bufferedProvider.WaveFormat.SampleRate);
            byte[] dst = new byte[size];
            SignalRateConverter.Convert(e.data,
                                        e.samplingRate,
                                        e.bytesPerSample,
                                        dst,
                                        _bufferedProvider.WaveFormat.SampleRate,
                                        dstBytes);

            float[] samples = new float[dst.Length / dstBytes * 2];
            for (int i = 0; i < dst.Length / dstBytes; i++)
            {
                float sample = 0.0f;
                if (dstBytes == 1)
                    sample = dst[i] / 128.0f - 1.0f;

                if (dstBytes == 2)
                    sample = BitConverter.ToInt16(dst, i * sizeof(short));

                if (dstBytes == 4)
                    sample = BitConverter.ToInt32(dst, i * sizeof(int));

                if (srcBytes == 2)
                    sample /= short.MaxValue;

                if (srcBytes == 4)
                    sample /= int.MaxValue;

                samples[(i * 2) + 0] = sample;
                samples[(i * 2) + 1] = sample;
            }

            _bufferedProvider.AddSamples(samples, _bufferedProvider.WaveFormat.SampleRate);
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
