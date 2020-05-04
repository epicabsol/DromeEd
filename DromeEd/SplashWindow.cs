using DromeEd.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DromeEd
{
    public partial class SplashWindow : CustomForm
    {
        private string _statusText = "";
        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
                if (InvokeRequired)
                {
                    Invoke(new Action(() => Invalidate()));
                }
                else
                {
                    Invalidate();
                }
            }
        }

        private float _statusProgress = 0;
        public float StatusProgress
        {
            get
            {
                return _statusProgress;
            }
            set
            {
                _statusProgress = value;
                if (InvokeRequired)
                {
                    Invoke(new Action(() => Invalidate()));
                }
                else
                {
                    Invalidate();
                }
            }
        }

        public bool LoadSuccessful = false;

        public SplashWindow()
        {
            InitializeComponent();
            SizeBorder = false;
            MaximumSize = Size;
        }

        private Task LoadTask;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            SolidBrush backgroundBrush = new SolidBrush(Theme.BackgroundColor);
            SolidBrush foregroundBrush = new SolidBrush(Theme.ForegroundColor);
            SolidBrush appbrush = new SolidBrush(Theme.ApplicationColor);
            SolidBrush textBrush = new SolidBrush(Theme.TextColor);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillPolygon(foregroundBrush, new Point[] { new Point(125, 0), new Point(150, Height), new Point(Width, Height), new Point(Width, 0) });
            e.Graphics.DrawString("Drome", Theme.TitleFont, textBrush, new Point(10, 10));
            e.Graphics.DrawString("Ed", Theme.TitleFontBold, backgroundBrush, new Point(128, 11));
            e.Graphics.DrawString("v" + Application.ProductVersion, Theme.StatusFont, textBrush, new Point(17, 55));
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            e.Graphics.DrawString(StatusText, Theme.StatusFont, textBrush, new Point(17, Height - 30));
            if (StatusProgress > 0)
            {
                e.Graphics.FillRectangle(appbrush, 2, Height - 3, StatusProgress * (Width - 4), 2);
            }
            backgroundBrush.Dispose();
            foregroundBrush.Dispose();
            appbrush.Dispose();
            textBrush.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
            Invalidate();
        }

        protected override async void OnClosed(EventArgs e)
        {
            Program.Filesystem?.Cancel();
            if (LoadTask != null)
                await LoadTask;
            base.OnClosed(e);
        }

        private async void SplashWindow_Load(object sender, EventArgs e)
        {
            string gameFolder = Program.Config["Context"]["GameFolder"];
            bool loadUnpacked = Program.Config["Context"]["LoadUnpacked"].ToLowerInvariant() == "true";
            if (gameFolder == "")
            {
                if (loadUnpacked)
                {
                    FolderBrowserDialog dialog = new FolderBrowserDialog();
                    dialog.Description = "Browse for GameData directory";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        gameFolder = System.IO.Path.GetDirectoryName(dialog.SelectedPath);
                        Program.Config["Context"]["GameFolder"] = gameFolder;
                        Activate();
                    }
                    else
                    {
                        LoadSuccessful = false;
                        Close();
                        return;
                    }
                }
                else
                {
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.Title = "Browse for GameData.gtc";
                    dialog.Filter = "Saracen 2 Game Data (*.gtc)|*.gtc";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        gameFolder = System.IO.Path.GetDirectoryName(dialog.FileName);
                        Program.Config["Context"]["GameFolder"] = gameFolder;
                        Activate();
                    }
                    else
                    {
                        LoadSuccessful = false;
                        Close();
                        return;
                    }
                }
            }
            Drome.Context.Current = new Drome.Context(Drome.Context.NextGenGame.DromeRacers, gameFolder);

            Program.Filesystem = new ATD.VFS.Filesystem();
            Program.Filesystem.LogMessageEvent += (o, message) => { StatusText = message; };
            Program.Filesystem.ProgressChangedEvent += (o, progress) => { StatusProgress = progress; };
            LoadTask = Task.Run(() =>
            {
                try
                {
                    if (loadUnpacked)
                    {
                        Program.Filesystem.LoadRaw(gameFolder, 2);
                    }
                    else
                    {
                        Program.Filesystem.LoadArchive(Drome.Context.Current.GamePath);
                    }
                    LoadSuccessful = true;
                }
                catch (OperationCanceledException)
                {
                    
                }
            });
            await LoadTask;
            LoadTask = null;
            StatusText = "";
            StatusProgress = 0;
            Drome.ClassRegistry.Initialize();
            await Task.Delay(500);
            Close();
        }
    }
}
