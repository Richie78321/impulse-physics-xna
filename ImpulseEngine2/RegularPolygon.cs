using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public class RegularPolygon : Polygon
    {
        private static Vector2[] GetVertices(Vector2 origin, float radius, int numberOfSides)
        {
            Vector2[] vertices = new Vector2[numberOfSides];
            float rotationPerVertex = 2 * (float)Math.PI / numberOfSides;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 vertex = new Vector2(origin.X, origin.Y - radius);

                //Rotate
                float xWithout = vertex.X - origin.X;
                float yWithout = vertex.Y - origin.Y;
                vertices[i].X = (float)((xWithout * Math.Cos(rotationPerVertex * i)) - (yWithout * Math.Sin(rotationPerVertex * i))) + origin.X;
                vertices[i].Y = (float)((xWithout * Math.Sin(rotationPerVertex * i)) + (yWithout * Math.Cos(rotationPerVertex * i))) + origin.Y;
            }

            return vertices;
        }

        //Object
        /// <summary>
        /// Creates a regular polygon.
        /// </summary>
        /// <param name="origin">The origin (center) of the polygon.</param>
        /// <param name="radius">The length from the origin (center) to the vertices of the polygon.</param>
        /// <param name="numberOfSides">The number of sides.</param>
        public RegularPolygon(Vector2 origin, float radius, int numberOfSides) : base(GetVertices(origin, radius, numberOfSides))
        {
            if (numberOfSides < 3) throw new Exception("Must have three or more sides!");
        }
    }
}
