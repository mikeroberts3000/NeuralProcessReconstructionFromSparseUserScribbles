using System.Collections.Generic;
using System.Diagnostics;
using Mojo.Interop;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using TinyText;

namespace Mojo.Segmenter2D
{
    public class RenderingStrategy : NotifyPropertyChanged, IRenderingStrategy
    {
        private const int POSITION_SLOT = 0;
        private const int TEXCOORD_SLOT = 1;
        private const int POSITION_NUM_BYTES_PER_COMPONENT = 4;
        private const int POSITION_NUM_COMPONENTS_PER_VERTEX = 2;
        private const int TEXCOORD_NUM_BYTES_PER_COMPONENT = 4;
        private const int TEXCOORD_NUM_COMPONENTS_PER_VERTEX = 2;
        private const int NUM_VERTICES = 4;
        private const int MAX_NUM_TEXT_CHARACTERS = 1024;

        private const Format POSITION_FORMAT = Format.R32G32_Float;
        private const Format TEXCOORD_FORMAT = Format.R32G32_Float;

        private static readonly Color4 CLEAR_COLOR = new Color4( 0.5f, 0.5f, 1.0f );

        private static readonly Vector2 POSITION_TOP_LEFT = new Vector2( -1f, 1f );
        private static readonly Vector2 POSITION_BOTTOM_LEFT = new Vector2( -1f, -1f );
        private static readonly Vector2 POSITION_TOP_RIGHT = new Vector2( 1f, 1f );
        private static readonly Vector2 POSITION_BOTTOM_RIGHT = new Vector2( 1f, -1f );

        private static readonly Vector2 TEXCOORD_TOP_LEFT = new Vector2( 0f, 0f );
        private static readonly Vector2 TEXCOORD_BOTTOM_LEFT = new Vector2( 0f, 1f );
        private static readonly Vector2 TEXCOORD_TOP_RIGHT = new Vector2( 1f, 0f );
        private static readonly Vector2 TEXCOORD_BOTTOM_RIGHT = new Vector2( 1f, 1f );

// ReSharper disable InconsistentNaming
        private static readonly IDictionary< string, int > D3D11_CUDA_TEXTURE_RENDERING_MODE_MAP = new Dictionary< string, int >
                                                                                                   {
                                                                                                       { "Default", -2 },
                                                                                                       { "SourceMapDoNotShowSegmentation", -1 },
                                                                                                       { "ColorMap", 0 },
                                                                                                       { "ConstraintMap", 1 },
                                                                                                       { "SourceMap", 2 },
                                                                                                       { "CorrespondenceMap", 3 },
                                                                                                   };
// ReSharper restore InconsistentNaming

        private static readonly IDictionary< ShaderResourceViewDimension, int > TEXTURE_DIMENSIONS_MAP =
            new Dictionary< ShaderResourceViewDimension, int >
            {
                { ShaderResourceViewDimension.Unknown, -1 },
                { ShaderResourceViewDimension.Texture2D, 2 },
                { ShaderResourceViewDimension.Texture3D, 3 }
            };

        private static readonly IDictionary<ShaderResourceViewDimension, string > TEXTURE_DIMENSIONS_NAME_MAP =
            new Dictionary<ShaderResourceViewDimension, string >
            {
                { ShaderResourceViewDimension.Unknown,   "Unknown" },
                { ShaderResourceViewDimension.Texture2D, "gCurrentTexture2D" },
                { ShaderResourceViewDimension.Texture3D, "gCurrentTexture3D" }
            };

        private readonly Stopwatch mStopwatch = new Stopwatch();

        private readonly Segmenter mSegmenter;
        private readonly Context mTinyTextContext;
        private readonly Effect mEffect;
        private readonly EffectPass mPass;
        private readonly InputLayout mInputLayout;
        private readonly Buffer mPositionVertexBuffer;
        private readonly Buffer mTexCoordVertexBuffer;

