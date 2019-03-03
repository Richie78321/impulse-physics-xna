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

namespace TangentialTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void paint(object sender, PaintEventArgs e)
        {
            Line testLine = new Line(new Vector2(PointToClient(MousePosition).X, PointToClient(MousePosition).Y), lineSlope);

            Polygon[] splitPolygons = Polygon.SplitPolygon(polygon, testLine);
            DrawPolygon(splitPolygons[0], Pens.Red, e);
            if (splitPolygons.Length == 2) DrawPolygon(splitPolygons[1], Pens.Blue, e);

            DrawLine(testLine, Pens.Black, e);
        }

        private void paintTimer_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void DrawPolygon(Polygon polygon, Pen drawPen, PaintEventArgs e)
        {
            LineSegment[] sideSegments = polygon.SideSegments;
            for (int i = 0; i < sideSegments.Length; i++)
            {
                DrawLineSegment(sideSegments[i], drawPen, e);
                e.Graphics.DrawString(i.ToString(), DefaultFont, Brushes.Black, VectorToPointF(sideSegments[i].Midpoint));
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

        private Polygon polygon;
        private float lineSlope = 0;
        private void Form1_Load(object sender, EventArgs e)
        {
            polygon = new RegularPolygon(new Vector2(Width / 2, Height / 2), 100, 3);
        }

        private void click(object sender, MouseEventArgs e)
        {
            lineSlope += (float)Math.Atan(Math.PI / 12);
        }
    }
}
