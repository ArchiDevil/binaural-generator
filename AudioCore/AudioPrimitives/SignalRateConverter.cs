using System;

namespace AudioCore.AudioPrimitives
{
    internal static class SignalRateConverter
    {
        static int GetSourceIndex(int destinationIndex, double conversionRatio) => (int)Math.Floor(destinationIndex * conversionRatio);

        internal static int CalculateOutputBufferSize(int srcLength,
                                                      int srcBytesPerSample,
                                                      int srcSampleRate,
                                                      int dstBytesPerSample,
                                                      int dstSampleRate) => srcLength * dstBytesPerSample / srcBytesPerSample * dstSampleRate / srcSampleRate;

        internal static void Convert(byte[] srcArray, int srcSampleRate, int srcBytesPerSample,
                                     byte[] dstArray, int dstSampleRate, int dstBytesPerSample)
        {
            if (srcBytesPerSample != 1 &&
                srcBytesPerSample != 2 &&
                srcBytesPerSample != 4)
            {
                throw new ArgumentOutOfRangeException("srcBytesPerSample");
            }

            if (dstBytesPerSample != 1 &&
                dstBytesPerSample != 2 &&
                dstBytesPerSample != 4)
            {
                throw new ArgumentOutOfRangeException("dstBytesPerSample");
            }

            double conversionRatio = (double)srcSampleRate / dstSampleRate;

            if (srcBytesPerSample == 1)
            {
                if (dstBytesPerSample == 1)
                    for (int i = 0; i < dstArray.Length; ++i)
                        dstArray[i] = srcArray[GetSourceIndex(i, conversionRatio)];

                if (dstBytesPerSample == 2)
                {
                    short[] dst = new short[dstArray.Length / sizeof(short)];
                    for (int i = 0; i < dst.Length; ++i)
                        dst[i] = srcArray[GetSourceIndex(i, conversionRatio)];
                    Buffer.BlockCopy(dst, 0, dstArray, 0, dstArray.Length);
                }

                if (dstBytesPerSample == 4)
                {
                    int[] dst = new int[dstArray.Length / sizeof(int)];
                    for (int i = 0; i < dst.Length; ++i)
                        dst[i] = srcArray[GetSourceIndex(i, conversionRatio)];
                    Buffer.BlockCopy(dst, 0, dstArray, 0, dstArray.Length);
                }
            }
            else if (srcBytesPerSample == 2)
            {
                if (dstBytesPerSample == 1)
                    throw new ArgumentOutOfRangeException("dstBytesPerSample", "destination amount of bytes is lower than source");

                short[] src = new short[srcArray.Length / sizeof(short)];
                Buffer.BlockCopy(srcArray, 0, src, 0, srcArray.Length);

                if (dstBytesPerSample == 2)
                {
                    short[] dst = new short[dstArray.Length / sizeof(short)];
                    for (int i = 0; i < dst.Length; ++i)
                        dst[i] = src[GetSourceIndex(i, conversionRatio)];
                    Buffer.BlockCopy(dst, 0, dstArray, 0, dstArray.Length);
                }

                if (dstBytesPerSample == 4)
                {
                    int[] dst = new int[dstArray.Length / sizeof(int)];
                    for (int i = 0; i < dst.Length; ++i)
                        dst[i] = src[GetSourceIndex(i, conversionRatio)];
                    Buffer.BlockCopy(dst, 0, dstArray, 0, dstArray.Length);
                }
            }
            else if (srcBytesPerSample == 4)
            {
                if (dstBytesPerSample == 1)
                    throw new ArgumentOutOfRangeException("dstBytesPerSample", "destination amount of bytes is lower than source");

                if (dstBytesPerSample == 2)
                    throw new ArgumentOutOfRangeException("dstBytesPerSample", "destination amount of bytes is lower than source");

                short[] src = new short[srcArray.Length / sizeof(short)];
                Buffer.BlockCopy(srcArray, 0, src, 0, srcArray.Length);

                if (dstBytesPerSample == 4)
                {
                    int[] dst = new int[dstArray.Length / sizeof(int)];
                    for (int i = 0; i < dst.Length; ++i)
                        dst[i] = src[GetSourceIndex(i, conversionRatio)];
                    Buffer.BlockCopy(dst, 0, dstArray, 0, dstArray.Length);
                }
            }
        }
    }
}
