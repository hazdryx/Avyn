using Avyn.Media.Audio;
using Avyn.Media.Video;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Avyn.Media
{
    /// <summary>
    ///     A class for retreiving stream information about a media file (audio/video).
    /// </summary>
    public class MediaInfo
    {
        /// <summary>
        ///     Gets the duration of the media.
        /// </summary>
        public TimeSpan Duration { get; }
        /// <summary>
        ///     Gets the short format name (usually used after -f in FFmpeg).
        /// </summary>
        public string FormatCode { get; }
        /// <summary>
        ///     Gets the format name of the media.
        /// </summary>
        public string FormatName { get; }
        /// <summary>
        ///     Gets stream information for all media streams.
        /// </summary>
        public IMediaStreamInfo[] Streams { get; }

        public MediaInfo(TimeSpan duration, string formatCode, string formatName, IMediaStreamInfo[] streams)
        {
            this.Duration = duration;
            this.FormatCode = formatCode;
            this.FormatName = formatName;
            this.Streams = streams;
        }

        /// <summary>
        ///     Gets the first video stream info.
        /// </summary>
        public VideoStreamInfo VideoStream => GetVideoStream(0);
        /// <summary>
        ///     Gets the first audio stream info.
        /// </summary>
        public AudioStreamInfo AudioStream => GetAudioStream(0);

        /// <summary>
        ///     Gets the nth video stream info in the file.
        /// </summary>
        /// <param name="index">The index of the video stream format.</param>
        /// <returns>The nth video stream format.</returns>
        public VideoStreamInfo GetVideoStream(int index)
        {
            int n = -1;
            foreach (IMediaStreamInfo format in Streams)
            {
                if (format is VideoStreamInfo && ++n == index) return (VideoStreamInfo)format;
            }
            return VideoStreamInfo.Empty();
        }

        /// <summary>
        ///     Gets the nth audio stream info in the file.
        /// </summary>
        /// <param name="index">The index of the audio stream format.</param>
        /// <returns>The nth audio stream format.</returns>
        public AudioStreamInfo GetAudioStream(int index)
        {
            int n = -1;
            foreach (IMediaStreamInfo format in Streams)
            {
                if (format is AudioStreamInfo && ++n == index) return (AudioStreamInfo)format;
            }
            return AudioStreamInfo.Empty();
        }

        /// <summary>
        ///     Gets all media stream information from a file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static MediaInfo FromFile(string filename)
        {
            if (!File.Exists(filename)) throw new IOException("Couldn't find file to get information from.");

            // Start an ffprobe process. These just output and forget.
            Process ffprobe = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe.exe",
                    Arguments = "-hide_banner -i \"" + filename + "\" -show_format -show_streams",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            ffprobe.Start();
            StreamReader output = ffprobe.StandardOutput;
            TimeSpan duration = TimeSpan.Zero;
            string formatCode = "";
            string formatName = "";
            List<IMediaStreamInfo> formats = new List<IMediaStreamInfo>();

            // Loop through each data section.
            Dictionary<string, string> section;
            while ((section = NextSection(output, out string name)) != null)
            {
                if (name == "STREAM")
                {
                    // Stream information.
                    if (section["codec_type"] == "video")
                    {
                        // Video stream info.
                        int width = int.Parse(section["width"]);
                        int height = int.Parse(section["height"]);
                        string[] rfps = section["r_frame_rate"].Split('/');
                        double fps = double.Parse(rfps[0]) / double.Parse(rfps[1]);

                        // Codec Information
                        string codec = section["codec_name"];
                        string pixelFormat = section["pix_fmt"];
                        int bitrate = int.Parse(section["bit_rate"]);

                        // Time Info
                        TimeSpan start = TimeSpan.Zero;
                        if(double.TryParse(section["start_time"], out double t0))
                        {
                            start = TimeSpan.FromSeconds(t0);
                        }
                        TimeSpan dur = TimeSpan.Zero;
                        if(double.TryParse(section["duration"], out double t1))
                        {
                            dur = TimeSpan.FromSeconds(t1);
                        }

                        formats.Insert(int.Parse(section["index"]), new VideoStreamInfo(width, height, fps, start, dur, codec, pixelFormat, bitrate));
                    }
                    else if (section["codec_type"] == "audio")
                    {
                        // Audio stream info.
                        int channels = int.Parse(section["channels"]);
                        int sampleRate = int.Parse(section["sample_rate"]);
                        string codec = section["codec_name"];

                        // Time Info
                        TimeSpan start = TimeSpan.Zero;
                        if (double.TryParse(section["start_time"], out double t0))
                        {
                            start = TimeSpan.FromSeconds(t0);
                        }
                        TimeSpan dur = TimeSpan.Zero;
                        if (double.TryParse(section["duration"], out double t1))
                        {
                            dur = TimeSpan.FromSeconds(t1);
                        }

                        formats.Insert(int.Parse(section["index"]), new AudioStreamInfo(start, dur, codec, channels, sampleRate, 16));
                    }
                }
                else if (name == "FORMAT")
                {
                    // Gets the full duration.
                    duration = TimeSpan.FromSeconds(double.Parse(section["duration"]));

                    // Gets format code.
                    string[] formatCodes = section["format_name"].Split(',');
                    formatCode = formatCodes[0]; // Default.

                    // Trys to match format code to file extension.
                    string fileExt = new FileInfo(filename).Extension;
                    foreach(string code in section["format_name"].Split(','))
                    {
                        if ("." + code == fileExt)
                        {
                            formatCode = code;
                            break;
                        }
                    }

                    // Gets format name.
                    formatName = section["format_long_name"];
                }
            }

            // returns the data as a MediaInfo instance.
            ffprobe.Dispose();
            return new MediaInfo(duration, formatCode, formatName, formats.ToArray());
        }
        private static Dictionary<string, string> NextSection(StreamReader read, out string name)
        {
            Dictionary<string, string> section = new Dictionary<string, string>();
            name = null;

            while (!read.EndOfStream)
            {
                string line = read.ReadLine().Trim();
                Debug.WriteLine(line); // Debugs the ffprobe output.
                if (line.Length == 0) continue;

                if (name == null)
                {
                    if (!line.StartsWith("[")) throw new FormatException("Must start with a TAG.");

                    name = line.Substring(1, line.Length - 2);
                    section = new Dictionary<string, string>();
                }
                else if (line == "[/" + name + "]") break;
                else
                {
                    string[] tokens = line.Split('=');
                    section.Add(tokens[0], tokens[1]);
                }
            }
            return name == null ? null : section;
        }
    }
}
