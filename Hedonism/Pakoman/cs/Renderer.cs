using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hedonism
{
    public class RenderInfo
    {
        public char renderImage { get; set; }
        public Vector2 renderPosition { get; set; }
    }

    public interface IRenderable
    {
        RenderInfo renderInfo { get; }

    }
}


