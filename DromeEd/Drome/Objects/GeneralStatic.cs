using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DromeEd.Drome.Objects
{
    [ObjectType("cGeneralStatic")]
    public class GeneralStatic : Object
    {
        public AtomPhysicsData AtomPhysicsData;
        public string ModelFilename = ""; // max 0x80 bytes, 0x7F chars
        public bool IsModelUnique = false; // true: Load a new instance of this model even if one already exists
        public bool UseModelForCollision = false; // true: generate collision shape from mesh data
        // 0x02 bytes padding

        public GeneralStatic(ObjectHeader header, BinaryReader reader) : base(header, reader)
        {
            AtomPhysicsData = new AtomPhysicsData(reader);
            ModelFilename = reader.ReadStringFilename();
            IsModelUnique = reader.ReadByte() > 0;
            UseModelForCollision = reader.ReadByte() > 0;
        }
    }
}
