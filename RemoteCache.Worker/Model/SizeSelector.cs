using System.Collections.Generic;
using System.Linq;

namespace RemoteCache.Worker.Model
{
    class SizeSelector
    {
        const int MinSize = 25;

        public Size GetBest(Size origin, Size target)
        {
            return ValideSubSizes(origin.width, origin.height)
                .Reverse()
                .FirstOrDefault(s => s.width >= target.width && s.height >= target.height);
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