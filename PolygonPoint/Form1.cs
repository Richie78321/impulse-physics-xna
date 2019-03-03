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
using ImpulseEngine2.Materials;

namespace PolygonPoint
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private RigidBody wheel1;
        private RigidBody wheel2;
        private void Form1_Load(object sender, EventArgs e)
        {
            wheel1 = new RigidBody(new RegularPolygon(new Vector2((Width - 100) / 2, Height / 2 - 50), 15, 8), DefinedMaterials.Rubber, 1);
            wheel2 = new RigidBody(new RegularPolygon(new Vector2((Width + 100) / 2, Height / 2 - 50), 15, 8), DefinedMaterials.Rubber, 1);
            RigidBody bridge = new RigidBody(new RotationRectangle(new ImpulseEngine2.RectangleF((Width - 100) / 2, Height / 2 - 100, 100, 50)), DefinedMaterials.Rubber, 1);
            bridge.AddTranslationalVelocity(new Vector2(0, .1F), isMomentum: false);
            //bridge.AddAngularVelocity(.1F, isMomentum: false);
            //wheel2.AddTranslationalVelocity(new Vector2(0, 3), isMomentum: false);

            //RigidBody tankBody = new RigidBody(new RotationRectangle(new ImpulseEngine2.RectangleF((Width - 100) / 2, Height / 2 - 200, 100, 150)), DefinedMaterials.Wood, 1);

            ElasticJoint elasticJoint1 = new ElasticJoint(.5F, bridge, new Vector2((Width - 100) / 2, Height / 2 - 50), wheel1, new Vector2((Width - 100) / 2, Height / 2 - 50));
            ElasticJoint elasticJoint2 = new ElasticJoint(.5F, bridge, new Vector2((Width + 100) / 2, Height / 2 - 50), wheel2, new Vector2((Width + 100) / 2, Height / 2 - 50));
            //ElasticJoint elasticJoint3 = new ElasticJoint(.5F, bridge, new Vector2((Width - 100) / 2, Height / 2 - 50), tankBody, new Vector2((Width - 100) / 2, Height / 2 - 50));

            RigidBody bound = new RigidBody(new RotationRectangle(new ImpulseEngine2.RectangleF(0, Height * (2F / 3), Width, Height / 3)), DefinedMaterials.Static);

            handler.AddMetaElement(new GravityMeta());
            handler.AddMetaElement(elasticJoint1);
            handler.AddMetaElement(elasticJoint2);
            handler.AddBody(wheel1);
            handler.AddBody(wheel2);
            handler.AddBody(bridge);
            handler.AddBody(bound);

            //RigidBody body1 = new RigidBody(new RotationRectangle(new ImpulseEngine2.RectangleF(0, 0, 200, 100)), DefinedMaterials.Wood);
            //RigidBody body2 = new RigidBody(new RotationRectangle(new ImpulseEngine2.RectangleF(0, 200, 200, 100)), DefinedMaterials.Wood);
            //body1.AddTranslationalVelocity(new Vector2(.1F, 0), isMomentum: false);

            //handler.AddBody(body1);
            //handler.AddBody(body2);

            //ElasticJoint joint = new ElasticJoint(.5F, body1, body1.CollisionPolygon.CenterPoint, body2, body2.CollisionPolygon.CenterPoint);

            //handler.AddMetaElement(joint);
        }

        private void paint(object sender, PaintEventArgs e)
        {
            RigidBody[] bodies = handler.GetBodies();
            foreach (RigidBody b in bodies) DrawPolygon(b.CollisionPolygon, Brushes.Blue, e);
        }

        private void DrawPolygon(Polygon polygon, Brush drawBrush, PaintEventArgs e)
        {
            PointF[] vertices = new PointF[polygon.Vertices.Length + 1];
            for (int i = 0; i < vertices.Length; i++) vertices[i] = new PointF(polygon.Vertices[(i + 1) % polygon.Vertices.Length].X, polygon.Vertices[(i + 1) % polygon.Vertices.Length].Y);
            e.Graphics.DrawPolygon(Pens.Blue, vertices);
        }

        private FastHandler<RigidBody> handler = new FastHandler<RigidBody>();
        private void drawTimer_Tick(object sender, EventArgs e)
        {
            if (followMouse)
            {
                wheel1.AddAngularVelocity(-.01F, isMomentum: false);
                wheel2.AddAngularVelocity(-.01F, isMomentum: false);
            }

            handler.Update(null);
            Invalidate();
        }

        private bool followMouse = false;
        private void click(object sender, MouseEventArgs e)
        {
            followMouse = !followMouse;
        }
    }
}
