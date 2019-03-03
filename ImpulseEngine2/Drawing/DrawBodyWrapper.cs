using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImpulseEngine2.Materials;

namespace ImpulseEngine2.Drawing
{
    public class DrawBodyWrapper : RigidBody
    {
        public readonly ITextureUV TextureUV;
        public DrawBodyWrapper(ITextureUV textureUV, Polygon collisionPolygon, Material material, int collisionLevel = 0) : base(collisionPolygon, material, collisionLevel)
        {
            this.TextureUV = textureUV;
        }

        public DrawBodyWrapper(ITextureUV textureUV, Polygon collisionPolygon, float Bounce, float Mass, float StaticFriction, float DynamicFriction, int collisionLevel = 0) : base(collisionPolygon, Bounce, Mass, StaticFriction, DynamicFriction, collisionLevel)
        {
            this.TextureUV = textureUV;
        }
    }
}
