using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using ImpulseEngine2.Materials;
using ImpulseEngine2.Drawing;

namespace ImpulseEngine2
{
    public class RigidBody
    {
        private const int TARGET_FPS = 60;
        private const float TARGET_FRAMETIME = 1000F / TARGET_FPS;

        public static bool AreColliding(RigidBody rigidBody1, RigidBody rigidBody2, out IntersectionEvent intersectionEvent)
        {
            //Get side segments
            RigidBody[] rigidBodies = new RigidBody[] { rigidBody1, rigidBody2 };
            LineSegment[][] sideSegments = new LineSegment[rigidBodies.Length][];
            for (int i = 0; i < sideSegments.Length; i++) sideSegments[i] = rigidBodies[i].CollisionPolygon.SideSegments;

            //Determine overlapping axes
            LineSegment minimumOverlap = null;
            RigidBody recipientRB = null;
            int recipientRBIndex = 0;
            LineSegment recipientSide = null;
            for (int i = 0; i < sideSegments.Length; i++)
            {
                if (rigidBodies[i].CollisionPolygon.ContainsPoint(rigidBodies[(i + 1) % rigidBodies.Length].CollisionPolygon.Vertices))
                {
                    for (int j = 0; j < sideSegments[i].Length; j++)
                    {
                        Line segmentNormal = sideSegments[i][j].NormalLine;
                        LineSegment[] bodyProjections = new LineSegment[rigidBodies.Length];
                        for (int k = 0; k < rigidBodies.Length; k++) bodyProjections[k] = rigidBodies[(k + i) % rigidBodies.Length].CollisionPolygon.PolygonProjection(segmentNormal);

                        //Determine overlap
                        LineSegment projectionOverlap;
                        if (bodyProjections[0].IsOverlapping(bodyProjections[1], out projectionOverlap))
                        {
                            //Check for minimum
                            Vector2 intersectionPoint = new Vector2();
                            if (projectionOverlap.IntersectsSegment(sideSegments[i][j], ref intersectionPoint) && (minimumOverlap == null || minimumOverlap.Length > projectionOverlap.Length))
                            {
                                minimumOverlap = projectionOverlap;
                                recipientRB = rigidBodies[i];
                                recipientRBIndex = i;
                                recipientSide = sideSegments[i][j];
                                //intersectionRecipient = new IntersectionRecipient(rigidBodies[i], sideSegments[i][j]);
                            }
                        }
                        else
                        {
                            //No overlap, not colliding
                            intersectionEvent = null;
                            return false;
                        }
                    }
                }
            }

            if (minimumOverlap != null)
            {
                intersectionEvent = new IntersectionEvent(rigidBodies[(recipientRBIndex + 1) % 2], recipientRB, new IntersectionData(false, minimumOverlap, recipientSide, true));
                return true;
            }
            else
            {
                intersectionEvent = null;
                return false;
            }
        }

        //public static bool AreColliding(RigidBody rigidBody1, RigidBody rigidBody2, out IntersectionEvent intersectionEvent)
        //{
        //    IntersectionData intersectionData;
        //    if (rigidBody1.CollisionPolygon.IntersectingPolygon(rigidBody2.CollisionPolygon, out intersectionData) && intersectionData.ValidOverlap)
        //    {
        //        RigidBody sender, reciever;
        //        if (intersectionData.RecipientIsFunctionCaller)
        //        {
        //            sender = rigidBody2;
        //            reciever = rigidBody1;
        //        }
        //        else
        //        {
        //            sender = rigidBody1;
        //            reciever = rigidBody2;
        //        }

        //        intersectionEvent = new IntersectionEvent(sender, reciever, intersectionData);
        //        return true;
        //    }
        //    else
        //    {
        //        intersectionEvent = null;
        //        return false;
        //    }
        //}

