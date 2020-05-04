using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ATD.VFS;
using DromeEd.Controls;

namespace DromeEd
{
    public partial class MainWindow : CustomForm
    {
        private class FileTreeEntry
        {
            public FileEntry FilesystemEntry;
            public TreeNode TreeNode;
            public string Filename { get => FilesystemEntry.Filename; }
            private bool _isFolder = false;
            public bool IsFolder
            {
                set
                {
                    _isFolder = value;
                }
                get
                {
                    return _isFolder;
                }
            }

            public FileTreeEntry(FileEntry filesystemEntry, TreeNode treeNode, bool isFolder)
            {
                FilesystemEntry = filesystemEntry;
                TreeNode = treeNode;
                IsFolder = isFolder;
                //if (!isFolder)
                treeNode.Tag = this;
            }
        }

        private Dictionary<string, FileTreeEntry> Files = new Dictionary<string, FileTreeEntry>();

        private const int CaptionButtonWidth = 35;
        private const int CaptionButtonHeight = 25;
        private Controls.CaptionButton CloseButton;
        private Controls.CaptionButton MaxButton;
        private Controls.CaptionButton MinButton;

        public MainWindow()
        {
            InitializeComponent();

            CloseButton = new DromeEd.Controls.CaptionButton(Width - CaptionButtonWidth - 1, 1, CaptionButtonWidth, CaptionButtonHeight, Properties.Resources.CaptionGlyphClose, HT_CLIENT);
            CloseButton.Clicked += (sender, args) => Close();
            CaptionButtons.Add(CloseButton);
            MaxButton = new DromeEd.Controls.CaptionButton(Width - CaptionButtonWidth * 2 - 1, 1, CaptionButtonWidth, CaptionButtonHeight, Properties.Resources.CaptionGlyphMaximize, HT_CLIENT);
            MaxButton.Clicked += (sender, args) => WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            CaptionButtons.Add(MaxButton);
            MinButton = new DromeEd.Controls.CaptionButton(Width - CaptionButtonWidth * 3 - 1, 1, CaptionButtonWidth, CaptionButtonHeight, Properties.Resources.CaptionGlyphMinimize, HT_CLIENT);
            MinButton.Clicked += (sender, args) => WindowState = FormWindowState.Minimized;
            CaptionButtons.Add(MinButton);
            treeView1.ParentFocus = titledPanel1.FocusZone;
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            // Populate treeview
            
            treeView1.Nodes.Clear();
            foreach (FileEntry entry in Program.Filesystem.Files.Values)
            {
                AddOrGetFileTreeEntry(entry.Filename);
            }
            treeView1.Nodes[0].Expand();

            //PropertyGrid.SelectedObject = PropertyGrid;

            //TabControl.AddTab(new DromeEd.Controls.TabItem("test new tab"));
            //TabControl.AddTab(new DromeEd.Controls.TabItem("second new tab"));
            //TabControl.AddTab(new DromeEd.Controls.TabItem("third new tab") { Content = new Button() { Text = "Tab 3's Button", Dock = DockStyle.Fill } });
            //TabControl.AddTab(new DromeEd.Controls.TabItem("fourth new tab"));

            //titledPanel1.FocusZone.Focused += (s, a) => treeView1.Invalidate();
            //titledPanel1.FocusZone.Unfocused += (s, a) => treeView1.Invalidate();
        }

