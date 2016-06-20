using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RemoteCache.Common;

namespace RemoteCache.Services.Downloader
{
    public class MediaConverter
    {
        public static readonly MediaConverter Instance = new MediaConverter();
        static byte[] WebmMagic = new byte[] { 0x1A, 0x45, 0xDF, 0xA3 };
        static byte[] GifMagic = new byte[] { 0x47, 0x49, 0x46, 0x38 };

        readonly SemaphoreSlim locker = new SemaphoreSlim(1);

        MediaConverter()
        {
        }

        public bool IsCanConvert(string path)
        {
            return IsGifVideo(path) || IsWebmVideo(path);
        }

        public async Task ConvertToMp4(string source, string target, string mp4Temp)
        {
            using (await locker.Use())
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
        }

        bool IsWebmVideo(string path)
        {
            using (var file = new FileStream(path, FileMode.Open))
            {
                var buf = new byte[WebmMagic.Length];
                file.Read(buf, 0, buf.Length);
                return WebmMagic.SequenceEqual(buf);
            }
        }

        bool IsGifVideo(string path)
        {
            using (var file = new FileStream(path, FileMode.Open))
            {
                var buf = new byte[GifMagic.Length];
                file.Read(buf, 0, buf.Length);
                return GifMagic.SequenceEqual(buf);
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
            public string Ext { get { return IsWindows() ? ".exe" : ""; } }

            static bool IsWindows()
            {
                return Environment.GetEnvironmentVariable("REMOTECACHE_OS")?.Contains("win") ?? false;
            }
        }
    }
}