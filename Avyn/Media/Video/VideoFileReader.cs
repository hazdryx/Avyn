using Hazdryx.Drawing;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Avyn.Media.Video
{
    /// <summary>
    ///     A class for reading frames from a video file sequentially.
    /// </summary>
    public class VideoFileReader : IVideoStream
    {
        private readonly Process ffmpeg;

        /// <summary>
        ///     Gets the video information for the video being read.
        /// </summary>
        public VideoStreamInfo VideoFormat { get; private set; }
        /// <summary>
        ///     Gets the current frame index.
        /// </summary>
        public int FrameIndex { get; private set; }
        /// <summary>
        ///     Gets the duration of the video.
        /// </summary>
        public TimeSpan? Duration { get; private set; }
        /// <summary>
        ///     Gets or sets the pixel buffer size.
        /// </summary>
        public int BufferSize { get; set; } = 32768;

        public bool CanRead => true;
        public bool CanWrite => false;

        public VideoFileReader(string filename)
        {
            MediaInfo info = MediaInfo.FromFile(filename);
            VideoStreamInfo format = info.VideoStream;
            if (format.IsEmpty) throw new ArgumentException("File does not contain any video.");

            Duration = info.Duration;
            VideoFormat = format;

            // Include -vf scale=-1:360 for rescaling.
            ffmpeg = FFmpeg.Query("-i @" + filename + " -f image2pipe -pix_fmt bgra -vcodec rawvideo -");
            ffmpeg.Start();

            new Task(() => FFmpeg.DebugStandardError(ffmpeg, "VideoFileReader")).Start();
        }

        /// <summary>
        ///     Reads the next frame in the video.
        /// </summary>
        /// <param name="bmp">The FastBitmap which the frame will be saved to.</param>
        /// <returns>Whether or not a frame was read.</returns>
        public bool ReadFrame(FastBitmap bmp)
        {
            if (bmp.Width != VideoFormat.Width || bmp.Height != VideoFormat.Height) throw new ArgumentException("The FastBitmap has invlid dimensions.");

            // Read bgra data.
            Stream output = ffmpeg.StandardOutput.BaseStream;
            byte[] buffer = new byte[BufferSize];
            int index = 0, next = BufferSize, length = bmp.Length * 4, read;

            while ((read = output.Read(buffer, 0, next)) > 0)
            {
                Buffer.BlockCopy(buffer, 0, bmp.Data, index, read);
                index += read;
                if (index + next >= length)
                {
                    next = length - index;
                    if (next <= 0) break;
                }
            }

            // Check if any data was read.
            if (index == 0) return false;
            else
            {
                FrameIndex++;
                return true;
            }
        }
        public void WriteFrame(FastBitmap bmp) => throw new InvalidOperationException();

        /// <summary>
        ///     Disposes of the FFmpeg process.
        /// </summary>
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
