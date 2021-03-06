using System;

namespace Avyn.Media.Video
{
    /// <summary>
    ///     Simple video information for VideoFileReader and VideoFileWriter.
    /// </summary>
    public struct VideoStreamInfo : IMediaStreamInfo
    {
        /// <summary>
        ///     An approximation of a high-quality H.264 compression ratio.
        /// </summary>
        public const double CompressionRatio = 8;

        /// <summary>
        ///     Gets the start time of the video.
        /// </summary>
        public TimeSpan StartTime { get; init; }
        /// <summary>
        ///     Gets the duration of the video.
        /// </summary>
        public TimeSpan? Duration { get; init; }
        /// <summary>
        ///     Gets the codec used for the media stream.
        /// </summary>
        public string Codec { get; init; }
        /// <summary>
        ///     Gets the pixel format of this video.
        /// </summary>
        public string PixelFormat { get; init; }
        /// <summary>
        ///     Gets the width of the video in pixels.
        /// </summary>
        public int Width { get; init; }
        /// <summary>
        ///     Gets the height of the video in pixels.
        /// </summary>
        public int Height { get; init; }
        /// <summary>
        ///     Gets the frame rate of the video in frames per second.
        /// </summary>
        public double FrameRate { get; init; }
        /// <summary>
        ///     Gets the bitrate of this video in bits per second.
        /// </summary>
        public int Bitrate { get; init; }
        /// <summary>
        ///     Gets whether this VideoFormat is empty.
        /// </summary>
        public bool IsEmpty { get; private set; }

        public VideoStreamInfo(int width, int height, double frameRate, TimeSpan start, TimeSpan? duration, string codec, string pixelFormat, int bitrate)
        {
            this.StartTime = start;
            this.Duration = duration;
            this.Codec = codec;
            this.PixelFormat = pixelFormat;
            this.Width = width;
            this.Height = height;
            this.FrameRate = frameRate;
            this.Bitrate = bitrate;
            this.IsEmpty = false;
        }
        public VideoStreamInfo(int width, int height, double frameRate, TimeSpan? duration, double compressionRatio, bool hwaccel)
        {
            this.StartTime = TimeSpan.Zero;
            this.Duration = duration;
            this.Codec = hwaccel ? "h264_nvenc" : "h264";
            this.PixelFormat = "yuv420p";
            this.Width = width;
            this.Height = height;
            this.FrameRate = frameRate;
            this.Bitrate = (int) Math.Round(Width * Height * frameRate / compressionRatio);
            this.IsEmpty = false;
        }
        public VideoStreamInfo(int width, int height, double frameRate, TimeSpan? duration)
        {
            this.StartTime = TimeSpan.Zero;
            this.Duration = duration;
            this.Codec = "h264";
            this.PixelFormat = "yuv420p";
            this.Width = width;
            this.Height = height;
            this.FrameRate = frameRate;
            this.Bitrate = (int) Math.Round(Width * Height * frameRate / CompressionRatio);
            this.IsEmpty = false;
        }

        /// <summary>
        ///     Creates an empty VideoFormat.
        /// </summary>
        /// <returns></returns>
        public static VideoStreamInfo Empty()
        {
            VideoStreamInfo format = new VideoStreamInfo
            {
                IsEmpty = true
            };
            return format;
        }
    }
}
