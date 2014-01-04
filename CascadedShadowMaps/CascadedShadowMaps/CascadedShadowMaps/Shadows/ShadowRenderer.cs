using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace CascadedShadowMaps.Shadows
{
    public class ShadowRenderer
    {
        public int NumShadowSplits
        {
            get { return 4; }
        }

        public int ShadowMapSize
        {
            get { return 512; }
        }

        public void GetShadowTransforms(
            Vector3 lightDirection, BoundingBox worldBoundingBox,
            Matrix cameraView, Matrix cameraProjection,
            out IList<Matrix> shadowSplitProjections,
            out IList<float> shadowSplitDistances,
            out IList<Matrix> tileTransforms,
            out IList<Vector4> tileBounds)
        {
            var cameraViewProjectionInverse = Matrix.Invert(cameraView * cameraProjection);

            // Create shadow view matrix.
            var shadowView = Matrix.CreateLookAt(
                Vector3.Zero, 
                lightDirection, 
                Vector3.Up);

            var viewDistance = 100;//worldBoundingBox.GetMaximumExtent();

            var splitPlanes = FrustumUtility
                .PracticalSplitScheme(NumShadowSplits, 1, viewDistance)
                .Select(x => -x)
                .ToList();

            var splitDistances = splitPlanes
                .Select(x => FrustumUtility.ConvertViewSpaceDepthToClipSpaceDepth(x, cameraProjection))
                .ToList();

            var shadowViewBoundingBox = BoundingBoxUtility.Transform(
                worldBoundingBox, shadowView);
            var minZ = -shadowViewBoundingBox.Max.Z;
            var maxZ = -shadowViewBoundingBox.Min.Z;

            var splitData = Enumerable.Range(0, NumShadowSplits).Select(i =>
            {
                var n = splitDistances[i];
                var f = splitDistances[i + 1];

                // Calculate four of the planes (but not near / far) using the
                // minimum and maximum of the X and Y coordinates of the view
                // frustum in light space.
                var frustumCorners = FrustumUtility
                    .GetWorldSpaceFrustumCorners(n, f, cameraViewProjectionInverse)
                    .Select(x => Vector3.Transform(x, shadowView))
                    .ToList();

                var min = frustumCorners.Aggregate(Vector3.Min);
                var max = frustumCorners.Aggregate(Vector3.Max);

                // Compute tile transform.

                // compute block index into shadow atlas
                var tileX = i;

                // The border keeps the pixel shader from reading outside the
                // valid range when we're using rotated Poisson disk filtering.
                float tileBorder = 3.0f / ShadowMapSize;

                // [x min, x max, y min, y max]
                var tileBounds2 = new Vector4(
                    0.25f * tileX + tileBorder,
                    0.25f * tileX + 0.25f - tileBorder,
                    tileBorder,
                    1.0f - tileBorder
                );

                // tile matrix: maps from clip space to shadow atlas block
                var tileMatrix = Matrix.Identity;
                tileMatrix.M11 = 0.125f;
                tileMatrix.M22 = -0.5f;
                tileMatrix.Translation = new Vector3(0.125f + tileX * 0.25f, 0.5f, 0);

                return new
                {
                    Distance = f,
                    Projection = Matrix.CreateOrthographicOffCenter(
                        min.X, max.X, min.Y, max.Y, minZ, maxZ),
                    TileTransform = tileMatrix,
                    TileBounds = tileBounds2
                };
            }).ToList();

            shadowSplitProjections = splitData
                .Select(x => shadowView * x.Projection)
                .ToList();
            shadowSplitDistances = splitData
                .Select(x => x.Distance)
                .ToList();
            tileTransforms = splitData
                .Select(x => shadowView * x.Projection * x.TileTransform)
                .ToList();
            tileBounds = splitData
                .Select(x => x.TileBounds)
                .ToList();
        }
    }
}