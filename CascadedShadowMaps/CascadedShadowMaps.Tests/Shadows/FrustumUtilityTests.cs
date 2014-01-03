using System.Linq;
using CascadedShadowMaps.Shadows;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace CascadedShadowMaps.Tests.Shadows
{
	[TestFixture]
	public class FrustumUtilityTests
	{
		[TestCase(0.0f, 1.0f, -4136.519f, 4136.519f, -9976.457f)]
		[TestCase(0.3f, 0.8f, -0.517715096f, 0.517715096f, 8.75012302f)]
		public void TestGetWorldSpaceFrustumCorners(
			float clipSpaceNear, float clipSpaceFar,
			float expectedX, float expectedY, float expectedZ)
		{
			// Arrange.
			var view = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up);
			var projection = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.PiOver4, 1.0f, 0.25f, 10000.0f);
			var viewProjectionInverse = Matrix.Invert(view * projection);

			// Act.
			var corners = FrustumUtility.GetWorldSpaceFrustumCorners(
				clipSpaceNear, clipSpaceFar, viewProjectionInverse)
				.ToList();

			// Assert.
			Assert.That(corners, Has.Count.EqualTo(8));
			AssertVector3RoughlyEqual(corners[0], new Vector3(expectedX, expectedY, expectedZ));
		}

		[TestCase(4, 1.0f, 100.0f, 1.0f, 14.4561386f, 30.25f, 53.4363861f, 100.0f)]
		[TestCase(3, 0.5f, 10.0f, 0.5f, 2.51193786f, 5.25868273f, 10.0f)]
		public void TestPracticalSplitScheme(int numSplits, float worldNear, float worldFar, params float[] expectedSplits)
		{
			// Act.
			var splits = FrustumUtility
				.PracticalSplitScheme(numSplits, worldNear, worldFar)
				.ToList();

			// Assert.
			Assert.That(splits, Has.Count.EqualTo(numSplits + 1));
			Assert.That(splits, Is.EquivalentTo(expectedSplits));
		}

		private static void AssertVector3RoughlyEqual(Vector3 actual, Vector3 expected)
		{
			Assert.That(actual.X, Is.EqualTo(expected.X).Within(0.01f));
			Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(0.01f));
			Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(0.01f));
		}
	}
}