using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DromeEd.Drome.Objects
{
    [ObjectType("cSkyBox")]
    public class Skybox : Object
    {
        public string ModelFilename = ""; // 0x00 + 0x80
        public bool IsWeatherSpecific = false; // 0x80 + 0x04
        public bool IsRaining = false; // 0x84 + 0x04

        public Skybox(ObjectHeader header, BinaryReader reader) : base(header, reader)
        {
            ModelFilename = reader.ReadStringFileName();
            IsWeatherSpecific = reader.ReadUInt32() > 0;
            IsRaining = reader.ReadUInt32() > 0;
        }
    }
}
