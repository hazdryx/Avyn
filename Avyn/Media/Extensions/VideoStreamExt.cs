using System;
using Avyn.Media.Video;
using Hazdryx.Drawing;

namespace Avyn.Media.Extensions
{
    /// <summary>
    ///     An extension class for IVideoStream.
    /// </summary>
    public static class VideoStreamExt
    {
        /// <summary>
        ///     Pipes video frames from this stream to the destination stream.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>The destination stream for chaining.</returns>
        public static IVideoStream Pipe(this IVideoStream src, IVideoStream dest)
        {
            if (!src.CanRead || !dest.CanWrite) throw new InvalidOperationException("Invalid stream types.");

            FastBitmap buffer = new FastBitmap(src.VideoFormat.Width, src.VideoFormat.Height);
            while (src.ReadFrame(buffer))
            {
                dest.WriteFrame(buffer);
            }

            return dest;
        }
        /// <summary>
        ///     Gets the current position in the video.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static TimeSpan GetPosition(this IVideoStream stream)
        {
            return new TimeSpan((long) Math.Round(stream.FrameIndex / stream.VideoFormat.FrameRate * TimeSpan.TicksPerSecond));
        }
    }
}
