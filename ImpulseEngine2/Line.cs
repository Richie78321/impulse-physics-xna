using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public class Line
    {
        private float slope;
        public float Slope => slope;

        private float yIntercept;
        public float YIntercept => yIntercept;

        protected Vector2 containedPoint;
        public Vector2 ContainedPoint => new Vector2(containedPoint.X, containedPoint.Y);

        /// <summary>
        /// Creates a new line.
        /// </summary>
        /// <param name="containedPoint">A sample point of the line.</param>
        /// <param name="slope">The slope of the line.</param>
        public Line(Vector2 containedPoint, float slope)
        {
            this.slope = slope;
            yIntercept = containedPoint.Y - (slope * containedPoint.X);
            this.containedPoint = containedPoint;
        }

        /// <summary>
        /// Projects a point onto the line.
        /// </summary>
        /// <param name="pointToProject">The point to project.</param>
        /// <returns>Returns the projection of the point.</returns>
        public Vector2 PointProjection(Vector2 pointToProject)
        {
            //Create perpendicular projection line
            Line projectionLine = new Line(pointToProject, -1F / slope);

            //Get point
            Vector2 intersectionPoint = new Vector2(float.NaN, float.NaN);
            if (!IntersectsLine(projectionLine, ref intersectionPoint)) throw new Exception("Perpendicular lines did not intersect!");

            return intersectionPoint;
        }

        /// <summary>
        /// Checks for an intersection with another line.
        /// </summary>
        /// <param name="intersectionLine">The other line.</param>
        /// <param name="intersectionPoint">The potential point of intersection.</param>
        /// <returns>Returns if the lines are intersecting.</returns>
        public bool IntersectsLine(Line intersectionLine, ref Vector2 intersectionPoint)
        {
            //Ensure that the lines are not parallel
            if (Slope == intersectionLine.Slope)
            {
                if (containedPoint == intersectionLine.containedPoint)
                {
                    //Make intersection point NaN to represent same line
                    intersectionPoint = new Vector2(float.NaN, float.NaN);
                    return true;
                }

                return false;
            }
            else if (float.IsInfinity(Slope))
            {
                //Find point where the line meets certain x-value
                intersectionPoint = new Vector2(containedPoint.X, (intersectionLine.Slope * containedPoint.X) + intersectionLine.YIntercept);
                return true;
            }
            else if (float.IsInfinity(intersectionLine.Slope)) return intersectionLine.IntersectsLine(this, ref intersectionPoint);
            else
            {
                //Use standard intersection determination
                intersectionPoint.X = (intersectionLine.YIntercept - YIntercept) / (Slope - intersectionLine.Slope);
                intersectionPoint.Y = (Slope * intersectionPoint.X) + YIntercept;
                return true;
            }
        }

        public Vector2 NormalizedNormal
        {
            get
            {
                Vector2 lineNormal;
                if (float.IsInfinity(Slope)) lineNormal = new Vector2(1, 0);
                else
                {
                    //Find slope
                    float lineAngle = (float)Math.Atan2(-1F / Slope, 1);
                    lineNormal = new Vector2((float)Math.Cos(lineAngle), (float)Math.Sin(lineAngle));
                }

                return lineNormal;
            }
        }

        public Vector2 PointAtX(float value)
        {
            if (float.IsInfinity(Slope))
            {
                //No change
                return new Vector2(float.NaN, float.NaN);
            }
            else if (Slope == 0)
            {
                return new Vector2(value, containedPoint.Y);
            }
            else
            {
                return new Vector2(value, (Slope * value) + YIntercept);
            }
        }

        public Vector2 PointAtY(float value)
        {
            if (Slope == 0)
            {
                //No change
                return new Vector2(float.NaN, float.NaN);
            }
            else if (float.IsInfinity(Slope))
            {
                return new Vector2(containedPoint.X, value);
            }
            else
            {
                return new Vector2((value - YIntercept) / Slope, value);
            }
        }

        private const float SAMESIDE_LENIENCY = .05F;
        /// <summary>
        /// Determines if a point is on the same side of the line as another point.
        /// </summary>
        /// <param name="questionPoint">The point to determine.</param>
        /// <param name="samplePoint">The point to match.</param>
        /// <returns>Returns if the points are on the same side of the line.</returns>
        public bool SharesPointSide(Vector2 questionPoint, Vector2 samplePoint)
        {
            Vector2 otherPoint = PointAtX(containedPoint.X + 1);
            if (float.IsNaN(otherPoint.X)) otherPoint = PointAtY(containedPoint.Y + 1);

            float samplePointLoc = ((samplePoint.X - containedPoint.X) * (otherPoint.Y - containedPoint.Y)) - ((samplePoint.Y - containedPoint.Y) * (otherPoint.X - containedPoint.X));
            float questionPointLoc = ((questionPoint.X - containedPoint.X) * (otherPoint.Y - containedPoint.Y)) - ((questionPoint.Y - containedPoint.Y) * (otherPoint.X - containedPoint.X));

            if (samplePointLoc <= 0) return questionPointLoc - SAMESIDE_LENIENCY <= 0;
            else return questionPointLoc + SAMESIDE_LENIENCY >= 0;

            //Vector2 lineNormal = NormalizedNormal;
            //float lineLocation = IntersectionEvent.DotProduct(containedPoint, lineNormal);

            //float samplePointLocation = IntersectionEvent.DotProduct(samplePoint, lineNormal);
            //float questionPointLocation = IntersectionEvent.DotProduct(questionPoint, lineNormal);

            //if (samplePointLocation <= lineLocation) return questionPointLocation <= lineLocation + SAMESIDE_LENIENCY;
            //else return questionPointLocation >= lineLocation - SAMESIDE_LENIENCY;
        }

        public float GreatestSameSide(Vector2[] points, Vector2 samplePoint)
        {
            float greatestDepth = float.NaN;

            Vector2 lineNormal = NormalizedNormal;
            for (int i = 0; i < points.Length; i++)
            {
                if (SharesPointSide(points[i], samplePoint))
                {
                    float penetrationDepth = Math.Abs(IntersectionEvent.DotProduct(containedPoint, lineNormal) - IntersectionEvent.DotProduct(points[i], lineNormal));
                    if (float.IsNaN(greatestDepth) || penetrationDepth > greatestDepth) greatestDepth = penetrationDepth;
                }
            }

            return greatestDepth;
        }
    }
}
