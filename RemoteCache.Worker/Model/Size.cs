namespace RemoteCache.Worker.Model
{
    struct Size
    {
        public readonly int width;
        public readonly int height;

        public Size(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public override string ToString()
        {
            return $"[{width}x{height}]";
        }
    }
}