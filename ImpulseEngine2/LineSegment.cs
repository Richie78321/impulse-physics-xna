using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public class LineSegment
    {
        public static float Distance(Vector2 vector1, Vector2 vector2)
        {
            return (float)Math.Sqrt(Math.Pow(vector1.X - vector2.X, 2) + Math.Pow(vector1.Y - vector2.Y, 2));
        }

        private static bool VectorEqualTo(Vector2 vector1, Vector2 vector2, float equalityLeniency = .001F)
        {
            return Math.Abs(vector1.X - vector2.X) <= equalityLeniency && Math.Abs(vector1.Y - vector2.Y) <= equalityLeniency;
        }

        //Object
        public readonly Vector2[] EndPoints;
        public readonly float Slope;
        public readonly float YIntercept;

        public Vector2 Midpoint => (EndPoints[0] + EndPoints[1]) / 2;

        public Line NormalLine => new Line(new Vector2((EndPoints[0].X + EndPoints[1].X) / 2, (EndPoints[0].Y + EndPoints[1].Y) / 2), -1F / Slope);

        public float Length => (float)Math.Sqrt(Math.Pow(EndPoints[0].X - EndPoints[1].X, 2) + Math.Pow(EndPoints[0].Y - EndPoints[1].Y, 2));

        public LineSegment(Vector2 endpoint1, Vector2 endpoint2)
        {
            EndPoints = new Vector2[] { endpoint1, endpoint2 };
            Slope = (endpoint2.Y - endpoint1.Y) / (endpoint2.X - endpoint1.X);
            YIntercept = endpoint1.Y - (Slope * endpoint1.X);
        }

        public LineSegment(Vector2 singlePoint, float slope)
        {
            EndPoints = new Vector2[] { singlePoint, singlePoint };
            Slope = slope;
            YIntercept = singlePoint.Y - (Slope * singlePoint.X);
        }

        public Line Line => new Line(EndPoints[0], Slope);

        /// <summary>
        /// Determines if a line intersects the segment.
        /// </summary>
        /// <param name="intersectionLine">The line to check.</param>
        /// <param name="intersectionPoint">The potential intersection point.</param>
        /// <returns>Returns if the lines intersect.</returns>
        public bool IntersectsLine(Line intersectionLine, ref Vector2 intersectionPoint)
        {
            if (Line.IntersectsLine(intersectionLine, ref intersectionPoint))
            {
                //Ensure on segment
                return ContainedOnSegment(intersectionPoint);
            }
            else return false;
        }

        /// <summary>
        /// Determines if a point is contained on the segment.
        /// </summary>
        /// <param name="pointContained">The point to check.</param>
        /// <returns>Returns if the point is contained on the segment.</returns>
        public bool ContainedOnSegment(Vector2 pointContained)
        {
            //Generate endpoint containment lines
            Line[] endpointLines = new Line[EndPoints.Length];
            for (int i = 0; i < endpointLines.Length; i++) endpointLines[i] = new Line(EndPoints[i], -(1F / Slope));

            //Generate midpoint
            Vector2 midpoint = Midpoint;

            //Ensure point is on same side as midpoint
            for (int i = 0; i < endpointLines.Length; i++) if (!endpointLines[i].SharesPointSide(pointContained, midpoint)) return false;

            return true;
        }

        public bool IntersectsSegment(LineSegment intersectionSegment, ref Vector2 intersectionPoint)
        {
            if (IntersectsLine(intersectionSegment.Line, ref intersectionPoint))
            {
                return intersectionSegment.ContainedOnSegment(intersectionPoint);
            }
            else return false;
        }

        /// <summary>
        /// Checks if the segments are overlapping.
        /// </summary>
        /// <param name="otherSegment">The other segment to check.</param>
        /// <param name="overlapSegment">If the segments are overlapping, this segment will be equal to the minimum magnitude segment including endpoints FROM BOTH SEGMENTS. The first endpoint is always from the parent line segment.</param>
        /// <returns></returns>
        public bool IsOverlapping(LineSegment otherSegment, out LineSegment overlapSegment)
        {
            //Find overlapping endpoint 
            List<LSVector> intersectionPoints = new List<LSVector>();
            for (int i = 0; i < EndPoints.Length; i++) if (otherSegment.ContainedOnSegment(EndPoints[i])) intersectionPoints.Add(new LSVector(this, EndPoints[i]));
            for (int i = 0; i < otherSegment.EndPoints.Length; i++) if (ContainedOnSegment(otherSegment.EndPoints[i])) intersectionPoints.Add(new LSVector(otherSegment, otherSegment.EndPoints[i]));

            if (intersectionPoints.Count > 0)
            {
                //Find extremes
                LSVector minPoint = intersectionPoints[0];
                LSVector maxPoint = intersectionPoints[0];
                if (!float.IsInfinity(Slope))
                {
                    for (int i = 1; i < intersectionPoints.Count; i++)
                    {
                        if (minPoint.Vector.X > intersectionPoints[i].Vector.X) minPoint = intersectionPoints[i];
                        if (maxPoint.Vector.X < intersectionPoints[i].Vector.X) maxPoint = intersectionPoints[i];
                    }
                }
                else
                {
                    //Vertical (use y)
                    for (int i = 1; i < intersectionPoints.Count; i++)
                    {
                        if (minPoint.Vector.Y > intersectionPoints[i].Vector.Y) minPoint = intersectionPoints[i];
                        if (maxPoint.Vector.Y < intersectionPoints[i].Vector.Y) maxPoint = intersectionPoints[i];
                    }
                }

                //Ensure maxima not same points
                if (minPoint.Vector != maxPoint.Vector)
                {
                    //Ensure maxima not of same parent line
                    if (minPoint.LineSegment == maxPoint.LineSegment)
                    {
                        //Choose one from other
                        LineSegment inclusionSegment;
                        if (minPoint.LineSegment == otherSegment) inclusionSegment = this;
                        else inclusionSegment = otherSegment;

                        Vector2 inclusionSegmentMin = inclusionSegment.GetMin();
                        Vector2 inclusionSegmentMax = inclusionSegment.GetMax();

                        float minPointDist = Distance(minPoint.Vector, inclusionSegmentMax);
                        float maxPointDist = Distance(maxPoint.Vector, inclusionSegmentMin);

                        if (minPointDist < maxPointDist)
                        {
                            if (inclusionSegment == this)
                            {
                                overlapSegment = new LineSegment(inclusionSegmentMax, minPoint.Vector);
                            }
                            else
                            {
                                overlapSegment = new LineSegment(minPoint.Vector, inclusionSegmentMax);
                            }
                        }
                        else
                        {
                            if (inclusionSegment == this)
                            {
                                overlapSegment = new LineSegment(inclusionSegmentMin, maxPoint.Vector);
                            }
                            else
                            {
                                overlapSegment = new LineSegment(maxPoint.Vector, inclusionSegmentMin);
                            }
                        }
                    }
                    else
                    {
                        if (minPoint.LineSegment == this)
                        {
                            overlapSegment = new LineSegment(minPoint.Vector, maxPoint.Vector);
                        }
                        else
                        {
                            overlapSegment = new LineSegment(maxPoint.Vector, minPoint.Vector);
                        }
                    }

                    return true;
                }
            }

            overlapSegment = null;
            return false;
        }

        public bool EqualTo(LineSegment otherSegment, float equalityLeniency)
        {
            return (VectorEqualTo(otherSegment.EndPoints[0], EndPoints[0]) && VectorEqualTo(otherSegment.EndPoints[1], EndPoints[1])) || (VectorEqualTo(otherSegment.EndPoints[0], EndPoints[1]) && VectorEqualTo(otherSegment.EndPoints[1], EndPoints[0]));
        }

        private Vector2 GetMin()
        {
            if (float.IsInfinity(Slope))
            {
                //Vertical, use y
                if (EndPoints[0].Y < EndPoints[1].Y) return EndPoints[0];
                else return EndPoints[1];
            }
            else
            {
                if (EndPoints[0].X < EndPoints[1].X) return EndPoints[0];
                else return EndPoints[1];
            }
        }

        private Vector2 GetMax()
        {
            if (float.IsInfinity(Slope))
            {
                //Vertical, use y
                if (EndPoints[0].Y > EndPoints[1].Y) return EndPoints[0];
                else return EndPoints[1];
            }
            else
            {
                if (EndPoints[0].X > EndPoints[1].X) return EndPoints[0];
                else return EndPoints[1];
            }
        }
    }

    public struct LSVector
    {
        public readonly LineSegment LineSegment;
        public readonly Vector2 Vector;

        public LSVector(LineSegment lineSegment, Vector2 vector)
        {
            this.LineSegment = lineSegment;
            this.Vector = vector;
        }
    }
}
