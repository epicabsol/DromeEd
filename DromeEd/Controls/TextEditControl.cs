using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DromeEd.Controls
{
    public partial class TextEditControl : UserControl
    {
        public new string Text { get => textBox.Text; set => textBox.Text = value; }

        public event EventHandler SaveRequested;

        public TextEditControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            toolStrip1.Renderer = new ToolstripRenderer();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SaveRequested?.Invoke(this, new EventArgs());
        }
    }
}
