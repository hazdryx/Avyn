using System;

namespace Avyn.Media.Audio
{
    public interface IAudioStream : IDisposable
    {
        /// <summary>
        ///     Gets the AudioFormat of the stream.
        /// </summary>
        AudioStreamInfo AudioFormat { get; }
        /// <summary>
        ///     Gets whether this stream can be read from.
        /// </summary>
        bool CanRead { get; }
        /// <summary>
        ///     Gets whether this stream can be written to.
        /// </summary>
        bool CanWrite { get; }
        /// <summary>
        ///     Gets the duration of the audio stream.
        /// </summary>
        TimeSpan? Duration { get; }


        /// <summary>
        ///     Reads audio samples into the sample buffer.
        /// </summary>
        /// <param name="buffer">Buffer to read into.</param>
        /// <param name="offset">Offset of the buffer.</param>
        /// <param name="count">Number of samples to read.</param>
        /// <returns>How many samples were read.</returns>
        int ReadSamples(short[] buffer, int offset, int count);
        /// <summary>
        ///     Reads normalized audio sample into a sample buffer.
        /// </summary>
        /// <param name="buffer">Buffer to read into.</param>
        /// <param name="offset">Offset of the buffer.</param>
        /// <param name="count">Number of samples to read.</param>
        /// <returns>How many samples were read.</returns>
        int ReadSamples(float[] buffer, int offset, int count);

        /// <summary>
        ///     Writes samples from the sample buffer.
        /// </summary>
        /// <param name="buffer">Buffer to write from.</param>
        /// <param name="offset">Offset of the buffer.</param>
        /// <param name="count">Number of samples to read.</param>
        void WriteSamples(short[] buffer, int offset, int count);
        /// <summary>
        ///     Writes samples from the normalized sample buffer.
        /// </summary>
        /// <param name="buffer">Buffer to write from.</param>
        /// <param name="offset">Offset of the buffer.</param>
        /// <param name="count">Number of samples to read.</param>
        void WriteSamples(float[] buffer, int offset, int count);
    }
}