        private FileTreeEntry AddOrGetFileTreeEntry(string filename)
        {
            // = filename.Replace("\\", "/");
            if (Files.ContainsKey(filename))
                return Files[filename];

            FileEntry fsEntry = Program.Filesystem.GetFileEntry(filename);

            FileTreeEntry newEntry = new FileTreeEntry(fsEntry, new TreeNode(System.IO.Path.GetFileName(filename)), fsEntry == null);
            if (!newEntry.IsFolder)
                newEntry.TreeNode.ContextMenuStrip = this.GameFileContextMenu;

            if (filename.Contains("\\"))
            {
                // Find the parent TreeNode
                AddOrGetFileTreeEntry(System.IO.Path.GetDirectoryName(filename)).TreeNode.Nodes.Add(newEntry.TreeNode);
            }
            else
            {
                treeView1.Nodes.Add(newEntry.TreeNode);
            }
            Files.Add(filename, newEntry);
            return newEntry;
        }

        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            //if (e.Bounds.Y != e.Node.Bounds.Y)
            //    return;
            //SolidBrush hilightBrush = new SolidBrush(titledPanel1.FocusZone.IsFocused ? Theme.ApplicationColor : Theme.BorderColor);
            //SolidBrush backBrush = new SolidBrush(treeView1.BackColor);
            //SolidBrush textBrush = new SolidBrush(treeView1.ForeColor);
            //e.Graphics.FillRectangle(backBrush, e.Bounds);
            //if (e.State.HasFlag(TreeNodeStates.Selected))
            //    e.Graphics.FillRectangle(hilightBrush, e.Bounds);
            ////e.Graphics.DrawString(e.Node.Text, treeView1.Font, textBrush, e.Node.Bounds.Left, e.Node.Bounds.Top + 0);
            //TextRenderer.DrawText(e.Graphics, e.Node.Text, treeView1.Font, new Point(e.Node.Bounds.Left, e.Node.Bounds.Top), treeView1.ForeColor);
            //if (e.Node.Nodes.Count > 0)
            //    e.Graphics.DrawImage(e.Node.IsExpanded ? Properties.Resources.TreeArrowOpen : Properties.Resources.TreeArrowClosed, e.Node.Bounds.Left - 13, e.Node.Bounds.Top + 6, 8, 8);
            //hilightBrush.Dispose();
            //backBrush.Dispose();
            //textBrush.Dispose();
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            //TreeNode node = treeView1.GetNodeAt(0, e.Y);
            //treeView1.SelectedNode = node;
            //treeView1.Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            if (IsHandleCreated)
            {
                int pad = WindowState == FormWindowState.Maximized ? 0 : 1;
                CloseButton.X = ClientSize.Width - CloseButton.Width - pad;
                MaxButton.X = ClientSize.Width - CloseButton.Width - MaxButton.Width - pad;
                MinButton.X = ClientSize.Width - CloseButton.Width - MaxButton.Width - MinButton.Width - pad;
                MaxButton.Image = WindowState == FormWindowState.Maximized ? Properties.Resources.CaptionGlyphRestore : Properties.Resources.CaptionGlyphMaximize;
            }
            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawImage(Properties.Resources.Banner, 6, 5, 256, 25);
        }

        public void OpenFile(FileEntry file)
        {
            string extension = System.IO.Path.GetExtension(file.Filename).ToLower();
            Editors.EditorBase ed = null;
            if (extension == ".cmu" || extension == ".txt" || extension == ".cfg" || extension == ".srf")
                ed = new Editors.TextEditor(file);
            else if (extension == ".md2")
                ed = new Editors.ModelEditor(file);
            else if (extension == ".wrl")
                ed = new Editors.WorldEditor(file);
            else if (extension == ".pc texture")
                ed = new Editors.TextureEditor(file);

            if (ed != null)
            {
                OpenEditor(ed);
                ed.SetFocusZone(TabControl.FocusZone);
            }
            else
                MessageBox.Show("No editor for that extension.");
        }

        public void OpenEditor(Editors.EditorBase editor)
        {
            TabControl.AddTab(editor.TabItem);
            editor.TabItem.Show();
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            FileTreeEntry file = e.Node.Tag as FileTreeEntry;
            if (file?.FilesystemEntry != null)
                OpenFile(file.FilesystemEntry);
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileTreeEntry selectedEntry = treeView1.SelectedNode?.Tag as FileTreeEntry;

            if (selectedEntry != null)
            {
                byte[] data = Program.Filesystem.GetFileData(selectedEntry.FilesystemEntry);

                SaveFileDialog dialog = new SaveFileDialog();
                string extension = System.IO.Path.GetExtension(selectedEntry.Filename);
                dialog.Filter = extension.ToUpper() + " File (*" + extension + ")|*" + extension + "|All files (*.*)|*.*";
                dialog.FileName = System.IO.Path.GetFileName(selectedEntry.Filename);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllBytes(dialog.FileName, data);
                }
            }
        }
    }
}
