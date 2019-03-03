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
using Microsoft.Xna.Framework;

namespace MomentOfInertia
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private FastHandler<RigidBody> fastHandler = new FastHandler<RigidBody>();

        private void drawTimer_Tick(object sender, EventArgs e)
        {
            fastHandler.Update(null);
            Invalidate();
        }

        private Polygon testy;
        private void Form1_Load(object sender, EventArgs e)
        {
            testy = new RegularPolygon(new Vector2(Width / 2, Height / 2), 50, 8);
        }

        private void paint(object sender, PaintEventArgs e)
        {
            RigidBody[] bodies = fastHandler.GetBodies();
            foreach (RigidBody b in bodies) DrawPolygon(b.CollisionPolygon, Pens.Black, e);

            DrawPolygon(testy, Pens.Black, e);
            e.Graphics.DrawString((testy.GetMomentOfInertia() / testy.GetArea()).ToString(), DefaultFont, Brushes.Black, new PointF(0, 0));
            testy.Rotate(.01F, testy.CenterPoint);
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
    }
}
