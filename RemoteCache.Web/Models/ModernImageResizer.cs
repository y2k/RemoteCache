using ImageResizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteCache.Web.Models
{
    public class ModernImageResizer : BaseImageResizer
    {
        public override Stream GetRect(string imagePath, int width, float minAspect = 1, float maxAspect = 1)
        {
            throw new NotImplementedException();
        }
    }
}
