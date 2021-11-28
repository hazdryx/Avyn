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
                "-f s16le -ac {0} -ar {1} -i - -f wav -c:a pcm_f32{4} -y @{2}",
                info.Channels, info.SampleRate, filename, BitConverter.IsLittleEndian ? "le" : "be"
            );
            new Task(() => FFmpeg.DebugStandardError(ffmpeg, "AudioFileWriter")).Start();
        }

        public TimeSpan? Duration { get; private set; }

        //
        // WRITING METHODS
        //
        public void WriteSamples(float[] buffer, int offset, int count)
        {
            Stream stream = ffmpeg.StandardInput.BaseStream;
            byte[] bytes = new byte[count * 4];
            for (int i = offset; i < count; i++)
            {
                byte[] buf = BitConverter.GetBytes(buffer[i]);
                bytes[i * 4 + 0] = buf[0];
                bytes[i * 4 + 1] = buf[1];
                bytes[i * 4 + 2] = buf[2];
                bytes[i * 4 + 3] = buf[3];
            }
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        //
        // READING METHODS
        //
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
            GC.SuppressFinalize(this);
        }
    }
}
