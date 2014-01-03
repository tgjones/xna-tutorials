using System.Linq;
using Microsoft.Xna.Framework;

namespace CascadedShadowMaps.Shadows
{
    public static class BoundingBoxExtensions
    {
        public static float GetMaximumExtent(this BoundingBox boundingBox)
        {
            return new[]
            {
                boundingBox.Max.X - boundingBox.Min.X,
                boundingBox.Max.Y - boundingBox.Min.Y,
                boundingBox.Max.Z - boundingBox.Min.Z,
            }.Max();
        }
    }

    public static class BoundingBoxUtility
    {
        public static BoundingBox Transform(BoundingBox boundingBox, Matrix matrix)
        {
            var xa = matrix.Right * boundingBox.Min.X;
            var xb = matrix.Right * boundingBox.Max.X;

            var ya = matrix.Up * boundingBox.Min.Y;
            var yb = matrix.Up * boundingBox.Max.Y;

            var za = matrix.Backward * boundingBox.Min.Z;
            var zb = matrix.Backward * boundingBox.Max.Z;

            return new BoundingBox(
                Vector3.Min(xa, xb) + Vector3.Min(ya, yb) + Vector3.Min(za, zb) + matrix.Translation,
                Vector3.Max(xa, xb) + Vector3.Max(ya, yb) + Vector3.Max(za, zb) + matrix.Translation);
        }
    }
}