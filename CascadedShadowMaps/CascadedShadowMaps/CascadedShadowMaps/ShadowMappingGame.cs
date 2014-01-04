#region File Description
//-----------------------------------------------------------------------------
// ShadowMapping.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using CascadedShadowMaps.Shadows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

#endregion

namespace CascadedShadowMaps
{
	/// <summary>
	/// Sample showing how to implement a simple shadow mapping technique where
	/// the shadow map always contains the contents of the viewing frustum
	/// </summary>
	public class ShadowMappingGame : Game
	{
		#region Constants

	    private const int WindowWidth = 800;
	    private const int WindowHeight = 480;

		#endregion

		#region Fields

	    private readonly GraphicsDeviceManager _graphics;
	    private SpriteBatch _spriteBatch;

		// Starting position and direction of our camera
	    private Vector3 _cameraPosition = new Vector3(0, 70, 100);
	    private Vector3 _cameraForward = new Vector3(0, -0.4472136f, -0.8944272f);

		// Light direction
	    private readonly Vector3 _lightDir = Vector3.Normalize(new Vector3(-0.3333333f, 0.6666667f, 0.6666667f));

	    private KeyboardState _currentKeyboardState, _lastKeyboardState;
	    private GamePadState _currentGamePadState;

		// Our two models in the scene
	    private Model _gridModel;
	    private Model _shipModel;

	    private float _rotateShip;

		// The shadow map render target
	    private RenderTarget2D _shadowRenderTarget;

		// Transform matrices
        private readonly Matrix _projection;
        private Matrix _view;

	    private bool _showSplits;

	    private readonly ShadowRenderer _shadowRenderer;

        private IList<Matrix> _tileTransforms;
        private IList<Vector4> _tileBounds;

        [StructLayout(LayoutKind.Sequential)]
        private struct DepthBiasState
        {
            public readonly float SlopeScaleDepthBias;
            public readonly float DepthBias;

            public DepthBiasState(float slopeScaleDepthBias, float depthBias)
            {
                SlopeScaleDepthBias = slopeScaleDepthBias;
                DepthBias = depthBias;
            }
        }

        private readonly DepthBiasState[] _shadowDepthBias = 
        {
            new DepthBiasState(2.5f, 0.0009f),
            new DepthBiasState(2.5f, 0.0009f),
            new DepthBiasState(2.5f, 0.0009f),
            new DepthBiasState(2.5f, 0.001f)
        };

		#endregion

		#region Initialization

		public ShadowMappingGame()
		{
			_graphics = new GraphicsDeviceManager(this);

			Content.RootDirectory = "Content";

			_graphics.PreferredBackBufferWidth = WindowWidth;
			_graphics.PreferredBackBufferHeight = WindowHeight;

		    const float aspectRatio = (float) WindowWidth / (float) WindowHeight;

		    _projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, aspectRatio, 1.0f, 1000.0f);

            _shadowRenderer = new ShadowRenderer();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// Load the two models we will be using in the sample
			_gridModel = Content.Load<Model>("grid");
			_shipModel = Content.Load<Model>("ship");

		    var rotatedPoissonDiskTexture = PoissonDiskUtility.CreateRotatedPoissonDiskTexture(GraphicsDevice);
		    foreach (var meshPart in new[] { _gridModel, _shipModel }.SelectMany(x => x.Meshes).SelectMany(x => x.MeshParts))
		    {
                meshPart.Effect.Parameters["RotatedPoissonDiskTexture"].SetValue(rotatedPoissonDiskTexture);
                meshPart.Effect.Parameters["PoissonKernel"].SetValue(PoissonDiskUtility.CreatePoissonKernel(_shadowRenderer.ShadowMapSize));
		    }

		    // Create floating point render target
		    _shadowRenderTarget = new RenderTarget2D(
                _graphics.GraphicsDevice,
		        _shadowRenderer.ShadowMapSize * _shadowRenderer.NumShadowSplits,
                _shadowRenderer.ShadowMapSize,
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
			return Matrix.CreateRotationY(MathHelper.ToRadians(_rotateShip))
				* Matrix.CreateTranslation(0, 15, 0);
		}

