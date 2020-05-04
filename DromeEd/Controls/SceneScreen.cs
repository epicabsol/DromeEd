//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.IO;
using System.Runtime.InteropServices;
using DromeEd.Drome;

namespace DromeEd.Controls
{
    public class Camera
    {
        public Vector3 Position;
        public float Yaw;
        public float Pitch;
        //public float Offset = 5;
        public SlideValue Offset = new SlideValue(100, 50);

        public float FOV = MathUtil.PiOverTwo;
        public float ZNear = 0.2f;
        public float ZFar = 15000.0f;
        public float Aspect = 1.0f;

        public Camera()
        {
            Offset.Speed = 0.3f;
            Offset.Threshold = 0.1f;
        }

        public Matrix BuildViewMatrix()
        {
            return Matrix.Translation(-Position) * Matrix.RotationY(-Yaw) * Matrix.RotationX(-Pitch) * Matrix.Translation(new Vector3(0, 0, Offset.CurrentValue));
        }

        public Matrix BuildProjectionMatrix()
        {
            return Matrix.PerspectiveFovLH(FOV, Aspect, ZNear, ZFar);
        }
    }

    public class SceneNode
    {
        public List<SceneNode> Children = new List<SceneNode>();

        public Matrix LocalTransform { get; set; } = Matrix.Identity;
        public Matrix WorldTransform { get; protected set; }

        public bool Visible { get; set; } = true;

        public void Render(SceneScreen screen)
        {
            if (Visible)
            {
                OnRender(screen);
                foreach (SceneNode child in Children)
                    child.Render(screen);
            }
        }

        protected virtual void OnRender(SceneScreen screen)
        {

        }

        public void Update(float timeStep, Matrix parentTransform)
        {
            OnUpdate(timeStep, parentTransform);
            foreach (SceneNode child in Children)
            {
                child.Update(timeStep, WorldTransform);
            }
        }

        protected virtual void OnUpdate(float timeStep, Matrix parentTransform)
        {
            WorldTransform = LocalTransform * parentTransform;
        }
    }

    public class StaticMeshNode : SceneNode
    {
        public SceneScreen.Mesh Mesh { get; set; } = null;
        public bool Solid = true;
        public bool Wireframe = false;

        public StaticMeshNode(SceneScreen.Mesh mesh ) : base()
        {
            Mesh = mesh;
        }

        protected override void OnRender(SceneScreen screen)
        {
            if (Mesh != null)
            {
                if (Solid)
                {
                    screen.RenderMesh(Mesh, WorldTransform);
                }
                if (Wireframe)
                {
                    screen.Renderer.D3DContext.Rasterizer.State = screen.WireframeRasterizerState;
                    screen.RenderMesh(Mesh, WorldTransform, screen.GetTexture("__blank"));
                    screen.Renderer.D3DContext.Rasterizer.State = screen.DefaultRasterizerState;
                }
            }
        }

        protected override void OnUpdate(float timeStep, Matrix parentTransform)
        {
            base.OnUpdate(timeStep, parentTransform);
            Mesh.DiffuseTexture?.Update(timeStep);
        }
    }

    public class SceneScreen : Screen
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct WorldVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector4 Color;
            public Vector2 UV0;
            public Vector2 UV1;
            public Vector2 UV2;
            public Vector2 UV3;

            public WorldVertex(Vector3 position, Vector3 normal, Vector4 color, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
            {
                Position = position;
                Normal = normal;
                Color = color;
                UV0 = uv0;
                UV1 = uv1;
                UV2 = uv2;
                UV3 = uv3;
            }

            public WorldVertex(float x, float y, float z, float nx, float ny, float nz, float r, float g, float b, float a)
            {
                Position = new Vector3(x, y, z);
                Normal = new Vector3(nx, ny, nz);
                Color = new Vector4(r, g, b, a);
                UV0 = UV1 = UV2 = UV3 = Vector2.Zero;
            }
        }

        public class Mesh : System.IDisposable
        {
            public Buffer VertexBuffer;
            public Buffer IndexBuffer;
            public SharpDX.Direct3D.PrimitiveTopology PrimitiveType;
            public int IndexCount;
            public RenderTexture DiffuseTexture;
            public bool SamplerMirrorU = false;
            public bool SamplerMirrorV = false;

