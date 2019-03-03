using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImpulseEngine2.Drawing;

namespace ImpulseEngine2
{
    public class Polygon
    {
        public static float Distance(Vector2 vector1, Vector2 vector2)
        {
            return (float)Math.Sqrt(Math.Pow(vector1.X - vector2.X, 2) + Math.Pow(vector1.Y - vector2.Y, 2));
        }

        public static Polygon[] SplitPolygon(Polygon polygon, Line splitLine)
        {
            //Ensure proper intersection
            if (polygon.IsLineIntersecting(splitLine) && !polygon.IsLineTangent(splitLine))
            {
                //Gather intersections
                LineSegment[] sideSegments = polygon.SideSegments;
                List<int> intersectedSegments = new List<int>();
                List<Vector2> intersectionPoints = new List<Vector2>();
                for (int i = 0; i < sideSegments.Length; i++)
                {
                    Vector2 intersectionPoint = new Vector2(float.NaN, float.NaN);
                    if (sideSegments[i].IntersectsLine(splitLine, ref intersectionPoint) && intersectionPoints.Where((b) => { return Math.Abs(b.X - intersectionPoint.X) <= EQUALITY_LENIENCY && Math.Abs(b.Y - intersectionPoint.Y) <= EQUALITY_LENIENCY; }).Count() == 0)
                    {
                        intersectedSegments.Add(i);
                        intersectionPoints.Add(intersectionPoint);
                    }
                }

                //Ensure no intersection anomalies
                if (intersectedSegments.Count != 2) return new Polygon[] { polygon };

                RectangleF originalBoundary = polygon.BoundaryRectangle;

                //Split
                Polygon[] polygons = new Polygon[2];
                int segmentIndex = intersectedSegments[0];
                int otherSegmentIndex = intersectedSegments[1];
                for (int j = 0; j < sideSegments[segmentIndex].EndPoints.Length; j++)
                {
                    //Find bound indecies
                    int sharedPointOffset = 0;
                    if (splitLine.SharesPointSide(sideSegments[otherSegmentIndex].EndPoints[1], sideSegments[segmentIndex].EndPoints[j])) sharedPointOffset = 1;
                    int vertexEndIndex = (otherSegmentIndex + sharedPointOffset) % polygon.vertices.Length;
                    int vertexStartIndex = (segmentIndex + j) % polygon.vertices.Length;
                    int vertexIndexOperator = (j * 2) - 1;

                    List<Vector2> vertices = new List<Vector2>();
                    vertices.Add(intersectionPoints[0]);
                    bool continueLoop = true;
                    while (continueLoop)
                    {
                        vertices.Add(polygon.vertices[vertexStartIndex]);
                        continueLoop = vertexStartIndex != vertexEndIndex;

                        vertexStartIndex += vertexIndexOperator;

                        //Clamp
                        if (vertexStartIndex < 0) vertexStartIndex = polygon.vertices.Length - 1;
                        else if (vertexStartIndex >= polygon.vertices.Length) vertexStartIndex = 0;
                    }
                    vertices.Add(intersectionPoints[1]);

                    //TODO : Add UV texture carry
                    polygons[j] = new Polygon(vertices.ToArray());
                }

                return polygons;
            }
            else return new Polygon[] { polygon };
        }

