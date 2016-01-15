using System.Drawing;
using System.IO;
using System.Linq;

namespace RemoteCache.Worker.Model
{
    class ImageMeta
    {
        public Size Get(string path)
        {
            var dimensions = File.ReadAllText($"{path}.info").Split(',').Select(s => int.Parse(s)).ToList();
            return new Size(dimensions[0], dimensions[1]);
        }

        public void Save(string path)
        {
            var size = Image.FromFile(path).Size;
            File.WriteAllText($"{path}.info", $"{size.Width},{size.Height}");
        }
    }
}