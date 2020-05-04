using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DromeEd.Drome;
using SharpDX;

namespace DromeEd.Drome.Objects
{
    [ObjectType("cVisRegion")]
    class VisRegion : Object
    {
        public Vector3 Position;
        public Vector3 Dimensions;
        public string ObjectName;

        public VisRegion(ObjectHeader header, System.IO.BinaryReader reader) : base(header, reader)
        {
            Position = reader.ReadVector3();
            Dimensions = reader.ReadVector3();
            ObjectName = reader.ReadString24();
        }
    }
}
