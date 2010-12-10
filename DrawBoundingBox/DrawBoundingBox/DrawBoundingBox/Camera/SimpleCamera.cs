using Microsoft.Xna.Framework;

namespace DrawBoundingBox.Camera
{
	public class SimpleCamera : DrawableGameComponent, ICameraService
	{
		public SimpleCamera(Game game)
			: base(game)
		{
			
		}

		public override void Update(GameTime gameTime)
		{
			View = Matrix.CreateLookAt(new Vector3(2000, 1800, -2000), new Vector3(0, -200, 0), Vector3.Up);
			Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
				GraphicsDevice.Viewport.AspectRatio,
				1.0f, 5000.0f);

			base.Update(gameTime);
		}

		public Matrix View { get; private set; }
		public Matrix Projection { get; private set; }
	}
}