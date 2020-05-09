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
        public class OctreeNode : VisOctreeNode
        {
            public Vector3 Min;
            public Vector3 Max;

            public OctreeNode(BinaryReader reader) : base(reader)
            {
                this.Min = reader.ReadVector3();
                this.Max = reader.ReadVector3();
            }
        }

        public class OctreeLeaf
        {
            public int VisID;
            public Vector3 Min;
            public Vector3 Max;
            public int FirstGroup; // Index into RenderGroups
            public int GroupCount;

            public OctreeLeaf(BinaryReader reader)
            {
                this.VisID = reader.ReadInt32();
                this.Min = reader.ReadVector3();
                this.Max = reader.ReadVector3();
                this.FirstGroup = reader.ReadInt32();
                this.GroupCount = reader.ReadInt32();
            }
        }

        public class VisOctreeNode
        {
            private const int ChildCount = 8;

            public int[] Children;
            public float XPlaneDistance;
            public float YPlaneDistance;
            public float ZPlaneDistance;

            public VisOctreeNode(BinaryReader reader)
            {
                this.Children = new int[ChildCount];
                for (int i = 0; i < Children.Length; i++)
                {
                    this.Children[i] = reader.ReadInt32();
                }

                this.XPlaneDistance = reader.ReadSingle();
                this.YPlaneDistance = reader.ReadSingle();
                this.ZPlaneDistance = reader.ReadSingle();
            }
        }

        public class VisOctreeLeaf
        {
            public int LeafVisOffset;
            public int ObjectVisOffset;

            public VisOctreeLeaf(BinaryReader reader)
            {
                this.LeafVisOffset = reader.ReadInt32();
                this.ObjectVisOffset = reader.ReadInt32();
            }
        }

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

        public TextureReference[] BitmapIndices;
        public MaterialProps[] MaterialIndices;

        public RenderGroup[] RenderGroups;
        public OctreeNode[] Nodes;
        public OctreeLeaf[] Leafs;
        public int[] RenderGroupIndices;
        //public int[] RenderGroupOrders;
        public Vector3[] GroupBounds;
        public VisOctreeNode[] VisNodes;
        public VisOctreeLeaf[] VisLeafs;
        public byte[] LeafVisData;
        public byte[] ObjectVisData;
        public string[] DetailModels;

        public VOMFile(BinaryReader reader)
        {
            //
            // cOctreeModel::ReadHeaderAndAllocateMemory
            //
            this.Header = new VOMHeader(reader);

            this.BitmapIndices = new TextureReference[Header.NumBitmaps];
            this.MaterialIndices = new MaterialProps[Header.NumMaterials];

            this.RenderGroups = new RenderGroup[Header.NumRenderGroups];
            this.Nodes = new OctreeNode[Header.NumNodes];
            this.Leafs = new OctreeLeaf[Header.NumLeafs];
            this.RenderGroupIndices = new int[Header.NumRenderGroupIndices];
            //this.RenderGroupOrders = new int[Header.NumRenderGroupIndices];
            this.GroupBounds = new Vector3[Header.NumRenderGroups * 2];
            this.VisNodes = new VisOctreeNode[Header.NumVisNodes];
            this.VisLeafs = new VisOctreeLeaf[Header.NumVisLeafs];
            //this.LeafVisData = new byte[Header.LeafVisDataSize];
            //this.ObjectVisData = new byte[Header.ObjectVisDataSize];
            this.DetailModels = new string[Header.NumDetailModels];

            //
            // ReadBitmaps
            //
            //const int textureReferenceLength = TextureReference.NameSize + sizeof(int) + sizeof(int);
            //int textureCount = reader.ReadInt32() / textureReferenceLength;
            int bitmapBlockLength = reader.ReadInt32();
            for (int i = 0; i < Header.NumBitmaps; i++)
            {
                this.BitmapIndices[i] = new TextureReference(reader);
            }

            //
            // ReadMaterials
            //
            int materialBlocklength = reader.ReadInt32();
            for (int i = 0; i < Header.NumMaterials; i++)
            {
                this.MaterialIndices[i] = new MaterialProps(reader);
            }

            //
            // ReadGCNGeometry
            //
            int gcnBlockLength = reader.ReadInt32();
            reader.BaseStream.Position += gcnBlockLength;

            //
            // ReadPS2Geometry
            //
            int ps2BlockLength = reader.ReadInt32();
            reader.BaseStream.Position += ps2BlockLength;

            //
            // ReadRenderGroups
            //
            int renderGroupBlockLength = reader.ReadInt32();
            for (int i = 0; i < Header.NumRenderGroups * 2; i++)
            {
                this.GroupBounds[i] = reader.ReadVector3();
            }
            for (int i = 0; i < Header.NumRenderGroups; i++)
            {
                this.RenderGroups[i] = new RenderGroup(reader, false);
            }

            //
            // ReadOctreeData
            //
            int octreeBlockLength = reader.ReadInt32();
            for (int i = 0; i < Header.NumNodes; i++)
            {
                this.Nodes[i] = new OctreeNode(reader);
            }
            for (int i = 0; i < Header.NumLeafs; i++)
            {
                this.Leafs[i] = new OctreeLeaf(reader);
            }
            for (int i = 0; i < Header.NumRenderGroupIndices; i++)
            {
                this.RenderGroupIndices[i] = reader.ReadInt32();
            }

            //
            // ReadVisibilityData
            //
            int visibilityBlockLength = reader.ReadInt32();
            for (int i = 0; i < Header.NumVisNodes; i++)
            {
                this.VisNodes[i] = new VisOctreeNode(reader);
            }
            for (int i = 0; i < Header.NumVisLeafs; i++)
            {
                this.VisLeafs[i] = new VisOctreeLeaf(reader);
            }
            this.LeafVisData = reader.ReadBytes(Header.LeafVisDataSize);
            this.ObjectVisData = reader.ReadBytes(Header.ObjectVisDataSize);
            for (int i = 0; i < Header.NumDetailModels; i++)
            {
                this.DetailModels[i] = reader.ReadString24();
            }

        }

        public void ExportOBJ(string filename)
        {
            using (System.IO.StreamWriter objWriter = new StreamWriter(filename, false))
            using (System.IO.StreamWriter mtlWriter = new StreamWriter(Path.ChangeExtension(filename, ".mtl"), false))
            {
                objWriter.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(filename) + ".mtl");

                HashSet<string> writtenMaterials = new HashSet<string>();

                int vertexOffset = 0;

                foreach (RenderGroup rg in this.RenderGroups)
                {
                    TextureBlend baseTexture = rg.TextureBlends.First(tb => tb.Effect == Texture.MapType.Base);
                    string texturePath = BitmapIndices[baseTexture.TextureIndex].MapName;
                    string materialName = Path.GetFileNameWithoutExtension(texturePath);

                    // Create the material if it's the first time it's being used
                    if (!writtenMaterials.Contains(materialName))
                    {
                        mtlWriter.WriteLine("newmtl " + materialName);
                        MaterialProps mat = this.MaterialIndices[rg.Material];
                        mtlWriter.WriteLine("Ka " + (mat.Ambient.X + mat.Emissive.X) + " " + (mat.Ambient.Y + mat.Emissive.Y) + " " + (mat.Ambient.Z + mat.Emissive.Z));
                        mtlWriter.WriteLine("Kd " + mat.Diffuse.X + " " + mat.Diffuse.Y + " " + mat.Diffuse.Z);
                        mtlWriter.WriteLine("Ks " + mat.Specular.X + " " + mat.Specular.Y + " " + mat.Specular.Z);
                        mtlWriter.WriteLine("Ns " + mat.Shininess);
                        mtlWriter.WriteLine("d " + (1.0f - mat.Transparency));

                        // Write Texture
                        if (Program.Filesystem != null)
                        {
                            if (texturePath.EndsWith(".tga"))
                            {
                                mtlWriter.WriteLine("map_Kd " + Path.GetFileName(texturePath));
                                using (MemoryStream ms = new MemoryStream(Program.Filesystem.GetFileData(Program.Filesystem.GetFileEntry(texturePath.Replace(".tga", "." + Context.Current.Platform.ToString() + " TEXTURE")))))
                                using (BinaryReader texReader = new BinaryReader(ms))
                                {
                                    Texture tex = new Texture(texReader);
                                    tex.DumpTGA(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(texturePath)) + ".tga");
                                }
                            }
                            else if (texturePath.EndsWith(".ifl"))
                            {
                                using (MemoryStream iflMs = new MemoryStream(Program.Filesystem.GetFileData(Program.Filesystem.GetFileEntry(texturePath))))
                                {
                                    IFLFile ifl = new IFLFile(iflMs, texturePath);
                                    mtlWriter.WriteLine("map_Kd " + Path.GetFileName(ifl.Frames[0].TextureFilename));
                                    using (MemoryStream ms = new MemoryStream(Program.Filesystem.GetFileData(Program.Filesystem.GetFileEntry(Path.Combine(Path.GetDirectoryName(texturePath), ifl.Frames[0].TextureFilename.Replace(".tga", "." + Context.Current.Platform.ToString() + " TEXTURE"))))))
                                    using (BinaryReader texReader = new BinaryReader(ms))
                                    {
                                        Texture tex = new Texture(texReader);
                                        tex.DumpTGA(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(ifl.Frames[0].TextureFilename)) + ".tga");
                                    }
                                }
                            }
                        }
                    }

                    objWriter.WriteLine("usemtl " + materialName);

                    // Write vertices
                    for (int v = 0; v < rg.VertexCount; v++)
                    {
                        objWriter.WriteLine("v " + (Context.Current.Game == Context.NextGenGame.LegoRacers2 ? -1.0f : -1.0f) * ParseFloat(rg.VertexBuffer.VertexData, v * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.PositionOffset) + " " + ParseFloat(rg.VertexBuffer.VertexData, v * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.PositionOffset + 4) + " " + ParseFloat(rg.VertexBuffer.VertexData, v * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.PositionOffset + 8));
                        objWriter.WriteLine("vn " + ParseFloat(rg.VertexBuffer.VertexData, v * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.NormalOffset) + " " + ParseFloat(rg.VertexBuffer.VertexData, v * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.NormalOffset + 4) + " " + ParseFloat(rg.VertexBuffer.VertexData, v * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.NormalOffset + 8));
                        objWriter.WriteLine("vt " + ParseFloat(rg.VertexBuffer.VertexData, v * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.UVOffset) + " " + ParseFloat(rg.VertexBuffer.VertexData, v * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.UVOffset + 4));
                    }

                    for (int i = 0; i < rg.IndexBuffer.IndexCount; i += 3)
                    {
                        int i1 = BitConverter.ToUInt16(rg.IndexBuffer.IndexData, i * 2) + vertexOffset + 1;
                        int i2 = BitConverter.ToUInt16(rg.IndexBuffer.IndexData, i * 2 + 2) + vertexOffset + 1;
                        int i3 = BitConverter.ToUInt16(rg.IndexBuffer.IndexData, i * 2 + 4) + vertexOffset + 1;
                        /*if (Context.Current.Game == Context.NextGenGame.LegoRacers2)
                        {
                            // Flip the order of the indices
                            int temp = i1;
                            i1 = i2;
                            i2 = i3;
                            i3 = temp;
                        }*/
                        objWriter.WriteLine("f " + i1 + "/" + i1 + "/" + i1 + " " + i2 + "/" + i2 + "/" + i2 + " " + i3 + "/" + i3 + "/" + i3);
                    }

                    vertexOffset += rg.VertexCount;
                }
            }

        }

        private static float ParseFloat(byte[] buffer, int offset)
        {
            return BitConverter.ToSingle(buffer, offset);
        }
    }
}
