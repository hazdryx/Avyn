using Hazdryx.Drawing;
using System;

namespace Avyn.Media.Video
{
    /// <summary>
    ///     An interface which all video streams implement.
    /// </summary>
    public interface IVideoStream : IDisposable
    {
        /// <summary>
        ///     Gets the video format of the stream.
        /// </summary>
        VideoStreamInfo VideoFormat { get; }
        /// <summary>
        ///     Gets whether this stream can be read from.
        /// </summary>
        bool CanRead { get; }
        /// <summary>
        ///     Gets whether this stream can be written to.
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        ///     Gets the current frame index.
        /// </summary>
        int FrameIndex { get; }
        /// <summary>
        ///     Gets the duration of the video.
        /// </summary>
        TimeSpan? Duration { get; }

        /// <summary>
        ///     Writes a frame to the video file.
        /// </summary>
        /// <param name="bmp">IBitmap object to get pixel data for the frame.</param>
        void WriteFrame(FastBitmap bmp);
        /// <summary>
        ///     Reads the next frame in the video.
        /// </summary>
        /// <param name="bmp">The IBitmap which the frame will be copied to.</param>
        /// <returns>Whether or not a frame was read.</returns>
        bool ReadFrame(FastBitmap bmp);
    }
}
