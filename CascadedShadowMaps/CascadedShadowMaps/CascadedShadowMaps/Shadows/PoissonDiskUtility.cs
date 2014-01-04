using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CascadedShadowMaps.Shadows
{
    public static class PoissonDiskUtility
    {
        public static Vector2[] CreatePoissonKernel(int shadowMapSize)
        {
            return GetPoissonKernel()
                .Select(x => x / shadowMapSize)
                .OrderBy(x => x.Length())
                .ToArray();
        }

        private static IEnumerable<Vector2> GetPoissonKernel()
        {
            return new[]
            {
                new Vector2(-0.326212f, -0.405810f),
                new Vector2(-0.840144f, -0.073580f),
                new Vector2(-0.695914f, 0.457137f),
                new Vector2(-0.203345f, 0.620716f),
                new Vector2(0.962340f, -0.194983f),
                new Vector2(0.473434f, -0.480026f),
                new Vector2(0.519456f, 0.767022f),
                new Vector2(0.185461f, -0.893124f),
                new Vector2(0.507431f, 0.064425f),
                new Vector2(0.896420f, 0.412458f),
                new Vector2(-0.321940f, -0.932615f),
                new Vector2(-0.791559f, -0.597710f)
            };
        }

        public static Texture3D CreateRotatedPoissonDiskTexture(GraphicsDevice graphicsDevice)
        {
            const int size = 32;

            var result = new Texture3D(graphicsDevice, 
                size, size, size, false, 
                SurfaceFormat.Rg32);

            result.SetData(CreateRandomRotations(size * size * size).ToArray());

            return result;
        }

        private static IEnumerable<ushort> CreateRandomRotations(int count)
        {
            var random = new Random();
            return Enumerable
                .Range(0, count)
                .Select(i => (float) (random.NextDouble() * Math.PI * 2))
                .SelectMany(r => new[] { Math.Cos(r), Math.Sin(r) })
                .Select(v => (UInt16) ((v * 0.5 + 0.5) * UInt16.MaxValue));
        }
    }
}