using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DromeEd.Drome.Objects
{
    [ObjectType("cCamera")]
    public class Camera : Object
    {


        public Camera(ObjectHeader header, System.IO.BinaryReader reader) : base(header, reader)
        {

        }
    }
}
