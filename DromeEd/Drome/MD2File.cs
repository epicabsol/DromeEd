using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpDX;

namespace DromeEd.Drome
{
    public static class Utils
    {
        public static uint EndianSwap(uint input)
        {
            return ((input & 0xFF000000) >> 0x18) | ((input & 0x00FF0000) >> 0x08) | ((input & 0x0000FF00) << 0x08) | ((input & 0x000000FF) << 0x18);
        }
    }

    public class TextureReference
    {
        private const int NameSize = 256;

        public string MapName = "";
        public Texture.MapType MapType;
        public int BaseIndex;

        public TextureReference(BinaryReader reader)
        {
            MapName = reader.ReadStringN(NameSize);
            MapType = (Texture.MapType)reader.ReadUInt32();
            BaseIndex = reader.ReadInt32();
        }
    }

    public class MaterialProps
    {
        private const int AnimationNameSize = 8;

        public Vector4 Ambient;
        public Vector4 Diffuse;
        public Vector4 Specular;
        public Vector4 Emissive;
        public float Shininess;
        public float Transparency;
        public uint TransparencyType;
        public uint PropertyBits;
        public string AnimationName;
        public uint pAnimationCallback = 0;

        public MaterialProps(BinaryReader reader)
        {
            Ambient = reader.ReadVector4();
            Diffuse = reader.ReadVector4();
            Specular = reader.ReadVector4();
            Emissive = reader.ReadVector4();
            Shininess = reader.ReadSingle();
            Transparency = reader.ReadSingle();
            TransparencyType = reader.ReadUInt32();
            PropertyBits = reader.ReadUInt32();
            AnimationName = reader.ReadStringN(AnimationNameSize);
        }
    }

    public class BlockHeader
    {
        // MD2 Model
        public const string MAGIC_MODEL0 = "MDL0";
        public const string MAGIC_MODEL2 = "MDL2";
        public const string MAGIC_MODEL3 = "MDL3";
        public const string MAGIC_SKIN1 = "SKN0"; // Skin data
        public const string MAGIC_SKIN2 = "SKN1"; // PS2 VU1 format skin
        public const string MAGIC_SKIN3 = "SKN2"; // Skin and morph data
        public const string MAGIC_SHADOW1 = "SHA0";
        public const string MAGIC_SHADOWPS2 = "P2S0"; // PS2 VU1 format shadow
        public const string MAGIC_COLLDATA = "COLD";
        public const string MAGIC_GEOMETRY = "GEO0";
        public const string MAGIC_GEOMETRY1 = "GEO1";
        public const string MAGIC_GEOMETRY2 = "GEO2"; // Has render groups with an ID
        public const string MAGIC_GEOMETRYPS2 = "P2G0";
        public const string MAGIC_INDEXPS2 = "P2I0";
        public const string MAGIC_ANCHOR = "ANC0";
        public const string MAGIC_SKINGCN = "SKNG"; // GCN skin data
        public const string MAGIC_GEOMETRYGCN = "GCG0"; // GCN geometry data

        // OLI terrain stuff
        public const string MAGIC_OLI_CMO = "\u0001CMO";
        public const string MAGIC_OLI_COI = "COI0";
        

        public string Signature { get; }
        public int Length { get; }

        public BlockHeader(string signature, int length)
        {
            Signature = signature;
            Length = length;
        }

        public BlockHeader(BinaryReader reader)
        {
            Signature = Encoding.ASCII.GetString(reader.ReadBytes(4));
            Length = reader.ReadInt32();
        }
    }

    public abstract class Block
    {
        public BlockHeader Header { get; }

        public Block(BlockHeader header)
        {
            Header = header;
        }
    }

    [Flags()]
    public enum BufferAccess : uint
    {
        None = 0,
        Read = 1,
        Write = 2,
        ReadWrite = Read | Write,
    }

    public class MDLBlock : Block
    {
        //public int Size;
        public Vector3 InertiaTensor;
        public float BoundingRadius;
        public bool AllowDistanceFade = false;
        public bool IncludesBoundingBox = false;
        public Vector3 AABBMin;
        public Vector3 AABBMax;
        public Vector3 AABBCenter;
        public float AABBYaw;
        public uint UseUniqueMaterials;
        public uint UseUniqueTextures;
        public uint UseGenericGeometry; // don't load platform specific stuff
        public BufferAccess VertexBufferAccessFlags; // for cVertexBuffer

