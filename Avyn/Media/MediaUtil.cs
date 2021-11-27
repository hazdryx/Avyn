using System;
using Avyn.Media.Audio;
using Avyn.Media.Video;
using Hazdryx.Drawing;

namespace Avyn.Media
{
    /// <summary>
    ///     Utilities and extension methods for IVideoStream and IAudioStream.
    /// </summary>
    public static class MediaUtil
    {
        /// <summary>
        ///     Pipes audio samples from this stream to the destination stream.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>The destination stream for chaining.</returns>
        public static IAudioStream Pipe(this IAudioStream src, IAudioStream dest)
        {
            if (!src.CanRead || !dest.CanWrite) throw new InvalidOperationException("Invalid stream types.");

            int read;
            short[] buffer = new short[4096];
            while ((read = src.ReadSamples(buffer, 0, buffer.Length)) > 0)
            {
                dest.WriteSamples(buffer, 0, read);
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
