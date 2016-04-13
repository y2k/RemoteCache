using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RemoteCache.Services
{
    public class LibGDResizer : BaseImageResizer
    {
        Func<int, int, IntPtr> gdImageCreateTrueColor;
        Action<IntPtr, IntPtr, int, int, int, int, int, int, int, int> gdImageCopyResized;
        Action<IntPtr> gdImageDestroy;
        Action<IntPtr> gdFree;
        gdImageJpegPtrDelegate gdImageJpegPtr;

        public LibGDResizer()
        {
            if (IsLinux)
            {
                gdImageCreateTrueColor = GDImportLinux.gdImageCreateTrueColor;
                gdImageCopyResized = GDImportLinux.gdImageCopyResized;
                gdImageDestroy = GDImportLinux.gdImageDestroy;
                gdFree = GDImportLinux.gdFree;
                gdImageJpegPtr = GDImportLinux.gdImageJpegPtr;
            }
            else
            {
                gdImageCreateTrueColor = GDImportOSX.gdImageCreateTrueColor;
                gdImageCopyResized = GDImportOSX.gdImageCopyResized;
                gdImageDestroy = GDImportOSX.gdImageDestroy;
                gdFree = GDImportOSX.gdFree;
                gdImageJpegPtr = GDImportOSX.gdImageJpegPtr;
            }
        }

        public override Stream GetRect(int? quality, string imagePath, int width, int height)
        {
            var srcImage = CreateImageFromBytes(File.ReadAllBytes(imagePath));
            var dstImage = gdImageCreateTrueColor(width, height);

            var destAspect = (float)width / height;
            var srcAspect = (float)GetWidth(srcImage) / GetHeight(srcImage);
            if (destAspect > srcAspect)
            {
                var h = (int)(GetWidth(srcImage) / destAspect);
                gdImageCopyResized(
                    dstImage, srcImage,
                    0, 0,
                    0, (GetHeight(srcImage) - h) / 2,
                    width, height,
                    GetWidth(srcImage), h);
            }
            else
            {
                var w = (int)(GetHeight(srcImage) * destAspect);
                gdImageCopyResized(
                    dstImage, srcImage,
                    0, 0,
                    (GetWidth(srcImage) - w) / 2, 0,
                    width, height,
                    w, GetHeight(srcImage));
            }
            gdImageDestroy(srcImage);

            int size;
            var buffer = gdImageJpegPtr(dstImage, out size, quality ?? 75);
            gdImageDestroy(dstImage);

            var data = new byte[size];
            Marshal.Copy(buffer, data, 0, data.Length);
            gdFree(buffer);
            return new MemoryStream(data);
        }

        private IntPtr CreateImageFromBytes(byte[] data)
        {
            Func<int, IntPtr, IntPtr> gdImageCreateFromPtr = null;
            if (IsLinux)
            {
                if (data[0] == 0xFF) gdImageCreateFromPtr = GDImportLinux.gdImageCreateFromJpegPtr;
                else if (data[0] == 0x89) gdImageCreateFromPtr = GDImportLinux.gdImageCreateFromPngPtr;
                else if (data[0] == 0x47) gdImageCreateFromPtr = GDImportLinux.gdImageCreateFromGifPtr;
            }
            else
            {
                if (data[0] == 0xFF) gdImageCreateFromPtr = GDImportOSX.gdImageCreateFromJpegPtr;
                else if (data[0] == 0x89) gdImageCreateFromPtr = GDImportOSX.gdImageCreateFromPngPtr;
                else if (data[0] == 0x47) gdImageCreateFromPtr = GDImportOSX.gdImageCreateFromGifPtr;
            }

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

        public static bool IsLinux
        {
            get { return Environment.GetEnvironmentVariable("OS_TYPE") == "LINUX"; }
        }

        delegate IntPtr gdImageJpegPtrDelegate(IntPtr a, out int b, int c);
    }
}