        private string CurrentTextureName
        {
            get
            {
                return TEXTURE_DIMENSIONS_NAME_MAP.ContainsKey( mSegmenter.D3D11CudaTextureEnumerator.Current.Value.Description.Dimension )
                           ? TEXTURE_DIMENSIONS_NAME_MAP[ mSegmenter.D3D11CudaTextureEnumerator.Current.Value.Description.Dimension ]
                           : TEXTURE_DIMENSIONS_NAME_MAP[ ShaderResourceViewDimension.Unknown ];
            }
        }

        private int CurrentTextureIndex
        {
            get
            {
                if ( !mSegmenter.ShowSegmentation )
                {
                    return D3D11_CUDA_TEXTURE_RENDERING_MODE_MAP[ "SourceMapDoNotShowSegmentation" ];
                }

                return D3D11_CUDA_TEXTURE_RENDERING_MODE_MAP.ContainsKey( mSegmenter.D3D11CudaTextureEnumerator.Current.Key )
                            ? D3D11_CUDA_TEXTURE_RENDERING_MODE_MAP[ mSegmenter.D3D11CudaTextureEnumerator.Current.Key ]
                            : D3D11_CUDA_TEXTURE_RENDERING_MODE_MAP[ "Default" ];
            }
        }

        private int CurrentTextureDimensions
        {
            get
            {
                return TEXTURE_DIMENSIONS_MAP.ContainsKey( mSegmenter.D3D11CudaTextureEnumerator.Current.Value.Description.Dimension )
                           ? TEXTURE_DIMENSIONS_MAP[ mSegmenter.D3D11CudaTextureEnumerator.Current.Value.Description.Dimension ]
                           : TEXTURE_DIMENSIONS_MAP[ ShaderResourceViewDimension.Unknown ];
            }
        }

        private int SplitMode
        {
            get
            {
                return mSegmenter.CurrentSegmenterToolMode == SegmenterToolMode.Split && mSegmenter.SplitNeuralProcess != null ? 1 : 0;
            }
        }

        private Vector3 CurrentNeuralProcessColor
        {
            get
            {
                return mSegmenter.CurrentNeuralProcess != null
                           ? mSegmenter.CurrentNeuralProcess.Color * ( 1.0f / 255.0f )
                           : Constants.NULL_NEURAL_PROCESS.Color;
            }
        }

        private Vector3 SplitNeuralProcessColor
        {
            get
            {
                return mSegmenter.SplitNeuralProcess != null
                           ? mSegmenter.SplitNeuralProcess.Color * ( 1.0f / 255.0f )
                           : Constants.NULL_NEURAL_PROCESS.Color;
            }
        }

        private string FrameTimeString
        {
            get
            {
                return mStopwatch.ElapsedMilliseconds == 0 ? "< 1 ms" : mStopwatch.ElapsedMilliseconds + " ms";
            }
        }

        private string CurrentNeuralProcessString
        {
            get
            {
                return mSegmenter.CurrentNeuralProcess != null ? mSegmenter.CurrentNeuralProcess.Name : "null";
            }
        }

        public float CurrentSliceCoordinate
        {
            get
            {
                return mSegmenter.CurrentSlice / (float)( mSegmenter.Interop.VolumeDescription.NumVoxelsZ - 1 );
            }
        }

