using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpDX;

namespace DromeEd.Drome
{
    public class OLIFile
    {
        public struct OLITriangle
        {
            public ushort Index1;
            public ushort Index2;
            public ushort Index3;
            public ushort MaterialIndex;

            public OLITriangle(BinaryReader reader)
            {
                Index1 = reader.ReadUInt16();
                Index2 = reader.ReadUInt16();
                Index3 = reader.ReadUInt16();
                MaterialIndex = reader.ReadUInt16();
            }
        }

        public struct RenderVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public float U;
            public float V;

            public RenderVertex(BinaryReader reader)
            {
                Position = reader.ReadVector3();
                Normal = reader.ReadVector3();
                U = reader.ReadSingle();
                V = reader.ReadSingle();
            }
        }

        public string CMOMagic;
        public uint ContentLength;

        public string COIMagic;
        public uint COIGeometryLength;
        public uint COIGeometryLength2;
        public uint COIUnk01; // Likely a count
        public uint COIUnk02; // A Length minus 0x10
        public uint COIUnk03; // 0x00
        public uint COIUnk04; // 0x04
        // 26 bytes of 0x00
        ushort COIUnk05; // 0x3F80
        public uint COIUnk06;
        public uint COIUnk07;
        //public uint COIVertexCount;
        //public uint COITriangleCount;
        public Vector3 AABBSize; // Side lengths of the AABB centered at the origin that encapsulates the whole mesh.
        public List<Vector3> COIVertices = new List<Vector3>();
        public List<OLITriangle> COITriangles = new List<OLITriangle>();

        public List<RenderVertex> RenderVertices = new List<RenderVertex>();

        public uint ContentLength2;
        public string CMOMagic2;

        public OLIFile(BinaryReader reader)
        {
            CMOMagic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            ContentLength = reader.ReadUInt32();
            long CMOContentStart = reader.BaseStream.Position;

            COIMagic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            COIGeometryLength = reader.ReadUInt32();
            COIGeometryLength2 = reader.ReadUInt32();
            COIUnk01 = reader.ReadUInt32();
            COIUnk02 = reader.ReadUInt32();
            COIUnk03 = reader.ReadUInt32();
            COIUnk04 = reader.ReadUInt32();
            reader.BaseStream.Position += 26; // Lots of empty space. The headers often mention leaving space for padding / future features, maybe that's what this is.
            COIUnk05 = reader.ReadUInt16();
            COIUnk06 = reader.ReadUInt32();
            COIUnk07 = reader.ReadUInt32();

            uint COIVertexCount = reader.ReadUInt32();
            uint COITriangleCount = reader.ReadUInt32();
            AABBSize = reader.ReadVector3();
            for (int i = 0; i < COIVertexCount; i++)
            {
                COIVertices.Add(reader.ReadVector3());
            }
            for (int i = 0; i < COITriangleCount; i++)
            {
                COITriangles.Add(new OLITriangle(reader));
            }

            uint CountTimesSix = reader.ReadUInt32(); // No idea why this is the number of triangles time six, maybe it's the number of bytes for the indices? but each triagle is 4 ushorts, not 3.
            byte[] MaterialIndices = reader.ReadBytes((int)COITriangleCount); // We don't care, this duplicates the material indices in the triangle (index) data

            byte[] Padding; // Usually 0x00, sometimes other stuff. Doesn't seem to mean anything though.
            if (reader.BaseStream.Position % 4 != 0)
                Padding = reader.ReadBytes(4 - ((int)reader.BaseStream.Position % 4));

            // HACK: Read vertices until TSM3
            string sentinel = Encoding.ASCII.GetString(reader.ReadBytes(4));
            while (sentinel != "TSM3")
            {
                reader.BaseStream.Position -= 4;
                RenderVertex v = new RenderVertex(reader);
                RenderVertices.Add(v);
                sentinel = Encoding.ASCII.GetString(reader.ReadBytes(4));
            }
        }

        public void DumpOBJ(string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("# Dumped with DromeEd.Console");

                foreach (Vector3 v in COIVertices)
                {
                    writer.WriteLine("v " + v.X + " " + v.Y + " " + v.Z);
                }

                foreach (OLITriangle triangle in COITriangles)
                {
                    writer.WriteLine("f " + (triangle.Index1 + 1) + " " + (triangle.Index2 + 1) + " " + (triangle.Index3 + 1));
                }
            }
        }
    }
}
