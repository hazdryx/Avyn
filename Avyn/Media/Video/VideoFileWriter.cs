using Hazdryx.Drawing;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Avyn.Media.Video
{
    /// <summary>
    ///     A class for writing frames to a video file sequentially.
    /// </summary>
    public class VideoFileWriter : IVideoStream
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
        public bool CanRead => false;
        public bool CanWrite => true;

        public VideoFileWriter(VideoStreamInfo info, string filename, int outputHeight)
        {
            this.VideoFormat = info;

            int outputWidth = outputHeight * info.Width / info.Height;

            ffmpeg = FFmpeg.Query(
                "-f rawvideo -c:v rawvideo -pix_fmt bgra -s {0}x{1} -r {2} -i - -vf scale={7}:{8}:flags=lanczos -c:v {3} -pix_fmt {4} -b:v {5} -y @{6}",
                info.Width, info.Height, info.FrameRate, info.Codec, info.PixelFormat, info.Bitrate, filename, outputWidth, outputHeight
            );
            ffmpeg.Start();
            new Task(() => FFmpeg.DebugStandardError(ffmpeg, "VideoFileWriter")).Start();
        }
        public VideoFileWriter(VideoStreamInfo info, string filename)
        {
            this.VideoFormat = info;
            ffmpeg = FFmpeg.Query(
                "-f rawvideo -c:v rawvideo -pix_fmt bgra -s {0}x{1} -r {2} -i - -c:v {3} -pix_fmt {4} -b:v {5} -y @{6}",
                info.Width, info.Height, info.FrameRate, info.Codec, info.PixelFormat, info.Bitrate, filename
            );
            ffmpeg.Start();
            new Task(() => FFmpeg.DebugStandardError(ffmpeg, "VideoFileWriter")).Start();
        }

        /// <summary>
        ///     Writes a frame to the video file.
        /// </summary>
        /// <param name="bmp">FastBitmap object to get pixel data for the frame.</param>
        public void WriteFrame(FastBitmap bmp)
        {
            if (bmp.Width != VideoFormat.Width || bmp.Height != VideoFormat.Height) throw new ArgumentException("The FastBitmap has invalid dimensions.");
            Stream stream = ffmpeg.StandardInput.BaseStream;

            byte[] buffer = new byte[bmp.Length * 4];
            Buffer.BlockCopy(bmp.Data, 0, buffer, 0, buffer.Length);

            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();

            FrameIndex++;
        }
        public bool ReadFrame(FastBitmap bmp) => throw new InvalidOperationException();


        /// <summary>
        ///     Closes the input stream and waits for FFmpeg to exit.
        /// </summary>
        public void Close()
        {
            ffmpeg.StandardInput.Close();
            ffmpeg.WaitForExit();
        }
        /// <summary>
        ///     Disposes of the FFmpeg process.
        /// </summary>
        public void Dispose()
        {
            Close();
            ffmpeg.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