        public RenderingStrategy( SlimDX.Direct3D11.Device device, DeviceContext deviceContext, Segmenter segmenter )
        {
            mSegmenter = segmenter;

            mStopwatch.Start();

            mEffect = EffectUtil.CompileEffect( device, @"Shaders\Segmenter2D.fx" );

            // create position vertex data, making sure to rewind the stream afterward
            var positionVertexDataStream = new DataStream( NUM_VERTICES * POSITION_NUM_COMPONENTS_PER_VERTEX * POSITION_NUM_BYTES_PER_COMPONENT, true, true );

            positionVertexDataStream.Write( POSITION_TOP_LEFT );
            positionVertexDataStream.Write( POSITION_BOTTOM_LEFT );
            positionVertexDataStream.Write( POSITION_TOP_RIGHT );
            positionVertexDataStream.Write( POSITION_BOTTOM_RIGHT );
            positionVertexDataStream.Position = 0;

            // create texcoord vertex data, making sure to rewind the stream afterward
            var texCoordVertexDataStream = new DataStream( NUM_VERTICES * TEXCOORD_NUM_COMPONENTS_PER_VERTEX * TEXCOORD_NUM_BYTES_PER_COMPONENT, true, true );

            texCoordVertexDataStream.Write( TEXCOORD_TOP_LEFT );
            texCoordVertexDataStream.Write( TEXCOORD_BOTTOM_LEFT );
            texCoordVertexDataStream.Write( TEXCOORD_TOP_RIGHT );
            texCoordVertexDataStream.Write( TEXCOORD_BOTTOM_RIGHT );
            texCoordVertexDataStream.Position = 0;

            // create the input layout
            var inputElements = new[]
                                {
                                    new InputElement( "POSITION", 0, POSITION_FORMAT, POSITION_SLOT ),
                                    new InputElement( "TEXCOORD", 0, TEXCOORD_FORMAT, TEXCOORD_SLOT )
                                };

            var technique = mEffect.GetTechniqueByName( "Segmenter2D" );
            mPass = technique.GetPassByName( "Segmenter2D" );

            mInputLayout = new InputLayout( device, mPass.Description.Signature, inputElements );

            // create the vertex buffers
            mPositionVertexBuffer = new Buffer( device,
                                                positionVertexDataStream,
                                                NUM_VERTICES * POSITION_NUM_COMPONENTS_PER_VERTEX * POSITION_NUM_BYTES_PER_COMPONENT,
                                                ResourceUsage.Default,
                                                BindFlags.VertexBuffer,
                                                CpuAccessFlags.None,
                                                ResourceOptionFlags.None,
                                                0 );

            mTexCoordVertexBuffer = new Buffer( device,
                                                texCoordVertexDataStream,
                                                NUM_VERTICES * TEXCOORD_NUM_COMPONENTS_PER_VERTEX * TEXCOORD_NUM_BYTES_PER_COMPONENT,
                                                ResourceUsage.Default,
                                                BindFlags.VertexBuffer,
                                                CpuAccessFlags.None,
                                                ResourceOptionFlags.None,
                                                0 );

            System.Threading.Thread.Sleep( 1000 );
            Trace.WriteLine( "\nMojo initializing TinyText.Context (NOTE: TinyText.Context generates D3D11 warnings)...\n" );

            bool result;
            mTinyTextContext = new Context( device, deviceContext, MAX_NUM_TEXT_CHARACTERS, out result );
            Release.Assert( result );

            Trace.WriteLine( "\nMojo finished Initializing TinyText.Context...\n" );
        }

        public void Dispose()
        {
            Trace.WriteLine( "\nMojo disposing TinyText.Context...\n" );

            mTinyTextContext.Dispose();

            Trace.WriteLine( "\nMojo finished disposing TinyText.Context...\n" );
            System.Threading.Thread.Sleep( 1000 );

            mTexCoordVertexBuffer.Dispose();
            mPositionVertexBuffer.Dispose();
            mInputLayout.Dispose();
            mEffect.Dispose();
        }

