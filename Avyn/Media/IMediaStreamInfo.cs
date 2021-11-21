using System;

namespace Avyn.Media
{
    public interface IMediaStreamInfo
    {
        /// <summary>
        ///     Gets the start time of the media stream.
        /// </summary>
        TimeSpan StartTime { get; }
        /// <summary>
        ///     Gets the duration of the media stream.
        /// </summary>
        TimeSpan Duration { get; }
        /// <summary>
        ///     Gets the codec used for the media stream.
        /// </summary>
        string Codec { get; }
    }
}
