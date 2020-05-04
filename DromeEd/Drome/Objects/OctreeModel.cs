using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DromeEd.Drome.Objects
{
    [ObjectType("cOctreeModel")]
    public class OctreeModel : Object
    {
        public class OctreeNode
        {

        }

        public string VOMFilename;
        public AtomPhysicsData Physics;

        // Debug rendering toggles are saved in the init data
        public bool RenderLeafs;
        public bool ShowVisLeaf;
        public bool DebugZBuffer;
        public bool TriangleCounts;
        public bool LockVisibility;
        public bool CyclePVSLeafs;
        public bool DrawNonPVSLeafs;

        public OctreeModel(ObjectHeader header, BinaryReader reader) : base(header, reader)
        {

        }
    }
}
