#region File Description
//-----------------------------------------------------------------------------
// ShadowMapping.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using CascadedShadowMaps.Shadows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace CascadedShadowMaps
{
	/// <summary>
	/// Sample showing how to implement a simple shadow mapping technique where
	/// the shadow map always contains the contents of the viewing frustum
	/// </summary>
	public class ShadowMappingGame : Microsoft.Xna.Framework.Game
	{
		#region Constants

		const int windowWidth = 800;
		const int windowHeight = 480;

		#endregion

		#region Fields

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		// Starting position and direction of our camera
		Vector3 cameraPosition = new Vector3(0, 70, 100);
		Vector3 cameraForward = new Vector3(0, -0.4472136f, -0.8944272f);
		BoundingFrustum cameraFrustum = new BoundingFrustum(Matrix.Identity);

		// Light direction
		Vector3 lightDir = Vector3.Normalize(new Vector3(-0.3333333f, 0.6666667f, 0.6666667f));
        //Vector3 lightDir = Vector3.Normalize(new Vector3(0.1f, 1.0f, 0.1f));

		KeyboardState currentKeyboardState, lastKeyboardState;
		GamePadState currentGamePadState;

		// Our two models in the scene
		Model gridModel;
		Model shipModel;

		float rotateShip = 0.0f;

		// The shadow map render target
		RenderTarget2D shadowRenderTarget;

		// Transform matrices
		Matrix world;
		Matrix view;
		Matrix projection;

	    private bool _showSplits;

	    private readonly ShadowRenderer _shadowRenderer;

		#endregion

		#region Initialization

		public ShadowMappingGame()
		{
			graphics = new GraphicsDeviceManager(this);

			Content.RootDirectory = "Content";

			graphics.PreferredBackBufferWidth = windowWidth;
			graphics.PreferredBackBufferHeight = windowHeight;

			float aspectRatio = (float)windowWidth / (float)windowHeight;

			projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
															 aspectRatio,
															 1.0f, 1000.0f);

            _shadowRenderer = new ShadowRenderer();
		}


		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// Load the two models we will be using in the sample
			gridModel = Content.Load<Model>("grid");
			shipModel = Content.Load<Model>("ship");

			// Create floating point render target
		    shadowRenderTarget = new RenderTarget2D(
                graphics.GraphicsDevice,
		        _shadowRenderer.ShadowMapSize * 2,
                _shadowRenderer.ShadowMapSize * 2,
		        false,
		        SurfaceFormat.Single,
		        DepthFormat.Depth24);
		}

		#endregion

		#region Update and Draw

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			HandleInput(gameTime);

			UpdateCamera(gameTime);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			// Render the scene to the shadow map
			CreateShadowMap();

			// Draw the scene using the shadow map
			DrawWithShadowMap();

			// Display the shadow map to the screen
			DrawShadowMapToScreen();

			base.Draw(gameTime);
		}

		#endregion

		#region Methods

		private Matrix CreateWorldMatrixForShip()
		{
			return Matrix.CreateRotationY(MathHelper.ToRadians(rotateShip))
				* Matrix.CreateTranslation(0, 15, 0);
		}

        IList<Matrix> _tileTransforms;
        IList<Vector4> _tileBounds;

		/// <summary>
		/// Renders the scene to the floating point render target then 
		/// sets the texture for use when drawing the scene.
		/// </summary>
		void CreateShadowMap()
		{
			// Set our render target to our floating point render target
			GraphicsDevice.SetRenderTarget(shadowRenderTarget);

			// Clear the render target to white or all 1's
			// We set the clear to white since that represents the 
			// furthest the object could be away
			GraphicsDevice.Clear(
                ClearOptions.Target | ClearOptions.DepthBuffer,
                Color.White, 1.0f, 0);

		    var worldBoundingBox = new[] { gridModel, shipModel }
                .SelectMany(x => x.Meshes)
                .Select(x => x.BoundingSphere)
                .Select(BoundingBox.CreateFromSphere)
                .Aggregate(new BoundingBox(), BoundingBox.CreateMerged);

		    IList<Matrix> shadowSplitProjections;
		    IList<float> shadowSplitDistances;
            _shadowRenderer.GetShadowTransforms(
                -lightDir, worldBoundingBox, view, projection, 
                out shadowSplitProjections,
                out shadowSplitDistances,
                out _tileTransforms,
                out _tileBounds);

			// Draw any occluders in our case that is just the ship model

			// Set the models world matrix so it will rotate
			world = CreateWorldMatrixForShip();

		    for (var i = 0; i < _shadowRenderer.NumShadowSplits; i++)
		    {
                // Setup viewport.
		        var x = i % 2;
		        var y = i / 2;
                GraphicsDevice.Viewport = new Viewport(
                    x * _shadowRenderer.ShadowMapSize,
                    y * _shadowRenderer.ShadowMapSize,
                    _shadowRenderer.ShadowMapSize,
                    _shadowRenderer.ShadowMapSize);

		        // Draw the ship model
		        DrawModel(shipModel, true, e =>
		        {
                    e.Parameters["LightViewProj"].SetValue(shadowSplitProjections[i]);
		        });
		    }

		    // Set render target back to the back buffer
			GraphicsDevice.SetRenderTarget(null);
		}

		/// <summary>
		/// Renders the scene using the shadow map to darken the shadow areas
		/// </summary>
		void DrawWithShadowMap()
		{
			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

			GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

			// Draw the grid
			world = Matrix.Identity;
			DrawModel(gridModel, false);

			// Draw the ship model
			world = CreateWorldMatrixForShip();
			DrawModel(shipModel, false);
		}

		/// <summary>
		/// Helper function to draw a model
		/// </summary>
		/// <param name="model">The model to draw</param>
		/// <param name="technique">The technique to use</param>
		void DrawModel(Model model, bool createShadowMap, Action<Effect> setParametersCallback = null)
		{
			string techniqueName = createShadowMap ? "CreateShadowMap" : "DrawWithShadowMap";

			Matrix[] transforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(transforms);

			// Loop over meshs in the model
			foreach (ModelMesh mesh in model.Meshes)
			{
				// Loop over effects in the mesh
				foreach (Effect effect in mesh.Effects)
				{
					// Set the currest values for the effect
					effect.CurrentTechnique = effect.Techniques[techniqueName];
					effect.Parameters["World"].SetValue(world);
					effect.Parameters["View"].SetValue(view);
					effect.Parameters["Projection"].SetValue(projection);
					effect.Parameters["LightDirection"].SetValue(lightDir);

				    if (setParametersCallback != null)
				        setParametersCallback(effect);

				    if (!createShadowMap)
				    {
                        effect.Parameters["ShowSplits"].SetValue(_showSplits);
				        effect.Parameters["ShadowMap"].SetValue(shadowRenderTarget);
                        effect.Parameters["ShadowTransform"].SetValue(_tileTransforms.ToArray());
                        effect.Parameters["TileBounds"].SetValue(_tileBounds.ToArray());
				    }
				}
				// Draw the mesh
				mesh.Draw();
			}
		}

		/// <summary>
		/// Render the shadow map texture to the screen
		/// </summary>
		void DrawShadowMapToScreen()
		{
			spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null);
			spriteBatch.Draw(shadowRenderTarget, new Rectangle(0, 0, 128, 128), Color.White);
			spriteBatch.End();

			GraphicsDevice.Textures[0] = null;
			GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
		}

		#endregion

		#region Handle Input

		/// <summary>
		/// Handles input for quitting the game.
		/// </summary>
		void HandleInput(GameTime gameTime)
		{
			float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

		    lastKeyboardState = currentKeyboardState;
			currentKeyboardState = Keyboard.GetState();
			currentGamePadState = GamePad.GetState(PlayerIndex.One);

			// Rotate the ship model
			rotateShip += currentGamePadState.Triggers.Right * time * 0.2f;
			rotateShip -= currentGamePadState.Triggers.Left * time * 0.2f;

			if (currentKeyboardState.IsKeyDown(Keys.Q))
				rotateShip -= time * 0.2f;
			if (currentKeyboardState.IsKeyDown(Keys.E))
				rotateShip += time * 0.2f;

		    if (currentKeyboardState.IsKeyDown(Keys.V) && !lastKeyboardState.IsKeyDown(Keys.V))
		        _showSplits = !_showSplits;

			// Check for exit.
			if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
				currentGamePadState.Buttons.Back == ButtonState.Pressed)
			{
				Exit();
			}
		}


		/// <summary>
		/// Handles input for moving the camera.
		/// </summary>
		void UpdateCamera(GameTime gameTime)
		{
			float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

			// Check for input to rotate the camera.
			float pitch = -currentGamePadState.ThumbSticks.Right.Y * time * 0.001f;
			float turn = -currentGamePadState.ThumbSticks.Right.X * time * 0.001f;

			if (currentKeyboardState.IsKeyDown(Keys.Up))
				pitch += time * 0.001f;

			if (currentKeyboardState.IsKeyDown(Keys.Down))
				pitch -= time * 0.001f;

			if (currentKeyboardState.IsKeyDown(Keys.Left))
				turn += time * 0.001f;

			if (currentKeyboardState.IsKeyDown(Keys.Right))
				turn -= time * 0.001f;

			Vector3 cameraRight = Vector3.Cross(Vector3.Up, cameraForward);
			Vector3 flatFront = Vector3.Cross(cameraRight, Vector3.Up);

			Matrix pitchMatrix = Matrix.CreateFromAxisAngle(cameraRight, pitch);
			Matrix turnMatrix = Matrix.CreateFromAxisAngle(Vector3.Up, turn);

			Vector3 tiltedFront = Vector3.TransformNormal(cameraForward, pitchMatrix *
														  turnMatrix);

			// Check angle so we cant flip over
			if (Vector3.Dot(tiltedFront, flatFront) > 0.001f)
			{
				cameraForward = Vector3.Normalize(tiltedFront);
			}

		    var cameraTranslationSpeed = 0.03f;
			// Check for input to move the camera around.
		    if (currentKeyboardState.IsKeyDown(Keys.W))
		        cameraPosition += cameraForward * time * cameraTranslationSpeed;

			if (currentKeyboardState.IsKeyDown(Keys.S))
				cameraPosition -= cameraForward * time * cameraTranslationSpeed;

			if (currentKeyboardState.IsKeyDown(Keys.A))
                cameraPosition += cameraRight * time * cameraTranslationSpeed;

			if (currentKeyboardState.IsKeyDown(Keys.D))
                cameraPosition -= cameraRight * time * cameraTranslationSpeed;

			cameraPosition += cameraForward *
							  currentGamePadState.ThumbSticks.Left.Y * time * 0.1f;

			cameraPosition -= cameraRight *
							  currentGamePadState.ThumbSticks.Left.X * time * 0.1f;

			if (currentGamePadState.Buttons.RightStick == ButtonState.Pressed ||
				currentKeyboardState.IsKeyDown(Keys.R))
			{
				cameraPosition = new Vector3(0, 50, 50);
				cameraForward = new Vector3(0, 0, -1);
			}

			cameraForward.Normalize();

			// Create the new view matrix
			view = Matrix.CreateLookAt(cameraPosition,
									   cameraPosition + cameraForward,
									   Vector3.Up);

			// Set the new frustum value
			cameraFrustum.Matrix = view * projection;
		}

		#endregion
	}
}
