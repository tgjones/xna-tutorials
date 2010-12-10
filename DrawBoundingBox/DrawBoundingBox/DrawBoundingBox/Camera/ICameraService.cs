using Microsoft.Xna.Framework;

namespace DrawBoundingBox.Camera
{
	public interface ICameraService
	{
		Matrix View { get; }
		Matrix Projection { get; }
	}
}