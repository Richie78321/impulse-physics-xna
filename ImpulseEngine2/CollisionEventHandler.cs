using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public class CollisionEventHandler<T> : IPhysicsHandler<T> where T : RigidBody
    {
        public event EventHandler OnIntersection;

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

            //Check for interactions
            for (int i = 0; i < rigidBodies.Count; i++)
            {
                for (int j = i + 1; j < rigidBodies.Count; j++)
                {
                    //Check low-level
                    if (LowLevelCollision(rigidBodies[i], rigidBodies[j]))
                    {
                        //Check high-level
                        IntersectionData intersectionData;
                        if (rigidBodies[i].CollisionPolygon.IntersectingPolygon(rigidBodies[j].CollisionPolygon, out intersectionData))
                        {
                            //Invoke event
                            OnIntersection.Invoke(this, new IntersectionEventArgs(rigidBodies[i], rigidBodies[j], intersectionData));
                        }
                    }
                }
            }
        }

        private bool LowLevelCollision(T body1, T body2)
        {
            //Find center distance
            float centerDistance = Polygon.Distance(body1.CollisionPolygon.CenterPoint, body2.CollisionPolygon.CenterPoint);
            return centerDistance <= body1.CollisionPolygon.MaximumRadius + body2.CollisionPolygon.MaximumRadius;
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

    public class IntersectionEventArgs : EventArgs
    {
        public readonly RigidBody Body1, Body2;
        public readonly IntersectionData IntersectionData;
        public IntersectionEventArgs(RigidBody body1, RigidBody body2, IntersectionData intersectionData)
        {
            Body1 = body1;
            Body2 = body2;
            IntersectionData = intersectionData;
        }
    }
}