        public List<TextureReference> TextureReferences = new List<TextureReference>();
        public List<MaterialProps> Materials = new List<MaterialProps>();

        public MDLBlock(BinaryReader reader, BlockHeader header) : base(header)
        {
            InertiaTensor = reader.ReadVector3();
            BoundingRadius = reader.ReadSingle();
            if (header.Signature != BlockHeader.MAGIC_MODEL0)
            {
                AllowDistanceFade = reader.ReadUInt32() > 0;
                IncludesBoundingBox = reader.ReadUInt32() > 0;
                if (IncludesBoundingBox)
                {
                    AABBMin = reader.ReadVector3();
                    AABBMax = reader.ReadVector3();
                    AABBCenter = reader.ReadVector3();
                    AABBYaw = reader.ReadSingle();
                }
                UseUniqueMaterials = reader.ReadUInt32();
                UseUniqueTextures = reader.ReadUInt32();
                UseGenericGeometry = reader.ReadUInt32();
                VertexBufferAccessFlags = (BufferAccess)reader.ReadUInt32();
                reader.ReadBytes(12 * 4);

            }
            else
            {
                AllowDistanceFade = false;
            }

            int textureReferenceCount = reader.ReadInt32();
            for (int i = 0; i < textureReferenceCount; i++)
            {
                TextureReferences.Add(new TextureReference(reader));
            }

            int materialCount = reader.ReadInt32();
            for (int i = 0; i < materialCount; i++)
            {
                Materials.Add(new MaterialProps(reader));
            }

        }
    }

    /*public class GEO1Block : Block
    {

        public List<LOD> LODs = new List<LOD>();

        public GEO1Block(BinaryReader reader, BlockHeader header) : base(header)
        {
            int LODCount = reader.ReadInt32();
            for (int i = 0; i < LODCount; i++)
            {
                LODs.Add(new LOD(reader));
            }
        }
    }*/

    public class GeometryBlock : Block
    {
        public class LOD
        {
            public enum LODLevel : uint
            {
                GeometryFirstLevel,
                GeometrySubdivisionDetail = 0,
                GeometryNormalDetail,
                GeometryReducedDetail,
                GeometrySortedDetail,
                GeometryCollisionMesh,
                GeometryFacialMorphTarget,
                GeometryBlankDetailLevel,
                GeometryNumLevels = 6,
                GeometryAllLevels
            }

            public LODLevel Type;
            public float MaxEdgeLength;
            public uint GroupCount; // 0x00000003
            Vector3[] Positions; // Positions for z-sorting the rendergroups
            public uint PositionsPointer; // 0x00000000 pPositions Just here because the C++ class is literally dumped to the file.
            public uint RenderGroupsPointer; // 0x0212DEA4 pRenderGroups Just here because the C++ class is literally dumped to the file.

            public List<RenderGroup> RenderGroups = new List<RenderGroup>();

            public LOD(BinaryReader reader)
            {
                Type = (LODLevel)reader.ReadUInt32();
                MaxEdgeLength = reader.ReadSingle();
                GroupCount = reader.ReadUInt32();
                PositionsPointer = reader.ReadUInt32();
                //if (Context.Current.Game == Context.NextGenGame.DromeRacers)
                    RenderGroupsPointer = reader.ReadUInt32();
                if (PositionsPointer > 0)
                {
                    Positions = new Vector3[GroupCount];
                    for (int i = 0; i < GroupCount; i++)
                    {
                        Positions[i] = reader.ReadVector3();
                    }
                }
                for (int i = 0; i < GroupCount; i++)
                {
                    RenderGroups.Add(new RenderGroup(reader));
                }
            }
        }

        public List<LOD> LODs = new List<LOD>();

        public GeometryBlock(BinaryReader reader, BlockHeader header) : base(header)
        {
            int LODCount = reader.ReadInt32();
            for (int i = 0; i < LODCount; i++)
            {
                LODs.Add(new LOD(reader));
            }
        }
    }

    public class Anchor
    {
        private const int NameSize = 0x20;
        public string AnchorName = "";
        public uint AnchorType;
        public uint MaterialIndex;
        public string LinkedFilename = "";
        public Vector3 AnchorPos;
        public Vector3 Forward;
        public Vector3 Right;
        public Vector3 Up;
        public Vector3 Offset;

