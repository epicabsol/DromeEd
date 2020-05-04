using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DromeEd.Drome
{
    [Flags()]
    public enum RenderEffects : ushort
    {
        RenderZBiased = 1,
        RenderWireframe = 2,
        RenderNoPolygons = 4,
        RenderNoZTesting = 8,
        RenderNoZWriting = 16,
        RenderNoCulling = 32,
        RenderNoLighting = 64,
        RenderMinimalSetup = 128,
        RenderNoClipping = 256,
        VertexColor = 512,
    }

    public enum PrimitiveType : uint
    {
        TriangleList,
        TriangleStrip,
        TriangleFan,
        LineList,
        LineStrip,
        PointList,
        PlatformSpecific
    }

    public enum TextureTileParam : byte
    {
        None = 0,
        Tile_U = 1,
        TileV = 2,
        MirrorU = 4,
        MirrorV = 8,
        NoBorders = 16
    }

    [Flags()]
    public enum VertexComponent : ushort
    {
        Position = 1,
        Normal = 2,
        Color = 4,
        UV = 8,
        BDNormal = 16,
        BSNormal = 32
    }

    public class TextureBlend
    {
        public Texture.MapType Effect;
        public ushort TextureIndex;
        public byte CoordIndex;
        public TextureTileParam TilingInfo;

        public TextureBlend(BinaryReader reader)
        {
            Effect = (Texture.MapType)reader.ReadUInt32();
            TextureIndex = reader.ReadUInt16();
            CoordIndex = reader.ReadByte();
            TilingInfo = (TextureTileParam)reader.ReadByte();
        }
    }

    public class VertexBuffer
    {
        // 'Source' Vertex buffer
        public uint PositionOffset;
        public uint NormalOffset;
        public uint ColorOffset;
        public uint UVOffset;
        public uint VertexSize;
        public uint UVCount;
        public VertexComponent VertexComponents;
        public ushort VertexCount2;
        public ushort ManagedBuffer;
        public ushort CurrentVertex;
        public uint pSourceBuffer; // Dummy
        public uint pVertexOffset; // Dummy
        public byte[] VertexData;

        public VertexBuffer(BinaryReader reader)
        {
            PositionOffset = reader.ReadUInt32();
            NormalOffset = reader.ReadUInt32();
            ColorOffset = reader.ReadUInt32();
            UVOffset = reader.ReadUInt32();
            VertexSize = reader.ReadUInt32();
            UVCount = reader.ReadUInt32();
            VertexComponents = (VertexComponent)reader.ReadUInt16();
            VertexCount2 = reader.ReadUInt16();
            ManagedBuffer = reader.ReadUInt16();
            CurrentVertex = reader.ReadUInt16();
            pSourceBuffer = reader.ReadUInt32();
            pVertexOffset = reader.ReadUInt32();
            VertexData = reader.ReadBytes(VertexCount2 * (int)VertexSize);
        }
    }

    public class IndexBuffer // loaded as sTempPrimData (without PrimitiveBufferCount) and used as cPrimitiveBuffer
    {
        public uint PrimitiveBufferCount;
        public PrimitiveType PrimitiveType;
        public uint IndexCount;
        public byte[] IndexData;

        public IndexBuffer(BinaryReader reader)
        {
            
            PrimitiveType = (PrimitiveType)reader.ReadUInt32();
            IndexCount = reader.ReadUInt32();
            IndexData = reader.ReadBytes((int)IndexCount * 2); // 2 bytes per index
        }
    }

    public class TextureReference // sBitMapName
    {
        public const int NameSize = 256;

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

    [Flags()]
    public enum MaterialFlagProps
    {
        Collidable = (1 << 0),
        CastShadowMap = (1 << 1),
        Invisible = (1 << 2),
        TwoSided = (1 << 3),
        NoBitmapShadows = (1 << 4),
        FlatDiffuse = (1 << 5)
    }

    public class MaterialProps // cMaterialProps, sizeof: 0x5C
    {
        private const int AnimationNameSize = 8;

        public Vector4 Ambient;
        public Vector4 Diffuse;
        public Vector4 Specular;
        public Vector4 Emissive;
        public float Shininess;
        public float Transparency;
        public uint TransparencyType;
        public MaterialFlagProps PropertyBits;
        public string AnimationName;
        public uint pAnimationCallback = 0;

        public MaterialProps(BinaryReader reader)
        {
            System.Diagnostics.Debug.WriteLine("Reading cMaterialProps starting at offset 0x" + reader.BaseStream.Position.ToString("X8"));
            Ambient = reader.ReadVector4();
            Diffuse = reader.ReadVector4();
            Specular = reader.ReadVector4();
            Emissive = reader.ReadVector4();
            Shininess = reader.ReadSingle();
            Transparency = reader.ReadSingle();
            TransparencyType = reader.ReadUInt32();
            PropertyBits = (MaterialFlagProps)reader.ReadUInt32();
            AnimationName = reader.ReadStringN(AnimationNameSize);
            pAnimationCallback = reader.ReadUInt32();
        }
    }

    public class RenderGroup
    {
        public uint ID; // Dummy
        public ushort PrimitiveCount;
        public ushort VertexCount;
        public ushort Material; // Material Index
        public RenderEffects Effects;
        uint pBumpData; // Dummy
        uint pVertexBuffer; // Dummy
        uint pIndexBuffer; // Dummy

        public const int BlendCount = 4;

        // cTextureBlends
        public ushort EffectsMask; // Each applied effect is a bit in this variable
        public ushort RendererReference;
        public ushort EffectCount;
        public byte Custom;
        public byte CoordsCount;
        public List<TextureBlend> TextureBlends = new List<TextureBlend>();

        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;

        public RenderGroup(BinaryReader reader, bool readPrimBufferCount)
        {
            if (Context.Current.Game >= Context.NextGenGame.DromeRacers)
                ID = reader.ReadUInt32();
            PrimitiveCount = reader.ReadUInt16();
            VertexCount = reader.ReadUInt16();
            Material = reader.ReadUInt16();
            Effects = (RenderEffects)reader.ReadUInt16();
            pBumpData = reader.ReadUInt32();
            pVertexBuffer = reader.ReadUInt32();
            pIndexBuffer = reader.ReadUInt32();

            EffectsMask = reader.ReadUInt16();
            RendererReference = reader.ReadUInt16();
            EffectCount = reader.ReadUInt16();
            Custom = reader.ReadByte();
            CoordsCount = reader.ReadByte();

            for (int i = 0; i < BlendCount; i++)
            {
                TextureBlends.Add(new TextureBlend(reader));
            }

            VertexBuffer = new VertexBuffer(reader);
            uint primitiveBufferCount = readPrimBufferCount ? reader.ReadUInt32() : 0;
            IndexBuffer = new IndexBuffer(reader);
            IndexBuffer.PrimitiveBufferCount = primitiveBufferCount;
        }
    }
}
