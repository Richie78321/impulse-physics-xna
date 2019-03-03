using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public interface MetaObject
    {
        void Update(GameTime gameTime, RigidBody[] bodies);
    }
}
