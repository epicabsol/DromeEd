using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using System.Windows.Forms;
using SharpDX.DXGI;

namespace DromeEd.Controls
{
    public abstract class Screen
    {
        public D3DRenderer Renderer { get; private set; }

        public Screen(D3DRenderer renderer)
        {
            Renderer = renderer;
        }

        public abstract void OnCreate();
        public abstract void OnCreateBuffers();
        public abstract void OnResizeBuffers();
        public abstract void OnDisposeBuffers();
        public abstract void OnDispose();

        public abstract void OnUpdate(float timeStep);
        public abstract void OnRender();

        public virtual void OnMouseDown(int x, int y, MouseButtons button)
        {

        }

        public virtual void OnMouseUp(int x, int y, MouseButtons button)
        {

        }

        public virtual void OnMouseMove(int x, int y)
        {

        }

        public virtual void OnKeyDown(Keys key)
        {

        }

        public virtual void OnKeyUp(Keys key)
        {

        }

        public virtual void OnMouseWheel(int amount)
        {

        }
    }

    public class D3DRenderer : RenderControl
    {
        public SwapChain DXGISwapChain { get; private set; } = null;
        public SharpDX.Direct3D11.Device D3DDevice { get; private set; } = null;
        public DeviceContext D3DContext { get; private set; } = null;

        public Texture2D Backbuffer = null;
        public RenderTargetView BackbufferRTV = null;
        public Texture2D Depthbuffer = null;
        public DepthStencilView DepthbufferDSV = null;

        public Screen Screen { get; set; }

        public D3DRenderer()
        {

        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode)
                Initialize();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (!DesignMode)
                Destroy();
            base.OnHandleDestroyed(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
                base.OnPaint(e);
            else
                OnRender();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                Destroy();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Screen?.OnMouseDown(e.X, e.Y, e.Button);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Screen?.OnMouseMove(e.X, e.Y);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Screen?.OnMouseUp(e.X, e.Y, e.Button);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            Screen?.OnKeyDown(e.KeyCode);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            Screen?.OnKeyUp(e.KeyCode);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Screen?.OnMouseWheel(e.Delta);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (D3DContext == null)
                return;

            if (Width == 0 || Height == 0)
                return;

            D3DContext.OutputMerger.SetRenderTargets((RenderTargetView)null);
            BackbufferRTV.Dispose();
            Backbuffer.Dispose();
            Screen.OnDisposeBuffers();

            D3DContext.Rasterizer.SetViewport(new Viewport(0, 0, Width, Height));
            DXGISwapChain.ResizeBuffers(1, Width, Height, Format.Unknown, SwapChainFlags.None);
            Screen.OnResizeBuffers();

            Backbuffer = DXGISwapChain.GetBackBuffer<Texture2D>(0);
            BackbufferRTV = new RenderTargetView(D3DDevice, Backbuffer);
            D3DContext.OutputMerger.SetRenderTargets(BackbufferRTV);
            Screen.OnCreateBuffers();
        }

        public void Initialize()
        {
            SwapChainDescription scdesc = new SwapChainDescription() { BufferCount = 1, IsWindowed = true, Flags = SwapChainFlags.None, ModeDescription = new ModeDescription(Width, Height, new Rational(60, 1), Format.B8G8R8A8_UNorm), OutputHandle = Handle, SampleDescription = new SampleDescription(1, 0), SwapEffect = SwapEffect.Discard, Usage = Usage.RenderTargetOutput };
            DeviceCreationFlags flags = DeviceCreationFlags.BgraSupport;
#if DEBUG
            flags |= DeviceCreationFlags.Debug;
#endif
            SharpDX.Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, flags, scdesc, out SharpDX.Direct3D11.Device device, out SwapChain sc);
            DXGISwapChain = sc;
            D3DDevice = device;
            D3DContext = D3DDevice.ImmediateContext;

            Backbuffer = DXGISwapChain.GetBackBuffer<Texture2D>(0);
            BackbufferRTV = new RenderTargetView(D3DDevice, Backbuffer);
            D3DContext.OutputMerger.SetRenderTargets(BackbufferRTV);

            Screen.OnCreate();

            D3DContext.Rasterizer.SetViewport(new Viewport(0, 0, Width, Height));
            Screen.OnCreateBuffers();

            watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            Application.Idle += OnTick;
        }

        public void Destroy()
        {
            Application.Idle -= OnTick;

            Screen.OnDisposeBuffers();

            Screen.OnDispose();

            BackbufferRTV.Dispose();

            D3DDevice.Dispose();
            DXGISwapChain.Dispose();
        }

        private System.Diagnostics.Stopwatch watch;
        private void OnTick(object sender, EventArgs e)
        {
            // Calculate time delta
            float elapsed = (float)watch.Elapsed.TotalSeconds;
            watch.Restart();

            // Get input ??

            // Update scene
            Screen.OnUpdate(elapsed);

            // Render scene
            OnRender();
        }

        private void OnRender()
        {
            if (DesignMode)
                return;

            if (!Visible)
                return;

            Screen.OnRender();
        }
    }
}
