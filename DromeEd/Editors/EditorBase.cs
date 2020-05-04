using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DromeEd.Editors
{
    public abstract class EditorBase
    {
        public Controls.TabItem TabItem = null;
        public System.Windows.Forms.Control BaseControl { get => TabItem.Content; set => TabItem.Content = value; }
        public virtual string TabTitle { get => TabItem?.Title; set => TabItem.Title = value + (Dirty ? " *" : ""); }

        public ATD.VFS.FileEntry File { get; private set; } = null;
        public bool Dirty { get; protected set; }

        public EditorBase(ATD.VFS.FileEntry file)
        {
            File = file;
            Dirty = false;
            TabItem = new Controls.TabItem(System.IO.Path.GetFileName(file.Filename));
            TabItem.Destroyed += (tab, args) =>
            {
                if (Dirty)
                {
                    if (System.Windows.Forms.MessageBox.Show("You have unsaved changes.\nSave?", "Closing '" + System.IO.Path.GetFileName(File.Filename) + "'", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        Save();
                    }
                }
            };
        }

        public virtual void SetFocusZone(Controls.FocusZone zone)
        {

        }

        public abstract void Save();
    }
}
