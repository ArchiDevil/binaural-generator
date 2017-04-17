using System;
using AudioCore.AudioPrimitives;
using AudioCore.SampleProviders;
using NetworkLayer;
using NetworkLayer.ProtocolShared;

namespace AudioCore.Layers
{
    public class ServerAudioLayer : RecordedAudioLayer
    {
        private ServerProtocol _protocol = null;
        private BufferedProvider _buffererProvider = null;

        public ServerAudioLayer(ServerProtocol protocol)
            : base()
        {
            _buffererProvider = new BufferedProvider(44100);
            _protocol = protocol ?? throw new ArgumentNullException("protocol");
            _protocol.VoiceWindowReceived += Protocol_VoiceWindowReceive;
            _protocol.SettingsReceived += _protocol_SettingsReceived;
            _protocol.ClientDisconnected += _protocol_ClientDisconnected;
            _protocol.ConnectionLost += _protocol_ClientDisconnected;

            _playbackProvider.AddProvider(_buffererProvider);
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
