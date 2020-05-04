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
            this.VOMFilename = reader.ReadStringFileName();
            this.Physics = new AtomPhysicsData(reader);
            this.RenderLeafs = reader.ReadInt32() != 0;
            this.ShowVisLeaf = reader.ReadInt32() != 0;
            this.DebugZBuffer = reader.ReadInt32() != 0;
            this.TriangleCounts = reader.ReadInt32() != 0;
            this.LockVisibility = reader.ReadInt32() != 0;
            this.CyclePVSLeafs = reader.ReadInt32() != 0;
            this.DrawNonPVSLeafs = reader.ReadInt32() != 0;


        }
    }
}
