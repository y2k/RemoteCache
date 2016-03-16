using System;

namespace RemoteCache.Common
{
    class WithAnalyzer
    {
        const int Base = 10;
        
        public bool IsNormalized { get; private set; }
        public int NormWidth { get; private set; }
        public int NormHeight { get; private set; }

        public WithAnalyzer(int width, int height)
        {
            var t = width;
            var n = 0;
            while (t >= Base)
            {
                t = t / Base;
                n++;
            }

            NormWidth = t * (int)Math.Pow(10, n);
            NormHeight = (int)(NormWidth / ((float)width / height));
            IsNormalized = NormWidth == width;
        }
    }
}