        public static Polygon[] FracturePolygon(Polygon polygon, int fractureDensity, Random random)
        {
            RectangleF boundaryRectangle = polygon.BoundaryRectangle;
            float widthProbability = boundaryRectangle.Width / (boundaryRectangle.Width + boundaryRectangle.Height);
            List<Polygon> fracturedPolygons = new List<Polygon>();
            fracturedPolygons.Add(polygon);

            //Apply fractures
            for (int i = 0; i < fractureDensity; i++)
            {
                Line fractureLine;
                if (random.NextDouble() > widthProbability)
                {
                    //Height
                    fractureLine = new LineSegment(new Vector2(boundaryRectangle.X, boundaryRectangle.Y + (float)(random.NextDouble() * boundaryRectangle.Height)), new Vector2(boundaryRectangle.X + boundaryRectangle.Width, boundaryRectangle.Y + (float)(random.NextDouble() * boundaryRectangle.Height))).Line;
                }
                else
                {
                    //Width
                    fractureLine = new LineSegment(new Vector2(boundaryRectangle.X + (float)(random.NextDouble() * boundaryRectangle.Width), boundaryRectangle.Y), new Vector2(boundaryRectangle.X + (float)(random.NextDouble() * boundaryRectangle.Width), boundaryRectangle.Y + boundaryRectangle.Height)).Line;
                }

                //Split on line
                Polygon[] currentPolygons = fracturedPolygons.ToArray();
                fracturedPolygons.Clear();
                for (int j = 0; j < currentPolygons.Length; j++)
                {
                    fracturedPolygons.AddRange(Polygon.SplitPolygon(currentPolygons[j], fractureLine));
                }
            }

            return fracturedPolygons.ToArray();
        }

        //Object
        protected Vector2[] vertices;
        protected Vector2 centerPoint;
        public Vector2[] Vertices => vertices;
        public Vector2 CenterPoint => centerPoint;

        public LineSegment[] SideSegments
        {
            get
            {
                LineSegment[] sides = new LineSegment[vertices.Length];
                for (int i = 0; i < vertices.Length; i++) sides[i] = new LineSegment(vertices[i], vertices[(i + 1) % vertices.Length]);
                return sides;
            }
        }

        private float maximumRadius;
        public float MaximumRadius => maximumRadius;

        /// <summary>
        /// Creates a polygon with the specified vertices. Must be a valid concave polygon.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        public Polygon(Vector2[] vertices)
        {
            this.vertices = vertices;

            if (vertices.Length < 3) throw new Exception("Not a valid polygon!");

            //Ensure not concave
            //Vector2 concavity;
            //if (IsConcave(out concavity)) throw new Exception("Concave polygons are not supported! (" + concavity + ")");

            //Find center point
            centerPoint = new Vector2(0, 0);
            for (int i = 0; i < vertices.Length; i++)
            {
                centerPoint.X += vertices[i].X;
                centerPoint.Y += vertices[i].Y;
            }
            centerPoint.X /= vertices.Length;
            centerPoint.Y /= vertices.Length;

            //Get max radius
            SetMaxRadius();
        }

        private void SetMaxRadius()
        {
            float maxDist = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                float potentialDist = Distance(vertices[i], CenterPoint);
                if (potentialDist > maxDist) maxDist = potentialDist;
            }

            maximumRadius = maxDist;
        }

        private const float EQUALITY_LENIENCY = .0001F;
        /// <summary>
        /// Determines if a point is contained within or on the polygon.
        /// </summary>
        /// <param name="point">The point to be checked.</param>
        /// <returns></returns>
        public bool ContainsPoint(Vector2 point)
        {
            LineSegment[] sideSegments = SideSegments;
            for (int i = 0; i < sideSegments.Length; i++) if (!sideSegments[i].Line.SharesPointSide(point, CenterPoint))
                    return false;
            return true;
        }

        /// <summary>
        /// Determines if any of a set of points are contained within or on the polygon.
        /// </summary>
        /// <param name="points">The set of points to be checked.</param>
        /// <returns></returns>
        public bool ContainsPoint(Vector2[] points)
        {
            for (int i = 0; i < points.Length; i++) if (ContainsPoint(points[i])) return true;
            return false;
        }

        private bool IsConcave(out Vector2 concavity)
        {
            for (int i = 1; i <= vertices.Length; i++)
            {
                //Get midpoint
                Vector2 midPoint = (vertices[i - 1] + vertices[(i + 1) % vertices.Length]) / 2;
                if (!ContainsPoint(midPoint))
                {
                    concavity = vertices[i % vertices.Length];
                    return true;
                }
            }

            concavity = new Vector2(float.NaN, float.NaN);
            return false;
        }

