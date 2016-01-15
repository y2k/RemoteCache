using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RemoteCache.Worker.Model
{
    class ImageResizer
    {
        ImageStorage storage;
        SizeSelector sizeSelector;

        public ImageResizer(ImageStorage storage, SizeSelector sizeSelector)
        {
            this.storage = storage;
            this.sizeSelector = sizeSelector;
        }

        public void Resize(Uri url)
        {
            var originalBitmap = Bitmap.FromFile(storage.GetPathForImage(url));
            var thumbSizes = sizeSelector.ValideSubSizes(originalBitmap.Width, originalBitmap.Height);
            
            foreach (var size in thumbSizes)
            {
                var thumb = Convert(originalBitmap, size.Item1, size.Item2);
                var thumbFile = storage.GetThubmnail(url, size.Item1, size.Item2);
                thumb.Save(thumbFile, originalBitmap.RawFormat);
            }
        }

        Bitmap Convert(Image image, int width, int height)
        {
            var fh = (float)width / image.Width * image.Height;
            var thumb = new Bitmap(width, height);
            
            using (var g = NewGraphics(thumb))
            {
                float s = (float)image.Height / image.Width;
                var rect = new RectangleF(0, -(thumb.Width * s - thumb.Height) / 2, thumb.Width, thumb.Width * s);
                if (rect.Height < thumb.Height)
                {
                    var correctScale = thumb.Height / rect.Height;
                    rect.Height *= correctScale;
                    rect.Width *= correctScale;
                    rect.X = (thumb.Width - rect.Width) / 2;
                    rect.Y = 0;
                }
                g.DrawImage(image, rect);
            }
            return thumb;
        }

        Graphics NewGraphics(Bitmap thumb)
        {
            var canvas = Graphics.FromImage(thumb);
            canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
            return canvas;
        }
    }
}