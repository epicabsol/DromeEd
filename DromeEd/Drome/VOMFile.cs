using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpDX;

namespace DromeEd.Drome
{
    public class VOMFile
    {
        

        public class VOMHeader
        {
            public const string VOMMagic = "VOM*";
            public const int CurrentVersion = 4;

            public string Magic;
            public int Version;

            public uint TimeStamp;

            public int MaxFacesPerLeaf;
            public int MaxOctreeDepth;

            public int NumBitmaps;
            public int NumMaterials;
            public int NumRenderGroups;
            public int NumRenderGroupIndices;

            public int NumNodes;
            public int NumLeafs;

            public Vector3 Min;
            public Vector3 Max;
            public float MaxY;
            public int NumVisNodes;
            public int NumVisLeafs;
            public int LeafVisDataSize;
            public int ObjectVisDataSize;
            public int NumDetailModels;
            public int BaseDetailVisID;
            public int CollisionDataSize;

            public VOMHeader(BinaryReader reader)
            {
                this.Magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
                this.Version = reader.ReadInt32();

                this.TimeStamp = reader.ReadUInt32();

                this.MaxFacesPerLeaf = reader.ReadInt32();
                this.MaxOctreeDepth = reader.ReadInt32();

                this.NumBitmaps = reader.ReadInt32();
                this.NumMaterials = reader.ReadInt32();
                this.NumRenderGroups = reader.ReadInt32();
                this.NumRenderGroupIndices = reader.ReadInt32();

                this.NumNodes = reader.ReadInt32();
                this.NumLeafs = reader.ReadInt32();

                this.Min = reader.ReadVector3();
                this.Max = reader.ReadVector3();
                this.MaxY = reader.ReadSingle();
                this.NumVisNodes = reader.ReadInt32();
                this.NumVisLeafs = reader.ReadInt32();
                this.LeafVisDataSize = reader.ReadInt32();
                this.ObjectVisDataSize = reader.ReadInt32();
                this.NumDetailModels = reader.ReadInt32();
                this.BaseDetailVisID = reader.ReadInt32();
                this.CollisionDataSize = reader.ReadInt32();
            }
        }


        /*public uint Unk11; // 0x1000
        public uint Unk12; // 0x00, 0x05, 0x0224
        public uint Unk13; // 0x00, 0x00, 0x05B8
        public uint MaterialCount;
        public uint Unk14; // 0x32, 0x15
        public uint Unk15; // 0x00, 0x00, 0x00
        public uint TextureReferencesLength;
        public List<TextureReference> Textures = new List<TextureReference>();
        public uint MaterialDataLength;
        public List<MaterialProps> Materials = new List<MaterialProps>();
        public uint GCVMLength;

        public uint P2G0Length;*/

        public VOMHeader Header;

        public int[] BitmapIndices;
        public int[] MaterialIndices;

        public VOMFile(BinaryReader reader)
        {
            this.Header = new VOMHeader(reader);
            this.BitmapIndices = new int[Header.NumBitmaps];
            this.MaterialIndices = new int[Header.NumMaterials];
        }
    }
}
