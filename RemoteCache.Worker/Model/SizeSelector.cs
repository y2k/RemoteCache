using System;
using System.Collections.Generic;

namespace RemoteCache.Worker.Model
{
    class SizeSelector
    {
        const int MinSize = 50;

        public IEnumerable<Tuple<int, int>> ValideSubSizes(int width, int height)
        {
            while (true)
            {
                width /= 2; height /= 2;
                if (width < MinSize || height < MinSize) break;
                yield return Tuple.Create(width, height);
            }
        }
    }
}