namespace RemoteCache.Worker.Model
{
    struct Size
    {
        public bool IsEmpty { get { return Width == 0 && Height == 0; } }

        public readonly int Width;
        public readonly int Height;

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public override string ToString()
        {
            return $"[{Width}x{Height}]";
        }
    }
}