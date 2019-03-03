using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ImpulseEngine2.Materials;

namespace ImpulseEngine2
{
    public class InteractionHandler<T> : FastHandler<T> where T : RigidBody
    {
        private float interactionElasticity;
        public InteractionHandler(float interactionElasticity)
        {
            this.interactionElasticity = interactionElasticity;
        }

        private MouseState prevMouseState = new MouseState();
        private MouseState currentMouseState = new MouseState();
        public void ControlUpdate(GameTime gameTime, MouseState mouseState, Vector2 scale, Vector2 translation)
        {
            currentMouseState = mouseState;
            //Apply transformations to position
            Vector2 relativeMousePosition = new Vector2((mouseState.Position.X + translation.X) * scale.X, (mouseState.Position.Y + translation.Y) * scale.Y);

            //Update control
            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                if (prevMouseState.LeftButton == ButtonState.Released)
                {
                    //Potentially add joint control
                    T[] bodies = GetBodies();
                    for (int i = 0; i < bodies.Length; i++)
                    {
                        if (bodies[i].CollisionPolygon.MaximumRadius >= LineSegment.Distance(bodies[i].CollisionPolygon.CenterPoint, relativeMousePosition) && bodies[i].CollisionPolygon.ContainsPoint(relativeMousePosition))
                        {
                            //Add joint control
                            CreateJointControl(relativeMousePosition, bodies[i]);
                            break;
                        }
                    }
                }
                else
                {
                    //Uphold current joint control (if exists)
                    if (interactionJoint != null)
                    {
                        UpdateJointControl(relativeMousePosition);
                    }
                }
            }
            else
            {
                if (prevMouseState.LeftButton == ButtonState.Pressed)
                {
                    //Remove joint control (if exists)
                    RemoveJointControl();
                }
            }
            Update(gameTime);

            prevMouseState = mouseState;
        }

        private ElasticJoint interactionJoint = null;
        private RigidBody interactionRigidBody = new RigidBody(new RotationRectangle(new RectangleF(0, 0, 2, 2)), DefinedMaterials.Static);
        private void CreateJointControl(Vector2 mousePosition, T interactingBody)
        {
            interactionRigidBody.CollisionPolygon.TranslateTo(mousePosition);
            interactionJoint = new ElasticJoint(interactionElasticity, interactingBody, interactionRigidBody, mousePosition);
            AddMetaElement(interactionJoint);
        }

        private void RemoveJointControl()
        {
            RemoveMetaElement(interactionJoint);
            interactionJoint = null;
        }

        private void UpdateJointControl(Vector2 mousePosition)
        {
            interactionRigidBody.CollisionPolygon.TranslateTo(mousePosition);
        }
    }
}
