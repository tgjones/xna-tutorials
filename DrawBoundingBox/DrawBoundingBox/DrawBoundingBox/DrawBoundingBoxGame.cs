using DrawBoundingBox.BoundingBoxes;
using DrawBoundingBox.Camera;
using Microsoft.Xna.Framework;

namespace DrawBoundingBox
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class DrawBoundingBoxGame : Game
	{
		public DrawBoundingBoxGame()
		{
			Content.RootDirectory = "Content";
			new GraphicsDeviceManager(this);
		}

		protected override void Initialize()
		{
			SimpleCamera cameraService = new SimpleCamera(this);
			Components.Add(cameraService);
			Services.AddService(typeof (ICameraService), cameraService);

			Components.Add(new ModelComponent(this, "Ship"));
			Components.Add(new BoundingBoxComponent(this, "Ship"));

			base.Initialize();
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			base.Draw(gameTime);
		}
	}
}
