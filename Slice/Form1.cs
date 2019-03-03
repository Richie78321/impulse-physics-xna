using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImpulseEngine2;
using ImpulseEngine2.Materials;
using Microsoft.Xna.Framework;

namespace Slice
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void paint(object sender, PaintEventArgs e)
        {
            RigidBody[] bodies = handler.GetBodies();
            foreach (RigidBody b in bodies)
            {
                DrawPolygon(b.CollisionPolygon, Pens.Black, e);
            }
        }

        private bool gravity = false;
        private List<Vector2> points = new List<Vector2>();
        private Random random = new Random();
        private void click(object sender, MouseEventArgs e)
        {
            //gravity = true;
            //RigidBody[] bodies = handler.GetBodies();
            //foreach (RigidBody b in bodies)
            //{
            //    if (b != bound)
            //    {
            //        Vector2 origin = b.CollisionPolygon.CenterPoint;
            //        Polygon[] split = Polygon.FracturePolygon(b.CollisionPolygon, 10, random);
            //        if (split.Length > 1)
            //        {
            //            Material material = b.Material;
            //            handler.RemoveBody(b);

            //            RigidBody[] newBodies = RigidBody.ApplySplit(b, split);
            //            for (int i = 0; i < newBodies.Length; i++)
            //            {
            //                handler.AddBody(newBodies[i]);
            //            }
            //        }
            //    }
            //}

            PointF mousePoint = PointToClient(MousePosition);
            points.Add(new Vector2(mousePoint.X, mousePoint.Y));

            if (points.Count == 2)
            {
                //Split bodies on line
                RigidBody[] bodies = handler.GetBodies();
                foreach (RigidBody b in bodies)
                {
                    if (b != bound)
                    {
                        Polygon[] split = Polygon.SplitPolygon(b.CollisionPolygon, new LineSegment(points[0], points[1]).Line);
                        if (split.Length > 1)
                        {
                            handler.RemoveBody(b);

                            RigidBody[] splitBodies = RigidBody.ApplySplit(b, split);
                            for (int i = 0; i < splitBodies.Length; i++) handler.AddBody(splitBodies[i]);
                        }
                    }
                }

                points.Clear();
            }
        }

        private void DrawPolygon(Polygon polygon, Pen drawPen, PaintEventArgs e)
        {
            LineSegment[] sideSegments = polygon.SideSegments;
            for (int i = 0; i < sideSegments.Length; i++)
            {
                DrawLineSegment(sideSegments[i], drawPen, e);
                //e.Graphics.DrawString(i.ToString(), DefaultFont, Brushes.Black, VectorToPointF(sideSegments[i].Midpoint));
            }
        }

        private PointF VectorToPointF(Vector2 vector)
        {
            return new PointF(vector.X, vector.Y);
        }

        private void DrawLineSegment(LineSegment lineSegment, Pen drawPen, PaintEventArgs e)
        {
            e.Graphics.DrawLine(drawPen, new PointF(lineSegment.EndPoints[0].X, lineSegment.EndPoints[0].Y), new PointF(lineSegment.EndPoints[1].X, lineSegment.EndPoints[1].Y));
        }

        private void DrawLine(Line line, Pen drawPen, PaintEventArgs e)
        {
            DrawLineSegment(new LineSegment(line.PointAtX(0), line.PointAtX(Width)), drawPen, e);
        }

        private void drawTimer_Tick(object sender, EventArgs e)
        {
            RigidBody[] bodies = handler.GetBodies();
            foreach (RigidBody b in bodies)
            {
                //b.AddTranslationalVelocity(new Vector2(0, .1F), isMomentum: false);
            }
            handler.Update(null);

            Invalidate();
        }

        private FastHandler<RigidBody> handler = new FastHandler<RigidBody>();
        private RigidBody bound;
        private void Form1_Load(object sender, EventArgs e)
        {
            //handler.AddMetaElement(new GlobalFrictionMeta(.001F));
            handler.AddMetaElement(new GravityMeta());

            bound = new RigidBody(new RotationRectangle(new ImpulseEngine2.RectangleF(0, Height * (2F / 3), Width, Height / 3)), DefinedMaterials.Static);
            handler.AddBody(bound);

            RigidBody sampleBody = new RigidBody(new RotationRectangle(new ImpulseEngine2.RectangleF(Width / 3, 100, Width / 3, Height / 3)), DefinedMaterials.Wood);
            //sampleBody.AddTranslationalVelocity(new Vector2(1, 0), isMomentum: false);
            //sampleBody.AddAngularVelocity(.1F, isMomentum: false);

            RigidBody sampleBody2 = new RigidBody(new RotationRectangle(new ImpulseEngine2.RectangleF(0, Height / 3, Width / 5, Height / 5)), DefinedMaterials.Wood);
            sampleBody2.AddAngularVelocity(.1F, isMomentum: false);

            RigidBody sampleBody3 = new RigidBody(new RotationRectangle(new ImpulseEngine2.RectangleF(Width * (4F / 5), Height / 3, Width / 5, Height / 5)), DefinedMaterials.Wood);
            sampleBody3.AddAngularVelocity(.1F, isMomentum: false);

            handler.AddBody(sampleBody);
            //handler.AddBody(sampleBody2);
            //handler.AddBody(sampleBody3);
        }
    }
}