        public void Render( DeviceContext deviceContext, Viewport viewport, RenderTargetView renderTargetView, DepthStencilView depthStencilView )
        {
            deviceContext.ClearRenderTargetView( renderTargetView, CLEAR_COLOR );

            if ( mSegmenter.DatasetLoaded )
            {
                deviceContext.InputAssembler.InputLayout = mInputLayout;
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

                deviceContext.InputAssembler.SetVertexBuffers( POSITION_SLOT,
                                                                new VertexBufferBinding( mPositionVertexBuffer,
                                                                                        POSITION_NUM_COMPONENTS_PER_VERTEX *
                                                                                        POSITION_NUM_BYTES_PER_COMPONENT,
                                                                                        0 ) );

                deviceContext.InputAssembler.SetVertexBuffers( TEXCOORD_SLOT,
                                                                new VertexBufferBinding( mTexCoordVertexBuffer,
                                                                                        TEXCOORD_NUM_COMPONENTS_PER_VERTEX *
                                                                                        TEXCOORD_NUM_BYTES_PER_COMPONENT,
                                                                                        0 ) );

                mEffect.GetVariableByName( CurrentTextureName ).AsResource().SetResource( mSegmenter.D3D11CudaTextureEnumerator.Current.Value );
                mEffect.GetVariableByName( "gSourceMapTexture3D" ).AsResource().SetResource( mSegmenter.Interop.D3D11CudaTextures.Get( "SourceMap" ) );
                mEffect.GetVariableByName( "gPrimalMapTexture3D" ).AsResource().SetResource( mSegmenter.Interop.D3D11CudaTextures.Get( "PrimalMap" ) );
                mEffect.GetVariableByName( "gColorMapTexture3D" ).AsResource().SetResource( mSegmenter.Interop.D3D11CudaTextures.Get( "ColorMap" ) );
                mEffect.GetVariableByName( "gConstraintMapTexture3D" ).AsResource().SetResource( mSegmenter.Interop.D3D11CudaTextures.Get( "ConstraintMap" ) );
                mEffect.GetVariableByName( "gPrimalMapThreshold" ).AsScalar().Set( Constants.Parameters.GetFloat( "PRIMAL_MAP_THRESHOLD" ) );
                mEffect.GetVariableByName( "gRecordingMode" ).AsScalar().Set( (int)Constants.RECORDING_MODE );
                mEffect.GetVariableByName( "gSplitMode" ).AsScalar().Set( SplitMode );
                mEffect.GetVariableByName( "gCurrentSliceCoordinate" ).AsScalar().Set( CurrentSliceCoordinate );
                mEffect.GetVariableByName( "gCurrentTextureIndex" ).AsScalar().Set( CurrentTextureIndex );
                mEffect.GetVariableByName( "gCurrentTextureDimensions" ).AsScalar().Set( CurrentTextureDimensions );
                mEffect.GetVariableByName( "gCurrentNeuralProcessColor" ).AsVector().Set( CurrentNeuralProcessColor );
                mEffect.GetVariableByName( "gSplitNeuralProcessColor" ).AsVector().Set( SplitNeuralProcessColor );

                mPass.Apply( deviceContext );
                deviceContext.Draw( NUM_VERTICES, 0 );

                if ( Constants.RECORDING_MODE == RecordingMode.NotRecording )
                {
                    mTinyTextContext.Print( viewport, "Current Z Slice Coordinate: " + CurrentSliceCoordinate + " (slice " + mSegmenter.CurrentSlice + ")", 10, 10 );
                    mTinyTextContext.Print( viewport, "Current Texture: " + mSegmenter.D3D11CudaTextureEnumerator.Current.Key, 10, 30 );
                    mTinyTextContext.Print( viewport, "Frame Time: " + FrameTimeString, 10, 50 );
                    mTinyTextContext.Print( viewport, "Primal Dual Energy Gap: " + mSegmenter.Interop.ConvergenceGap, 10, 70 );
                    mTinyTextContext.Print( viewport, "Primal Dual Energy Gap Delta: " + mSegmenter.Interop.ConvergenceGapDelta, 10, 90 );
                    mTinyTextContext.Print( viewport, "Current Neural Process: " + CurrentNeuralProcessString, 10, 110 );
                    mTinyTextContext.Print( viewport, "Segmenter Dimension Mode: " + mSegmenter.DimensionMode, 10, 130 );
                    mTinyTextContext.Print( viewport, "Segmenter Max Foreground Cost Delta: " + mSegmenter.Interop.MaxForegroundCostDelta, 10, 150 );
                    mTinyTextContext.Render();
                }

                mStopwatch.Reset();
            }
            else
            {
                mTinyTextContext.Print( viewport, "No dataset loaded.", 10, 10 );
                mTinyTextContext.Render();
            }
        }
    }
}
