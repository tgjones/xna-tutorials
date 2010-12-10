using DrawBoundingBox.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DrawBoundingBox
{
	public class ModelComponent : DrawableGameComponent
	{
		private readonly string _modelAssetName;
		private Model _model;

		public ModelComponent(Game game, string modelAssetName)
			: base(game)
		{
			_modelAssetName = modelAssetName;
		}

		protected override void LoadContent()
		{
			_model = Game.Content.Load<Model>(_modelAssetName);
			base.LoadContent();
		}

		public override void Draw(GameTime gameTime)
		{
			ICameraService cameraService = (ICameraService)Game.Services.GetService(typeof(ICameraService));
			_model.Draw(Matrix.Identity, cameraService.View, cameraService.Projection);
			base.Draw(gameTime);
		}
	}
}