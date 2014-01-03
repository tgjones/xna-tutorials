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
            get { return 256; }
        }

        public void GetShadowTransforms(
            Vector3 lightDirection, BoundingBox worldBoundingBox,
            Matrix cameraView, Matrix cameraProjection,
            out IList<Matrix> shadowSplitProjections,
            out IList<float> shadowSplitDistances,
            out IList<Matrix> tileTransforms,
            out IList<Vector4> tileBounds)
        {
            var cameraViewProjection = cameraView * cameraProjection;
            var cameraViewProjectionInverse = Matrix.Invert(cameraViewProjection);

            // Create shadow view matrix.
            var shadowView = Matrix.CreateLookAt(
                Vector3.Zero, 
                lightDirection, 
                Vector3.Up);

            var viewDistance = worldBoundingBox.GetMaximumExtent();

            var splitPlanes = FrustumUtility
                .PracticalSplitScheme(NumShadowSplits, 1, viewDistance)
                .Select(x => -x);

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

                var frustumCorners = FrustumUtility
                    .GetWorldSpaceFrustumCorners(n, f, cameraViewProjectionInverse)
                    .Select(x => Vector3.Transform(x, shadowView))
                    .ToList();

                var min = frustumCorners.Aggregate(Vector3.Min);
                var max = frustumCorners.Aggregate(Vector3.Max);

                // Compute tile transform.

                // compute block index into shadow atlas
                int tileX = i % 2;
                int tileY = i / 2;

                // [x min, x max, y min, y max]
                float tileBorder = 3.0f / ShadowMapSize;
                var tileBounds2 = new Vector4(
                    0.5f * tileX + tileBorder,
                    0.5f * tileX + 0.5f - tileBorder,
                    0.5f * tileY + tileBorder,
                    0.5f * tileY + 0.5f - tileBorder
                );

                // tile matrix: maps from clip space to shadow atlas block
                var tileMatrix = Matrix.Identity;
                tileMatrix.M11 = 0.25f;
                tileMatrix.M22 = -0.25f;
                tileMatrix.Translation = new Vector3(0.25f + tileX * 0.5f, 0.25f + tileY * 0.5f, 0);

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