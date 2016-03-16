namespace RemoteCache.Common
{
    class WithAnalyzer
    {
        public bool IsTwoPower { get; private set; }
        public int NormWidth { get; private set; }
        public int NormHeight { get; private set; }

        public WithAnalyzer(int width, int height)
        {
            var t = width;
            var n = 0;
            while (t > 0)
            {
                t = t >> 1;
                n++;
            }

            NormWidth = 1 << (n - 1);
            NormHeight = (int)(NormWidth / ((float)width / height));
            IsTwoPower = NormWidth == width;
        }
    }
}