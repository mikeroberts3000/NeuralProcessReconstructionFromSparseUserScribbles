using System.Diagnostics;
using SlimDX;
using SlimDX.Direct3D11;
using TinyText;

namespace Mojo.Breadcrumber3D
{
    internal class RenderingStrategy : IRenderingStrategy
    {
        private const int MAX_NUM_TEXT_CHARACTERS = 1024;

        private static readonly Color4 CLEAR_COLOR = new Color4( 0.5f, 0.5f, 1.0f );

        private readonly Stopwatch mStopwatch = new Stopwatch();

        private readonly Breadcrumber mBreadcrumber;
        private readonly DebugRenderer mDebugRenderer;
        private readonly Context mTinyTextContext;

        private string FrameTimeString
        {
            get
            {
                return mStopwatch.ElapsedMilliseconds == 0 ? "< 1 ms" : mStopwatch.ElapsedMilliseconds + " ms";
            }
        }

        public RenderingStrategy( Device device, DeviceContext deviceContext, Breadcrumber breadcrumber )
        {
            mBreadcrumber = breadcrumber;
            mDebugRenderer = new DebugRenderer( device );

            mStopwatch.Start();

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

            mDebugRenderer.Dispose();
        }

        public void Render( DeviceContext deviceContext, Viewport viewport, RenderTargetView renderTargetView, DepthStencilView depthStencilView )
        {
            deviceContext.ClearRenderTargetView( renderTargetView, CLEAR_COLOR );
            deviceContext.ClearDepthStencilView( depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0x00 );

            // axes
            mDebugRenderer.RenderLine( deviceContext, new Vector3( 0f, 0f, 0f ), new Vector3( 2f, 0f, 0f ), new Vector3( 1f, 0f, 0f ), mBreadcrumber.Camera );
            mDebugRenderer.RenderLine( deviceContext, new Vector3( 0f, 0f, 0f ), new Vector3( 0f, 2f, 0f ), new Vector3( 0f, 1f, 0f ), mBreadcrumber.Camera );
            mDebugRenderer.RenderLine( deviceContext, new Vector3( 0f, 0f, 0f ), new Vector3( 0f, 0f, 2f ), new Vector3( 0f, 0f, 1f ), mBreadcrumber.Camera );

            //// volume
            //mDebugRenderer.RenderBoxGreyScaleTexture3DWireframe(
            //    deviceContext,
            //    new Vector3( 0f, 0f, mBreadcrumber.NormalizedVolumeExtent.Z * mBreadcrumber.CurrentSlice / ( mBreadcrumber.VolumeDescription.NumVoxelsZ - 1f ) ),
            //    mBreadcrumber.NormalizedVolumeExtent,
            //    new Vector3( 0f, 1f, mBreadcrumber.CurrentSlice / ( mBreadcrumber.VolumeDescription.NumVoxelsZ - 1f ) ),
            //    new Vector3( 1f, 0f, 1f ),
            //    mBreadcrumber.SourceTexture,
            //    new Vector3( 1f, 1f, 1f ),
            //    mBreadcrumber.Camera );

            //// breadcrumbs
            //foreach ( var neuralProcessDescription in mBreadcrumber.NeuralProcessDescriptions.Internal.Values )
            //{
            //    foreach ( var breadcrumbDescription in neuralProcessDescription.BreadcrumbDescriptions )
            //    {
            //        mDebugRenderer.RenderSphereSolidWireframe( deviceContext,
            //                                                   MathUtil.TransformAndHomogeneousDivide(
            //                                                       breadcrumbDescription.Position,
            //                                                       mBreadcrumber.VolumeIndexToNormalizedVolumeCoordinates *
            //                                                       mBreadcrumber.NormalizedVolumeCoordinatesToWorldCoordinates ),
            //                                                   0.001f,
            //                                                   MathUtil.ConvertToFloatColor( neuralProcessDescription.Color ),
            //                                                   new Vector3( 1f, 1f, 1f ),
            //                                                   mBreadcrumber.Camera );
            //    }
            //}

            // branch start and end points
            foreach ( var neuralProcessDescription in mBreadcrumber.NeuralProcessDescriptions.Internal.Values )
            {
                foreach ( var branch in neuralProcessDescription.Branches )
                {
                    mDebugRenderer.RenderSphereSolidWireframe( deviceContext,
                                                               MathUtil.TransformAndHomogeneousDivide(
                                                                   branch.P1,
                                                                   mBreadcrumber.VolumeIndexToNormalizedVolumeCoordinates *
                                                                   mBreadcrumber.NormalizedVolumeCoordinatesToWorldCoordinates ),
                                                               0.01f,
                                                               MathUtil.ConvertToFloatColor( neuralProcessDescription.Color ),
                                                               new Vector3( 1f, 1f, 1f ),
                                                               mBreadcrumber.Camera );

                    mDebugRenderer.RenderSphereSolidWireframe( deviceContext,
                                                               MathUtil.TransformAndHomogeneousDivide(
                                                                   branch.P2,
                                                                   mBreadcrumber.VolumeIndexToNormalizedVolumeCoordinates *
                                                                   mBreadcrumber.NormalizedVolumeCoordinatesToWorldCoordinates ),
                                                               0.01f,
                                                               MathUtil.ConvertToFloatColor( neuralProcessDescription.Color ),
                                                               new Vector3( 1f, 1f, 1f ),
                                                               mBreadcrumber.Camera );
                }
            }

            // shortest paths
            foreach ( var shortestPathDescriptions in mBreadcrumber.ShortestPathDescriptions.Internal.Values )
            {
                var shortestPathDescription = shortestPathDescriptions[ 0 ];

                foreach ( var shortestPathEdge in shortestPathDescription.SmoothPath )
                {
                    var p1 = MathUtil.TransformAndHomogeneousDivide(
                        shortestPathEdge.P1,
                        mBreadcrumber.VolumeIndexToNormalizedVolumeCoordinates *
                        mBreadcrumber.NormalizedVolumeCoordinatesToWorldCoordinates );

                    var p2 = MathUtil.TransformAndHomogeneousDivide(
                        shortestPathEdge.P2,
                        mBreadcrumber.VolumeIndexToNormalizedVolumeCoordinates *
                        mBreadcrumber.NormalizedVolumeCoordinatesToWorldCoordinates );

                    mDebugRenderer.RenderLine( deviceContext, p1, p2, new Vector3( 1f, 1f, 0f ), mBreadcrumber.Camera );
                }

                shortestPathDescription = shortestPathDescriptions[ 1 ];

                foreach ( var shortestPathEdge in shortestPathDescription.SmoothPath )
                {
                    var p1 = MathUtil.TransformAndHomogeneousDivide(
                        shortestPathEdge.P1,
                        mBreadcrumber.VolumeIndexToNormalizedVolumeCoordinates *
                        mBreadcrumber.NormalizedVolumeCoordinatesToWorldCoordinates );

                    var p2 = MathUtil.TransformAndHomogeneousDivide(
                        shortestPathEdge.P2,
                        mBreadcrumber.VolumeIndexToNormalizedVolumeCoordinates *
                        mBreadcrumber.NormalizedVolumeCoordinatesToWorldCoordinates );

                    mDebugRenderer.RenderLine( deviceContext, p1, p2, new Vector3( 1f, 0f, 1f ), mBreadcrumber.Camera );
                }

                shortestPathDescription = shortestPathDescriptions[ 2 ];

                foreach ( var shortestPathEdge in shortestPathDescription.SmoothPath )
                {
                    var p1 = MathUtil.TransformAndHomogeneousDivide(
                        shortestPathEdge.P1,
                        mBreadcrumber.VolumeIndexToNormalizedVolumeCoordinates *
                        mBreadcrumber.NormalizedVolumeCoordinatesToWorldCoordinates );

                    var p2 = MathUtil.TransformAndHomogeneousDivide(
                        shortestPathEdge.P2,
                        mBreadcrumber.VolumeIndexToNormalizedVolumeCoordinates *
                        mBreadcrumber.NormalizedVolumeCoordinatesToWorldCoordinates );

                    mDebugRenderer.RenderLine( deviceContext, p1, p2, new Vector3( 0f, 1f, 1f ), mBreadcrumber.Camera );
                }
            }

            var currentEdgeUnnormalized = mBreadcrumber.ShortestPathDescriptions.Get( "Trail 1" )[ 0 ].SmoothPath[ mBreadcrumber.CurrentEdge ];

            var currentEdgeP1 = MathUtil.TransformAndHomogeneousDivide(
                currentEdgeUnnormalized.P1,
                mBreadcrumber.VolumeIndexToNormalizedVolumeCoordinates );

            var currentEdgeP2 = MathUtil.TransformAndHomogeneousDivide(
                currentEdgeUnnormalized.P2,
                mBreadcrumber.VolumeIndexToNormalizedVolumeCoordinates );

            var forward = currentEdgeP2 - currentEdgeP1;
            var upHint = new Vector3( 0f, 1f, 0f );
            var right = Vector3.Cross( upHint, forward );
            var up = Vector3.Cross( forward, right );
            forward.Normalize();
            right.Normalize();
            up.Normalize();

            var q1 = currentEdgeP1 - right - up;
            var q2 = currentEdgeP1 - right + up;
            var q3 = currentEdgeP1 + right + up;
            var q4 = currentEdgeP1 + right - up;

            mDebugRenderer.RenderQuadGreyScaleTexture3DWireframe(
                deviceContext,
                MathUtil.TransformAndHomogeneousDivide( q1, mBreadcrumber.NormalizedVolumeCoordinatesToWorldCoordinates ),
                MathUtil.TransformAndHomogeneousDivide( q2, mBreadcrumber.NormalizedVolumeCoordinatesToWorldCoordinates ),
                MathUtil.TransformAndHomogeneousDivide( q3, mBreadcrumber.NormalizedVolumeCoordinatesToWorldCoordinates ),
                MathUtil.TransformAndHomogeneousDivide( q4, mBreadcrumber.NormalizedVolumeCoordinatesToWorldCoordinates ),
                MathUtil.TransformAndHomogeneousDivide( q1, mBreadcrumber.NormalizedVolumeCoordinatesToTextureCoordinates ),
                MathUtil.TransformAndHomogeneousDivide( q2, mBreadcrumber.NormalizedVolumeCoordinatesToTextureCoordinates ),
                MathUtil.TransformAndHomogeneousDivide( q3, mBreadcrumber.NormalizedVolumeCoordinatesToTextureCoordinates ),
                MathUtil.TransformAndHomogeneousDivide( q4, mBreadcrumber.NormalizedVolumeCoordinatesToTextureCoordinates ),
                mBreadcrumber.SourceTexture,
                new Vector3( 1f, 1f, 1f ),
                mBreadcrumber.Camera );

            if ( Constants.RECORDING_MODE == RecordingMode.NotRecording )
            {
                mTinyTextContext.Print( viewport, "Frame Time: " + FrameTimeString, 10, 10 );
                mTinyTextContext.Render();
            }

            mStopwatch.Reset();
        }
    }
}
