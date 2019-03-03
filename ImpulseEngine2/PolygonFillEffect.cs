using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImpulseEngine2
{
    public class PolygonFillEffect : BasicEffect
    {
        public PolygonFillEffect(GraphicsDevice device, Texture2D texture) : base(device)
        {
            this.TextureEnabled = true;
            this.Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
            this.Texture = texture;
        }
    }
}
