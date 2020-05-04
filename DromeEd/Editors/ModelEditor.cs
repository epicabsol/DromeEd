using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATD.VFS;
using DromeEd.Controls;

namespace DromeEd.Editors
{
    public class ModelEditor : EditorBase
    {
        public Controls.ModelEditorControl EditorControl;

        public ModelEditor(FileEntry file) : base(file)
        {
            EditorControl = new Controls.ModelEditorControl();
            BaseControl = EditorControl;
            this.TabItem.Destroyed += (sender, args) =>
            {
                EditorControl.Destroy();
            };

            EditorControl.Load += (sender, args) =>
            {
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream(Program.Filesystem.GetFileData(File)))
                using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
                {
                    EditorControl.LoadModel(reader, File.Filename);
                }
            };

        }

        public override void Save()
        {
            throw new NotImplementedException();
        }

        public override void SetFocusZone(FocusZone zone)
        {
            base.SetFocusZone(zone);
            EditorControl.styledTreeView1.ParentFocus = zone;
        }
    }
}
