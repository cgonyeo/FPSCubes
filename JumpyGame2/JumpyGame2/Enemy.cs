using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace JumpyGame2
{
    public class Enemy
    {
        Box me;
        Game1 game;
        public Vector3 tarLoc;
        Boolean dead = false;
        static int locCounter = 0;
        SoundEffect soundEffect;
        float timeSinceSound = 6;
        
        public Enemy(Game1 game)
        {
            this.game = game;
            me = new Box(getNewLocation(), 1, 1, 1, 0.1f);
            game.space.Add(me);
            Matrix transform = Matrix.CreateScale(me.Width, me.Height, me.Length);// *Matrix.CreateTranslation(me.Position);
            EntityModel model = new EntityModel(me, game.Content.Load<Model>("redCube"), Matrix.Identity, game);
            game.Components.Add(model);
            me.Tag = model;
            soundEffect = game.Content.Load<SoundEffect>("fuse");

            tarLoc = getNewLocation();
        }

        private Vector3 getNewLocation()
        {
            Vector3 temp = game.moveLocations[locCounter];
            locCounter++;
            if (locCounter >= game.moveLocations.Length)
                locCounter = 0;
            return temp;
        }
        public void update(GameTime gameTime)
        {
            if (!dead)
            {
                
                Random random = new Random();

                if ((game.player.Position - me.Position).Length() < 50) //if we're within 50 of player, set target location as player
                {
                    tarLoc = game.player.Position;
                }
                else if (random.NextDouble() < 0.01)
                {
                    tarLoc = getNewLocation();
                }
                if (me.LinearVelocity.Length() < 10)
                {
                    Vector3 pathTemp = tarLoc - me.Position; //calculate new trajectory
                    pathTemp.Normalize(); //set new travel path to a length of 1
                    Vector3 path = new Vector3(pathTemp.X, 0, pathTemp.Z);
                    me.LinearVelocity += path;
                }
                else if (me.LinearVelocity.Length() > 50)
                {
                    dead = true;

                    game.Components.Remove((EntityModel)me.Tag);
                    Matrix transform = Matrix.CreateScale(me.Width, me.Height, me.Length);
                    EntityModel model = new EntityModel(me, game.Content.Load<Model>("blackCube"), transform, game);
                    game.Components.Add(model);
                    me.Tag = model;

                    game.makeEnemies(2);
                }


                if ((me.Position - game.player.Position).Length() < 2)
                {
                    game.health -= (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000f;
                    if (timeSinceSound < 5)
                        timeSinceSound += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000f;
                    else
                    {
                        timeSinceSound = 0;
                        soundEffect.Play();
                    }
                }
            }
        }

        public Boolean isDead()
        {
            return dead;
        }
        
    }
}
