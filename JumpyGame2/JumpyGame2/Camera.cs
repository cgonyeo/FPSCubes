using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.Entities;

namespace JumpyGame2
{

    public class Camera
    {

        public Vector3 Position { get; set; }
        float yaw;
        float pitch;
        Entity player;

        public float Yaw
        {
            get
            {
                return yaw;
            }
            set
            {
                yaw = MathHelper.WrapAngle(value);
            }
        }

        public float Pitch
        {
            get
            {
                return pitch;
            }
            set
            {
                pitch = MathHelper.Clamp(value, -MathHelper.PiOver2, MathHelper.PiOver2);
            }
        }

        public Matrix ViewMatrix { get; private set; }

        public Matrix ProjectionMatrix { get; set; }

        public Matrix WorldMatrix { get; private set; }

        public Game1 Game { get; private set; }

        public Camera(Game1 game, Vector3 position, Entity player)
        {
            Game = game;
            Position = position;
            this.player = player;
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 4f / 3f, .1f, 10000.0f);
            Mouse.SetPosition(200, 200);
        }

        public void Update(float dt)
        {

            Yaw += (200 - Game.MouseState.X) * dt * .12f;
            Pitch += (200 - Game.MouseState.Y) * dt * .12f;

            Mouse.SetPosition(200, 200);

            WorldMatrix = Matrix.CreateFromAxisAngle(Vector3.Right, Pitch) * Matrix.CreateFromAxisAngle(Vector3.Up, Yaw);
            Position = player.Position - WorldMatrix.Forward * 0;
            WorldMatrix = WorldMatrix * Matrix.CreateTranslation(Position);
            ViewMatrix = Matrix.Invert(WorldMatrix);
        }
    }
}
