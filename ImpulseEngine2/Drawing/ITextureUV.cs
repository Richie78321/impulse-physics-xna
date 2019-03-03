using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImpulseEngine2.Drawing
{
    public interface ITextureUV
    {
        Texture2D GetTexture();
        Vector2[] GetUVPoints();
        RectangleF GetUVBounds();
    }
}
