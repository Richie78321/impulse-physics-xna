using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImpulseEngine2;
using ImpulseEngine2.Materials;
using Microsoft.Xna.Framework.Graphics;

namespace Destruction
{
    public class DestructionBlock : RigidBody
    {
        public static Texture2D BlockTexture;

        public static Texture2D ProjectileTexture;

        private static Material GetMaterial(bool projectile)
        {
            if (!projectile) return DefinedMaterials.Wood;
            else return DefinedMaterials.Rock;
        }

        //Object
        private bool projectile;

        public DestructionBlock(RectangleF rectangle, bool projectile = false) : base(new RotationRectangle(rectangle), GetMaterial(projectile))
        {
            this.projectile = projectile;
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            if (projectile)
            {
                CollisionPolygon.FillPolygon(graphicsDevice, ProjectileTexture);
            }
            else
            {
                CollisionPolygon.FillPolygon(graphicsDevice, BlockTexture);
            }
        }
    }
}
