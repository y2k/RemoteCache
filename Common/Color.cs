using System;

namespace RemoteCache.Common
{
    public class Color
    {
        public int R { get { return (color >> 16) & 0xFF; } }
        public int G { get { return (color >> 8) & 0xFF; } }
        public int B { get { return color & 0xFF; } }

        readonly int color;

        public Color(string textColor)
        {
            color = Convert.ToInt32(textColor, 16);
        }
    }
}