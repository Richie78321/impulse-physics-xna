using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImpulseEngine2
{
    public class Triangle : Polygon
    {
        /// <summary>
        /// Creates a triangle.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="vertex3"></param>
        public Triangle(Vector2 vertex1, Vector2 vertex2, Vector2 vertex3) : base(new Vector2[] { vertex1, vertex2, vertex3 })
        {
        }

        public override Triangle[] GetTriangles()
        {
            return new Triangle[] { this };
        }

        public float GetBase()
        {
            return LineSegment.Distance(vertices[0], vertices[1]);
        }

        public float GetHeight()
        {
            Line normalLine = new LineSegment(vertices[0], vertices[1]).NormalLine;
            return LineSegment.Distance(normalLine.PointProjection(vertices[0]), normalLine.PointProjection(vertices[2]));
        }
    }
}