        /// <summary>
        /// Projects the polygon onto a specified axis.
        /// </summary>
        /// <param name="axis">The axis to project onto.</param>
        /// <returns>Returns the projection segment on the axis.</returns>
        public LineSegment PolygonProjection(Line axis)
        {
            Vector2 minMagnitude = new Vector2(float.NaN, float.NaN);
            Vector2 maxMagnitude = new Vector2(float.NaN, float.NaN);

            //Find extreme projections
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 vertexProjection = axis.PointProjection(vertices[i]);
                if (axis.Slope != 0)
                {
                    if (float.IsNaN(minMagnitude.Y) || vertexProjection.Y < minMagnitude.Y)
                    {
                        minMagnitude = vertexProjection;
                    }
                    if (float.IsNaN(maxMagnitude.Y) || vertexProjection.Y > maxMagnitude.Y)
                    {
                        maxMagnitude = vertexProjection;
                    }
                }
                else
                {
                    //No change in y, use x
                    if (float.IsNaN(minMagnitude.X) || vertexProjection.X < minMagnitude.X)
                    {
                        minMagnitude = vertexProjection;
                    }
                    if (float.IsNaN(maxMagnitude.X) || vertexProjection.X > maxMagnitude.X)
                    {
                        maxMagnitude = vertexProjection;
                    }
                }
            }

