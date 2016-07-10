using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using RemoteCache.Common;

namespace RemoteCache.Services.Resizer
{
    public class LibGDResizer : IImageResizer
    {
        public Task<Stream> GetRectAsync(int? quality, string imagePath, int width, int height, Color color = null)
        {
            return Task.Run(() => GetRect(quality, imagePath, width, height, color));
        }

        Stream GetRect(int? quality, string imagePath, int width, int height, Color color)
        {
            var srcImage = CreateImageFromBytes(File.ReadAllBytes(imagePath));
            var dstImage = GDImport.gdImageCreateTrueColor(width, height);

            if (color != null)
            {
                var c = GDImport.gdImageColorAllocate(dstImage, color.R, color.G, color.B);
                GDImport.gdImageFill(dstImage, 0, 0, c);
            }

            var destAspect = (float)width / height;
            var srcAspect = (float)GetWidth(srcImage) / GetHeight(srcImage);
            if (destAspect > srcAspect)
            {
                var h = (int)(GetWidth(srcImage) / destAspect);
                GDImport.gdImageCopyResized(
                    dstImage, srcImage,
                    0, 0,
                    0, (GetHeight(srcImage) - h) / 2,
                    width, height,
                    GetWidth(srcImage), h);
            }
            else
            {
                var w = (int)(GetHeight(srcImage) * destAspect);
                GDImport.gdImageCopyResized(
                    dstImage, srcImage,
                    0, 0,
                    (GetWidth(srcImage) - w) / 2, 0,
                    width, height,
                    w, GetHeight(srcImage));
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

        IntPtr CreateImageFromBytes(byte[] data)
        {
            Func<int, IntPtr, IntPtr> gdImageCreateFromPtr = null;
            if (data[0] == 0xFF) gdImageCreateFromPtr = GDImport.gdImageCreateFromJpegPtr;
            else if (data[0] == 0x89) gdImageCreateFromPtr = GDImport.gdImageCreateFromPngPtr;
            else if (data[0] == 0x47) gdImageCreateFromPtr = GDImport.gdImageCreateFromGifPtr;

            if (gdImageCreateFromPtr == null) throw new NotImplementedException();
            var ptr = Marshal.AllocHGlobal(data.Length);
            try
            {
                Marshal.Copy(data, 0, ptr, data.Length);
                return gdImageCreateFromPtr(data.Length, ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        int GetWidth(IntPtr image) => Marshal.ReadInt32(image, IntPtr.Size);
        int GetHeight(IntPtr image) => Marshal.ReadInt32(image, IntPtr.Size + 4);

        delegate IntPtr gdImageJpegPtrDelegate(IntPtr a, out int b, int c);
    }
}