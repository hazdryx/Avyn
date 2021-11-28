using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Avyn.Media.Audio
{
    public class AudioFileReader : IAudioStream
    {
        private readonly Process ffmpeg;

        public int BufferSize { get; set; } = 32768;

        public AudioStreamInfo AudioFormat { get; private set; }
        public bool CanRead => true;
        public bool CanWrite => false;

        public AudioFileReader(string filename)
        {
            MediaInfo info = MediaInfo.FromFile(filename);
            AudioStreamInfo format = info.AudioStream;
            if (format.IsEmpty) throw new ArgumentException("File does not contain audio.");

            Duration = info.Duration;
            AudioFormat = format;

            ffmpeg = FFmpeg.Query("-i @{0} -f s16le -", filename);
            new Task(() => FFmpeg.DebugStandardError(ffmpeg, "AudioFileReader")).Start();
        }

        public TimeSpan? Duration { get; private set; }

        //
        // READING METHODS
        //
        public int ReadSamples(float[] buffer, int offset, int count)
        {
            // Read audio data.
            byte[] buf = new byte[count * 2];
            int read = ReadSampleBytes(buf);

            // Convert to short.
            for (int i = 0; i < read; i += 2)
            {
                buffer[offset + i / 2] = (short)(buf[i + 0] | buf[i + 1] << 8) / (float)short.MaxValue;
            }
            return read / 2;
        }
        private int ReadSampleBytes(byte[] buf)
        {
            // Setup variables.
            Stream stream = ffmpeg.StandardOutput.BaseStream;
            int index = 0, next = BufferSize, read;
            if (buf.Length < BufferSize) next = buf.Length;

            // Read audio data.
            while ((read = stream.Read(buf, index, next)) > 0)
            {
                index += read;
                if (index + next >= buf.Length)
                {
                    next = buf.Length - index;
                    if (next <= 0) break;
                }
            }
            return index;
        }

        //
        // WRITING METHODS
        //
        public void WriteSamples(float[] buffer, int offset, int count) => throw new InvalidOperationException();

        public void Dispose()
        {
            try
            {
                ffmpeg.Kill();
            }
            catch (InvalidOperationException) { }
            catch (Win32Exception) { }

            ffmpeg.WaitForExit();
            ffmpeg.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
