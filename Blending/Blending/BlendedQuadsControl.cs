#region File Description
//-----------------------------------------------------------------------------
// SpinningTriangleControl.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace WinFormsGraphicsDevice
{
    /// <summary>
    /// Example control inherits from GraphicsDeviceControl, which allows it to
    /// render using a GraphicsDevice. This control shows how to draw animating
    /// 3D graphics inside a WinForms application. It hooks the Application.Idle
    /// event, using this to invalidate the control, which will cause the animation
    /// to constantly redraw.
    /// </summary>
    public class BlendedQuadsControl : GraphicsDeviceControl
    {
    	private SpriteBatch _spriteBatch;
    	private Texture2D _redTexture, _greenTexture;

		public BlendFunction ColorBlendFunction { get; set; }
		public Blend ColorSourceBlend { get; set; }
		public Blend ColorDestinationBlend { get; set; }

        /// <summary>
        /// Initializes the control.
        /// </summary>
        protected override void Initialize()
        {
        	_spriteBatch = new SpriteBatch(GraphicsDevice);

        	_redTexture = new Texture2D(GraphicsDevice, 300, 200);
        	_redTexture.SetData(Enumerable.Repeat(Color.Red, 300 * 200).ToArray());

			_greenTexture = new Texture2D(GraphicsDevice, 300, 200);
			_greenTexture.SetData(Enumerable.Repeat(Color.Green, 300 * 200).ToArray());

            // Hook the idle event to constantly redraw our animation.
            Application.Idle += delegate { Invalidate(); };
        }

        /// <summary>
        /// Draws the control.
        /// </summary>
        protected override void Draw()
        {
            GraphicsDevice.Clear(Color.White);

        	var blendState = new BlendState
        	{
				AlphaBlendFunction = ColorBlendFunction,
				AlphaSourceBlend = ColorSourceBlend,
				AlphaDestinationBlend = ColorDestinationBlend,

				ColorBlendFunction = ColorBlendFunction,
				ColorSourceBlend = ColorSourceBlend,
				ColorDestinationBlend = ColorDestinationBlend
        	};

        	_spriteBatch.Begin(SpriteSortMode.Immediate, blendState);
        	_spriteBatch.Draw(_redTexture, new Rectangle(100, 100, 300, 200), Color.White);
			_spriteBatch.Draw(_greenTexture, new Rectangle(200, 200, 300, 200), Color.White);
        	_spriteBatch.End();
        }
    }
}
