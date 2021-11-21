using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Avyn.Media.Audio
{
    public class AudioFileWriter : IAudioStream
    {
        private readonly Process ffmpeg;

        public AudioStreamInfo AudioFormat { get; private set; }
        public bool CanRead => false;
        public bool CanWrite => true;

        public AudioFileWriter(AudioStreamInfo info, string filename)
        {
            this.AudioFormat = info;
            ffmpeg = FFmpeg.Query(
                "-f s16le -ac {0} -ar {1} -i - -f wav -c:a pcm_s16le -y @{2}",
                info.Channels, info.SampleRate, filename
            );
            new Task(() => FFmpeg.DebugStandardError(ffmpeg, "AudioFileWriter")).Start();
        }

        public TimeSpan Duration { get; private set; }

        //
        // WRITING METHODS
        //
        public void WriteSamples(short[] buffer, int offset, int count)
        {
            Stream stream = ffmpeg.StandardInput.BaseStream;
            byte[] bytes = new byte[count * 2];
            for(int i = offset; i < count; i++)
            {
                bytes[i * 2 + 0] = (byte) ((buffer[i] >> 0) & 0xFF);
                bytes[i * 2 + 1] = (byte) ((buffer[i] >> 8) & 0xFF);
            }
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }
        public void WriteSamples(float[] buffer, int offset, int count)
        {
            Stream stream = ffmpeg.StandardInput.BaseStream;
            byte[] bytes = new byte[count * 2];
            for (int i = offset; i < count; i++)
            {
                short s = (short) Math.Round(buffer[i] * short.MaxValue);
                bytes[i * 2 + 0] = (byte)((s >> 0) & 0xFF);
                bytes[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
            }
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        //
        // READING METHODS
        //
        public int ReadSamples(short[] buffer, int offset, int count) => throw new InvalidOperationException();
        public int ReadSamples(float[] buffer, int offset, int count) => throw new InvalidOperationException();

        /// <summary>
        ///     Closes the input stream and waits for FFmpeg to exit.
        /// </summary>
        public void Close()
        {
            ffmpeg.StandardInput.Close();
            ffmpeg.WaitForExit();
        }
        public void Dispose()
        {
            Close();
            ffmpeg.Dispose();
        }
    }
}
