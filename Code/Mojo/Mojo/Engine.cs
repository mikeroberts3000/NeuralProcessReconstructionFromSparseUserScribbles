using System;
using System.Linq;
using Mojo.Interop;
using SlimDX.DXGI;

namespace Mojo
{
    public class Engine : IDisposable
    {
        private Factory mDxgiFactory;
        private SlimDX.Direct3D11.Device mD3D11Device;

        public Dictionary< Viewer > Viewers { get; private set; }

        public Segmenter Segmenter { get; private set; }

        public Engine( Dictionary< RenderingPaneHwndDescription > renderingPaneHwndDescriptions )
        {
            Console.WriteLine( "\nMojo initializing...\n" );

            D3D11.Initialize( out mDxgiFactory, out mD3D11Device );
            Cuda.Initialize( mD3D11Device );
            Thrust.Initialize();

            Segmenter = new Segmenter
            {
                Interop = new Interop.Segmenter( mD3D11Device, mD3D11Device.ImmediateContext, Constants.Parameters )
            };

            Viewers = new Dictionary< Viewer >
                      {
                          {
                              "Segmenter2D", new Viewer
                                             {
                                                 RenderingPane = new RenderingPane( mDxgiFactory,
                                                                                    mD3D11Device,
                                                                                    mD3D11Device.ImmediateContext,
                                                                                    renderingPaneHwndDescriptions.Get( "Segmenter2D" ).Handle,
                                                                                    renderingPaneHwndDescriptions.Get( "Segmenter2D" ).Width,
                                                                                    renderingPaneHwndDescriptions.Get( "Segmenter2D" ).Height,
                                                                                    new Segmenter2D.RenderingStrategy( mD3D11Device,
                                                                                                                       mD3D11Device.ImmediateContext,
                                                                                                                       Segmenter ) ),
                                                 UserInputHandler = new Segmenter2D.UserInputHandler( Segmenter )
                                             }
                              }
                      };
        }

        public void Dispose()
        {
            Viewers.Internal.Values.ToList().ForEach( viewer => viewer.Dispose() );
            Segmenter.Dispose();

            Thrust.Terminate();
            Cuda.Terminate();
            D3D11.Terminate( ref mDxgiFactory, ref mD3D11Device );

            Console.WriteLine( "\nMojo terminating...\n" );
        }

        public void Update()
        {
            Segmenter.Update();
            Viewers.Internal.ToList().ForEach( viewer => viewer.Value.RenderingPane.Render() );
        }
    }
}