            //Create segment
            return new LineSegment(minMagnitude, maxMagnitude);
        }

        private float totalRotation = 0;
        public float TotalRotation => totalRotation;
        /// <summary>
        /// Rotates the polygon by a specified radian amount.
        /// </summary>
        /// <param name="rotation">The amount to rotate in radians.</param>
        /// <param name="axis">The z-axis on which to rotate.</param>
        public void Rotate(float rotation, Vector2 axis)
        {
            //Rotate vertices along axis
            for (int i = 0; i < vertices.Length; i++)
            {
                float xWithout = vertices[i].X - axis.X;
                float yWithout = vertices[i].Y - axis.Y;

                vertices[i].X = (float)((xWithout * Math.Cos(rotation)) - (yWithout * Math.Sin(rotation))) + axis.X;
                vertices[i].Y = (float)((xWithout * Math.Sin(rotation)) + (yWithout * Math.Cos(rotation))) + axis.Y;
            }

            //Rotate center point along axis
            float centerXWithout = centerPoint.X - axis.X;
            float centerYWithout = centerPoint.Y - axis.Y;
            centerPoint.X = (float)((centerXWithout * Math.Cos(rotation)) - (centerYWithout * Math.Sin(rotation))) + axis.X;
            centerPoint.Y = (float)((centerXWithout * Math.Sin(rotation)) + (centerYWithout * Math.Cos(rotation))) + axis.Y;

            totalRotation += rotation;
        }

        /// <summary>
        /// Translates the polygon by a specified amount.
        /// </summary>
        /// <param name="translation">The amount of translation.</param>
        public void Translate(Vector2 translation)
        {
            //Translate vertices
            for (int i = 0; i < vertices.Length; i++) vertices[i] += translation;

            //Translate center point
            centerPoint += translation;
        }

        public void TranslateTo(Vector2 translationTarget)
        {
            Translate(translationTarget - CenterPoint);
        }

        /// <summary>
        /// Scales the polygon by a specified ratio (no scale is 1).
        /// </summary>
        /// <param name="scale">The new scale</param>
        public void Scale(Vector2 scale)
        {
            //Scale vertices
            for (int i = 0; i < vertices.Length; i++) vertices[i] *= scale;

            //Scale center point
            centerPoint *= scale;
        }

        /// <summary>
        /// A rectangle representing the footprint of the polygon.
        /// </summary>
        public RectangleF BoundaryRectangle
        {
            get
            {
                //Find majors
                float width = PolygonProjection(new Line(centerPoint, 0)).Length;
                float height = PolygonProjection(new Line(centerPoint, float.PositiveInfinity)).Length;
                return new RectangleF(centerPoint.X - (width / 2), centerPoint.Y - (height / 2), width, height);
            }
        }

        /// <summary>
        /// Gets the area of the polygon.
        /// </summary>
        /// <returns></returns>
        public float GetArea()
        {
            Vector2[] vertices = Vertices;

            //Sum vertex multiplication
            float multSum = 0;
            for (int i = 0; i < vertices.Length; i++) multSum += vertices[i].X * vertices[(i + 1) % vertices.Length].Y;
            for (int i = 0; i < vertices.Length; i++) multSum -= vertices[i].Y * vertices[(i + 1) % vertices.Length].X;

            return Math.Abs(multSum / 2);
        }

        /// <summary>
        /// Splits the polygon into triangles.
        /// </summary>
        /// <returns>Returns the set of triangles.</returns>
        public virtual Triangle[] GetTriangles()
        {
            List<Vector2> vertices = new List<Vector2>();
            vertices.AddRange(Vertices);
            List<Triangle> triangles = new List<Triangle>();

            while (vertices.Count > 2)
            {
                triangles.Add(new Triangle(vertices[0], vertices[1], vertices[2]));
                vertices.Remove(vertices[1]);
            }

            return triangles.ToArray();
        }

        private const float INTERSECT_EQUALITY_LENIENCY = .001F;
        //Least greatest penetration depth of sides with equal collision axes (avoids intersection thing)
        /// <summary>
        /// Checks if intersecting other convex polygon using the Separating Axis Theorem.
        /// </summary>
        /// <param name="otherPolygon">The other polygon to check for intersection with.</param>
        /// <param name="intersectionData">The intersection data giving the axis of minimum penetration and the recipient of the penetration.</param>
        /// <returns>Returns value indicating whether or not there is an intersection.</returns>
        public bool IntersectingPolygon(Polygon otherPolygon, out IntersectionData intersectionData)
        {
            Polygon[] polygons = new Polygon[] { this, otherPolygon };
            intersectionData = new IntersectionData(false, null, null, false);
            LineSegment[][] sideSegments = new LineSegment[2][];
            sideSegments[0] = SideSegments;
            sideSegments[1] = otherPolygon.SideSegments;

            for (int i = 0; i < sideSegments.Length; i++)
            {
                for (int j = 0; j < sideSegments[i].Length; j++)
                {
                    //Check for overlap on normal projection
                    Line sideSegmentNormal = sideSegments[i][j].NormalLine;
                    LineSegment projectionOverlap;
                    if ((i == 0 && PolygonProjection(sideSegmentNormal).IsOverlapping(otherPolygon.PolygonProjection(sideSegmentNormal), out projectionOverlap)) || (i != 0 && otherPolygon.PolygonProjection(sideSegmentNormal).IsOverlapping(PolygonProjection(sideSegmentNormal), out projectionOverlap)))
                    {
                        if ((intersectionData.IntersectionAxis == null || intersectionData.IntersectionAxis.Length > projectionOverlap.Length))
                        {
                            Vector2 testPoint = new Vector2();
                            intersectionData = new IntersectionData(i == 0, projectionOverlap, sideSegments[i][j], sideSegments[i][j].IntersectsSegment(projectionOverlap, ref testPoint));
                        }
                    }
                    else return false;
                }
            }

            return true;
        }

        public bool IsLineTangent(Line testLine)
        {
            Line testLineNormal = new Line(testLine.ContainedPoint, -1F / testLine.Slope);

            LineSegment projection = PolygonProjection(testLineNormal);
            Vector2 containedPointProjection = testLineNormal.PointProjection(testLine.ContainedPoint);

            bool overlap = false;
            for (int i = 0; i < projection.EndPoints.Length; i++)
            {
                if (Math.Abs(containedPointProjection.X - projection.EndPoints[0].X) <= EQUALITY_LENIENCY && Math.Abs(containedPointProjection.Y - projection.EndPoints[0].Y) <= EQUALITY_LENIENCY)
                {
                    overlap = true;
                    break;
                }
            }

            return overlap;
        }

        public bool IsLineIntersecting(Line testLine)
        {
            Line testLineNormal = new Line(testLine.ContainedPoint, -1F / testLine.Slope);

            LineSegment projection = PolygonProjection(testLineNormal);
            Vector2 containedPointProjection = testLineNormal.PointProjection(testLine.ContainedPoint);

            return projection.ContainedOnSegment(containedPointProjection);
        }

        public void DrawPolygon(SpriteBatch spriteBatch, Texture2D lineTexture, float lineWeight = 1F)
        {
            LineSegment[] sideSegments = SideSegments;
            foreach (LineSegment b in sideSegments)
            {
                DrawSegment(spriteBatch, b, lineTexture, lineWeight);
            }
        }

        private void DrawSegment(SpriteBatch spriteBatch, LineSegment lineSegment, Texture2D lineTexture, float lineWeight = 1F)
        {
            float lineAngle = (float)Math.Atan2(lineSegment.EndPoints[1].Y - lineSegment.EndPoints[0].Y, lineSegment.EndPoints[1].X - lineSegment.EndPoints[0].X);
            spriteBatch.Draw(lineTexture, lineSegment.Midpoint, null, Color.White, lineAngle + ((float)Math.PI / 2), new Vector2(lineTexture.Width / 2, lineTexture.Height / 2), new Vector2(lineWeight / lineTexture.Width, lineSegment.Length / lineTexture.Height), SpriteEffects.None, 0);
        }

        public void FillPolygon(GraphicsDevice graphicsDevice, ITextureUV uvPoints, Vector2 translation, Vector2 scale)
        {
            PolygonFillEffect fillEffect = new PolygonFillEffect(graphicsDevice, uvPoints.GetTexture());

            //Split into triangles
            Triangle[] triangles = GetTriangles();

            //Scale and translate
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i].Translate(translation);
                triangles[i].Scale(scale);
            }

            //Get UV map points and ensure validity
            Vector2[] uvMapPoints = uvPoints.GetUVPoints();
            if (uvMapPoints.Length != triangles.Length * 3) throw new Exception("Invalid UV points. They are not of the proper length.");

            //Apply triangles
            VertexPositionTexture[] vertexTexturePositions = new VertexPositionTexture[triangles.Length * 3];
            int[] indexData = new int[vertexTexturePositions.Length];
            for (int i = 0; i < vertexTexturePositions.Length; i++)
            {
                vertexTexturePositions[i].Position =  new Vector3(triangles[i / 3].Vertices[i % 3], 0);
                vertexTexturePositions[i].TextureCoordinate = uvMapPoints[i];
                indexData[i] = i;
            }

            foreach (EffectPass pass in fillEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertexTexturePositions, 0, vertexTexturePositions.Length, indexData, 0, triangles.Length);
            }

            fillEffect.Dispose();
        }
    }

    public struct IntersectionData
    {
        public readonly bool RecipientIsFunctionCaller;
        public readonly LineSegment IntersectionAxis;
        public readonly LineSegment PenetratedSideSegment;
        public readonly bool ValidOverlap;

        public IntersectionData(bool RecipientIsFunctionCaller, LineSegment IntersectionAxis, LineSegment PenetratedSideSegment, bool ValidOverlap)
        {
            this.RecipientIsFunctionCaller = RecipientIsFunctionCaller;
            this.IntersectionAxis = IntersectionAxis;
            this.PenetratedSideSegment = PenetratedSideSegment;
            this.ValidOverlap = ValidOverlap;
        }
    }
}
