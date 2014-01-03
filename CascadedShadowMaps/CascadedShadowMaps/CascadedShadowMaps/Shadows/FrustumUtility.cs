using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace CascadedShadowMaps.Shadows
{
	public static class FrustumUtility
	{
		public static IEnumerable<Vector3> GetWorldSpaceFrustumCorners(
			float clipSpaceNear, float clipSpaceFar,
			Matrix viewProjectionInverse)
		{
			var clipSpaceBoundingBox = new BoundingBox(
				new Vector3(-1, -1, clipSpaceNear), 
				new Vector3(1, 1, clipSpaceFar));
			return clipSpaceBoundingBox.GetCorners().Select(v =>
			{
				var vt = Vector4.Transform(v, viewProjectionInverse);
				vt /= vt.W;

				return new Vector3(vt.X, vt.Y, vt.Z);
			});
		}

		public static IEnumerable<float> PracticalSplitScheme(int numSplits, float n, float f)
		{
			for (int i = 0; i < numSplits; ++i)
			{
				float p = (float) i / numSplits;
				float cLog = n * (float) System.Math.Pow(f / n, p);
				float cLin = n + (f - n) * p;

				yield return 0.5f * (cLog + cLin);
			}

			yield return f;
		}

		public static float ConvertViewSpaceDepthToClipSpaceDepth(float depth, Matrix projectionMatrix)
		{
			var result = Vector4.Transform(new Vector3(0, 0, depth), projectionMatrix);
			return (result.W != 0) ? result.Z / result.W : 0;
		}
	}
}