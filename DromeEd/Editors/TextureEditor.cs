using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATD.VFS;

namespace DromeEd.Editors
{
    public class TextureEditor : EditorBase
    {
        public Controls.TextureEditorControl Control = null;
        public Drome.Texture Texture = null;

        public TextureEditor(FileEntry file) : base(file)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(Program.Filesystem.GetFileData(file)))
            using (System.IO.BinaryReader reader = new System.IO.BinaryReader(ms))
            {
                Texture = new Drome.Texture(reader);
            }

            Control = new Controls.TextureEditorControl();
            //Control.Init
            Control.Texture = Texture;
            BaseControl = Control;
            
        }

        public override void Save()
        {
            throw new NotImplementedException();
        }
    }
}
