using System;
using System.Diagnostics;
using System.IO;

namespace Avyn.Media
{
    /// <summary>
    ///     A simple class for querying ffmpeg.exe.
    /// </summary>
    public static class FFmpeg
    {
        /// <summary>
        ///     Starts an FFmpeg process with the query and arguments.
        ///     
        ///     Note:
        ///     1. The query and arguments are passed into string.Format
        ///     and should be formated as such.
        ///     2. Strings which would require quotes (such as file names)
        ///     can have @ put in front to shorten code.
        ///     3. This calls "ffmpeg.exe", so that file must be in a system path.
        /// </summary>
        /// <param name="query">Query string for FFmpeg in string.Format format.</param>
        /// <param name="args">Arguments which go into string.Format.</param>
        /// <returns>The ffmpeg process which was started.</returns>
        public static Process Query(string query, params object[] args)
        {
            string[] tokens = query.Split(' ');
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].StartsWith("@")) tokens[i] = "\"" + tokens[i].Substring(1) + "\"";
            }
            string arguments = "-hide_banner " + string.Format(string.Join(" ", tokens), args);

            Debug.WriteLine("FFmpeg Query | " + arguments);

            Process ffmpeg = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }
            };
            ffmpeg.Start();
            return ffmpeg;
        }

        /// <summary>
        ///     Reads the standard error of a ffmpeg Process and writes to the Debug console. 
        /// </summary>
        /// <param name="ffmpeg"></param>
        /// <param name="header"></param>
        /// <param name="callback"></param>
        public static void DebugStandardError(Process ffmpeg, string header, Action<string> callback = null)
        {
            StreamReader err = ffmpeg.StandardError;
            while (!ffmpeg.HasExited && !err.EndOfStream)
            {
                string line = err.ReadLine();
                Debug.WriteLine(header + " (FFmpeg) | " + line);
                callback?.Invoke(line);
            }
        }

        //
        // FFmpeg Quick Queries
        //

        /// <summary>
        ///     Interweaves a video and audio stream into a single file.
        /// </summary>
        /// <param name="videoPath">Path of the video file.</param>
        /// <param name="audioPath">Path of the audio file.</param>
        /// <param name="duration">Duration of the final video.</param>
        /// <param name="filename">Output filename.</param>
        public static void Interweave(string videoPath, string audioPath, TimeSpan duration, string filename)
        {
            Process ffmpeg = Query(
                "-i @{0} -i @{1} -c copy -map 0:v:0 -map 1:a:0 -t {2} -y @{3}",
                videoPath, audioPath, duration.TotalSeconds, filename
            );
            DebugStandardError(ffmpeg, "Interweave");
            ffmpeg.WaitForExit();
            ffmpeg.Dispose();
        }
        /// <summary>
        ///     Interweaves a video and audio stream into a single file.
        ///     The duration of output video is equal to the input video.
        /// </summary>
        /// <param name="videoPath">Path of the video file.</param>
        /// <param name="audioPath">Path of the audio file.</param>
        /// <param name="filename">Output filename.</param>
        public static void Interweave(string videoPath, string audioPath, string filename)
        {
            MediaInfo info = MediaInfo.FromFile(videoPath);
            Interweave(videoPath, audioPath, info.Duration, filename);
        }
    }
}
