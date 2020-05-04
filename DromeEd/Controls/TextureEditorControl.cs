using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DromeEd.Controls
{
    public class TextureEditorControl : UserControl
    {
        private ToolStrip ToolStrip;
        private ToolStripButton ExportButton;
        private PictureBox PreviewPictureBox;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel TextureInfoLabel;
        private Drome.Texture _texture = null;
        public Drome.Texture Texture
        {
            get
            {
                return _texture;
            }
            set
            {
                PreviewPictureBox.Image = null;
                PreviewBitmap?.Dispose();
                PreviewBitmap = null;
                _texture = value;

                if (Texture != null)
                {
                    // Generate preview texture
#if !DEBUG
                    try
                    {
#endif
                        PreviewBitmap = GenerateTexturePreview(Texture);
                        PreviewPictureBox.Image = PreviewBitmap;
#if !DEBUG
                }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        System.Diagnostics.Debugger.Break();
                    }
#endif
                    TextureInfoLabel.Text = Texture.ToString();
                    TextureInfoLabel.ToolTipText = Texture.ToString();
                }
            }
        }

        public System.Drawing.Bitmap PreviewBitmap { get; private set; } = null;

        public TextureEditorControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            ToolStrip.Renderer = new ToolstripRenderer();
        }

        private void InitializeComponent()
        {
            this.ToolStrip = new System.Windows.Forms.ToolStrip();
            this.ExportButton = new System.Windows.Forms.ToolStripButton();
            this.PreviewPictureBox = new System.Windows.Forms.PictureBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.TextureInfoLabel = new System.Windows.Forms.ToolStripLabel();
            this.ToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ToolStrip
            // 
            this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExportButton,
            this.toolStripSeparator1,
            this.TextureInfoLabel});
            this.ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.ToolStrip.Size = new System.Drawing.Size(522, 25);
            this.ToolStrip.TabIndex = 1;
            this.ToolStrip.Text = "toolStrip1";
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
            // PreviewPictureBox
            // 
            this.PreviewPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PreviewPictureBox.Location = new System.Drawing.Point(0, 25);
            this.PreviewPictureBox.Name = "PreviewPictureBox";
            this.PreviewPictureBox.Size = new System.Drawing.Size(522, 477);
            this.PreviewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PreviewPictureBox.TabIndex = 2;
            this.PreviewPictureBox.TabStop = false;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // TextureInfoLabel
            // 
            this.TextureInfoLabel.Name = "TextureInfoLabel";
            this.TextureInfoLabel.Size = new System.Drawing.Size(86, 22);
            this.TextureInfoLabel.Text = "toolStripLabel1";
            // 
            // TextureEditorControl
            // 
            this.Controls.Add(this.PreviewPictureBox);
            this.Controls.Add(this.ToolStrip);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Name = "TextureEditorControl";
            this.Size = new System.Drawing.Size(522, 502);
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Drawing.Bitmap GenerateTexturePreview(Drome.Texture texture)
        {
            System.Drawing.Bitmap result = new System.Drawing.Bitmap((int)texture.Width, (int)texture.Height);
            System.Drawing.Imaging.BitmapData data = result.LockBits(new System.Drawing.Rectangle(0, 0, (int)texture.Width, (int)texture.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Acquire pixel data and flip it
            byte[] rawPixelData = texture.DumpPixelData();
            byte[] flippedPixelData = new byte[(int)texture.Width * (int)texture.Height * 4];
            for (int row = 0; row < texture.Height; row++)
            {
                for (int col = 0; col < texture.Width; col++)
                {
                    int actualRow = (int)texture.Height - row - 1;
                    flippedPixelData[actualRow * texture.Width * 4 + col * 4] = rawPixelData[row * texture.Width * 4 + col * 4];
                    flippedPixelData[actualRow * texture.Width * 4 + col * 4 + 1] = rawPixelData[row * texture.Width * 4 + col * 4 + 1];
                    flippedPixelData[actualRow * texture.Width * 4 + col * 4 + 2] = rawPixelData[row * texture.Width * 4 + col * 4 + 2];
                    flippedPixelData[actualRow * texture.Width * 4 + col * 4 + 3] = rawPixelData[row * texture.Width * 4 + col * 4 + 3];
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(flippedPixelData, 0, data.Scan0, (int)texture.Width * (int)texture.Height * 4);
            result.UnlockBits(data);
            return result;
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Portable Network Graphic (*.png)|*.png|Truevision Targa Image (*.tga)|*.tga";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FilterIndex == 1)
                {
                    // PNG file: Save from preview
                    PreviewBitmap.Save(dialog.FileName);
                }
                else if (dialog.FilterIndex == 2)
                {
                    // TGA file: Save from raw texture
                    Texture.DumpTGA(dialog.FileName);
                }
            }
        }
    }
}
