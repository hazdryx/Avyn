using System;
using Avyn.Media.Audio;

namespace Avyn.Media.Extensions
{
    /// <summary>
    ///     An extension class for IAudioStream.
    /// </summary>
    public static class AudioStreamExt
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
            float[] buffer = new float[4096];
            while ((read = src.ReadSamples(buffer, 0, buffer.Length)) > 0)
            {
                dest.WriteSamples(buffer, 0, read);
            }

            return dest;
        }
    }
}
