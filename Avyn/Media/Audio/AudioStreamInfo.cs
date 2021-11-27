using System;

namespace Avyn.Media.Audio
{
    public struct AudioStreamInfo : IMediaStreamInfo
    {
        /// <summary>
        ///     Gets the start time of the audio.
        /// </summary>
        public TimeSpan StartTime { get; init; }
        /// <summary>
        ///     Gets the duration of the audio.
        /// </summary>
        public TimeSpan? Duration { get; init; }
        /// <summary>
        ///     Gets the codec used for the media stream.
        /// </summary>
        public string Codec { get; init; }
        /// <summary>
        ///     Gets the number of channels.
        /// </summary>
        public int Channels { get; init; }
        /// <summary>
        ///     Gets the sample rate in samples per second (Hz).
        /// </summary>
        public int SampleRate { get; init; }
        /// <summary>
        ///     Gets the bits per sample.
        /// </summary>
        public int BitsPerSample { get; init; }
        /// <summary>
        ///     Gets whether this AudioFormat is empty.
        /// </summary>
        public bool IsEmpty { get; private set; }

        public AudioStreamInfo(TimeSpan start, TimeSpan? duration, string codec, int channels, int sampleRate, int bitsPerSample)
        {
            this.StartTime = start;
            this.Duration = duration;
            this.Codec = codec;
            this.Channels = channels;
            this.SampleRate = sampleRate;
            this.BitsPerSample = bitsPerSample;
            this.IsEmpty = false;
        }

        /// <summary>
        ///     Creates an empty AudioFormat.
        /// </summary>
        /// <returns></returns>
        public static AudioStreamInfo Empty()
        {
            AudioStreamInfo format = new AudioStreamInfo
            {
                IsEmpty = true
            };
            return format;
        }
    }
}
