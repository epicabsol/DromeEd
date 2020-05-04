namespace DromeEd.Controls
{
    partial class ModelEditorControl
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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.LODLabel = new System.Windows.Forms.ToolStripLabel();
            this.LODDropDown = new System.Windows.Forms.ToolStripComboBox();
            this.d3DRenderer1 = new DromeEd.Controls.D3DRenderer();
            this.styledTreeView1 = new DromeEd.Controls.StyledTreeView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.ExportButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.renderOriginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderWireframeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderSolidToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExportButton,
            this.toolStripSeparator1,
            this.LODLabel,
            this.LODDropDown,
            this.toolStripDropDownButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(504, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // LODLabel
            // 
            this.LODLabel.Name = "LODLabel";
            this.LODLabel.Size = new System.Drawing.Size(62, 22);
            this.LODLabel.Text = "Show LOD";
            // 
            // LODDropDown
            // 
            this.LODDropDown.AutoSize = false;
            this.LODDropDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.LODDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LODDropDown.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.LODDropDown.ForeColor = System.Drawing.Color.White;
            this.LODDropDown.Name = "LODDropDown";
            this.LODDropDown.Size = new System.Drawing.Size(175, 23);
            this.LODDropDown.SelectedIndexChanged += new System.EventHandler(this.LODDropDown_SelectedIndexChanged);
            // 
            // d3DRenderer1
            // 
            this.d3DRenderer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.d3DRenderer1.Location = new System.Drawing.Point(186, 25);
            this.d3DRenderer1.Name = "d3DRenderer1";
            this.d3DRenderer1.Screen = null;
            this.d3DRenderer1.Size = new System.Drawing.Size(318, 453);
            this.d3DRenderer1.TabIndex = 1;
            // 
            // styledTreeView1
            // 
            this.styledTreeView1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.styledTreeView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.styledTreeView1.Dock = System.Windows.Forms.DockStyle.Left;
            this.styledTreeView1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
            this.styledTreeView1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.styledTreeView1.ForeColor = System.Drawing.Color.White;
            this.styledTreeView1.Location = new System.Drawing.Point(0, 25);
            this.styledTreeView1.Name = "styledTreeView1";
            this.styledTreeView1.ParentFocus = null;
            this.styledTreeView1.Size = new System.Drawing.Size(186, 453);
            this.styledTreeView1.TabIndex = 2;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(186, 25);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 453);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // ExportButton
            // 
            this.ExportButton.Image = global::DromeEd.Properties.Resources.ButtonExport;
            this.ExportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(69, 22);
            this.ExportButton.Text = "Export...";
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renderOriginsToolStripMenuItem,
            this.renderWireframeToolStripMenuItem,
            this.renderSolidToolStripMenuItem});
            this.toolStripDropDownButton1.Image = global::DromeEd.Properties.Resources.ButtonAnchor;
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(29, 22);
            this.toolStripDropDownButton1.Text = "toolStripDropDownButton1";
            // 
            // renderOriginsToolStripMenuItem
            // 
            this.renderOriginsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.renderOriginsToolStripMenuItem.CheckOnClick = true;
            this.renderOriginsToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.renderOriginsToolStripMenuItem.Name = "renderOriginsToolStripMenuItem";
            this.renderOriginsToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.renderOriginsToolStripMenuItem.Text = "Render Origins";
            // 
            // renderWireframeToolStripMenuItem
            // 
            this.renderWireframeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.renderWireframeToolStripMenuItem.CheckOnClick = true;
            this.renderWireframeToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.renderWireframeToolStripMenuItem.Name = "renderWireframeToolStripMenuItem";
            this.renderWireframeToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.renderWireframeToolStripMenuItem.Text = "Render Wireframe";
            // 
            // renderSolidToolStripMenuItem
            // 
            this.renderSolidToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.renderSolidToolStripMenuItem.CheckOnClick = true;
            this.renderSolidToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.renderSolidToolStripMenuItem.Name = "renderSolidToolStripMenuItem";
            this.renderSolidToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.renderSolidToolStripMenuItem.Text = "Render Solid";
            // 
            // ModelEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.d3DRenderer1);
            this.Controls.Add(this.styledTreeView1);
            this.Controls.Add(this.toolStrip1);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "ModelEditorControl";
            this.Size = new System.Drawing.Size(504, 478);
            this.Load += new System.EventHandler(this.ModelEditorControl_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private D3DRenderer d3DRenderer1;
        private System.Windows.Forms.ToolStripButton ExportButton;
        private System.Windows.Forms.Splitter splitter1;
        public StyledTreeView styledTreeView1;
        private System.Windows.Forms.ToolStripLabel LODLabel;
        private System.Windows.Forms.ToolStripComboBox LODDropDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem renderOriginsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renderWireframeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renderSolidToolStripMenuItem;
    }
}
