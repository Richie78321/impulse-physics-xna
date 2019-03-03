using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ImpulseEngine2;
using System;

namespace Destruction
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private FastHandler<RigidBody> fastHandler = new FastHandler<RigidBody>();
        public Game1()
        {
            IsMouseVisible = true;
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

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            DestructionBlock.BlockTexture = Content.Load<Texture2D>("woodTile");
            DestructionBlock.ProjectileTexture = Content.Load<Texture2D>("stoneTile");

            //Create environment
            fastHandler.AddBody(new RigidBody(new RotationRectangle(new RectangleF(0, GraphicsDevice.Viewport.Height - 10, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)), ImpulseEngine2.Materials.DefinedMaterials.Static));
            SceneSetup();
        }

        private const int DIMENSIONS = 10;
        private void SceneSetup()
        {
            float blockSize = MathHelper.Min(GraphicsDevice.Viewport.Width / DIMENSIONS, GraphicsDevice.Viewport.Height / DIMENSIONS) - .5F;
            for (int y = DIMENSIONS - 1; y >= 0; y--)
            {
                for (int x = 0; x < DIMENSIONS; x++)
                {
                    fastHandler.AddBody(new DestructionBlock(new RectangleF(((GraphicsDevice.Viewport.Width - (blockSize * DIMENSIONS)) / 2) + (x * blockSize), (GraphicsDevice.Viewport.Height - (blockSize * DIMENSIONS) - 10) + (blockSize * y), blockSize, blockSize)));
                }
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private KeyboardState pastKeyboardState = Keyboard.GetState();
        private Random random = new Random();
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
            if (Keyboard.GetState().IsKeyDown(Keys.A) && pastKeyboardState.IsKeyUp(Keys.A))
            {
                DestructionBlock catalystBlock = new DestructionBlock(new RectangleF(-40, random.Next(40, GraphicsDevice.Viewport.Height - 40), 40, 40), projectile: true);
                catalystBlock.AddTranslationalVelocity(new Vector2(20, 0), isMomentum: false);
                catalystBlock.AddAngularVelocity((float)(random.NextDouble() / 10), isMomentum: false);
                fastHandler.AddBody(catalystBlock);
            }

            //RigidBody[] bodies = fastHandler.GetBodies();
            //foreach (RigidBody b in bodies) b.AddTranslationalVelocity(new Vector2(0, .5F), isMomentum: false);

            fastHandler.Update(gameTime);

            pastKeyboardState = Keyboard.GetState();
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
            RigidBody[] bodies = fastHandler.GetBodies();
            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] is DestructionBlock b)
                {
                    b.Draw(GraphicsDevice);
                }
            }

            base.Draw(gameTime);
        }
    }
}
