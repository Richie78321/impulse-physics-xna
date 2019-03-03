using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public class SimpleHandler<T> : IPhysicsHandler<T> where T : RigidBody
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

        public void Update(GameTime gameTime)
        {
            //Update meta objects
            RigidBody[] rigidBodyArrays = rigidBodies.ToArray();
            foreach (MetaObject b in metaObjects) b.Update(gameTime, rigidBodyArrays);

            //Update bodies
            foreach (RigidBody b in rigidBodies) b.Update(gameTime);

            //Check for interactions
            for (int i = 0; i < rigidBodies.Count; i++)
            {
                for (int j = i + 1; j < rigidBodies.Count; j++)
                {
                    IntersectionEvent intersectionEvent;
                    if (RigidBody.AreColliding(rigidBodies[i], rigidBodies[j], out intersectionEvent))
                    {
                        intersectionEvent.SettleCollision();
                    }
                }
            }
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
