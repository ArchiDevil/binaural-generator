using System.IO;
using System.Runtime.InteropServices;

namespace SharedLibrary
{
    public class WavFile
    {
        [StructLayout(LayoutKind.Sequential)]
        // Структура, описывающая заголовок WAV файла.
        private struct WavHeader
        {
            // WAV-формат начинается с RIFF-заголовка:

            // Содержит символы "RIFF" в ASCII кодировке
            // (0x52494646 в big-endian представлении)
            public uint ChunkId;

            // 36 + subchunk2Size, или более точно:
            // 4 + (8 + subchunk1Size) + (8 + subchunk2Size)
            // Это оставшийся размер цепочки, начиная с этой позиции.
            // Иначе говоря, это размер файла - 8, то есть,
            // исключены поля chunkId и chunkSize.
            public uint ChunkSize;

            // Содержит символы "WAVE"
            // (0x57415645 в big-endian представлении)
            public uint Format;

            // Формат "WAVE" состоит из двух подцепочек: "fmt " и "data":
            // Подцепочка "fmt " описывает формат звуковых данных:

            // Содержит символы "fmt "
            // (0x666d7420 в big-endian представлении)
            public uint Subchunk1Id;

            // 16 для формата PCM.
            // Это оставшийся размер подцепочки, начиная с этой позиции.
            public uint Subchunk1Size;

            // Аудио формат, полный список можно получить здесь http://audiocoding.ru/wav_formats.txt
            // Для PCM = 1 (то есть, Линейное квантование).
            // Значения, отличающиеся от 1, обозначают некоторый формат сжатия.
            public ushort AudioFormat;

            // Количество каналов. Моно = 1, Стерео = 2 и т.д.
            public ushort NumChannels;

            // Частота дискретизации. 8000 Гц, 44100 Гц и т.д.
            public uint SampleRate;

            // sampleRate * numChannels * bitsPerSample/8
            public uint ByteRate;

            // numChannels * bitsPerSample/8
            // Количество байт для одного сэмпла, включая все каналы.
            public ushort BlockAlign;

            // Так называемая "глубиная" или точность звучания. 8 бит, 16 бит и т.д.
            public ushort BitsPerSample;

            // Подцепочка "data" содержит аудио-данные и их размер.

            // Содержит символы "data"
            // (0x64617461 в big-endian представлении)
            public uint Subchunk2Id;

            // numSamples * numChannels * bitsPerSample/8
            // Количество байт в области данных.
            public uint Subchunk2Size;

            // Далее следуют непосредственно Wav данные.

            internal WavHeader(long contentSize, int numChannels)
            {
                ChunkId = 0x46464952;
                Format = 0x45564157;
                Subchunk1Id = 0x20746d66;
                Subchunk1Size = 16;
                AudioFormat = 1;
                NumChannels = (ushort)numChannels;
                SampleRate = 44100;
                BitsPerSample = 16;
                Subchunk2Id = 0x61746164;

                ByteRate = SampleRate * NumChannels * BitsPerSample / 8;
                BlockAlign = (ushort)(NumChannels * BitsPerSample / 8);
                Subchunk2Size = (uint)(contentSize * BitsPerSample / 8);
                ChunkSize = 4 + (8 + Subchunk1Size) + (8 + Subchunk2Size);
            }
        }

        static public void Save(string filename, short[] content)
        {
            var header = new WavHeader(content.LongLength, 2);
            byte[] headerBytes = FileUtilities.StructureToByteArray(header);

            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(headerBytes);

                    foreach (short value in content)
                    {
                        bw.Write(value);
                    }
                }
            }
        }
    }
}
