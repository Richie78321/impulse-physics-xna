using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public class NoCollisionHandler<T> : IPhysicsHandler<T> where T : RigidBody
    {
        private List<T> rigidBodies = new List<T>();

        public bool AddBody(T rigidBody)
        {
            rigidBodies.Add(rigidBody);
            return true;
        }

        public bool RemoveBody(T rigidBody)
        {
            return rigidBodies.Remove(rigidBody);
        }

        public virtual void Update(GameTime gameTime)
        {
            //Update meta objects
            RigidBody[] rigidBodyArray = rigidBodies.ToArray();
            foreach (MetaObject b in metaObjects) b.Update(gameTime, rigidBodyArray);

            //Update bodies
            foreach (RigidBody b in rigidBodies) b.Update(gameTime);
        }

        public T[] GetBodies()
        {
            return rigidBodies.ToArray();
        }

        private List<MetaObject> metaObjects = new List<MetaObject>();

        public bool AddMetaElement(MetaObject metaObject)
        {
            metaObjects.Add(metaObject);
            return true;
        }

        public bool RemoveMetaElement(MetaObject metaObject)
        {
            return metaObjects.Remove(metaObject);
        }
    }
}
