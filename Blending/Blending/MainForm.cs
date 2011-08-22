using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace Blending
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void cboColorSourceBlend_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			blendedQuadsControl1.ColorSourceBlend = (Blend) Enum.Parse(typeof(Blend), (string) cboColorSourceBlend.SelectedItem);
		}

		private void cboColorDestinationBlend_SelectedIndexChanged(object sender, EventArgs e)
		{
			blendedQuadsControl1.ColorDestinationBlend = (Blend)Enum.Parse(typeof(Blend), (string)cboColorDestinationBlend.SelectedItem);
		}

		private void cboColorBlendFunction_SelectedIndexChanged(object sender, EventArgs e)
		{
			blendedQuadsControl1.ColorBlendFunction = (BlendFunction)Enum.Parse(typeof(BlendFunction), (string)cboColorBlendFunction.SelectedItem);
		}
	}
}
