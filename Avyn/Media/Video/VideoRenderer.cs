using System;
using Hazdryx.Drawing;

namespace Avyn.Media.Video
{
    public abstract class VideoRenderer : IVideoStream
    {
        /// <summary>
        ///     Gets the video information for the video being read.
        /// </summary>
        public VideoStreamInfo VideoFormat { get; private set; }
        /// <summary>
        ///     Gets the current frame index.
        /// </summary>
        public int FrameIndex { get; private set; }
        public bool CanRead => true;
        public bool CanWrite => false;

        public TimeSpan Position
        {
            get => new TimeSpan((long) Math.Round(FrameIndex / VideoFormat.FrameRate * TimeSpan.TicksPerSecond));
        }
        public TimeSpan? Duration => VideoFormat.Duration;

        public virtual bool ReadFrame(FastBitmap bmp)
        {
            if (bmp.Width != VideoFormat.Width || bmp.Height != VideoFormat.Height) throw new ArgumentException("The FastBitmap has invlid dimensions.");
            if (Duration.HasValue && Position >= Duration.Value) return false;

            if (RenderFrame(bmp, Position))
            {
                FrameIndex++;
                return true;
            }
            else return false;
        }
        public void WriteFrame(FastBitmap bmp) => throw new InvalidOperationException();

        /// <summary>
        ///     Renders a frame at a specific time.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="time"></param>
        public abstract bool RenderFrame(FastBitmap target, TimeSpan time);
        public abstract void Dispose();
    }
}
