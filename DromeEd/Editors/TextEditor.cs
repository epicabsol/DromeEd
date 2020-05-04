using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATD.VFS;

namespace DromeEd.Editors
{
    public class TextEditor : EditorBase
    {
        public Controls.TextEditControl Editor;


        public TextEditor(FileEntry file) : base(file)
        {
            Editor = new Controls.TextEditControl();
            Editor.Text = Encoding.ASCII.GetString(Program.Filesystem.GetFileData(file)).Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
            BaseControl = Editor;
            Editor.SaveRequested += (sender, args) => Save();
        }

        public override void Save()
        {
            
        }
    }
}
