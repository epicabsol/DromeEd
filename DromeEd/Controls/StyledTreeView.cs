using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DromeEd.Controls
{
    public class StyledTreeView : TreeView
    {
        public FocusZone ParentFocus { get; set; }

        public StyledTreeView()
        {
            DrawMode = TreeViewDrawMode.OwnerDrawAll;
            BackColor = Theme.WellColor;
            ForeColor = Theme.TextColor;
            BorderStyle = BorderStyle.None;
            Font = new Font("Segoe UI", 9.75f);
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            //base.OnDrawNode(e);
            if (e.Bounds.Y != e.Node.Bounds.Y)
                return;
            SolidBrush hilightBrush = new SolidBrush((ParentFocus?.IsFocused ?? false) ? Theme.ApplicationColor : Theme.BorderColor);
            SolidBrush backBrush = new SolidBrush(BackColor);
            SolidBrush textBrush = new SolidBrush(ForeColor);
            e.Graphics.FillRectangle(backBrush, e.Bounds);
            if (e.State.HasFlag(TreeNodeStates.Selected))
                e.Graphics.FillRectangle(hilightBrush, e.Bounds);
            //e.Graphics.DrawString(e.Node.Text, treeView1.Font, textBrush, e.Node.Bounds.Left, e.Node.Bounds.Top + 0);
            TextRenderer.DrawText(e.Graphics, e.Node.Text, Font, new Point(e.Node.Bounds.Left, e.Node.Bounds.Top), ForeColor);
            if (e.Node.Nodes.Count > 0)
                e.Graphics.DrawImage(e.Node.IsExpanded ? Properties.Resources.TreeArrowOpen : Properties.Resources.TreeArrowClosed, e.Node.Bounds.Left - 13, e.Node.Bounds.Top + 6, 8, 8);
            hilightBrush.Dispose();
            backBrush.Dispose();
            textBrush.Dispose();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            TreeNode node = GetNodeAt(0, e.Y);
            SelectedNode = node;
            Invalidate();
        }
    }
}
