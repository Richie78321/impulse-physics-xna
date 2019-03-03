using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public class ElasticJoint : MetaObject
    {
        private const float JOINT_SLOP = 1F;
        private const float MAGNITUDE_LIMIT = 10F;

        private const int SETTLE_RESOLUTION = 5;

        //Object
        private RigidBody rb1, rb2;
        private PolygonPoint p1, p2;

        private float elasticity;
        private float targetDistance;

        /// <summary>
        /// Creates a joint that applies forces to both bodies to keep the points together.
        /// </summary>
        /// <param name="elasticity">The elasticity of the joint.</param>
        /// <param name="rb1">The first RigidBody.</param>
        /// <param name="rb2">The second RigidBody.</param>
        /// <param name="point">The point to preserve.</param>
        /// <param name="stretchLimit">The limit of stretching before the joint breaks.</param>
        public ElasticJoint(float elasticity, RigidBody rb1, RigidBody rb2, Vector2 point)
        {
            targetDistance = 0;
            this.rb1 = rb1;
            this.rb2 = rb2;
            this.p1 = new PolygonPoint(point, rb1.CollisionPolygon);
            this.p2 = new PolygonPoint(point, rb2.CollisionPolygon);

            this.elasticity = elasticity;
        }

        /// <summary>
        /// Creates a joint that applies forces to both bodies to keep the points at a specified distance.
        /// </summary>
        /// <param name="elasticity">Elasticity of the joint.</param>
        /// <param name="rb1">The first RigidBody.</param>
        /// <param name="p1">The point on the first RigidBody.</param>
        /// <param name="rb2">The second RigidBody.</param>
        /// <param name="p2">The point on the second RigidBody.</param>
        /// <param name="stretchLimit"></param>
        public ElasticJoint(float elasticity, RigidBody rb1, Vector2 p1, RigidBody rb2, Vector2 p2)
        {
            targetDistance = LineSegment.Distance(p1, p2);
            this.rb1 = rb1;
            this.rb2 = rb2;
            this.p1 = new PolygonPoint(p1, rb1.CollisionPolygon);
            this.p2 = new PolygonPoint(p2, rb2.CollisionPolygon);

            this.elasticity = elasticity;
        }

        public void Update(GameTime gameTime, RigidBody[] rigidBodies)
        {
            PositionalCorrection(p2.GetPoint() - p1.GetPoint());

            LineSegment disparitySegment = new LineSegment(p1.GetPoint(), p2.GetPoint());
            float disparityMagnitude = disparitySegment.Length;
            if (disparityMagnitude - targetDistance > JOINT_SLOP)
            {
                float springForce = elasticity * disparityMagnitude;
                rb1.ApplyForce((p2.GetPoint() - p1.GetPoint()) * springForce, p1.GetPoint());
                rb2.ApplyForce((p1.GetPoint() - p2.GetPoint()) * springForce, p2.GetPoint());
            }
        }

        private const float CORRECTION_PERCENTAGE = .2F;
        private void PositionalCorrection(Vector2 separationAxis)
        {
            float separationMagnitude = separationAxis.Length();
            if (separationMagnitude >= JOINT_SLOP)
            {
                float totalMass = rb1.Mass + rb2.Mass;
                Vector2 correctionMagnitude = separationAxis * CORRECTION_PERCENTAGE;
                rb2.CollisionPolygon.Translate(-correctionMagnitude * (rb2.Mass / totalMass));
                rb1.CollisionPolygon.Translate(correctionMagnitude * (rb1.Mass / totalMass));
            }
        }
    }
}
