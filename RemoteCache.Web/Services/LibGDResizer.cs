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
            var data = Environment.OSVersion.Platform == PlatformID.MacOSX
                ? GetRectOSX(quality, imagePath, width, height)
                : GetRectLinux(quality, imagePath, width, height);
            return new MemoryStream(data);
        }

        byte[] GetRectLinux(int? quality, string imagePath, int width, int height)
        {
            var srcImage = GDImportLinux.gdImageCreateFromFile(imagePath+".png");
            var dstImage = GDImportLinux.gdImageCreateTrueColor(width, height);

            var destAspect = (float)width / height;
            var srcAspect = (float)GetWidth(srcImage) / GetHeight(srcImage);
            if (destAspect > srcAspect)
            {
                GDImportLinux.gdImageCopyResized(
                    dstImage, srcImage,
                    0, 0,
                    0, (int)(GetHeight(srcImage) - GetHeight(srcImage) / destAspect) / 2,
                    width, height,
                    GetWidth(srcImage), (int)(GetHeight(srcImage) / destAspect));
            }
            else
            {
                GDImportLinux.gdImageCopyResized(
                    dstImage, srcImage,
                    0, 0,
                    (int)(GetWidth(srcImage) - GetWidth(srcImage) * destAspect) / 2, 0,
                    width, height,
                    (int)(GetWidth(srcImage) * destAspect), GetHeight(srcImage));
            }
            GDImportLinux.gdImageDestroy(srcImage);

            int size;
            var buffer = GDImportLinux.gdImageJpegPtr(dstImage, out size, quality ?? 75);
            GDImportLinux.gdImageDestroy(dstImage);

            var data = new byte[size];
            Marshal.Copy(buffer, data, 0, data.Length);
            GDImportLinux.gdFree(buffer);
            return data;
        }

        byte[] GetRectOSX(int? quality, string imagePath, int width, int height)
        {
            var srcImage = GDImportOSX.gdImageCreateFromFile(imagePath + ".jpeg");
            var dstImage = GDImportOSX.gdImageCreateTrueColor(width, height);

            var destAspect = (float)width / height;
            var srcAspect = (float)GetWidth(srcImage) / GetHeight(srcImage);
            if (destAspect > srcAspect)
            {
                GDImportOSX.gdImageCopyResized(
                    dstImage, srcImage,
                    0, 0,
                    0, (int)(GetHeight(srcImage) - GetHeight(srcImage) / destAspect) / 2,
                    width, height,
                    GetWidth(srcImage), (int)(GetHeight(srcImage) / destAspect));
            }
            else
            {
                GDImportOSX.gdImageCopyResized(
                    dstImage, srcImage,
                    0, 0,
                    (int)(GetWidth(srcImage) - GetWidth(srcImage) * destAspect) / 2, 0,
                    width, height,
                    (int)(GetWidth(srcImage) * destAspect), GetHeight(srcImage));
            }
            GDImportOSX.gdImageDestroy(srcImage);

            int size;
            var buffer = GDImportOSX.gdImageJpegPtr(dstImage, out size, quality ?? 75);
            GDImportOSX.gdImageDestroy(dstImage);

            var data = new byte[size];
            Marshal.Copy(buffer, data, 0, data.Length);
            GDImportOSX.gdFree(buffer);
            return data;
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