        public Anchor(BinaryReader reader)
        {
            AnchorName = reader.ReadStringN(NameSize);
            AnchorType = reader.ReadUInt32();
            MaterialIndex = reader.ReadUInt32();
            LinkedFilename = reader.ReadStringFilename();
            AnchorPos = reader.ReadVector3();
            Forward = reader.ReadVector3();
            Right = reader.ReadVector3();
            Up = reader.ReadVector3();
            Offset = reader.ReadVector3();
        }

        public override string ToString()
        {
            return "<" + AnchorPos.X + ", " + AnchorPos.Y + ", " + AnchorPos.Z + ">: " + AnchorName;
        }
    }

    public class AnchorBlock : Block
    {
        public List<Anchor> Anchors = new List<Anchor>();

        public AnchorBlock(BinaryReader reader, BlockHeader header) : base(header)
        {
            byte anchorCount = reader.ReadByte();

            for (int i = 0; i < anchorCount; i++)
            {
                Anchors.Add(new Anchor(reader));
            }
        }
    }

    public class MD2File
    {
        public Dictionary<string, Block> Blocks = new Dictionary<string, Block>();

        public MD2File(BinaryReader reader)
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length - 4)
            {
                BlockHeader header = new BlockHeader(reader);
                long offset = reader.BaseStream.Position;
                System.Console.WriteLine("Reading Block '" + header.Signature + "' : " + ((int)reader.BaseStream.Position).ToString("X8") + " + " + header.Length.ToString("X8") + " bytes");

                Block newBlock = null;
                if (header.Signature == BlockHeader.MAGIC_MODEL0 || header.Signature == BlockHeader.MAGIC_MODEL2 || header.Signature == BlockHeader.MAGIC_MODEL3)
                {
                    newBlock = new MDLBlock(reader, header);
                }
                else if (header.Signature == BlockHeader.MAGIC_GEOMETRY ||
                         header.Signature == BlockHeader.MAGIC_GEOMETRY1 ||
                         header.Signature == BlockHeader.MAGIC_GEOMETRY2)
                {
                    newBlock = new GeometryBlock(reader, header);
                }
                else if (header.Signature == BlockHeader.MAGIC_ANCHOR)
                {
                    newBlock = new AnchorBlock(reader, header);
                }
                
                if (newBlock != null)
                {
                    Blocks.Add(header.Signature, newBlock);
                }

                reader.BaseStream.Position = offset + header.Length;
            }
        }

        public void ExportOBJ(string filename)
        {
            MDLBlock mdl;// = (Blocks[BlockHeader.MAGIC_MODEL3] ?? Blocks[BlockHeader.MAGIC_MODEL2]) as MDLBlock;
            if (Blocks.ContainsKey(BlockHeader.MAGIC_MODEL2))
                mdl = Blocks[BlockHeader.MAGIC_MODEL2] as MDLBlock;
            else if (Blocks.ContainsKey(BlockHeader.MAGIC_MODEL3))
                mdl = Blocks[BlockHeader.MAGIC_MODEL3] as MDLBlock;
            else
                throw new Exception("No supported MDL block found.");

            GeometryBlock geo;// = Blocks[BlockHeader.MAGIC_GEOMETRY2] as GeometryBlock;
            if (Blocks.ContainsKey(BlockHeader.MAGIC_GEOMETRY1))
                geo = Blocks[BlockHeader.MAGIC_GEOMETRY1] as GeometryBlock;
            else if (Blocks.ContainsKey(BlockHeader.MAGIC_GEOMETRY2))
                geo = Blocks[BlockHeader.MAGIC_GEOMETRY2] as GeometryBlock;
            else
                throw new Exception("No supported GEO block found.");

            using (StreamWriter matwriter = new StreamWriter(Path.ChangeExtension(filename, ".mtl")))
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(filename) + ".mtl");

                int m = 0;
                int vertexOffset = 0;
                foreach (GeometryBlock.LOD lod in geo.LODs)
                {
                    writer.WriteLine("g LOD" + lod.Type.ToString());
                    foreach (RenderGroup group in lod.RenderGroups)
                    {
                        // Write a new material
                        MaterialProps mat = mdl.Materials[group.Material];
                        matwriter.WriteLine("newmtl " + Path.GetFileNameWithoutExtension(filename) + "_" + m);
                        matwriter.WriteLine("Ka " + (mat.Ambient.X + mat.Emissive.X) + " " + (mat.Ambient.Y + mat.Emissive.Y) + " " + (mat.Ambient.Z + mat.Emissive.Z));
                        matwriter.WriteLine("Kd " + mat.Diffuse.X + " " + mat.Diffuse.Y + " " + mat.Diffuse.Z);
                        matwriter.WriteLine("Ks " + mat.Specular.X + " " + mat.Specular.Y + " " + mat.Specular.Z);
                        matwriter.WriteLine("Ns " + mat.Shininess);
                        matwriter.WriteLine("d " + (1.0f - mat.Transparency));

                        foreach (TextureBlend blend in group.TextureBlends)
                        {
                            if (blend.Effect == Texture.MapType.Base)
                            {
                                matwriter.WriteLine("map_Kd " + Path.GetFileName(mdl.TextureReferences[blend.TextureIndex].MapName));
                                // Write Texture
                                if (Program.Filesystem != null)
                                {
                                    using (MemoryStream ms = new MemoryStream(Program.Filesystem.GetFileData(Program.Filesystem.GetFileEntry(mdl.TextureReferences[blend.TextureIndex].MapName.Replace(".tga", ".PC TEXTURE")))))
                                    using (BinaryReader texReader = new BinaryReader(ms))
                                    {
                                        Texture tex = new Texture(texReader);
                                        tex.DumpTGA(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(mdl.TextureReferences[blend.TextureIndex].MapName)) + ".tga");
                                    }
                                }
                            }
                        }

                        writer.WriteLine("usemtl " + Path.GetFileNameWithoutExtension(filename) + "_" + m);
                        writer.WriteLine("o LOD" + lod.Type.ToString() + "_group" + lod.RenderGroups.IndexOf(group));

                        // Write vertices
                        for (int v = 0; v < group.VertexCount; v++)
                        {
                            writer.WriteLine("v " + (Context.Current.Game == Context.NextGenGame.LegoRacers2 ? -1.0f : 1.0f) * ParseFloat(group.VertexBuffer.VertexData, v * (int)group.VertexBuffer.VertexSize + (int)group.VertexBuffer.PositionOffset) + " " + ParseFloat(group.VertexBuffer.VertexData, v * (int)group.VertexBuffer.VertexSize + (int)group.VertexBuffer.PositionOffset + 4) + " " + ParseFloat(group.VertexBuffer.VertexData, v * (int)group.VertexBuffer.VertexSize + (int)group.VertexBuffer.PositionOffset + 8));
                            writer.WriteLine("vn " + ParseFloat(group.VertexBuffer.VertexData, v * (int)group.VertexBuffer.VertexSize + (int)group.VertexBuffer.NormalOffset) + " " + ParseFloat(group.VertexBuffer.VertexData, v * (int)group.VertexBuffer.VertexSize + (int)group.VertexBuffer.NormalOffset + 4) + " " + ParseFloat(group.VertexBuffer.VertexData, v * (int)group.VertexBuffer.VertexSize + (int)group.VertexBuffer.NormalOffset + 8));
                            writer.WriteLine("vt " + ParseFloat(group.VertexBuffer.VertexData, v * (int)group.VertexBuffer.VertexSize + (int)group.VertexBuffer.UVOffset) + " " + ParseFloat(group.VertexBuffer.VertexData, v * (int)group.VertexBuffer.VertexSize + (int)group.VertexBuffer.UVOffset + 4));
                        }

                        for (int i = 0; i < group.IndexBuffer.IndexCount; i += 3)
                        {
                            int i1 = BitConverter.ToUInt16(group.IndexBuffer.IndexData, i * 2) + vertexOffset + 1;
                            int i2 = BitConverter.ToUInt16(group.IndexBuffer.IndexData, i * 2 + 2) + vertexOffset + 1;
                            int i3 = BitConverter.ToUInt16(group.IndexBuffer.IndexData, i * 2 + 4) + vertexOffset + 1;
                            /*if (Context.Current.Game == Context.NextGenGame.LegoRacers2)
                            {
                                // Flip the order of the indices
                                int temp = i1;
                                i1 = i2;
                                i2 = i3;
                                i3 = temp;
                            }*/
                            writer.WriteLine("f " + i1 + "/" + i1 + "/" + i1 + " " + i2 + "/" + i2 + "/" + i2 + " " + i3 + "/" + i3 + "/" + i3);
                        }

                        vertexOffset += group.VertexCount;
                        m++;
                    }
                }
            }
        }

        private static float ParseFloat(byte[] buffer, int offset)
        {
            return BitConverter.ToSingle(buffer, offset);
        }
    }
}
