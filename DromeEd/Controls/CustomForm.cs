using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DromeEd.Controls
{
    public class CaptionButton
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public bool MouseOver;
        public bool MouseDown;
        public Bitmap Image;
        public int HitTestCode = 0x02; // HT_NOWHERE

        public CaptionButton(int x, int y, int width, int height, Bitmap image, int hitTestCode = 0x02)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Image = image;
            HitTestCode = hitTestCode;
        }

        public event EventHandler Clicked = null;

        public void OnClicked()
        {
            Clicked?.Invoke(this, new EventArgs());
        }
    }

    public class CustomForm : Form
    {
        #region "DWM Interop"
        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("dwmapi")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi")]
        static extern bool DwmDefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref IntPtr plResult);
        #endregion

        #region "Win32 Interop"
        private const int WM_NCCALCSIZE = 0x0083;
        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_GETMINMAXINFO = 0x0024;

        protected const int HT_NOWHERE = 0x00;
        protected const int HT_CLIENT = 0x01;
        protected const int HT_CAPTION = 0x02;
        protected const int HT_CLOSE = 0x14;
        protected const int HT_MINBUTTON = 0x08;
        protected const int HT_MAXBUTTON = 0x09;
        protected const int HT_LEFT = 0x0A;
        protected const int HT_RIGHT = 0x0B;
        protected const int HT_TOP = 0x0C;
        protected const int HT_TOPLEFT = 0x0D;
        protected const int HT_TOPRIGHT = 0x0E;
        protected const int HT_BOTTOM = 0x0F;
        protected const int HT_BOTTOMLEFT = 0x10;
        protected const int HT_BOTTOMRIGHT = 0x11;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct NCCALCSIZE_PARAMS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public RECT[] rgrc;
            public WINDOWPOS lppos;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        /// <summary>
        /// Contains information about the placement of a window on the screen.
        /// </summary>
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            /// <summary>
            /// The length of the structure, in bytes. Before calling the GetWindowPlacement or SetWindowPlacement functions, set this member to sizeof(WINDOWPLACEMENT).
            /// <para>
            /// GetWindowPlacement and SetWindowPlacement fail if this member is not set correctly.
            /// </para>
            /// </summary>
            public int Length;

            /// <summary>
            /// Specifies flags that control the position of the minimized window and the method by which the window is restored.
            /// </summary>
            public int Flags;

            /// <summary>
            /// The current show state of the window.
            /// </summary>
            public ShowWindowCommands ShowCmd;

            /// <summary>
            /// The coordinates of the window's upper-left corner when the window is minimized.
            /// </summary>
            public POINT MinPosition;

            /// <summary>
            /// The coordinates of the window's upper-left corner when the window is maximized.
            /// </summary>
            public POINT MaxPosition;

            /// <summary>
            /// The window's coordinates when the window is in the restored position.
            /// </summary>
            public RECT NormalPosition;

            /// <summary>
            /// Gets the default (empty) value.
            /// </summary>
            public static WINDOWPLACEMENT Default
            {
                get
                {
                    WINDOWPLACEMENT result = new WINDOWPLACEMENT();
                    result.Length = Marshal.SizeOf(result);
                    return result;
                }
            }
        }

        enum ShowWindowCommands
        {
            /// <summary>
            /// Hides the window and activates another window.
            /// </summary>
            Hide = 0,
            /// <summary>
            /// Activates and displays a window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the window 
            /// for the first time.
            /// </summary>
            Normal = 1,
            /// <summary>
            /// Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,
            /// <summary>
            /// Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?
                          /// <summary>
                          /// Activates the window and displays it as a maximized window.
                          /// </summary>       
            ShowMaximized = 3,
            /// <summary>
            /// Displays a window in its most recent size and position. This value 
            /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
            /// the window is not activated.
            /// </summary>
            ShowNoActivate = 4,
            /// <summary>
            /// Activates the window and displays it in its current size and position. 
            /// </summary>
            Show = 5,
            /// <summary>
            /// Minimizes the specified window and activates the next top-level 
            /// window in the Z order.
            /// </summary>
            Minimize = 6,
            /// <summary>
            /// Displays the window as a minimized window. This value is similar to
            /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowMinNoActive = 7,
            /// <summary>
            /// Displays the window in its current size and position. This value is 
            /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowNA = 8,
            /// <summary>
            /// Activates and displays the window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position. 
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,
            /// <summary>
            /// Sets the show state based on the SW_* value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,
            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
            /// that owns the window is not responding. This flag should only be 
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }
        #endregion

        protected List<CaptionButton> CaptionButtons = new List<CaptionButton>();

        public bool SizeBorder = true;

        protected override void WndProc(ref Message m)
        {
            if (DesignMode)
            {
                base.WndProc(ref m);
                return;
            }
            if (m.Msg == WM_NCCALCSIZE)
            {
                if (m.WParam.ToInt32() == 1)
                {
                    MARGINS borderless = new MARGINS() { cxLeftWidth = 1, cxRightWidth = 1, cyBottomHeight = 1, cyTopHeight = 1 };
                    DwmExtendFrameIntoClientArea(Handle, ref borderless);

                    WINDOWPLACEMENT pl = new WINDOWPLACEMENT();
                    GetWindowPlacement(m.HWnd, ref pl);

                    if (pl.ShowCmd == ShowWindowCommands.Maximize)
                    {
                        NCCALCSIZE_PARAMS p = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(m.LParam);

                        p.rgrc[0].Left += 7;
                        p.rgrc[0].Right -= 8;
                        p.rgrc[0].Top += 7;
                        p.rgrc[0].Bottom -= 8;

                        Marshal.StructureToPtr(p, m.LParam, true);
                    }
                }

                m.Result = IntPtr.Zero;
                return;
            }
            else if (m.Msg == WM_NCHITTEST)
            {
                Point mousePos = PointToClient(new Point((m.LParam.ToInt32() & 0xFFFF), (m.LParam.ToInt32() >> 16)));
                foreach (CaptionButton b in CaptionButtons)
                {
                    if (mousePos.X >= b.X && mousePos.X < b.X + b.Width && mousePos.Y >= b.Y && mousePos.Y < b.Y + b.Height)
                    {
                        m.Result = new IntPtr(b.HitTestCode);
                        return;
                    }
                }
                m.Result = new IntPtr(mousePos.X >= 0 && mousePos.X < Width && mousePos.Y >= 0 && mousePos.Y < Height ? HT_CAPTION : HT_NOWHERE);
                if (SizeBorder && WindowState == FormWindowState.Normal)
                {
                    int res = m.Result.ToInt32();
                    int col = 0;
                    if (mousePos.X >= ClientSize.Width - 5)
                        col = 2;
                    else if (mousePos.X > 5)
                        col = 1;

                    int row = 0;
                    if (mousePos.Y >= ClientSize.Height - 5)
                        row = 2;
                    else if (mousePos.Y > 5)
                        row = 1;

                    if (col == 0)
                    {
                        if (row == 0)
                            res = HT_TOPLEFT;
                        if (row == 1)
                            res = HT_LEFT;
                        if (row == 2)
                            res = HT_BOTTOMLEFT;
                    }
                    if (col == 1)
                    {
                        if (row == 0)
                            res = HT_TOP;
                        if (row == 2)
                            res = HT_BOTTOM;
                    }
                    if (col == 2)
                    {
                        if (row == 0)
                            res = HT_TOPRIGHT;
                        if (row == 1)
                            res = HT_RIGHT;
                        if (row == 2)
                            res = HT_BOTTOMRIGHT;
                    }

                    m.Result = new IntPtr(res);
                }
                return;
            }
            
            base.WndProc(ref m);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.Sizable;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.BackgroundColor);
            Pen pen = new Pen(Theme.BorderColor);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            pen.Dispose();

            SolidBrush hilightBrush = new SolidBrush(Theme.BorderColor);
            SolidBrush darkenBrush = new SolidBrush(Color.FromArgb(70, 0, 0, 0));

            foreach (CaptionButton b in CaptionButtons)
            {
                if (b.MouseDown)
                    e.Graphics.FillRectangle(darkenBrush, b.X, b.Y, b.Width, b.Height);
                else if (b.MouseOver)
                    e.Graphics.FillRectangle(hilightBrush, b.X, b.Y, b.Width, b.Height);

                if (b.Image != null)
                {
                    e.Graphics.DrawImage(b.Image, b.X + b.Width / 2 - b.Image.Width / 2, b.Y + b.Height / 2 - b.Image.Height / 2, b.Image.Width, b.Image.Height);
                }
            }

            hilightBrush.Dispose();
            darkenBrush.Dispose();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            foreach (CaptionButton b in CaptionButtons)
            {
                if (e.X >= b.X && e.X < b.X + b.Width && e.Y >= b.Y && e.Y < b.Y + b.Height)
                {
                    b.MouseOver = true;
                }
                else
                {
                    b.MouseOver = false;
                    b.MouseDown = false;
                }
            }
            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            foreach (CaptionButton b in CaptionButtons)
            {
                b.MouseOver = false;
                b.MouseDown = false;
            }
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            foreach (CaptionButton b in CaptionButtons)
            {
                if (e.X >= b.X && e.X < b.X + b.Width && e.Y >= b.Y && e.Y < b.Y + b.Height)
                {
                    b.MouseDown = true;
                }
            }
            base.OnMouseDown(e);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            foreach (CaptionButton b in CaptionButtons)
            {
                if (e.X >= b.X && e.X < b.X + b.Width && e.Y >= b.Y && e.Y < b.Y + b.Height)
                {
                    b.OnClicked();
                }
                b.MouseDown = false;
            }
            base.OnMouseUp(e);
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
            Update();
        }
    }
}
