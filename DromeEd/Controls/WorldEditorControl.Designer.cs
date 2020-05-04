namespace DromeEd.Controls
{
    partial class WorldEditorControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorldEditorControl));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.Renderer = new DromeEd.Controls.D3DRenderer();
            this.SceneTreeView = new System.Windows.Forms.TreeView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(856, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // Renderer
            // 
            this.Renderer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Renderer.Location = new System.Drawing.Point(231, 25);
            this.Renderer.Name = "Renderer";
            this.Renderer.Screen = null;
            this.Renderer.Size = new System.Drawing.Size(625, 626);
            this.Renderer.TabIndex = 1;
            // 
            // SceneTreeView
            // 
            this.SceneTreeView.Dock = System.Windows.Forms.DockStyle.Left;
            this.SceneTreeView.Location = new System.Drawing.Point(0, 25);
            this.SceneTreeView.Name = "SceneTreeView";
            this.SceneTreeView.Size = new System.Drawing.Size(231, 626);
            this.SceneTreeView.TabIndex = 2;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(231, 25);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 626);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            // 
            // WorldEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.Renderer);
            this.Controls.Add(this.SceneTreeView);
            this.Controls.Add(this.toolStrip1);
            this.Name = "WorldEditorControl";
            this.Size = new System.Drawing.Size(856, 651);
            this.Load += new System.EventHandler(this.WorldEditorControl_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private D3DRenderer Renderer;
        private System.Windows.Forms.TreeView SceneTreeView;
        private System.Windows.Forms.Splitter splitter1;
    }
}
