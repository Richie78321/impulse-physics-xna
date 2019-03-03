using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImpulseEngine2.Drawing
{
    public class StandardUVMap : ITextureUV
    {
        private Vector2[] uvPoints;
        private Texture2D texture;

        public StandardUVMap(Texture2D texture, Polygon polygon) : this(texture, polygon, new Rectangle(0, 0, texture.Width, texture.Height))
        {
        }

        public StandardUVMap(Texture2D texture, Polygon polygon, Rectangle sourceRectangle)
        {
            this.texture = texture;

            //Get triangle vertices
                                            Triangle[] triangles = polygon.GetTriangles();
            Vector2[] polygonVertices = new Vector2[triangles.Length * 3];
            for (int i = 0; i < polygonVertices.Length; i++) polygonVertices[i] = triangles[i / 3].Vertices[i % 3];

            RectangleF boundaryRectangle = polygon.BoundaryRectangle;
            float polygonMajor = Math.Max(boundaryRectangle.Width, boundaryRectangle.Height);

            //Make vertices relative
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                polygonVertices[i] -= boundaryRectangle.Location;
                polygonVertices[i] /= polygonMajor;
            }

            //Map to minor axis of source
            float sourceMinor = Math.Min(sourceRectangle.Width, sourceRectangle.Height);
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                polygonVertices[i] *= sourceMinor;
                polygonVertices[i] += new Vector2(sourceRectangle.X, sourceRectangle.Y);
            }

            //Make relative to axes
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                polygonVertices[i].X /= texture.Width;
                polygonVertices[i].Y /= texture.Height;
            }

            uvPoints = polygonVertices;
        }

        public StandardUVMap(Texture2D texture, Polygon polygon, Polygon parentPolygon, ITextureUV parentUVMap)
        {
            this.texture = texture;

            //Rotate relative to parent
            Polygon childCopy = new Polygon(polygon.Vertices);
            childCopy.Rotate(-parentPolygon.TotalRotation % (float)(2 * Math.PI), parentPolygon.CenterPoint);

            //Get triangle vertices
            Triangle[] triangles = childCopy.GetTriangles();
            Vector2[] polygonVertices = new Vector2[triangles.Length * 3];
            for (int i = 0; i < polygonVertices.Length; i++) polygonVertices[i] = triangles[i / 3].Vertices[i % 3];

            RectangleF parentBoundary = parentPolygon.BoundaryRectangle;
            float polygonMajor = Math.Max(parentBoundary.Width, parentBoundary.Height);

            //Make vertices relative to parent
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                polygonVertices[i] -= parentBoundary.Location;
                polygonVertices[i] /= polygonMajor;
            }

            //Map to minor axis of source
            RectangleF uvBounds = parentUVMap.GetUVBounds();
            RectangleF sourceRectangle = new RectangleF(uvBounds.X * texture.Width, uvBounds.Y * texture.Height, uvBounds.Width * texture.Width, uvBounds.Height * texture.Height);
            float sourceMinor = Math.Min(sourceRectangle.Width, sourceRectangle.Height);
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                polygonVertices[i] *= sourceMinor;
                polygonVertices[i] += new Vector2(sourceRectangle.X, sourceRectangle.Y);
            }

            //Make relative to axes
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                polygonVertices[i].X /= texture.Width;
                polygonVertices[i].Y /= texture.Height;
            }

            uvPoints = polygonVertices;

            throw new NotImplementedException();
        }

        public Texture2D GetTexture()
        {
            return texture;
        }

        public RectangleF GetUVBounds()
        {
            return new Polygon(uvPoints).BoundaryRectangle;
        }

        public Vector2[] GetUVPoints()
        {
            return uvPoints;
        }
    }
}
