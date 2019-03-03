using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public class IntersectionEvent
    {
        public static Vector2 CrossProduct(Vector2 vector, float scalar)
        {
            return new Vector2(scalar * vector.Y, -scalar * vector.X);
        }

        public static Vector2 CrossProduct(float scalar, Vector2 vector)
        {
            return new Vector2(-scalar * vector.Y, scalar * vector.X);
        }

        public static float CrossProduct(Vector2 vector1, Vector2 vector2)
        {
            return (vector1.X * vector2.Y) - (vector1.Y * vector2.X);
        }

        public static float DotProduct(Vector2 vector1, Vector2 vector2)
        {
            return (vector1.X * vector2.X) + (vector1.Y * vector2.Y);
        }

        public static float GetLimitedRecip(float value)
        {
            float recip = 1F / value;
            if (float.IsInfinity(recip)) recip = 0;
            return recip;
        }

        public static Vector2 GetPerpendicular(Vector2 vector)
        {
            return new Vector2(-vector.Y, vector.X);
        }

        public const int SETTLE_RESOLUTION = 5;

        //Object
        private enum Collider
        {
            Receiver,
            Sender
        }

        private RigidBody[] rigidBodies;
        public Vector2[] intersectionPoints;

        public IntersectionData intersectionData;
        public LineSegment collisionAxis;
        private Vector2 collisionNormal;
        private Vector2 relativeVelocity;

        /// <summary>
        /// Creates a new intersection event.
        /// </summary>
        /// <param name="rigidBody1">The first body involved (arbitrary).</param>
        /// <param name="rigidBody2">The second body involved (arbitrary).</param>
        /// <param name="collisionAxis">The axis of the collision (traditionally the axis of minimum intersection between the bodies).</param>
        /// <param name="intersectionRecipient">The recipient of the intersection (the body with a valid penetrated side segment).</param>
        public IntersectionEvent(RigidBody sender, RigidBody reciever, IntersectionData intersectionData)
        {
            this.collisionAxis = intersectionData.IntersectionAxis;
            this.intersectionData = intersectionData;

            float collisionNormalAngle = (float)Math.Atan2(collisionAxis.EndPoints[1].Y - collisionAxis.EndPoints[0].Y, collisionAxis.EndPoints[1].X - collisionAxis.EndPoints[0].X);
            collisionNormal = new Vector2((float)Math.Cos(collisionNormalAngle), (float)Math.Sin(collisionNormalAngle));

            //Set order of bodies
            rigidBodies = new RigidBody[2];
            rigidBodies[(int)Collider.Sender] = sender;
            rigidBodies[(int)Collider.Receiver] = reciever;
        }

        private const float EQUALITY_LENIENCY = .05F;
        private bool broken = false;
        //private void SetIntersectionPoints()
        //{
        //    intersectionPoints = new Vector2[2];

        //    Vector2[] senderVertices = rigidBodies[(int)Collider.Sender].CollisionPolygon.Vertices;
        //    float midPointProjection = DotProduct((intersectionData.PenetratedSideSegment.EndPoints[0] + intersectionData.PenetratedSideSegment.EndPoints[1]) / 2, collisionNormal);

        //    //Find sender intersection point
        //    List<Vector2> maxDistVertices = new List<Vector2>();
        //    float maxDist = 0;
        //    for (int i = 0; i < senderVertices.Length; i++)
        //    {
        //        //Ensure contained on correct side of line
        //        if (rigidBodies[(int)Collider.Receiver].CollisionPolygon.ContainsPoint(senderVertices[i]))
        //        {
        //            //Find projection magnitude
        //            float projectionMagnitude = DotProduct(senderVertices[i], collisionNormal);
        //            float dist = Math.Abs(projectionMagnitude - midPointProjection);
        //            if (dist > maxDist)
        //            {
        //                maxDist = dist;
        //                maxDistVertices.Clear();
        //                maxDistVertices.Add(senderVertices[i]);
        //            }
        //            else if (Math.Abs(dist - maxDist) <= EQUALITY_LENIENCY)
        //            {
        //                maxDistVertices.Add(senderVertices[i]);
        //            }
        //        }
        //    }

        //    Vector2 averagePoint = Vector2.Zero;
        //    for (int i = 0; i < maxDistVertices.Count; i++) averagePoint += maxDistVertices[i];
        //    averagePoint /= maxDistVertices.Count;

        //    intersectionPoints[(int)Collider.Sender] = averagePoint;

        //    Vector2 sideIntersection = new Vector2();
        //    if (!intersectionData.PenetratedSideSegment.Line.IntersectsLine(new Line(intersectionPoints[(int)Collider.Sender], collisionAxis.Slope), ref sideIntersection)) throw new Exception("Error while finding support points!");

        //    intersectionPoints[(int)Collider.Receiver] = sideIntersection;
        //}
        private void SetIntersectionPoints()
        {
            Vector2[] senderVertices = rigidBodies[(int)Collider.Sender].CollisionPolygon.Vertices;
            Line intersectionAxis = intersectionData.IntersectionAxis.Line;

            Vector2 senderVertex = new Vector2();
            float maxPenetrationDepth = float.NaN;
            for (int i = 0; i < senderVertices.Length; i++)
            {
                //Ensure penetrating
                if (intersectionData.PenetratedSideSegment.Line.SharesPointSide(senderVertices[i], rigidBodies[(int)Collider.Receiver].CollisionPolygon.CenterPoint))
                {
                    //Get penetration depth
                    float penetrationDepth = (intersectionAxis.PointProjection(senderVertices[i]) - intersectionAxis.PointProjection(intersectionData.PenetratedSideSegment.EndPoints[0])).Length();
                    if (float.IsNaN(maxPenetrationDepth) || maxPenetrationDepth < penetrationDepth)
                    {
                        senderVertex = senderVertices[i];
                        maxPenetrationDepth = penetrationDepth;
                    }
                }
            }

            if (float.IsNaN(maxPenetrationDepth)) throw new Exception("Could not find any sender vertices that penetrated the recipient polygon.");

            //Get recipient point
            Vector2 recipientPoint = new Vector2();
            if (!intersectionData.PenetratedSideSegment.IntersectsLine(new Line(senderVertex, intersectionData.IntersectionAxis.Slope), ref recipientPoint))
            {
                broken = true;
            }

            intersectionPoints = new Vector2[2];
            intersectionPoints[(int)Collider.Sender] = senderVertex;
            intersectionPoints[(int)Collider.Receiver] = recipientPoint;
        }

        /// <summary>
        /// Settles the collision by applying an impulse to both bodies.
        /// </summary>
        public void SettleCollision()
        {
            if (!broken)
            {
                for (int i = 0; i < SETTLE_RESOLUTION; i++)
                {
                    //Set intersection points
                    SetIntersectionPoints();

                    //Get local velocity
                    relativeVelocity = rigidBodies[(int)Collider.Sender].GetLocalVelocity(intersectionPoints[(int)Collider.Sender]) - rigidBodies[(int)Collider.Receiver].GetLocalVelocity(intersectionPoints[(int)Collider.Receiver]);

                    //Settle
                    if ((rigidBodies[0].CollisionLevel != rigidBodies[1].CollisionLevel || rigidBodies[0].CollisionLevel == 0) && DotProduct(relativeVelocity, collisionNormal) > 0)
                    {
                        float impulseMagnitude = GetImpulseMagnitude(collisionNormal);

                        //Apply force
                        rigidBodies[(int)Collider.Receiver].ApplyForce(-impulseMagnitude * collisionNormal, intersectionPoints[(int)Collider.Receiver]);
                        rigidBodies[(int)Collider.Sender].ApplyForce(impulseMagnitude * collisionNormal, intersectionPoints[(int)Collider.Sender]);

                        ApplyFriction(impulseMagnitude);
                        ApplyPositionalCorrection();
                    }
                }
            }
        }

        private float GetImpulseMagnitude(Vector2 normalOfIntersection)
        {
            Vector2 senderRadius = intersectionPoints[(int)Collider.Sender] - rigidBodies[(int)Collider.Sender].CollisionPolygon.CenterPoint;
            Vector2 receiverRadius = intersectionPoints[(int)Collider.Receiver] - rigidBodies[(int)Collider.Receiver].CollisionPolygon.CenterPoint;
            senderRadius.Normalize();
            receiverRadius.Normalize();

            float COR = Math.Min(rigidBodies[0].Bounce, rigidBodies[1].Bounce);

            float numerator = -(1 + COR) * DotProduct(relativeVelocity, normalOfIntersection);
            float denominator = GetLimitedRecip(rigidBodies[0].Mass) + GetLimitedRecip(rigidBodies[1].Mass);
            float intertialDenominator = ((float)Math.Pow(CrossProduct(senderRadius, normalOfIntersection), 2) * GetLimitedRecip(rigidBodies[(int)Collider.Sender].Inertia)) + ((float)Math.Pow(CrossProduct(receiverRadius, normalOfIntersection), 2) * GetLimitedRecip(rigidBodies[(int)Collider.Receiver].Inertia));

            return numerator / (denominator + intertialDenominator);
        }

        private void ApplyFriction(float impulseMagnitude)
        {
            Vector2 collisionTangent = relativeVelocity - (DotProduct(relativeVelocity, collisionNormal) * collisionNormal);
            collisionTangent.Normalize();
            float tangentImpulse = GetImpulseMagnitude(collisionTangent);

            //Employ coulomb friction (static)
            float staticCoefficient = (float)Math.Sqrt(Math.Pow(rigidBodies[(int)Collider.Sender].StaticFriction, 2) + Math.Pow(rigidBodies[(int)Collider.Receiver].StaticFriction, 2));
            if (Math.Abs(tangentImpulse) > Math.Abs(impulseMagnitude * staticCoefficient))
            {
                //Employ dynamic friction
                float dynamicCoefficient = (float)Math.Sqrt(Math.Pow(rigidBodies[(int)Collider.Sender].DynamicFriction, 2) + Math.Pow(rigidBodies[(int)Collider.Receiver].DynamicFriction, 2));
                tangentImpulse = impulseMagnitude * dynamicCoefficient;
            }

            rigidBodies[(int)Collider.Sender].ApplyForce(tangentImpulse * collisionTangent, intersectionPoints[(int)Collider.Sender]);
            rigidBodies[(int)Collider.Receiver].ApplyForce(-tangentImpulse * collisionTangent, intersectionPoints[(int)Collider.Receiver]);
        }

        private const float POSITIONAL_CORRECTION = .5F;
        private const float POSITIONAL_SLOP = .02F;
        private void ApplyPositionalCorrection()
        {
            if (collisionAxis.Length >= POSITIONAL_SLOP)
            {
                float correctionMagnitude = (Math.Max(collisionAxis.Length - POSITIONAL_SLOP, 0) / (GetLimitedRecip(rigidBodies[(int)Collider.Receiver].Mass) + GetLimitedRecip(rigidBodies[(int)Collider.Sender].Mass))) * POSITIONAL_CORRECTION;
                rigidBodies[(int)Collider.Receiver].CollisionPolygon.Translate(collisionNormal * correctionMagnitude * GetLimitedRecip(rigidBodies[(int)Collider.Receiver].Mass));
                rigidBodies[(int)Collider.Sender].CollisionPolygon.Translate(-collisionNormal * correctionMagnitude * GetLimitedRecip(rigidBodies[(int)Collider.Sender].Mass));
            }
        }
    }
}
