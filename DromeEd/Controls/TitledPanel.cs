using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DromeEd.Controls
{
    class TitledPanel : Panel
    {
        public FocusZone FocusZone = new FocusZone();

        public string Caption { get; set; }

        public TitledPanel()
        {
            Padding = new Padding(1, 25, 1, 1);
            FocusZone.Focused += (sender, e) => Invalidate(true);
            FocusZone.Unfocused += (sender, e) => Invalidate(true);
            FocusZone.Bind(this);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            SolidBrush backBrush = new SolidBrush(BackColor);
            SolidBrush accentBrush = new SolidBrush(Theme.ApplicationColor);
            SolidBrush textBrush = new SolidBrush(ForeColor);

            e.Graphics.Clear(Theme.BorderColor);
            e.Graphics.FillRectangle(backBrush, 1, 1, Width - 2, Height - 2);
            if (FocusZone.IsFocused)
                e.Graphics.FillRectangle(accentBrush, 1, 1, Width - 2, Padding.Top - 1);
            e.Graphics.DrawString(Caption, Font, textBrush, 3, 2);

            backBrush.Dispose();
            accentBrush.Dispose();
            textBrush.Dispose();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            FocusZone.Focus();
        }
    }
}