            public Mesh(SharpDX.Direct3D11.Device device, WorldVertex[] vertices, uint[] indices, SharpDX.Direct3D.PrimitiveTopology topology)
            {
                Create(device, vertices, indices, topology);
            }

            public Mesh(SharpDX.Direct3D11.Device device, RenderGroup rg)
            {
                WorldVertex[] vertices = new WorldVertex[rg.VertexCount];

                // Copy the vertex data into the render vertices
                for (int i = 0; i < vertices.Length; i++)
                {
                    // Color
                    if (rg.VertexBuffer.VertexComponents.HasFlag(VertexComponent.Color))
                        vertices[i].Color = new Vector4(rg.VertexBuffer.VertexData[i * rg.VertexBuffer.VertexSize + rg.VertexBuffer.ColorOffset + 2] / 255.0f,
                                                        rg.VertexBuffer.VertexData[i * rg.VertexBuffer.VertexSize + rg.VertexBuffer.ColorOffset + 1] / 255.0f,
                                                        rg.VertexBuffer.VertexData[i * rg.VertexBuffer.VertexSize + rg.VertexBuffer.ColorOffset] / 255.0f,
                                                        rg.VertexBuffer.VertexData[i * rg.VertexBuffer.VertexSize + rg.VertexBuffer.ColorOffset + 3] / 255.0f);
                    else
                        vertices[i].Color = Vector4.One;

                    // Normal
                    if (rg.VertexBuffer.VertexComponents.HasFlag(VertexComponent.Normal))
                        vertices[i].Normal = new Vector3(System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.NormalOffset),
                                                         System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.NormalOffset + 4),
                                                         System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.NormalOffset + 8));

