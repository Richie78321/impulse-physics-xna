using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public class GravityMeta : MetaObject
    {
        private float gravityAcceleration;
        public GravityMeta(float gravityAcceleration = .1F)
        {
            this.gravityAcceleration = gravityAcceleration;
        }

        public void Update(GameTime gameTime, RigidBody[] bodies)
        {
            //Apply gravity
            foreach (RigidBody b in bodies)
            {
                b.AddTranslationalVelocity(new Vector2(0, gravityAcceleration), isMomentum: false);
            }
        }
    }
}
