using System;
using System.Windows;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace Mojo
{
    public class RenderingPane : IDisposable
    {
        private readonly Factory mDxgiFactory;
        private readonly SlimDX.Direct3D11.Device mD3D11Device;
        private readonly DeviceContext mD3D11DeviceContext;
        private readonly SwapChain mSwapChain;
        private Texture2D mD3D11RenderTargetTexture2D;
        private Texture2D mD3D11DepthStencilTexture2D;
        private RenderTargetView mD3D11RenderTargetView;
        private DepthStencilView mD3D11DepthStencilView;
        private Viewport mViewport;

        public IRenderingStrategy RenderingStrategy { get; private set; }

        public RenderingPane( Factory dxgiFactory, SlimDX.Direct3D11.Device d3D11Device, DeviceContext d3D11DeviceContext, IntPtr handle, int width, int height, IRenderingStrategy renderingStrategy )
        {
            mDxgiFactory = dxgiFactory;
            mD3D11Device = d3D11Device;
            mD3D11DeviceContext = d3D11DeviceContext;
            RenderingStrategy = renderingStrategy;

            var swapChainDescription = new SwapChainDescription
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription( width,
                                            height,
                                            new Rational( 60, 1 ),
                                            Format.R8G8B8A8_UNorm ),
                IsWindowed = true,
                OutputHandle = handle,
                SampleDescription = new SampleDescription( 1, 0 ),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            mSwapChain = new SwapChain( mDxgiFactory, mD3D11Device, swapChainDescription );
            mDxgiFactory.SetWindowAssociation( handle, WindowAssociationFlags.IgnoreAll );

            CreateD3D11Resources( width, height );
        }

        public void Dispose()
        {
            DestroyD3D11Resources();

            mSwapChain.Dispose();
            RenderingStrategy.Dispose();
        }

        public void Render()
        {
            mD3D11DeviceContext.OutputMerger.SetTargets( mD3D11DepthStencilView, mD3D11RenderTargetView );
            mD3D11DeviceContext.Rasterizer.SetViewports( mViewport );
            RenderingStrategy.Render( mD3D11DeviceContext, mViewport, mD3D11RenderTargetView, mD3D11DepthStencilView );
            mSwapChain.Present( 0, PresentFlags.None );
        }

        public void SetSize( Size size )
        {
            DestroyD3D11Resources();

            mSwapChain.ResizeBuffers( 1,
                                        (int)size.Width,
                                        (int)size.Height,
                                        Format.R8G8B8A8_UNorm,
                                        SwapChainFlags.None );

            mSwapChain.ResizeTarget( new ModeDescription( (int)size.Width,
                                                            (int)size.Height,
                                                            new Rational( 60, 1 ),
                                                            Format.R8G8B8A8_UNorm ) );

            CreateD3D11Resources( (int)size.Width, (int)size.Height );
            Render();
        }

        private void CreateD3D11Resources( int width, int height )
        {
            mD3D11RenderTargetTexture2D = SlimDX.Direct3D11.Resource.FromSwapChain<Texture2D>( mSwapChain, 0 );
            mD3D11RenderTargetView = new RenderTargetView( mD3D11Device, mD3D11RenderTargetTexture2D );

            var depthStencilTexture2DDescription = new Texture2DDescription
            {
                BindFlags = BindFlags.DepthStencil,
                Format = Format.D24_UNorm_S8_UInt,
                Width = width,
                Height = height,
                MipLevels = 1,
                SampleDescription = new SampleDescription( 1, 0 ),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.Shared,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };

            mD3D11DepthStencilTexture2D = new Texture2D( mD3D11Device, depthStencilTexture2DDescription );
            mD3D11DepthStencilView = new DepthStencilView( mD3D11Device, mD3D11DepthStencilTexture2D );

            mViewport = new Viewport( 0, 0, width, height, 0f, 1f );
        }

        private void DestroyD3D11Resources()
        {
            mD3D11DepthStencilView.Dispose();
            mD3D11DepthStencilTexture2D.Dispose();

            mD3D11RenderTargetView.Dispose();
            mD3D11RenderTargetTexture2D.Dispose();
        }
    }
}
