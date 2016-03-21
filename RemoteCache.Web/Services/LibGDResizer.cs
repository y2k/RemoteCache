using System;
using System.IO;
using System.Runtime.InteropServices;
using RemoteCache.Web.Models;

namespace RemoteCache.Services
{
    public class LibGDResizer : BaseImageResizer
    {
        public override Stream GetRect(int? quality, string imagePath, int width, int height)
        {
            // Console.WriteLine("q={0},path={1},w={2},h={3}", quality, imagePath, width, height);

            var srcImage = GDImport.gdImageCreateFromFile(imagePath + ".jpeg");
            var dstImage = GDImport.gdImageCreateTrueColor(width, height);

            // Console.WriteLine("src={0}, dest={1}, width={2}", srcImage, dstImage, GetWidth(srcImage));

            var destAspect = (float)width / height;
            var srcAspect = (float)GetWidth(srcImage) / GetHeight(srcImage);
            if (destAspect > srcAspect)
            {
                GDImport.gdImageCopyResized(
                    dstImage, srcImage,
                    0, 0,
                    0, (int)(GetHeight(srcImage) - GetHeight(srcImage) / destAspect) / 2,
                    width, height,
                    GetWidth(srcImage), (int)(GetHeight(srcImage) / destAspect));
            }
            else
            {
                GDImport.gdImageCopyResized(
                    dstImage, srcImage,
                    0, 0,
                    (int)(GetWidth(srcImage) - GetWidth(srcImage) * destAspect) / 2, 0,
                    width, height,
                    (int)(GetWidth(srcImage) * destAspect), GetHeight(srcImage));
            }
            GDImport.gdImageDestroy(srcImage);

            int size;
            var buffer = GDImport.gdImageJpegPtr(dstImage, out size, quality ?? 75);
            GDImport.gdImageDestroy(dstImage);

            var data = new byte[size];
            Marshal.Copy(buffer, data, 0, data.Length);
            GDImport.gdFree(buffer);

            return new MemoryStream(data);
        }

        int GetWidth(IntPtr image)
        {
            return Marshal.ReadInt32(image, 4);
        }

        int GetHeight(IntPtr image)
        {
            return Marshal.ReadInt32(image, 8);
        }
    }
}