        public static RigidBody[] ApplySplit(RigidBody originalBody, Polygon[] newSplits, ITextureUV parentUV, out StandardUVMap[] childrenUV)
        {
            Material originalMaterial = originalBody.material;

            RigidBody[] newBodies = new RigidBody[newSplits.Length];

            if (parentUV != null)
            {
                childrenUV = new StandardUVMap[newBodies.Length];
                for (int i = 0; i < childrenUV.Length; i++) childrenUV[i] = new StandardUVMap(parentUV.GetTexture(), newSplits[i], originalBody.CollisionPolygon, parentUV);
            }
            else childrenUV = null;

            for (int i = 0; i < newBodies.Length; i++)
            {
                newBodies[i] = new RigidBody(newSplits[i], originalMaterial);

                //Apply proportional velocity
                newBodies[i].AddTranslationalVelocity(originalBody.TranslationalVelocity, isMomentum: false);

                //Apply tangential velocity
                newBodies[i].AddTranslationalVelocity(originalBody.GetLocalVelocity(newBodies[i].CollisionPolygon.CenterPoint) - originalBody.TranslationalVelocity, isMomentum: false);
            }

            return newBodies;
        }

        //Object
        public readonly Polygon CollisionPolygon;

        public readonly float Bounce;
        public readonly float Mass;
        public readonly float Inertia;
        public readonly float StaticFriction;
        public readonly float DynamicFriction;

        private float _angularVelocity;
        public float AngularVelocity => _angularVelocity;
        public float AngularMomentum => _angularVelocity * Mass;

        private Vector2 _translationalVelocity;
        public Vector2 TranslationalVelocity => new Vector2(_translationalVelocity.X, _translationalVelocity.Y);
        public Vector2 TranslationalMomentum => TranslationalVelocity * Mass;

        /// <summary>
        /// Add angular velocity to the body.
        /// </summary>
        /// <param name="forceValue">The value of the force.</param>
        /// <param name="isMomentum"></param>
        public void AddAngularVelocity(float forceValue, bool isMomentum = true)
        {
            if (Mass > 0)
            {
                if (float.IsNaN(forceValue)) forceValue = 0;

                if (isMomentum) _angularVelocity += forceValue / Mass;
                else _angularVelocity += forceValue;
            }
        }

        /// <summary>
        /// Set the angular velocity of the body.
        /// </summary>
        /// <param name="forceValue">The value of the force.</param>
        /// <param name="isMomentum"></param>
        public void SetAngularVelocity(float forceValue, bool isMomentum = true)
        {
            if (float.IsNaN(forceValue)) forceValue = 0;

            if (isMomentum) _angularVelocity = forceValue / Mass;
            else _angularVelocity = forceValue;
        }

        /// <summary>
        /// Add translational velocity to the body.
        /// </summary>
        /// <param name="forceValue">The value of the force.</param>
        /// <param name="isMomentum"></param>
        public void AddTranslationalVelocity(Vector2 forceValue, bool isMomentum = true)
        {
            if (Mass > 0)
            {
                if (float.IsNaN(forceValue.X)) forceValue.X = 0;
                if (float.IsNaN(forceValue.Y)) forceValue.Y = 0;

                if (isMomentum) _translationalVelocity += forceValue / Mass;
                else _translationalVelocity += forceValue;
            }
        }

        /// <summary>
        /// Set the translational velocity of the body.
        /// </summary>
        /// <param name="forceValue">The value of the force.</param>
        /// <param name="isMomentum"></param>
        public void SetTranslationalVelocity(Vector2 forceValue, bool isMomentum = true)
        {
            if (float.IsNaN(forceValue.X)) forceValue.X = 0;
            if (float.IsNaN(forceValue.Y)) forceValue.Y = 0;

            if (isMomentum) _translationalVelocity = forceValue / Mass;
            else _translationalVelocity = forceValue;
        }

        /// <summary>
        /// Get the velocity of this point on the polygon. A sum of the translational and tangential velocities at the point.
        /// </summary>
        /// <param name="pointOnPolygon">The point of the local velocity.</param>
        /// <returns>Returns the velocity at this point.</returns>
        public Vector2 GetLocalVelocity(Vector2 pointOnPolygon)
        {
            Vector2 radiusVector = pointOnPolygon - CollisionPolygon.CenterPoint;
            Vector2 tangentialVelocity = IntersectionEvent.CrossProduct(_angularVelocity, radiusVector);

            return tangentialVelocity + _translationalVelocity;
        }

