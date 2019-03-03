using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ImpulseEngine2
{
    public struct RectangleF
    {
        public Vector2 Location => new Vector2(X, Y);
        public float X;
        public float Y;

        public Vector2 Dimensions => new Vector2(Width, Height);
        public float Width;
        public float Height;

        public RectangleF(Vector2 location, Vector2 dimensions) : this(location.X, location.Y, dimensions.X, dimensions.Y) { }

        public RectangleF(float X, float Y, float Width, float Height)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
        }

        public Rectangle RoundedRectangle => new Rectangle((int)Math.Floor(Location.X), (int)Math.Floor(Location.Y), (int)Math.Ceiling(Dimensions.X), (int)Math.Ceiling(Dimensions.Y));

        public RectangleF Merge(RectangleF otherRec)
        {
            float minX = Math.Min(otherRec.X, X), maxX = Math.Max(otherRec.X + otherRec.Width, X + Width), minY = Math.Min(otherRec.Y, Y), maxY = Math.Max(otherRec.Y + otherRec.Height, Y + Height);
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public Vector2 Center => new Vector2(X + (Width / 2), Y + (Height / 2));

        public bool ContainsPoint(Vector2 point)
        {
            return point.X >= X && point.Y >= Y && point.X <= X + Width && point.Y <= Y + Height;
        }

        public Vector2[] Vertices => new Vector2[] { new Vector2(X, Y), new Vector2(X + Width, Y), new Vector2(X + Width, Y + Height), new Vector2(X, Y + Height) };

        public bool IsIntersecting(RectangleF rectangle)
        {
            //Own vertices
            Vector2[] ownVertices = Vertices;
            for (int i = 0; i < ownVertices.Length; i++) if (rectangle.ContainsPoint(ownVertices[i])) return true;

            //Other vertices
            Vector2[] otherVertices = rectangle.Vertices;
            for (int i = 0; i < otherVertices.Length; i++) if (this.ContainsPoint(otherVertices[i])) return true;

            return false;
        }
    }
}