                    // Position
                    if (rg.VertexBuffer.VertexComponents.HasFlag(VertexComponent.Normal))
                        vertices[i].Position = new Vector3(System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.PositionOffset),
                                                           System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.PositionOffset + 4),
                                                           System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.PositionOffset + 8));

                    // Texcoords
                    if (rg.VertexBuffer.VertexComponents.HasFlag(VertexComponent.UV))
                    {
                        vertices[i].UV0 = new Vector2(System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.UVOffset),
                                                      System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.UVOffset + 4));
                        if (rg.VertexBuffer.UVCount > 1)
                            vertices[i].UV1 = new Vector2(System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.UVOffset + 8),
                                                          System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.UVOffset + 12));
                        if (rg.VertexBuffer.UVCount > 2)
                            vertices[i].UV2 = new Vector2(System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.UVOffset + 16),
                                                          System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.UVOffset + 20));
                        if (rg.VertexBuffer.UVCount > 3)
                            vertices[i].UV3 = new Vector2(System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.UVOffset + 24),
                                                          System.BitConverter.ToSingle(rg.VertexBuffer.VertexData, i * (int)rg.VertexBuffer.VertexSize + (int)rg.VertexBuffer.UVOffset + 28));
                    }

                }

                uint[] indices = new uint[rg.IndexBuffer.IndexCount];
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] = System.BitConverter.ToUInt16(rg.IndexBuffer.IndexData, i * 2);
                }

                SharpDX.Direct3D.PrimitiveTopology topology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                switch (rg.IndexBuffer.PrimitiveType)
                {
                    case Drome.PrimitiveType.LineList:
                        topology = SharpDX.Direct3D.PrimitiveTopology.LineList; break;
                    case Drome.PrimitiveType.LineStrip:
                        topology = SharpDX.Direct3D.PrimitiveTopology.LineStrip; break;
                    case Drome.PrimitiveType.PointList:
                        topology = SharpDX.Direct3D.PrimitiveTopology.PointList; break;
                    case Drome.PrimitiveType.TriangleFan:
                        throw new System.Exception();
                    case Drome.PrimitiveType.TriangleList:
                        topology = SharpDX.Direct3D.PrimitiveTopology.TriangleList; break;
                    case Drome.PrimitiveType.TriangleStrip:
                        topology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip; break;
                }

                Create(device, vertices, indices, topology);
            }

            private void Create(SharpDX.Direct3D11.Device device, WorldVertex[] vertices, uint[] indices, SharpDX.Direct3D.PrimitiveTopology topology)
            {
                VertexBuffer = Buffer.Create(device, vertices, new BufferDescription(Utilities.SizeOf<WorldVertex>() * vertices.Length, BindFlags.VertexBuffer, ResourceUsage.Immutable));

                IndexBuffer = Buffer.Create(device, indices, new BufferDescription(Utilities.SizeOf<uint>() * indices.Length, BindFlags.IndexBuffer, ResourceUsage.Immutable));

                PrimitiveType = topology;
                IndexCount = indices.Length;
            }

            public void Dispose()
            {
                VertexBuffer.Dispose();
                IndexBuffer.Dispose();
            }
        }

        public struct FrameData
        {
            public Matrix Projection;
            public Matrix View;
        }

        public struct InstanceData
        {
            public Matrix Model;
        }

        public class ConstantBuffer<T> : System.IDisposable where T : struct
        {
            public Buffer Buffer = null;

            private T _value;
            public T Value
            {
                get
                {
                    return _value;
                }
            }

            public ConstantBuffer(SharpDX.Direct3D11.Device device, T value = default(T))
            {
                _value = value;
                Buffer = Buffer.Create<T>(device, ref value, new BufferDescription(Utilities.SizeOf<T>(), BindFlags.ConstantBuffer, ResourceUsage.Default));
            }

            public void SetValue(SharpDX.Direct3D11.DeviceContext context, T value)
            {
                _value = value;
                context.UpdateSubresource<T>(ref value, Buffer);
            }

            public void Dispose()
            {
                Buffer.Dispose();
            }
        }

        public class RenderTexture
        {
            public virtual ShaderResourceView SRV { get; }

            public RenderTexture()
            {

            }

            public RenderTexture(ShaderResourceView srv)
            {
                SRV = srv;
            }
            
            public virtual void Update(float timeStep)
            {

            }
        }

        public class AnimatedTexture : RenderTexture
        {
            private class Frame
            {
                public ShaderResourceView SRV;
                public float Length;

                public Frame(string animationFilename, Drome.IFLFile.IFLFrame frame, SceneScreen screen)
                {
                    SRV = screen.GetTexture(Path.Combine(Path.GetDirectoryName(animationFilename), frame.TextureFilename.Replace(".tga", ".pc texture")));
                    Length = frame.DurationTicks / 30.0f;
                }
            }

            public override ShaderResourceView SRV { get => CurrentFrame.SRV; }

            private List<Frame> Frames = new List<Frame>();
            public float ElapsedTime;
            public int CurrentFrameIndex = 0;
            private Frame CurrentFrame { get => Frames[CurrentFrameIndex]; }

            public AnimatedTexture(Drome.IFLFile file, SceneScreen screen)
            {
                foreach (Drome.IFLFile.IFLFrame frame in file.Frames)
                {
                    Frames.Add(new Frame(file.Filename, frame, screen));
                }
            }

            public override void Update(float timeStep)
            {
                ElapsedTime += timeStep;
                while (ElapsedTime >= CurrentFrame.Length)
                {
                    ElapsedTime -= CurrentFrame.Length;
                    CurrentFrameIndex += 1;
                    if (CurrentFrameIndex == Frames.Count)
                        CurrentFrameIndex = 0;
                }
            }

        }

        private class LoadedTexture : System.IDisposable
        {
            public string Filename { get; }
            public ShaderResourceView SRV { get; } = null;
            private Texture2D Texture = null;

            public LoadedTexture(SharpDX.Direct3D11.Device device, byte[] BGRAData, int width, int height)
            {
                System.IntPtr texdata = Marshal.UnsafeAddrOfPinnedArrayElement(BGRAData, 0);
                Texture = new Texture2D(device, new Texture2DDescription() { ArraySize = 1, BindFlags = BindFlags.ShaderResource, CpuAccessFlags = CpuAccessFlags.None, Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm, Width = width, Height = height, MipLevels = 1, Usage = ResourceUsage.Immutable, SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0) },
                    new DataBox[] { new DataBox(texdata, 4 * width, 0) });
                SRV = new ShaderResourceView(device, Texture);
            }

            public static LoadedTexture LoadPCTexture(SharpDX.Direct3D11.Device device, string filename)
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(Program.Filesystem.GetFileData(Program.Filesystem.GetFileEntry(filename))))
                using (System.IO.BinaryReader reader = new System.IO.BinaryReader(ms))
                {
                    Drome.Texture tex = new Drome.Texture(reader);
                    byte[] data = tex.DumpPixelData();
                    return new LoadedTexture(device, data, (int)tex.Width, (int)tex.Height);
                }
            }

            public static LoadedTexture LoadTGA(SharpDX.Direct3D11.Device device, byte[] TGAData)
            {
                using (MemoryStream ms = new MemoryStream(TGAData))
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    // Read TGA header
                    byte IDLength = reader.ReadByte();
                    byte ColorMapType = reader.ReadByte();
                    byte DataTypeCode = reader.ReadByte();
                    ushort ColorMapOrigin = reader.ReadUInt16();
                    ushort ColorMapLength = reader.ReadUInt16();
                    byte ColorMapBitDepth = reader.ReadByte();
                    ushort XOrigin = reader.ReadUInt16();
                    ushort YOrigin = reader.ReadUInt16();
                    ushort Width = reader.ReadUInt16();
                    ushort Height = reader.ReadUInt16();
                    byte BitDepth = reader.ReadByte();
                    byte ImageDescriptor = reader.ReadByte();
                    string ID = "";
                    if (IDLength > 0)
                        ID = Encoding.ASCII.GetString(reader.ReadBytes(IDLength));

                    if (BitDepth != 32)
                        throw new System.BadImageFormatException("TGAs need to be 32bit!");

                    if (DataTypeCode == 1)
                    {
                        throw new System.BadImageFormatException("No paletted TGAs!");
                    }
                    else if (DataTypeCode == 2)
                    {
                        if (ColorMapLength > 0)
                            ms.Seek(ColorMapLength, SeekOrigin.Current);

                        return new LoadedTexture(device, reader.ReadBytes(Width * Height * 4), Width, Height);
                    }
                    else
                    {
                        throw new System.BadImageFormatException("Bad TGA format!");
                    }
                }
            }

            public void Dispose()
            {
                SRV.Dispose();
                Texture.Dispose();
            }
        }
        private Dictionary<string, LoadedTexture> TextureCache = new Dictionary<string, LoadedTexture>();

        public VertexShader WorldVertexShader = null;
        public PixelShader WorldPixelShader = null;

        public ConstantBuffer<FrameData> FrameConstantBuffer;
        public ConstantBuffer<InstanceData> InstanceConstantBuffer;

        public InputLayout WorldInputLayout = null;
        public DepthStencilState DepthState = null;

        public Texture2D Depthbuffer = null;
        public DepthStencilView DepthbufferDSV = null;

        public SamplerState DefaultSamplerState = null;
        public RasterizerState DefaultRasterizerState = null;
        public RasterizerState WireframeRasterizerState = null;
        public BlendState AlphaBlendState = null;

        public List<SceneNode> Nodes = new List<SceneNode>();

        public Camera Camera = new Camera();

        public event System.EventHandler<float> Updated;

        public SceneScreen(D3DRenderer renderer) : base(renderer)
        {

        }

        public override void OnCreate()
        {
            WorldVertexShader = new VertexShader(Renderer.D3DDevice, System.IO.File.ReadAllBytes("Shaders\\WorldVertexShader.cso"));
            WorldPixelShader = new PixelShader(Renderer.D3DDevice, System.IO.File.ReadAllBytes("Shaders\\WorldPixelShader.cso"));
            WorldInputLayout = new InputLayout(Renderer.D3DDevice, System.IO.File.ReadAllBytes("Shaders\\WorldVertexShader.cso"), new InputElement[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0),
                new InputElement("TEXCOORD", 1, Format.R32G32_Float, 0),
                new InputElement("TEXCOORD", 2, Format.R32G32_Float, 0),
                new InputElement("TEXCOORD", 3, Format.R32G32_Float, 0) });

            Renderer.D3DContext.InputAssembler.InputLayout = WorldInputLayout;
            Renderer.D3DContext.VertexShader.Set(WorldVertexShader);
            Renderer.D3DContext.PixelShader.Set(WorldPixelShader);

            FrameConstantBuffer = new ConstantBuffer<FrameData>(Renderer.D3DDevice);
            InstanceConstantBuffer = new ConstantBuffer<InstanceData>(Renderer.D3DDevice);
            Renderer.D3DContext.VertexShader.SetConstantBuffers(0, new Buffer[] { FrameConstantBuffer.Buffer, InstanceConstantBuffer.Buffer });
            DepthState = new DepthStencilState(Renderer.D3DDevice, new DepthStencilStateDescription()
            {
                DepthComparison = Comparison.Less,
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                IsStencilEnabled = false
            });

            SamplerStateDescription ssdesc = new SamplerStateDescription() { AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap, Filter = Filter.MinMagMipLinear, AddressW = TextureAddressMode.Wrap, BorderColor = new Color(255, 0, 0, 255), ComparisonFunction = Comparison.Never, MaximumAnisotropy = 1, MaximumLod = float.MaxValue, MinimumLod = float.MinValue, MipLodBias = 0.0f };
            DefaultSamplerState = new SamplerState(Renderer.D3DDevice, ssdesc);
            Renderer.D3DContext.PixelShader.SetSampler(0, DefaultSamplerState);
            TextureCache.Add("__error", LoadedTexture.LoadTGA(Renderer.D3DDevice, Properties.Resources.ErrorTexture));
            TextureCache.Add("__blank", LoadedTexture.LoadTGA(Renderer.D3DDevice, Properties.Resources.NoTexture));

            RasterizerStateDescription rsdesc = new RasterizerStateDescription()
            {
                CullMode = CullMode.Back,
                FillMode = FillMode.Solid,
            };
            DefaultRasterizerState = new RasterizerState(Renderer.D3DDevice, rsdesc);
            rsdesc.FillMode = FillMode.Wireframe;
            WireframeRasterizerState = new RasterizerState(Renderer.D3DDevice, rsdesc);
            BlendStateDescription bsdesc = new BlendStateDescription();
            bsdesc.RenderTarget[0] = new RenderTargetBlendDescription()
            {
                AlphaBlendOperation = BlendOperation.Add,
                BlendOperation = BlendOperation.Add,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                DestinationAlphaBlend = BlendOption.One,
                SourceBlend = BlendOption.SourceAlpha,
                SourceAlphaBlend = BlendOption.One,
                IsBlendEnabled = true,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            };
            AlphaBlendState = new BlendState(Renderer.D3DDevice, bsdesc);
            Renderer.D3DContext.OutputMerger.BlendState = AlphaBlendState;
        }

        public override void OnCreateBuffers()
        {
            if (Renderer.Width == 0 || Renderer.Height == 0)
                return;
            Depthbuffer = new Texture2D(Renderer.D3DDevice, new Texture2DDescription()
            {
                ArraySize = 1,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.D32_Float,
                Height = Renderer.Height,
                Width = Renderer.Width,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default
            });
            DepthbufferDSV = new DepthStencilView(Renderer.D3DDevice, Depthbuffer);
            Renderer.D3DContext.OutputMerger.SetRenderTargets(DepthbufferDSV, Renderer.BackbufferRTV);
            Renderer.D3DContext.OutputMerger.SetDepthStencilState(DepthState);
        }

        public override void OnDispose()
        {
            AlphaBlendState.Dispose();
            DefaultRasterizerState.Dispose();
            WireframeRasterizerState.Dispose();

            foreach (LoadedTexture tex in TextureCache.Values)
            {
                tex.Dispose();
            }
            TextureCache.Clear();

            DepthState?.Dispose();
            DepthState = null;
            WorldInputLayout?.Dispose();
            WorldInputLayout = null;
            WorldPixelShader?.Dispose();
            WorldPixelShader = null;
            WorldVertexShader?.Dispose();
            WorldVertexShader = null;
        }

        public override void OnDisposeBuffers()
        {
            Renderer.D3DContext.OutputMerger.SetRenderTargets((RenderTargetView)null);
            DepthbufferDSV?.Dispose();
            DepthbufferDSV = null;
            Depthbuffer?.Dispose();
            Depthbuffer = null;
        }

        public override void OnRender()
        {
            Renderer.D3DContext.ClearRenderTargetView(Renderer.BackbufferRTV, new Color(0, 0, 0, 255));
            Renderer.D3DContext.ClearDepthStencilView(DepthbufferDSV, DepthStencilClearFlags.Depth, 1.0f, 0x00);

            foreach (SceneNode node in Nodes)
            {
                node.Render(this);
            }

            Renderer.DXGISwapChain.Present(0, PresentFlags.None);
        }

        public void RenderMesh(Mesh mesh, Matrix transform, ShaderResourceView diffuseTextureOverride = null)
        {
            InstanceData instanceData = new InstanceData();
            instanceData.Model = transform;
            InstanceConstantBuffer.SetValue(Renderer.D3DContext, instanceData);

            Renderer.D3DContext.PixelShader.SetShaderResource(0, diffuseTextureOverride ?? mesh.DiffuseTexture?.SRV);
            Renderer.D3DContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(mesh.VertexBuffer, Utilities.SizeOf<WorldVertex>(), 0));
            Renderer.D3DContext.InputAssembler.SetIndexBuffer(mesh.IndexBuffer, Format.R32_UInt, 0);
            Renderer.D3DContext.InputAssembler.PrimitiveTopology = mesh.PrimitiveType;
            Renderer.D3DContext.DrawIndexed(mesh.IndexCount, 0, 0);
        }

        public override void OnResizeBuffers()
        {
            if (Renderer.Width == 0 || Renderer.Height == 0)
                return;
            Camera.Aspect = Renderer.Width / (float)Renderer.Height;
        }

        public override void OnUpdate(float timeStep)
        {
            FrameData frameData = new FrameData();
            frameData.Projection = Camera.BuildProjectionMatrix();
            frameData.View = Camera.BuildViewMatrix();
            FrameConstantBuffer.SetValue(Renderer.D3DContext, frameData);

            Updated?.Invoke(this, timeStep);
            //Camera.Yaw += timeStep;
            Camera.Offset.Update();

            foreach (SceneNode node in Nodes)
                node.Update(timeStep, Matrix.Identity);
        }


        private bool Orbiting = false;
        private bool Panning = false;
        private bool Zooming = false;
        private int LastX = 0;
        private int LastY = 0;
        public override void OnMouseDown(int x, int y, MouseButtons button)
        {
            base.OnMouseDown(x, y, button);
            if (button == MouseButtons.Left)
            {
                Orbiting = true;
            }
            if (button == MouseButtons.Middle)
            {
                Panning = true;
            }
            if (button == MouseButtons.Right)
            {
                Zooming = true;
            }
            LastX = x;
            LastY = y;
        }

        public override void OnMouseMove(int x, int y)
        {
            base.OnMouseMove(x, y);

            if (Orbiting)
            {
                Camera.Yaw += (x - LastX) * 0.0075f;
                Camera.Pitch += (y - LastY) * 0.01f;
                if (Camera.Pitch > MathUtil.PiOverTwo)
                    Camera.Pitch = MathUtil.PiOverTwo;
                else if (Camera.Pitch < -MathUtil.PiOverTwo)
                    Camera.Pitch = -MathUtil.PiOverTwo;
            }
            if (Panning)
            {
                Matrix m = Camera.BuildViewMatrix();
                m.Invert();
                Camera.Position += m.Left * (x - LastX);
                Camera.Position += m.Up * (y - LastY);
            }
            if (Zooming)
            {
                Camera.Offset.TargetValue = (1.0f + (y - LastY) * 0.03f) * Camera.Offset.TargetValue;
            }

            LastX = x;
            LastY = y;
        }

        public override void OnMouseUp(int x, int y, MouseButtons button)
        {
            base.OnMouseUp(x, y, button);
            if (button == MouseButtons.Left)
            {
                Orbiting = false;
            }
            if (button == MouseButtons.Middle)
            {
                Panning = false;
            }
            if (button == MouseButtons.Right)
            {
                Zooming = false;
            }
        }

        public override void OnMouseWheel(int amount)
        {
            base.OnMouseWheel(amount);
            Camera.Offset.TargetValue += ((float)amount / 120.0f) * Camera.Offset.TargetValue * -0.3f;
        }

        public RenderTexture LoadTextureReference(TextureReference r)
        {
            if (r.MapType == Drome.Texture.MapType.Base)
            {
                try
                {
                    if (r.MapName.ToLower().EndsWith(".ifl"))
                    {
                        using (MemoryStream ms = new MemoryStream(Program.Filesystem.GetFileData(Program.Filesystem.GetFileEntry(r.MapName))))
                        {
                            Drome.IFLFile ifl = new Drome.IFLFile(ms, r.MapName);
                            AnimatedTexture a = new AnimatedTexture(ifl, this);
                            return a;
                        }
                    }
                    else
                    {
                        RenderTexture t = new RenderTexture(GetTexture(r.MapName.Replace(".tga", ".pc texture")));
                        return t;
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Exception loading base texture '" + r.MapName + "'.");
                    return null;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Extra TextureReference map type: " + r.MapType.ToString() + " (" + r.MapType + ")");
                return null;
            }
        }

        public List<Mesh> LoadModel(Drome.MD2File model)
        {
            Drome.GeometryBlock geo = model.Blocks[Drome.BlockHeader.MAGIC_GEOMETRY2] as Drome.GeometryBlock;
            Drome.MDLBlock mdl = model.Blocks[Drome.BlockHeader.MAGIC_MODEL3] as Drome.MDLBlock;
            List<Mesh> result = new List<Mesh>();

            List<RenderTexture> textures = new List<RenderTexture>();

            int i = 0;
            foreach (Drome.TextureReference r in mdl.TextureReferences)
            {
                textures.Add(LoadTextureReference(r));
                i++;
            }

            foreach (Drome.GeometryBlock.LOD lod in geo.LODs)
            {
                //if (lod.Type != Drome.GEO2Block.LOD.LODLevel.GeometryNormalDetail)
                    //continue;
                foreach (RenderGroup rg in lod.RenderGroups)
                {
                    Mesh mesh = new Mesh(Renderer.D3DDevice, rg);
                    mesh.DiffuseTexture = new RenderTexture(GetTexture("__blank"));
                    if (rg.Material < mdl.Materials.Count)
                    {
                        Drome.MaterialProps mat = mdl.Materials[rg.Material];
                        // TODO: Use mat
                    }
                    else
                    {
                        mesh.DiffuseTexture = new RenderTexture(GetTexture("__error"));
                        continue;
                        //throw new Exception("RenderGroup specified a material outside the materials defined!");
                    }

                    foreach (TextureBlend blend in rg.TextureBlends)
                    {
                        if (blend.Effect == Drome.Texture.MapType.Base)
                        {
                            System.Diagnostics.Debug.WriteLine("Found base map!");
                            if (blend.TextureIndex < textures.Count)
                            {
                                mesh.DiffuseTexture = textures[blend.TextureIndex];
                                mesh.SamplerMirrorU = blend.TilingInfo.HasFlag(TextureTileParam.MirrorU);
                                mesh.SamplerMirrorV = blend.TilingInfo.HasFlag(TextureTileParam.MirrorV);
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("ERROR: TextureBlend.TextureIndex out of range!");
                                mesh.DiffuseTexture = new RenderTexture(GetTexture("__error"));
                            }
                        }
                    }

                   result.Add(mesh);
                }
            }
            return result;
        }

        #region Texture Cache

        public ShaderResourceView GetTexture(string key)
        {
            key = key.ToLower();
            // Return the texture if it's already loaded
            if (TextureCache.ContainsKey(key))
                return TextureCache[key].SRV;

            try
            {
                ATD.VFS.FileEntry file = Program.Filesystem.GetFileEntry(key);
                if (file == null)
                    return TextureCache["__error"].SRV;
                LoadedTexture tex = LoadedTexture.LoadPCTexture(Renderer.D3DDevice, file.Filename);
                TextureCache.Add(key, tex);
                return tex.SRV;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception loading texture: \n " + ex.ToString());
                return TextureCache["__error"].SRV; // TODO: return ErrorTexture
            }
        }

        #endregion
    }
}
