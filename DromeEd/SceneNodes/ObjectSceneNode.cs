using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DromeEd.SceneNodes
{
    public class ObjectSceneNode : Controls.SceneNode
    {
        public Drome.Object Object { get; }

        public ObjectSceneNode(Controls.SceneScreen screen, Drome.Object _object)
        {
            Object = _object;
        }
    }
}
