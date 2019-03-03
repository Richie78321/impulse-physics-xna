using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public class RotationRectangle : Polygon
    {
        private static Vector2[] GetVertices(RectangleF originalRectangle)
        {
            Vector2[] vertices = new Vector2[4];
            vertices[0] = new Vector2(originalRectangle.X, originalRectangle.Y);
            vertices[1] = new Vector2(originalRectangle.X + originalRectangle.Width, originalRectangle.Y);
            vertices[2] = new Vector2(originalRectangle.X + originalRectangle.Width, originalRectangle.Y + originalRectangle.Height);
            vertices[3] = new Vector2(originalRectangle.X, originalRectangle.Y + originalRectangle.Height);

            return vertices;
        }

        //Object
        /// <summary>
        /// Creates a rectangle that is rotated by a specified radian amount.
        /// </summary>
        /// <param name="originalRectangle">The dimensions of the rectangle.</param>
        /// <param name="initialRotation">The amount of rotation along the centroid z-axis.</param>
        public RotationRectangle(RectangleF originalRectangle, float initialRotation = 0F) : base(GetVertices(originalRectangle))
        {
            //Rotate by initial rotation
            Rotate(initialRotation, centerPoint);
        }
    }
}
