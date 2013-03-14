using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Entities;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.MathExtensions;
using BEPUphysics.DataStructures;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using System.Collections;

namespace JumpyGame2
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public Space space;
        public Camera Camera;
        public Model cube;

        public KeyboardState KeyboardState;
        public MouseState MouseState;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D HUDgun, rect1, rect2, rect3, healthText, splash1, splash2, splash3, scoreText, youdead;
        int screenWidth, screenHeight, score = 0, screen = 0;
        public Box player;
        Boolean landed = false, fired = false, started = false, released = true, dead = false;
        public float fieldSize = 400;
        List<Enemy> enemies = new List<Enemy>();
        List<Enemy> newEnemies = new List<Enemy>();
        public float health = 30;
        public Vector3[] moveLocations;
        SoundEffectInstance instance;
        SpriteFont spriteFont;
        string time;
        TimeSpan timer, eTime, cTime;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            screenWidth = 1200;
            screenHeight = 720; 
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            //graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            moveLocations = new Vector3[100];
            Random r = new Random();
            for (int i = 0; i < moveLocations.Length; i++)
            {
                float x = r.Next((int)fieldSize) - fieldSize / 2f;
                float y = 15f;
                float z = r.Next((int)fieldSize) - fieldSize / 2f;
                moveLocations[i] = new Vector3(x, y, z);
            }

            base.Initialize();
        }
        public Enemy[] getEnemies()
        {
            Enemy[] enenemies = new Enemy[enemies.Count];
            enemies.CopyTo(enenemies);
            return enenemies;
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            space = new Space();

            cube = Content.Load<Model>("cube");

            SoundEffect soundEffect = Content.Load<SoundEffect>("Battleships");
            instance = soundEffect.CreateInstance();
            instance.IsLooped = true;
            spriteFont = Content.Load<SpriteFont>("SpriteFont1");
            newCube(new Vector3(0f, 0f, 0f), fieldSize, 0.1f, fieldSize);
            newCube(new Vector3(fieldSize / 2, 5.1f, 0f), 1, 10, fieldSize);
            newCube(new Vector3(-fieldSize / 2, 5.1f, 0f), 1, 10, fieldSize);
            newCube(new Vector3(0f, 5.1f, fieldSize / 2), fieldSize, 10, 1);
            newCube(new Vector3(0f, 5.1f, -fieldSize / 2), fieldSize, 10, 1);

            makeTerrain();
            makeEnemies(75);

            player = new Box(new Vector3(0f, 100f, 0f), 2f, 2f, 2f, 0.1f);
            space.Add(player);
            StaticModel skybox = new StaticModel(Content.Load<Model>("skybox"), Matrix.CreateScale(fieldSize), this);
            Components.Add(skybox);

            HUDgun = Content.Load<Texture2D>("gunImage");
            healthText = Content.Load<Texture2D>("health");
            scoreText = Content.Load<Texture2D>("score");
            youdead = Content.Load<Texture2D>("youDied");
            rect1 = new Texture2D(GraphicsDevice, 1, 1);
            rect1.SetData(new[] { Color.Black });
            rect2 = new Texture2D(GraphicsDevice, 1, 1);
            rect2.SetData(new[] { Color.Green });
            rect3 = new Texture2D(GraphicsDevice, 1, 1);
            rect3.SetData(new[] { Color.LightBlue });
            splash1 = Content.Load<Texture2D>("splash1");
            splash2 = Content.Load<Texture2D>("splash2");
            splash3 = Content.Load<Texture2D>("splash3");

            player.CollisionInformation.Events.InitialCollisionDetected += ContactCreated;

            space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            Camera = new Camera(this, new Vector3(0f, 1f, 2f), player);
        }

        void ContactCreated(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            var otherEntityInformation = other as EntityCollidable;
            if (otherEntityInformation != null)
            {
                landed = true;
            }
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();


            if (health < 0)
                dead = true;

            timer += gameTime.ElapsedGameTime;

            eTime = gameTime.TotalGameTime;

            if (eTime.Minutes < 10)

                time = "Time - 0" + (int)(eTime.Minutes);

            else

                time = "Time - " + (int)(eTime.Minutes);

            if (eTime.Seconds < 10)

                time += ":0" + (int)(eTime.Seconds);

            else

                time += ":" + (int)(eTime.Seconds);


            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
                
            if (started && !dead)
            {   
                space.Update();
                Camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

                List<Enemy> deadEnemies = new List<Enemy>();

                foreach (Enemy e in enemies)
                {
                    e.update(gameTime);
                    if (e.isDead())
                    {
                        deadEnemies.Add(e);
                        score++;
                    }
                }
                foreach (Enemy e in deadEnemies)
                {
                    enemies.Remove(e);
                }

                foreach (Enemy e in newEnemies)
                {
                    enemies.Add(e);
                }
                newEnemies.Clear();

                if (MouseState.LeftButton == ButtonState.Pressed && !fired)
                {
                    fired = true;
                    fire();
                }
                if (MouseState.LeftButton == ButtonState.Released && fired)
                {
                    fired = false;
                }
            }
            if (landed && started && !dead)
            {
                if (KeyboardState.IsKeyDown(Keys.W))
                    MoveForward();
                if (KeyboardState.IsKeyDown(Keys.A))
                    MoveLeft();
                if (KeyboardState.IsKeyDown(Keys.S))
                    MoveBack();
                if (KeyboardState.IsKeyDown(Keys.D))
                    MoveRight();
                if (KeyboardState.IsKeyDown(Keys.Space))
                    jump();
            }

            if (!started)
                if (KeyboardState.IsKeyDown(Keys.Space) && released)
                {
                    advance();
                    released = false;
                }
                else if (!KeyboardState.IsKeyDown(Keys.Space))
                    released = true;

           

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            base.Draw(gameTime);
            
            spriteBatch.Begin();
            spriteBatch.Draw(HUDgun, new Rectangle(0, 0, screenWidth, screenHeight), new Rectangle(0, 0, 1920, 1080), Color.White);
            spriteBatch.Draw(rect1, new Vector2(screenWidth / 2 - 20, screenHeight / 2 - 2.5f), null, Color.Black, 0f, Vector2.Zero, new Vector2(40, 5), SpriteEffects.None, 0f);
            spriteBatch.Draw(rect1, new Vector2(screenWidth / 2 - 2.5f, screenHeight / 2 - 20), null, Color.Black, 0f, Vector2.Zero, new Vector2(5, 40), SpriteEffects.None, 0f);
            spriteBatch.Draw(rect2, new Vector2(0, screenHeight - 20), null, Color.LightGreen, 0f, Vector2.Zero, new Vector2(screenWidth * health / 30, 40), SpriteEffects.None, 0f);
            spriteBatch.Draw(rect3, new Vector2(0, screenHeight - 40), null, Color.Red, 0f, Vector2.Zero, new Vector2(score % screenWidth, 20), SpriteEffects.None, 0f);
            spriteBatch.Draw(healthText, new Vector2(screenWidth / 2 - 50, screenHeight - 30), Color.White);
            spriteBatch.Draw(scoreText, new Vector2(screenWidth / 2 - 50, screenHeight - 50), Color.White);
            if(screen == 0)
                spriteBatch.Draw(splash1, new Rectangle(0, 0, screenWidth, screenHeight), new Rectangle(0, 0, 1920, 1080), Color.White);
            if (screen == 1)
                spriteBatch.Draw(splash2, new Rectangle(0, 0, screenWidth, screenHeight), new Rectangle(0, 0, 1920, 1080), Color.White);
            if (screen == 2)
                spriteBatch.Draw(splash3, new Rectangle(0, 0, screenWidth, screenHeight), new Rectangle(0, 0, 1920, 1080), Color.White);
            if(dead)
                spriteBatch.Draw(youdead, new Rectangle(0, 0, screenWidth, screenHeight), new Rectangle(0, 0, 1920, 1080), Color.White);

            spriteBatch.DrawString(spriteFont, timer.ToString(), new Vector2(10f, 10f), Color.White);
            spriteBatch.End();


            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        #region movement
        private void MoveForward()
        {
            if(player.LinearVelocity.Length() < 10)
                player.LinearVelocity += Camera.WorldMatrix.Forward / 2;
        }

        private void MoveBack()
        {
            if (player.LinearVelocity.Length() < 10)
                player.LinearVelocity += Camera.WorldMatrix.Backward / 2;
        }

        private void MoveLeft()
        {
            if (player.LinearVelocity.Length() < 10)
                player.LinearVelocity += Camera.WorldMatrix.Left / 2;
        }

        private void MoveRight()
        {
            if (player.LinearVelocity.Length() < 10)
                player.LinearVelocity += Camera.WorldMatrix.Right / 2;
        }

        private void jump()
        {
            if (landed)
            {
                player.LinearVelocity += new Vector3(0f, 5f, 0f);
                landed = false;
            }
        }
        #endregion
        

        private void newCube(Vector3 location, float sizeX, float sizeY, float sizeZ)
        {
            Box platformBounds = new Box(location, sizeX, sizeY, sizeZ);
            space.Add(platformBounds);
            Matrix transform = Matrix.CreateScale(platformBounds.Width, platformBounds.Height, platformBounds.Length) * Matrix.CreateTranslation(location);
            StaticModel model = new StaticModel(cube, transform, this);
            Components.Add(model);
            platformBounds.Tag = model;
        }

        private void makeTerrain()
        {
            Random random = new Random();

            for (int x = 0; x < 1000; x++)
            {
                float height = ((float)random.NextDouble()) *10f;
                //newCube(new Vector3(random.Next((int)fieldSize) - fieldSize / 2, height / 2, random.Next((int)fieldSize) - fieldSize / 2), random.Next(10) + 1, height, random.Next(10) + 1);
                newCube(new Vector3(((float)random.NextDouble()) * fieldSize - fieldSize / 2f, height / 2f, ((float)random.NextDouble()) * fieldSize - fieldSize / 2f), ((float)random.NextDouble()) * 10f + 1f, height, ((float)random.NextDouble()) * 10f + 1f);
            }
        }

        public void makeEnemies(int num)
        {
            for (int x = 0; x < num; x++)
            {
                newEnemies.Add(new Enemy(this));
            }
        }

        private void fire()
        {
            Box bullet = new Box(Camera.Position + Camera.WorldMatrix.Forward, 1f, 1f, 1f, 1000f);
            bullet.LinearVelocity = Camera.WorldMatrix.Forward*100;
            space.Add(bullet);
            EntityModel model = new EntityModel(bullet, Content.Load<Model>("blueCube"), Matrix.Identity, this);
            Components.Add(model);
            bullet.Tag = model;
        }

        private void advance()
        {
            screen++;
            if (screen == 3)
            {
                instance.Volume = .8f;
                instance.Play();
                started = true;
            }
        }
    }
}
