using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace DromeEd.Controls
{
    public class TabItem
    {
        private const float Padding = 3;

        public Font Font = new Font("Segoe UI", 11);
        private string _title = "New Tab";
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                Width.TargetValue = TextRenderer.MeasureText(Title, Font).Width + Padding * 3 + 16;
            }
        }
        public System.Drawing.Bitmap Icon;
        private bool _selected;
        public bool Selected
        {
            get
            {
                return _selected;
            }
        }
        public event EventHandler Created;
        public event EventHandler Destroyed;
        public event EventHandler Shown;
        public event EventHandler Hidden;
        public Control Content;

        public SlideValue Width = new SlideValue(0, 0);
        public SlideValue Left = new SlideValue(0, 0);

        public bool MouseDown = false;
        public bool MouseOver = false;

        public bool CloseMouseHover = false;

        public TabItem(string title)
        {
            Title = title;
            //Width.CurrentValue = Width.TargetValue;
        }

        public void Create()
        {
            Created?.Invoke(this, new EventArgs());
        }

        public void Destroy()
        {
            Destroyed?.Invoke(this, new EventArgs());
        }

        public void Show()
        {
            _selected = true;
            Shown?.Invoke(this, new EventArgs());
        }

        public void Hide()
        {
            _selected = false;
            Hidden?.Invoke(this, new EventArgs());
        }
    }

    public class TabStrip : Control
    {
        public FocusZone FocusZone { get; private set; } = new FocusZone();
        private Timer UpdateTimer;
        private List<TabItem> _tabs = new List<TabItem>();

        public IEnumerable<TabItem> Tabs { get => _tabs; }

        public event EventHandler<TabItem> TabAdded;
        public event EventHandler<TabItem> TabRemoved;
        public event EventHandler<TabItem> TabShown;
        public event EventHandler<TabItem> TabHidden;

        public TabStrip()
        {
            DoubleBuffered = true;
            UpdateTimer = new Timer();
            UpdateTimer.Interval = 10;
            UpdateTimer.Tick += (sender, e) =>
            {
                float left = 0;
                foreach (TabItem t in Tabs)
                {
                    if (!t.MouseDown)
                    {
                        t.Left.TargetValue = left;
                    }
                    t.Width.Update();
                    t.Left.Update();
                    left += t.Width.CurrentValue;
                }

                Invalidate();
            };
            UpdateTimer.Start();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            FocusZone.Bind(Parent);
        }

        public void AddTab(TabItem item)
        {
            item.Created += TabItem_Create;
            item.Destroyed += TabItem_Destroy;
            item.Shown += TabItem_Show;
            item.Hidden += TabItem_Hide;

            _tabs.Add(item);
            item.Create();
            if (_tabs.Count == 1)
                item.Show();

            float offset = 0;
            for (int i = 0; i < _tabs.Count - 1; i++)
            {
                offset += _tabs[i].Width.CurrentValue;
            }
            item.Left.TargetValue = offset;
            item.Left.CurrentValue = item.Left.TargetValue;
        }

        public void CloseTab(TabItem item)
        {
            item.Destroy();
        }

        private void TabItem_Create(object sender, EventArgs e)
        {
            TabAdded?.Invoke(sender, sender as TabItem);
        }

        private void TabItem_Destroy(object sender, EventArgs e)
        {
            int index = _tabs.IndexOf(sender as TabItem);
            TabRemoved?.Invoke(sender, sender as TabItem);
            _tabs.Remove(sender as TabItem);
            if (_tabs.Count > 0)
            {
                if (index > 0)
                {
                    _tabs[index - 1].Show();
                }
                else
                {
                    _tabs[index].Show();
                }
            }
        }

        private void TabItem_Show(object sender, EventArgs e)
        {
            foreach (TabItem t in Tabs)
                if (!t.Equals(sender) && t.Selected)
                    t.Hide();

            TabShown?.Invoke(sender, sender as TabItem);
        }

        private void TabItem_Hide(object sender, EventArgs e)
        {
            TabHidden?.Invoke(sender, sender as TabItem);
        }

        public TabItem HitTest(int x, int y)
        {
            for (int i = _tabs.Count - 1; i >= 0; i--)
            {
                if (x >= _tabs[i].Left.CurrentValue && x < _tabs[i].Left.CurrentValue + _tabs[i].Width.CurrentValue && (_tabs[i].MouseDown || !IsMouseDown))
                    return _tabs[i];
            }
            return null;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);

            SolidBrush colorBrush = new SolidBrush(FocusZone.IsFocused ? Theme.ApplicationColor : Theme.BorderColor);
            SolidBrush lightBrush = new SolidBrush(Theme.ApplicationColorLight);
            SolidBrush hilightBrush = new SolidBrush(Color.FromArgb(50, 255, 255, 255));

            foreach (TabItem item in Tabs)
            {
                if (item.Selected)
                {
                    e.Graphics.FillRectangle(colorBrush, item.Left.CurrentValue, 0, item.Width.CurrentValue, Height - 2);
                }
                else if (item.MouseOver)
                {
                    e.Graphics.FillRectangle(lightBrush, item.Left.CurrentValue, 0, item.Width.CurrentValue, Height - 2);
                }
                if (item.Selected || item.MouseOver)
                {
                    e.Graphics.DrawImage(Properties.Resources.CloseIcon, item.Left.CurrentValue + item.Width.CurrentValue - 4 - 16, 4, 16, 16);
                    if (item.CloseMouseHover)
                    {
                        e.Graphics.FillRectangle(hilightBrush, item.Left.CurrentValue + item.Width.CurrentValue - 8 - 16, 0, 24, 24);
                    }
                }
                TextRenderer.DrawText(e.Graphics, item.Title, item.Font, new Point((int)item.Left.CurrentValue, 2), Color.White);
            }
            if (IsMouseDown)
            {
                foreach (TabItem item in Tabs)
                {
                    if (item.MouseDown)
                    {
                        e.Graphics.FillRectangle(item.Selected ? colorBrush : lightBrush, item.Left.CurrentValue, 0, item.Width.CurrentValue, Height - 2);
                        TextRenderer.DrawText(e.Graphics, item.Title, item.Font, new Point((int)item.Left.CurrentValue, 2), Color.White);
                        e.Graphics.DrawImage(Properties.Resources.CloseIcon, item.Left.CurrentValue + item.Width.CurrentValue - 4 - 16, 4, 16, 16);
                        if (item.CloseMouseHover)
                        {
                            e.Graphics.FillRectangle(hilightBrush, item.Left.CurrentValue + item.Width.CurrentValue - 8 - 16, 0, 24, 24);
                        }
                    }
                }
            }

            e.Graphics.FillRectangle(colorBrush, 0, Height - 2, Width, 2);
            colorBrush.Dispose();
            lightBrush.Dispose();
            hilightBrush.Dispose();
        }

        private bool IsMouseDown = false;
        private Point LastMouse;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            TabItem i = HitTest(e.X, e.Y);
            foreach (TabItem t in Tabs)
            {
                t.MouseOver = false;
                t.CloseMouseHover = false;
            }
            if (i != null)
            {
                i.MouseOver = true;
                if (e.X >= i.Left.CurrentValue + i.Width.CurrentValue - 8 - 16 && e.X < i.Left.CurrentValue + i.Width.CurrentValue &&
                    e.Y >= 0 && e.Y < 8 + 16)
                {
                    i.CloseMouseHover = true;
                }
            }
            base.OnMouseMove(e);
            Invalidate();
            if (IsMouseDown)
            {
                for (int j = 0; j < _tabs.Count; j++)
                {
                    TabItem t = _tabs[j];
                    if (t.MouseDown)
                    {
                        t.Left.TargetValue += e.X - LastMouse.X;
                        if (t.Left.TargetValue < 0)
                            t.Left.TargetValue = 0;
                        //t.Left.CurrentValue = t.Left.TargetValue;
                        int index = _tabs.IndexOf(t);
                        // Check for move left
                        if (index > 0 && t.Left.TargetValue < _tabs[index - 1].Left.TargetValue + _tabs[index - 1].Width.TargetValue * 0.333f)
                        {
                            // Reorder one left
                            _tabs.Remove(t);
                            _tabs.Insert(index - 1, t);
                            j--;
                        }
                        // Check for move right
                        else if (index < _tabs.Count - 1 && t.Left.TargetValue + t.Width.TargetValue > _tabs[index + 1].Left.TargetValue + _tabs[index + 1].Width.TargetValue * 0.666f)
                        {
                            _tabs.Remove(t);
                            _tabs.Insert(index + 1, t);
                            j++;
                        }
                    }
                }
            }
            LastMouse = e.Location;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            FocusZone.Focus();
            TabItem i = HitTest(e.X, e.Y);
            if (i != null)
            {
                if (!i.Selected)
                    i.Show();
                if (e.Button == MouseButtons.Left)
                {
                    i.MouseDown = true;
                    IsMouseDown = true;
                    Capture = true;
                }
                else if (e.Button == MouseButtons.Middle)
                {
                    i.Destroy();
                }
            }
            base.OnMouseDown(e);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                TabItem t = _tabs[i];
                t.MouseDown = false;
                if (e.X >= t.Left.CurrentValue + t.Width.CurrentValue - 8 - 16 && e.X < t.Left.CurrentValue + t.Width.CurrentValue &&
                    e.Y >= 0 && e.Y < 8 + 16)
                {
                    t.Destroy();
                    i--;
                }
            }
            IsMouseDown = false;
            base.OnMouseUp(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            foreach (TabItem i in Tabs)
            {
                i.MouseOver = false;
                i.CloseMouseHover = false;
            }
            base.OnMouseLeave(e);
            Invalidate();
        }
    }
}
