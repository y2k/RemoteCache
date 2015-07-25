using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

namespace RemoteCacheDownloader
{
    public class GifConverter
    {
        public static readonly GifConverter Instance = new GifConverter();

        const int MaxParallerThread = 2;

        Semaphore locker = new Semaphore(MaxParallerThread, MaxParallerThread);

        GifConverter()
        {
        }

        public bool IsCanConvert(string path)
        {
            using (Image img = Image.FromFile(path))
                return img.RawFormat.Equals(ImageFormat.Gif);
        }

        public void ConvertToMp4(string source, string target, string mp4Temp)
        {
            locker.WaitOne();
            try
            {
                const string args = "-i \"{0}\" -vprofile baseline -vcodec libx264 -acodec aac -strict -2 -g 30 -pix_fmt yuv420p -vf \"scale=trunc(in_w/2)*2:trunc(in_h/2)*2\" -f mp4 \"{1}\"";

                var startInfo = new ProcessStartInfo
                {
                    FileName = GetFFFMPEG(),
                    Arguments = string.Format(args, source, mp4Temp),
                    UseShellExecute = false,
                };
                Process.Start(startInfo).WaitForExit();

                File.Move(mp4Temp, target);
            }
            finally
            {
                locker.Release();
            }
        }

        public void ValidateFFMMPEG()
        {
            GetFFFMPEG();
        }

        string GetFFFMPEG()
        {
            var path = Environment.GetEnvironmentVariable("REMOTECACHE_FFMPEG_DIR");
            if (path == null)
                throw new Exception("Env 'REMOTECACHE_FFMPEG_DIR' not found");
            return Path.Combine(path, "ffmpeg" + new PlatformExt().Ext);
        }

        class PlatformExt
        {
            static readonly PlatformID[] Wins = new PlatformID[]
            {
                PlatformID.Win32NT,
                PlatformID.Win32S,
                PlatformID.Win32Windows,
                PlatformID.WinCE,
                PlatformID.Xbox
            };

            public string Ext { get { return IsWindows() ? ".exe" : ""; } }

            static bool IsWindows()
            {
                return Wins.Contains(Environment.OSVersion.Platform);
            }
        }
    }
}