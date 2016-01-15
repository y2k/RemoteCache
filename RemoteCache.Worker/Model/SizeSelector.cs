using System.Collections.Generic;
using System.Linq;

namespace RemoteCache.Worker.Model
{
    class SizeSelector
    {
        const int MinSize = 25;

        public Size GetBest(Size origin, Size target)
        {
            return ValideSubSizes(origin.Width, origin.Height)
                .Reverse()
                .FirstOrDefault(s => s.Width >= target.Width && s.Height >= target.Height);
        }

        public IEnumerable<Size> ValideSubSizes(int width, int height)
        {
            while (true)
            {
                width /= 2; height /= 2;
                if (width < MinSize || height < MinSize) break;
                yield return new Size(width, height);
            }
        }
    }
}