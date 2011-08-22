namespace Blending
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.cboColorSourceBlend = new System.Windows.Forms.ComboBox();
			this.cboColorDestinationBlend = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.cboColorBlendFunction = new System.Windows.Forms.ComboBox();
			this.blendedQuadsControl1 = new WinFormsGraphicsDevice.BlendedQuadsControl();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(14, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(98, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Color Source Blend";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(155, 13);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(117, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Color Destination Blend";
			// 
			// cboColorSourceBlend
			// 
			this.cboColorSourceBlend.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboColorSourceBlend.FormattingEnabled = true;
			this.cboColorSourceBlend.Items.AddRange(new object[] {
            "Zero",
            "One",
            "SourceColor",
            "InverseSourceColor",
            "SourceAlpha",
            "InverseSourceAlpha",
            "DestinationAlpha",
            "InverseDestinationAlpha",
            "DestinationColor",
            "InverseDestinationColor",
            "SourceAlphaSaturation",
            "BlendFactor",
            "InverseBlendFactor"});
			this.cboColorSourceBlend.Location = new System.Drawing.Point(17, 30);
			this.cboColorSourceBlend.Name = "cboColorSourceBlend";
			this.cboColorSourceBlend.Size = new System.Drawing.Size(121, 21);
			this.cboColorSourceBlend.TabIndex = 2;
			this.cboColorSourceBlend.SelectedIndexChanged += new System.EventHandler(this.cboColorSourceBlend_SelectedIndexChanged);
			// 
			// cboColorDestinationBlend
			// 
			this.cboColorDestinationBlend.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboColorDestinationBlend.FormattingEnabled = true;
			this.cboColorDestinationBlend.Items.AddRange(new object[] {
            "Zero",
            "One",
            "SourceColor",
            "InverseSourceColor",
            "SourceAlpha",
            "InverseSourceAlpha",
            "DestinationAlpha",
            "InverseDestinationAlpha",
            "DestinationColor",
            "InverseDestinationColor",
            "SourceAlphaSaturation",
            "BlendFactor",
            "InverseBlendFactor"});
			this.cboColorDestinationBlend.Location = new System.Drawing.Point(158, 30);
			this.cboColorDestinationBlend.Name = "cboColorDestinationBlend";
			this.cboColorDestinationBlend.Size = new System.Drawing.Size(121, 21);
			this.cboColorDestinationBlend.TabIndex = 3;
			this.cboColorDestinationBlend.SelectedIndexChanged += new System.EventHandler(this.cboColorDestinationBlend_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(295, 12);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(105, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Color Blend Function";
			// 
			// cboColorBlendFunction
			// 
			this.cboColorBlendFunction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboColorBlendFunction.FormattingEnabled = true;
			this.cboColorBlendFunction.Items.AddRange(new object[] {
            "Add",
            "Subtract",
            "ReverseSubtract",
            "Min",
            "Max"});
			this.cboColorBlendFunction.Location = new System.Drawing.Point(298, 30);
			this.cboColorBlendFunction.Name = "cboColorBlendFunction";
			this.cboColorBlendFunction.Size = new System.Drawing.Size(121, 21);
			this.cboColorBlendFunction.TabIndex = 5;
			this.cboColorBlendFunction.SelectedIndexChanged += new System.EventHandler(this.cboColorBlendFunction_SelectedIndexChanged);
			// 
			// blendedQuadsControl1
			// 
			this.blendedQuadsControl1.ColorBlendFunction = Microsoft.Xna.Framework.Graphics.BlendFunction.Add;
			this.blendedQuadsControl1.ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.One;
			this.blendedQuadsControl1.ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.One;
			this.blendedQuadsControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.blendedQuadsControl1.Location = new System.Drawing.Point(0, 57);
			this.blendedQuadsControl1.Name = "blendedQuadsControl1";
			this.blendedQuadsControl1.Size = new System.Drawing.Size(624, 385);
			this.blendedQuadsControl1.TabIndex = 6;
			this.blendedQuadsControl1.Text = "blendedQuadsControl1";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(624, 442);
			this.Controls.Add(this.cboColorBlendFunction);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.cboColorDestinationBlend);
			this.Controls.Add(this.cboColorSourceBlend);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.blendedQuadsControl1);
			this.Name = "MainForm";
			this.Text = "Blending Demo";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cboColorSourceBlend;
		private System.Windows.Forms.ComboBox cboColorDestinationBlend;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cboColorBlendFunction;
		private WinFormsGraphicsDevice.BlendedQuadsControl blendedQuadsControl1;
	}
}

