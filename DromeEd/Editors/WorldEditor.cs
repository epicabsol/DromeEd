using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATD.VFS;

namespace DromeEd.Editors
{
    public class WorldEditor : EditorBase
    {
        public Controls.WorldEditorControl Control;

        public WorldEditor(FileEntry file) : base(file)
        {
            Control = new Controls.WorldEditorControl();
            BaseControl = Control;
            TabItem.Destroyed += (sender, args) => {
                Control.Destroy();
            };

            Control.Load += (sender, args) => {
                Control.LoadWorld(file);
            };
        }

        public override void Save()
        {
            throw new NotImplementedException();
        }
    }
}
