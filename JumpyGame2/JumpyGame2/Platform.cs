using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;

namespace JumpyGame2
{
    class Platform : DrawableGameComponent
    {
        Entity entity;
        Model model;
        public Matrix Transform;

        public Platform(Entity entity, Model model, Matrix transform, Game game)
            : base(game)
        {
            this.entity = entity;
            this.model = model;
            this.Transform = transform;

        }

        public override void Draw(GameTime gameTime)
        {
            //Notice that the entity's worldTransform property is being accessed here.
            //This property is returns a rigid transformation representing the orientation
            //and translation of the entity combined.
            //There are a variety of properties available in the entity, try looking around
            //in the list to familiarize yourself with it.
            Matrix worldMatrix = Transform * entity.WorldTransform;

            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in model.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] *
                        Matrix.CreateRotationX(-90)
                        * Matrix.CreateTranslation(entity.Position);
                    /*effect.View = (Game as GettingStartedGame).Camera.ViewMatrix;
                    effect.Projection = (Game as GettingStartedGame).Camera.ProjectionMatrix;*/
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();

                /*model.CopyAbsoluteBoneTransformsTo(boneTransforms);
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = boneTransforms[mesh.ParentBone.Index] * worldMatrix;
                        effect.View = (Game as GettingStartedGame).Camera.ViewMatrix;
                        effect.Projection = (Game as GettingStartedGame).Camera.ProjectionMatrix;
                    }
                    mesh.Draw();
                }
                base.Draw(gameTime);*/
            }
        }
    }
}