	    /// <summary>
		/// Renders the scene to the floating point render target then 
		/// sets the texture for use when drawing the scene.
		/// </summary>
		private void CreateShadowMap()
		{
			// Set our render target to our floating point render target
			GraphicsDevice.SetRenderTarget(_shadowRenderTarget);

			// Clear the render target to white or all 1's
			// We set the clear to white since that represents the 
			// furthest the object could be away
			GraphicsDevice.Clear(
                ClearOptions.Target | ClearOptions.DepthBuffer,
                Color.White, 1.0f, 0);

		    var worldBoundingBox = new[] { _gridModel, _shipModel }
                .SelectMany(x => x.Meshes)
                .Select(x => x.BoundingSphere)
                .Select(BoundingBox.CreateFromSphere)
                .Aggregate(new BoundingBox(), BoundingBox.CreateMerged);

		    IList<Matrix> shadowSplitProjections;
		    IList<float> shadowSplitDistances;
            _shadowRenderer.GetShadowTransforms(
                -_lightDir, worldBoundingBox, _view, _projection, 
                out shadowSplitProjections,
                out shadowSplitDistances,
                out _tileTransforms,
                out _tileBounds);

			// Draw any occluders in our case that is just the ship model

			// Set the models world matrix so it will rotate
			var world = CreateWorldMatrixForShip();

            // Render each shadow split.
		    for (var i = 0; i < _shadowRenderer.NumShadowSplits; i++)
		    {
                // Setup viewport.
                GraphicsDevice.Viewport = new Viewport(
                    i * _shadowRenderer.ShadowMapSize, 0,
                    _shadowRenderer.ShadowMapSize,
                    _shadowRenderer.ShadowMapSize);

		        // Draw the ship model
		        int i1 = i;
		        DrawModel(_shipModel, world, true, e =>
		        {
                    e.Parameters["LightViewProj"].SetValue(shadowSplitProjections[i1]);
                    e.Parameters["DepthBias"].StructureMembers["SlopeScaleDepthBias"].SetValue(_shadowDepthBias[i1].SlopeScaleDepthBias);
                    e.Parameters["DepthBias"].StructureMembers["DepthBias"].SetValue(_shadowDepthBias[i1].DepthBias);
		        });
		    }

		    // Set render target back to the back buffer
			GraphicsDevice.SetRenderTarget(null);
		}

		/// <summary>
		/// Renders the scene using the shadow map to darken the shadow areas
		/// </summary>
		private void DrawWithShadowMap()
		{
			_graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

			GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

		    Action<Effect> setParameters = effect =>
		    {
		        effect.Parameters["ShowSplits"].SetValue(_showSplits);
		        effect.Parameters["ShadowMap"].SetValue(_shadowRenderTarget);
		        effect.Parameters["ShadowTransform"].SetValue(_tileTransforms.ToArray());
		        effect.Parameters["TileBounds"].SetValue(_tileBounds.ToArray());
		    };

			// Draw the grid
            DrawModel(_gridModel, Matrix.Identity, false, setParameters);

			// Draw the ship model
            DrawModel(_shipModel, CreateWorldMatrixForShip(), false, setParameters);
		}

		/// <summary>
		/// Helper function to draw a model
		/// </summary>
		private void DrawModel(Model model, Matrix worldMatrix, bool createShadowMap, Action<Effect> setParametersCallback)
		{
			var techniqueName = createShadowMap ? "CreateShadowMap" : "DrawWithShadowMap";

			// Loop over meshs in the model
			foreach (ModelMesh mesh in model.Meshes)
			{
				// Loop over effects in the mesh
			    foreach (Effect effect in mesh.Effects)
			    {
			        // Set the currest values for the effect
			        effect.CurrentTechnique = effect.Techniques[techniqueName];
			        effect.Parameters["World"].SetValue(worldMatrix);
			        effect.Parameters["View"].SetValue(_view);
			        effect.Parameters["Projection"].SetValue(_projection);
			        effect.Parameters["LightDirection"].SetValue(_lightDir);

			        setParametersCallback(effect);
			    }

			    // Draw the mesh
				mesh.Draw();
			}
		}

		/// <summary>
		/// Render the shadow map texture to the screen
		/// </summary>
        private void DrawShadowMapToScreen()
		{
			_spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null);
			_spriteBatch.Draw(_shadowRenderTarget, new Rectangle(0, 0, 128 * 4, 128), Color.White);
			_spriteBatch.End();

