using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ImpulseEngine2;
using ImpulseEngine2.Materials;
using ImpulseEngine2.Drawing;
using System.Collections;
using System.Collections.Generic;

namespace TestShatter
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        private Texture2D placeholder;
        private FastHandler<RigidBody> handler = new FastHandler<RigidBody>();
        private RigidBody shatterTester;

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            placeholder = Content.Load<Texture2D>("placeholder");

            shatterTester = new RigidBody(new RotationRectangle(new RectangleF(GraphicsDevice.Viewport.Width / 2 - 50, GraphicsDevice.Viewport.Height / 2 - 100, 100, 100)), DefinedMaterials.Wood);
            handler.AddBody(shatterTester);
            handler.AddMetaElement(new GravityMeta());
            handler.AddBody(new RigidBody(new RotationRectangle(new RectangleF(0, GraphicsDevice.Viewport.Height / 2, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height / 2)), DefinedMaterials.Static));

            RigidBody[] bodies = handler.GetBodies();
            standardUVMaps = new List<StandardUVMap>(bodies.Length);
            for (int i = 0; i < bodies.Length; i++) standardUVMaps.Add(new StandardUVMap(placeholder, bodies[i].CollisionPolygon, new Rectangle(0, 0, placeholder.Width, placeholder.Height)));
        }

        private List<StandardUVMap> standardUVMaps;
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private bool done = false;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            handler.Update(gameTime);
            if (!done && Keyboard.GetState().IsKeyDown(Keys.W))
            {
                RigidBody shatterBody = handler.GetBodies()[0];
                Polygon[] shatter = Polygon.FracturePolygon(shatterBody.CollisionPolygon, 2, new System.Random());

                StandardUVMap[] textureUVs;
                RigidBody[] newBodies = RigidBody.ApplySplit(shatterBody, shatter, standardUVMaps[0], out textureUVs);

                handler.RemoveBody(shatterBody);
                foreach (RigidBody b in newBodies) handler.AddBody(b);

                standardUVMaps.RemoveAt(0);
                standardUVMaps.AddRange(textureUVs);

                done = true;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            RigidBody[] bodies = handler.GetBodies();
            for (int i = 0; i < bodies.Length; i++) bodies[i].CollisionPolygon.FillPolygon(GraphicsDevice, placeholder, standardUVMaps[i]);

            base.Draw(gameTime);
        }
    }
}
