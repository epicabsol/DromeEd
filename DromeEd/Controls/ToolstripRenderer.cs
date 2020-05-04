using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace DromeEd.Controls
{
    public class ToolstripRenderer : ToolStripProfessionalRenderer
    {
        public ToolstripRenderer() : base(new CT())
        {

        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            base.OnRenderToolStripBackground(e);
            e.Graphics.Clear(e.ToolStrip.BackColor);
            Pen border = new Pen(Theme.BorderColor);
            e.Graphics.DrawLine(border, 0, e.ToolStrip.Height - 1, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            border.Dispose();
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            //base.OnRenderToolStripBorder(e);
        }

        protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
        {
            //base.OnRenderGrip(e);
        }

        protected override void OnRenderToolStripContentPanelBackground(ToolStripContentPanelRenderEventArgs e)
        {
            //base.OnRenderToolStripContentPanelBackground(e);
        }

        private class CT : ProfessionalColorTable
        {
            public override Color ButtonSelectedHighlight => Color.Yellow;// Theme.ApplicationColor;
            public override Color ToolStripBorder => Theme.BackgroundColor;
            public override Color ToolStripGradientBegin => Theme.BackgroundColor;
            public override Color ToolStripGradientEnd => Theme.BackgroundColor;
            public override Color ButtonPressedHighlight => Theme.ApplicationColor;
            public override Color ButtonCheckedHighlight => Theme.ApplicationColor;
            public override Color MenuItemSelected => Theme.ApplicationColor;
            public override Color ButtonSelectedGradientBegin => Theme.BorderColor;
            public override Color ButtonSelectedGradientMiddle => Theme.BorderColor;
            public override Color ButtonSelectedGradientEnd => Theme.BorderColor;
            public override Color ButtonSelectedBorder => Theme.BorderColor;
            public override Color ButtonPressedBorder => Theme.BorderColor;// Theme.ApplicationColor;
            public override Color ButtonPressedHighlightBorder => Theme.ApplicationColor;
            public override Color ButtonPressedGradientBegin => Theme.ApplicationColor;
            public override Color ButtonPressedGradientMiddle => Theme.ApplicationColor;
            public override Color ButtonPressedGradientEnd => Theme.ApplicationColor;
            public override Color MenuBorder => Theme.ApplicationColor;
            public override Color MenuItemBorder => Theme.ApplicationColor;
            public override Color MenuItemPressedGradientBegin => Theme.ApplicationColor;
            public override Color MenuItemPressedGradientMiddle => Theme.ApplicationColor;
            public override Color MenuItemPressedGradientEnd => Theme.ApplicationColor;
            public override Color MenuItemSelectedGradientBegin => Theme.BorderColor;
            public override Color MenuItemSelectedGradientEnd => Theme.BorderColor;
            public override Color MenuStripGradientBegin => Color.Magenta;
            public override Color SeparatorLight => Theme.BorderColor;
            public override Color SeparatorDark => Theme.BackgroundColor;
            public override Color ImageMarginGradientBegin => Theme.BackgroundColor;
            public override Color ImageMarginRevealedGradientBegin => Theme.BackgroundColor;
        }
    }
}
