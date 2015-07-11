using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RemoteCacheDownloader
{
    public class GifConverter
    {
        public static readonly GifConverter Instance = new GifConverter();

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
            const string args = "-i {0} -vprofile baseline -vcodec libx264 -acodec aac -strict -2 -g 30 -pix_fmt yuv420p -vf \"scale=trunc(in_w/2)*2:trunc(in_h/2)*2\" -f mp4 {1}";

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = GetFFFMPEG(),
                    Arguments = string.Format(args, source, mp4Temp)
                }).WaitForExit();

            File.Move(mp4Temp, target);
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
            return Path.Combine(path, "ffmpeg");
        }
    }
}