using System.IO;
using System.Threading.Tasks;
using RemoteCache.Common;

namespace RemoteCache.Services
{
    public interface IImageResizer
    {
        Task<Stream> GetRectAsync(int? quality, string imagePath, int width, int height, Color color = null);
    }
}