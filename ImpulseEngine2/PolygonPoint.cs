using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public class PolygonPoint
    {
        private float initialRotation;
        private Polygon polygon;

        private float radius;
        private float angle;

        public PolygonPoint(Vector2 point, Polygon polygon)
        {
            initialRotation = polygon.TotalRotation;
            this.polygon = polygon;

            radius = LineSegment.Distance(point, polygon.CenterPoint);
            angle = (float)Math.Atan2(point.Y - polygon.CenterPoint.Y, point.X - polygon.CenterPoint.X);
        }

        public Vector2 GetPoint()
        {
            float relativeAngle = angle + (polygon.TotalRotation - initialRotation);
            return new Vector2(radius * (float)Math.Cos(relativeAngle), radius * (float)Math.Sin(relativeAngle)) + polygon.CenterPoint;
        }
    }
}