        /// <summary>
        /// Applies a force at the given application point on the polygon. Will calculate torque.
        /// </summary>
        /// <param name="forceVector">The value of the force.</param>
        /// <param name="applicationPoint">The point on the polygon at which to apply the force.</param>
        public void ApplyForce(Vector2 forceVector, Vector2 applicationPoint)
        {
            AddTranslationalVelocity(forceVector * IntersectionEvent.GetLimitedRecip(Mass), isMomentum: false);

            Vector2 radiusVector = applicationPoint - CollisionPolygon.CenterPoint;
            radiusVector.Normalize();
            AddAngularVelocity(IntersectionEvent.GetLimitedRecip(Inertia) * IntersectionEvent.CrossProduct(radiusVector, forceVector), isMomentum: false);
        }

        private Material material;
        public Material Material => material;

        private int collisionLevel;
        public int CollisionLevel => collisionLevel;

        /// <summary>
        /// Create a new RigidBody with custom material parameters.
        /// </summary>
        /// <param name="collisionPolygon">The collision polygon of the body.</param>
        /// <param name="Bounce">The coefficient of restitution (bounciness).</param>
        /// <param name="Mass">The mass of the body.</param>
        /// <param name="StaticFriction">The friction of the body at rest.</param>
        /// <param name="DynamicFriction">The friction of the body while not at rest.</param>
        public RigidBody(Polygon collisionPolygon, float Bounce, float Mass, float StaticFriction, float DynamicFriction, int collisionLevel = 0)
        {
            this.StaticFriction = StaticFriction;
            this.CollisionPolygon = collisionPolygon;
            this.Bounce = Bounce;
            this.Mass = Mass;
            this.DynamicFriction = DynamicFriction;
            this.Inertia = GetInertia(Mass);
            this.collisionLevel = collisionLevel;

            //Set material
            material = new Material(Mass / collisionPolygon.GetArea(), Bounce, StaticFriction, DynamicFriction);
        }

        /// <summary>
        /// Create a new RigidBody with a material.
        /// </summary>
        /// <param name="collisionPolygon">The collision polygon of the body.</param>
        /// <param name="material">The material of the body.</param>
        public RigidBody(Polygon collisionPolygon, Material material, int collisionLevel = 0)
        {
            this.StaticFriction = material.StaticFriction;
            this.CollisionPolygon = collisionPolygon;
            this.Bounce = material.Bounce;
            this.Mass = material.Density * collisionPolygon.GetArea();
            this.DynamicFriction = material.DynamicFriction;
            this.Inertia = GetInertia(Mass);
            this.material = material;
            this.collisionLevel = collisionLevel;
        }

        private const float INERTIAL_DIVISOR = 25;
        private float GetInertia(float mass)
        {
            //return mass * 150;

            //Sum vertices
            Vector2[] vertices = CollisionPolygon.Vertices;
            float massDivision = mass / vertices.Length;
            float inertialSum = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                inertialSum += massDivision * (float)Math.Pow(LineSegment.Distance(vertices[i], CollisionPolygon.CenterPoint), 2);
            }

            return inertialSum / INERTIAL_DIVISOR;
        }

        /// <summary>
        /// Updates the polygon's position and rotation based on its translational and angular velocities.
        /// </summary>
        /// <param name="gameTime">The GameTime of the environment. Target is 60 FPS. Will assume perfect time if left null.</param>
        public void Update(GameTime gameTime)
        {
            //Get time portion
            float timePortion = 1;
            if (gameTime != null) timePortion = (float)gameTime.ElapsedGameTime.TotalMilliseconds / TARGET_FRAMETIME;

            //Apply velocities
            CollisionPolygon.Translate(_translationalVelocity);
            CollisionPolygon.Rotate(_angularVelocity, CollisionPolygon.CenterPoint);
        }
    }
}
