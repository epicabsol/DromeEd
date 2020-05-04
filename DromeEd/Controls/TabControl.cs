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
    public partial class TabControl : UserControl
    {
        public TabControl()
        {
            InitializeComponent();
            tabStrip1.TabAdded += TabAdded;
            tabStrip1.TabRemoved += TabRemoved;
            tabStrip1.TabShown += TabShown;
            tabStrip1.TabHidden += TabHidden;
        }

        public new Image BackgroundImage { get => ContentPanel.BackgroundImage; set => ContentPanel.BackgroundImage = value; }

        public IEnumerable<TabItem> Tabs { get => tabStrip1.Tabs; }

        public FocusZone FocusZone { get => tabStrip1.FocusZone; }

        public void AddTab(TabItem item)
        {
            tabStrip1.AddTab(item);
        }

        public void RemoveTab(TabItem item)
        {
            tabStrip1.CloseTab(item);
        }

        private void TabAdded(object sender, TabItem item)
        {
            if (item.Content != null)
            {
                ContentPanel.Controls.Add(item.Content);
                item.Content.Visible = false;
            }
        }

        private void TabRemoved(object sender, TabItem item)
        {
            if (item.Content != null)
                ContentPanel.Controls.Remove(item.Content);
        }

        private void TabShown(object sender, TabItem item)
        {
            if (item.Content != null)
                item.Content.Visible = true;
        }

        private void TabHidden(object sender, TabItem item)
        {
            if (item.Content != null)
                item.Content.Visible = false;
        }

        private void ContentPanel_Paint(object sender, PaintEventArgs e)
        {
            Pen border = new Pen(Theme.BorderColor);
            e.Graphics.DrawLine(border, 0, 0, 0, ContentPanel.Height - 1);
            e.Graphics.DrawLine(border, 0, ContentPanel.Height - 1, ContentPanel.Width - 1, ContentPanel.Height - 1);
            e.Graphics.DrawLine(border, ContentPanel.Width - 1, 0, ContentPanel.Width - 1, ContentPanel.Height - 1);
            border.Dispose();
        }

        private void ContentPanel_Resize(object sender, EventArgs e)
        {
            ContentPanel.Invalidate();
        }
    }
}
