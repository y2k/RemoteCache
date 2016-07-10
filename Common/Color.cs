using System;

namespace RemoteCache.Common
{
    public class Color
    {
        readonly uint color;

        public Color(string textColor)
        {
            color = Convert.ToUInt32(textColor, 16);
        }
    }
}