			GraphicsDevice.Textures[0] = null;
			GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
		}

		#endregion

		#region Handle Input

		/// <summary>
		/// Handles input for quitting the game.
		/// </summary>
		private void HandleInput(GameTime gameTime)
		{
			var time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

		    _lastKeyboardState = _currentKeyboardState;
			_currentKeyboardState = Keyboard.GetState();
			_currentGamePadState = GamePad.GetState(PlayerIndex.One);

			// Rotate the ship model
			_rotateShip += _currentGamePadState.Triggers.Right * time * 0.2f;
			_rotateShip -= _currentGamePadState.Triggers.Left * time * 0.2f;

			if (_currentKeyboardState.IsKeyDown(Keys.Q))
				_rotateShip -= time * 0.2f;
			if (_currentKeyboardState.IsKeyDown(Keys.E))
				_rotateShip += time * 0.2f;

		    if (_currentKeyboardState.IsKeyDown(Keys.V) && !_lastKeyboardState.IsKeyDown(Keys.V))
		        _showSplits = !_showSplits;

			// Check for exit.
			if (_currentKeyboardState.IsKeyDown(Keys.Escape) ||
				_currentGamePadState.Buttons.Back == ButtonState.Pressed)
			{
				Exit();
			}
		}

		/// <summary>
		/// Handles input for moving the camera.
		/// </summary>
        private void UpdateCamera(GameTime gameTime)
		{
			var time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

			// Check for input to rotate the camera.
			float pitch = -_currentGamePadState.ThumbSticks.Right.Y * time * 0.001f;
			float turn = -_currentGamePadState.ThumbSticks.Right.X * time * 0.001f;

		    var cameraRotationSpeed = time * 0.001f;

			if (_currentKeyboardState.IsKeyDown(Keys.Up))
                pitch += cameraRotationSpeed;

			if (_currentKeyboardState.IsKeyDown(Keys.Down))
                pitch -= cameraRotationSpeed;

			if (_currentKeyboardState.IsKeyDown(Keys.Left))
                turn += cameraRotationSpeed;

			if (_currentKeyboardState.IsKeyDown(Keys.Right))
                turn -= cameraRotationSpeed;

			Vector3 cameraRight = Vector3.Cross(Vector3.Up, _cameraForward);
			Vector3 flatFront = Vector3.Cross(cameraRight, Vector3.Up);

			Matrix pitchMatrix = Matrix.CreateFromAxisAngle(cameraRight, pitch);
			Matrix turnMatrix = Matrix.CreateFromAxisAngle(Vector3.Up, turn);

			Vector3 tiltedFront = Vector3.TransformNormal(_cameraForward, pitchMatrix * turnMatrix);

			// Check angle so we cant flip over
		    if (Vector3.Dot(tiltedFront, flatFront) > 0.001f)
		        _cameraForward = Vector3.Normalize(tiltedFront);

		    var cameraTranslationSpeed = time * 0.03f;

			// Check for input to move the camera around.
		    if (_currentKeyboardState.IsKeyDown(Keys.W))
		        _cameraPosition += _cameraForward * cameraTranslationSpeed;

			if (_currentKeyboardState.IsKeyDown(Keys.S))
				_cameraPosition -= _cameraForward * cameraTranslationSpeed;

			if (_currentKeyboardState.IsKeyDown(Keys.A))
                _cameraPosition += cameraRight * cameraTranslationSpeed;

			if (_currentKeyboardState.IsKeyDown(Keys.D))
                _cameraPosition -= cameraRight * cameraTranslationSpeed;

			_cameraPosition += _cameraForward * _currentGamePadState.ThumbSticks.Left.Y * cameraTranslationSpeed;
            _cameraPosition -= cameraRight * _currentGamePadState.ThumbSticks.Left.X * cameraTranslationSpeed;

			if (_currentGamePadState.Buttons.RightStick == ButtonState.Pressed ||
				_currentKeyboardState.IsKeyDown(Keys.R))
			{
				_cameraPosition = new Vector3(0, 50, 50);
				_cameraForward = new Vector3(0, 0, -1);
			}

			_cameraForward.Normalize();

			// Create the new view matrix
		    _view = Matrix.CreateLookAt(_cameraPosition,
		        _cameraPosition + _cameraForward,
		        Vector3.Up);
		}

		#endregion
	}
}
