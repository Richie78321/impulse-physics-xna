using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public interface IPhysicsHandler<T> where T : RigidBody
    {
        /// <summary>
        /// Updates the physical simulation.
        /// </summary>
        /// <param name="gameTime">The GameTime of the environment. Target is 60 FPS. Will assume perfect time if left null.</param>
        void Update(GameTime gameTime);

        bool AddBody(T rigidBody);
        bool RemoveBody(T rigidBody);

        bool AddMetaElement(MetaObject metaObject);

        /// <summary>
        /// </summary>
        /// <returns>Returns bodies currently being simulated.</returns>
        T[] GetBodies();
    }
}
