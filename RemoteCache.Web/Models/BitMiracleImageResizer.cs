using System.Drawing;
using System.IO;
using BitMiracle.LibJpeg;

namespace RemoteCache.Web.Models
{
    class BitMiracleImageResizer : DefaultImageResizer {
        
        protected override void EncodeToJpeg(Stream stream, Image image, int quality) {
            JpegImage
                .FromBitmap((Bitmap)image)
                .WriteJpeg(stream, new CompressionParameters { Quality = quality });
        }
    }
}