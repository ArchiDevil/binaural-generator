using System;
using System.Linq;

using AudioCore.AudioPrimitives;
using AudioCore.SampleProviders;
using NetworkLayer;
using NetworkLayer.ProtocolShared;

namespace AudioCore.Layers
{
    public class ServerAudioLayer : RecordedAudioLayer
    {
        private ServerProtocol _protocol = null;
        private BufferedProvider _bufferedProvider = null;

        public ServerAudioLayer(ServerProtocol protocol)
            : base(8000, 16, 1, 50)
        {
            _bufferedProvider = new BufferedProvider(44100);
            _protocol = protocol ?? throw new ArgumentNullException("protocol");
            _protocol.VoiceWindowReceived += Protocol_VoiceWindowReceive;
            _protocol.SettingsReceived += _protocol_SettingsReceived;
            _protocol.ClientDisconnected += _protocol_ClientDisconnected;
            _protocol.ConnectionLost += _protocol_ClientDisconnected;

            _playbackProvider.AddProvider(_bufferedProvider);
            _recorder.RecorderInput += Recorder_RecorderInput;
        }

        private void _protocol_ClientDisconnected(object sender, EventArgs e)
        {
            PlaybackEnabled = false;
            RecordingEnabled = false;
        }

        private void _protocol_SettingsReceived(object sender, SettingsDataEventArgs e)
        {
            BasicSignalModel[] signals = new BasicSignalModel[e.channels.Length];
            for (int i = 0; i < e.channels.Length; ++i)
            {
                signals[i] = new BasicSignalModel()
                {
                    Enabled = e.channels[i].enabled,
                    Frequency = e.channels[i].carrierFrequency,
                    Difference = e.channels[i].differenceFrequency,
                    Gain = e.channels[i].volume
                };
            }

            BasicNoiseModel noise = new BasicNoiseModel()
            {
                Enabled = e.noise.enabled,
                Gain = e.noise.volume,
                Smoothness = e.noise.smoothness
            };

            SetSignalSettings(signals, noise);
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

        private void Recorder_RecorderInput(object sender, RecoderInputEventArgs e)
        {
            _protocol.SendVoiceWindow(e.Format.SampleRate, e.Format.BitsPerSample / 8, e.Buffer.Take(e.BytesRecorded).ToArray());
        }
    }
}
