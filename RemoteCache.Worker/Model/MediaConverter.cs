﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

namespace RemoteCache.Worker.Model
{
    public class MediaConverter
    {
        public static readonly MediaConverter Instance = new MediaConverter();
        static byte[] WebmMagic = new byte[] { 0x1A, 0x45, 0xDF, 0xA3 };

        const int MaxParallerThread = 2;

        Semaphore locker = new Semaphore(MaxParallerThread, MaxParallerThread);

        MediaConverter()
        {
        }

        public bool IsCanConvert(string path)
        {
            if (IsWebmVideo(path)) return true;
            using (Image img = Image.FromFile(path))
               return img.RawFormat.Equals(ImageFormat.Gif);
        }

        public void ConvertToMp4(string source, string target, string mp4Temp)
        {
            locker.WaitOne();
            try
            {
                var preset = IsWebmVideo(source) ? "veryfast" : "medium";
                var args = $"-i \"{source}\" -preset {preset} -vprofile baseline -vcodec libx264 -acodec aac -strict -2 -g 30 -pix_fmt yuv420p -vf \"scale=trunc(in_w/2)*2:trunc(in_h/2)*2\" -f mp4 \"{mp4Temp}\"";

                var startInfo = new ProcessStartInfo
                {
                    FileName = GetFFFMPEG(),
                    Arguments = args,
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

        bool IsWebmVideo(string path) {
            using (var file = new FileStream(path, FileMode.Open)) {
                var buf = new byte[4];
                file.Read(buf, 0, buf.Length);
                return WebmMagic.SequenceEqual(buf);
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
