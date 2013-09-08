using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DrWPF.Windows.Data;
using Mojo.Interop;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;
using SlimDX;
using SlimDX.Direct3D11;

namespace Mojo
{
    public class Breadcrumber
    {
        public Interop.Breadcrumber Interop { get; set; }

        public VolumeDescription VolumeDescription { get; set; }
        public Dictionary<NeuralProcessDescription> NeuralProcessDescriptions { get; set; }
        public Dictionary<IList<Edge>> AugmentedDelaunyEdges { get; set; }
        public Dictionary<IList<ShortestPathDescription>> ShortestPathDescriptions { get; set; }

        public ShaderResourceView SourceTexture { get; set; }
        public Camera Camera { get; set; }

        public float AnisotropyFactor { get; set; }
        public Vector3 NormalizedVolumeExtent { get; set; }

        public Matrix VolumeIndexToNormalizedVolumeCoordinates { get; set; }
        public Matrix NormalizedVolumeCoordinatesToTextureCoordinates { get; set; }
        public Matrix NormalizedVolumeCoordinatesToWorldCoordinates { get; set; }

        private readonly Device mD3D11Device;
        private readonly DeviceContext mD3D11DeviceContext;

        public int CurrentEdge { get; set; }
        public int CurrentSlice { get; set; }
        public float CurrentDistanceThreshold { get; set; }

        //private Texture3D mSourceTexture;

// ReSharper disable InconsistentNaming
        public Breadcrumber( Device d3d11Device, DeviceContext d3d11DeviceContext )
        {
            mD3D11Device = d3d11Device;
            mD3D11DeviceContext = d3d11DeviceContext;
        }
// ReSharper restore InconsistentNaming

        public void LoadDataset( DatasetDescription datasetDescription )
        {


            Release.Assert( false );

            //datasetDescription.NeuralProcessDescriptions.Get( "Trail 1" ).BreadcrumbDescriptions.Add(
            //    datasetDescription.NeuralProcessDescriptions.Get( "Trail 2" ).BreadcrumbDescriptions.First() );

            //datasetDescription.NeuralProcessDescriptions.Get( "Trail 1" ).BreadcrumbDescriptions.Add(
            //    datasetDescription.NeuralProcessDescriptions.Get( "Trail 3" ).BreadcrumbDescriptions.First() );

            //datasetDescription.NeuralProcessDescriptions.Get( "Trail 1" ).BreadcrumbDescriptions.Add(
            //    datasetDescription.NeuralProcessDescriptions.Get( "Trail 4" ).BreadcrumbDescriptions.First() );

            //datasetDescription.NeuralProcessDescriptions.Get( "Trail 1" ).BreadcrumbDescriptions.Add(
            //    datasetDescription.NeuralProcessDescriptions.Get( "Trail 5" ).BreadcrumbDescriptions.First() );

            //datasetDescription.NeuralProcessDescriptions.Get( "Trail 1" ).BreadcrumbDescriptions.Add(
            //    datasetDescription.NeuralProcessDescriptions.Get( "Trail 6" ).BreadcrumbDescriptions.First() );

            //datasetDescription.NeuralProcessDescriptions.Get( "Trail 1" ).BreadcrumbDescriptions.Add(
            //    datasetDescription.NeuralProcessDescriptions.Get( "Trail 7" ).BreadcrumbDescriptions.First() );

            //datasetDescription.NeuralProcessDescriptions.Get( "Trail 1" ).Branches =
            //    new List<Edge>
            //            {
            //                new Edge
            //                     {
            //                         P1 = datasetDescription.NeuralProcessDescriptions.Get( "Trail 2" ).BreadcrumbDescriptions.First().Position,
            //                         P2 = datasetDescription.NeuralProcessDescriptions.Get( "Trail 3" ).BreadcrumbDescriptions.First().Position
            //                     },
            //                new Edge
            //                     {
            //                         P1 = datasetDescription.NeuralProcessDescriptions.Get( "Trail 4" ).BreadcrumbDescriptions.First().Position,
            //                         P2 = datasetDescription.NeuralProcessDescriptions.Get( "Trail 5" ).BreadcrumbDescriptions.First().Position
            //                     },
            //                new Edge
            //                     {
            //                         P1 = datasetDescription.NeuralProcessDescriptions.Get( "Trail 6" ).BreadcrumbDescriptions.First().Position,
            //                         P2 = datasetDescription.NeuralProcessDescriptions.Get( "Trail 7" ).BreadcrumbDescriptions.First().Position
            //                     }
            //            };

            //datasetDescription.NeuralProcessDescriptions.Internal =
            //    new ObservableDictionary< string, NeuralProcessDescription >(
            //        ( from neuralProcessDescription in
            //              datasetDescription.NeuralProcessDescriptions.Internal
            //          where neuralProcessDescription.Key == "Trail 1"
            //          select neuralProcessDescription ).ToDictionary( b => b.Key,
            //                                                          b => b.Value ) );



            //Interop.LoadDataset( datasetDescription );

            //VolumeDescription = datasetDescription.VolumeDescriptions.Get( "SourceMap" );

            //var textureDesc3D = new Texture3DDescription
            //                    {
            //                        Width = VolumeDescription.NumVoxelsX,
            //                        Height = VolumeDescription.NumVoxelsY,
            //                        Depth = VolumeDescription.NumVoxelsZ,
            //                        MipLevels = 1,
            //                        Usage = ResourceUsage.Default,
            //                        BindFlags = BindFlags.ShaderResource,
            //                        Format = VolumeDescription.DxgiFormat
            //                    };

            //var shaderResourceViewDesc = new ShaderResourceViewDescription
            //                             {
            //                                 Format = VolumeDescription.DxgiFormat,
            //                                 Dimension = ShaderResourceViewDimension.Texture3D,
            //                                 MipLevels = -1
            //                             };

            //mSourceTexture = new Texture3D( mD3D11Device, textureDesc3D );

            //VolumeDescription.DataStream.Seek( 0, SeekOrigin.Begin );

            //mD3D11DeviceContext.UpdateSubresource(
            //    new DataBox(
            //        VolumeDescription.NumVoxelsX *
            //        VolumeDescription.NumBytesPerVoxel,
            //        VolumeDescription.NumVoxelsY *
            //        VolumeDescription.NumVoxelsX *
            //        VolumeDescription.NumBytesPerVoxel,
            //        VolumeDescription.DataStream ),
            //    mSourceTexture,
            //    0 );

            //SourceTexture = new ShaderResourceView( mD3D11Device, mSourceTexture, shaderResourceViewDesc );

            //AnisotropyFactor = 10f;

            //NormalizedVolumeExtent =
            //    new Vector3( 1f,
            //                 (float)VolumeDescription.NumVoxelsY / VolumeDescription.NumVoxelsX,
            //                 ( VolumeDescription.NumVoxelsZ * AnisotropyFactor ) / VolumeDescription.NumVoxelsX );

            //VolumeIndexToNormalizedVolumeCoordinates =
            //    Matrix.Scaling( NormalizedVolumeExtent.X / VolumeDescription.NumVoxelsX,
            //                    NormalizedVolumeExtent.Y / VolumeDescription.NumVoxelsY,
            //                    NormalizedVolumeExtent.Z / VolumeDescription.NumVoxelsZ );

            //NormalizedVolumeCoordinatesToWorldCoordinates =
            //    Matrix.Reflection( new Plane( Vector3.Zero, Vector3.UnitY ) ) *
            //    Matrix.Translation( 0f, NormalizedVolumeExtent.Y, 0f );

            //NormalizedVolumeCoordinatesToTextureCoordinates =
            //    Matrix.Scaling( 1f / NormalizedVolumeExtent.X,
            //                    1f / NormalizedVolumeExtent.Y,
            //                    1f / NormalizedVolumeExtent.Z );

            //Camera = new Camera( NormalizedVolumeExtent * 4f,
            //                     NormalizedVolumeExtent / 2f,
            //                     new Vector3( 0f, 1f, 0f ),
            //                     Matrix.PerspectiveFovLH( (float)Math.PI / 4, 1f, 0.1f, 100f ) );

            //NeuralProcessDescriptions = datasetDescription.NeuralProcessDescriptions;

            //AugmentedDelaunyEdges = AugmentDelaunyEdges( datasetDescription.NeuralProcessDescriptions, Interop.DelaunyEdges, VolumeIndexToNormalizedVolumeCoordinates );
            //ShortestPathDescriptions = ComputeShortestPaths( datasetDescription.NeuralProcessDescriptions, AugmentedDelaunyEdges, VolumeIndexToNormalizedVolumeCoordinates );
        }

        public void UnloadDataset()
        {
            SourceTexture.Dispose();
            //mSourceTexture.Dispose();
        }

        public void Update()
        {
        }

        private static Dictionary< IList< ShortestPathDescription > > ComputeShortestPaths( Dictionary< NeuralProcessDescription > neuralProcessDescriptions, Dictionary< IList< Edge > > delaunyEdges, Matrix volumeIndexToNormalizedVolumeCoordinates )
        {
            return new Dictionary< IList< ShortestPathDescription > >
                   {
                       Internal =
                           new ObservableDictionary< string, IList< ShortestPathDescription > >(
                           ( from neuralProcessDescription in neuralProcessDescriptions.Internal.Values
                             where neuralProcessDescription.Branches != null
                             select new
                                    {
                                        neuralProcessDescription.Name,
                                        ShortestPathDescriptions = from branch in neuralProcessDescription.Branches
                                                                   let shortestPath =
                                                                       ComputeShortestPath( branch,
                                                                                            delaunyEdges.Get( neuralProcessDescription.Name ),
                                                                                            volumeIndexToNormalizedVolumeCoordinates )
                                                                   select new ShortestPathDescription
                                                                          {
                                                                              Branch = branch,
                                                                              ShortestPath = shortestPath,
                                                                              SmoothPath = ComputeSmoothPath( shortestPath )
                                                                          }
                                    } ).ToDictionary( shorestPathDescription => shorestPathDescription.Name,
                                                      shortestPathDescription =>
                                                      (IList< ShortestPathDescription >)
                                                      shortestPathDescription.ShortestPathDescriptions.ToList() ) )
                   };
        }

        private static IList< Edge > ComputeShortestPath( Edge branch, IEnumerable< Edge > delaunyEdges, Matrix volumeIndexToNormalizedVolumeCoordinates )
        {
            var graph = new UndirectedGraph<Vector3, Edge<Vector3>>();

            foreach ( var delaunyEdge in delaunyEdges )
            {
                graph.AddVerticesAndEdge( new Edge<Vector3>( delaunyEdge.P1, delaunyEdge.P2 ) );
            }

            var dijkstraShortestPathAlgorithm = new UndirectedDijkstraShortestPathAlgorithm<Vector3, Edge<Vector3>>( graph, edge => CalculateEdgeWeights( edge, volumeIndexToNormalizedVolumeCoordinates ) );
            var visitor = new UndirectedVertexPredecessorRecorderObserver<Vector3, Edge<Vector3>>();

            using ( visitor.Attach( dijkstraShortestPathAlgorithm ) )
            {
                dijkstraShortestPathAlgorithm.Compute( branch.P1 );
            }

            IEnumerable<Edge<Vector3>> shortestPath;
            visitor.TryGetPath( branch.P2, out shortestPath );

            var edgeList = ( from edge in shortestPath select new Edge { P1 = edge.Source, P2 = edge.Target } ).ToList();
            var orientedEdgeList = new List< Edge >();

            var edgeListP1 = edgeList[ 0 ].P1;
            var edgeListP2 = edgeList[ 0 ].P2;

            orientedEdgeList.Add( !edgeList[ 0 ].P1.Equals( branch.P1 )
                                      ? new Edge { P1 = edgeListP2, P2 = edgeListP1 }
                                      : new Edge { P1 = edgeListP1, P2 = edgeListP2 } );

            for ( int i = 1; i < edgeList.Count; i++ )
            {
                edgeListP1 = edgeList[ i ].P1;
                edgeListP2 = edgeList[ i ].P2;

                orientedEdgeList.Add( !edgeList[ i ].P1.Equals( orientedEdgeList[ i - 1 ].P2 )
                                          ? new Edge { P1 = edgeListP2, P2 = edgeListP1 }
                                          : new Edge { P1 = edgeListP1, P2 = edgeListP2 } );
            }

            return orientedEdgeList;
        }

        private static double CalculateEdgeWeights( Edge< Vector3 > edge, Matrix volumeIndexToNormalizedVolumeCoordinates )
        {
            var dSquared = Vector3.DistanceSquared( MathUtil.TransformAndHomogeneousDivide( edge.Source, volumeIndexToNormalizedVolumeCoordinates ),
                                                    MathUtil.TransformAndHomogeneousDivide( edge.Target, volumeIndexToNormalizedVolumeCoordinates ) );

            var d = Math.Sqrt( dSquared );

            return dSquared + d;
        }

        private static IList<Edge> ComputeSmoothPath( IList<Edge> coursePath )
        {
            var smoothPath = SubdividePath( coursePath );

            for ( int i = 0; i < 2; i++ )
            {
                smoothPath = SubdividePath( smoothPath );
            }

            return smoothPath;
        }

        private static IList<Edge> SubdividePath( IList<Edge> coursePath )
        {
            var smoothPath = new List<Edge>
                             {
                                 new Edge
                                 {
                                     P1 = coursePath[ 0 ].P1,
                                     P2 = ( 0.25f * coursePath[ 0 ].P1 ) + ( 0.75f * coursePath[ 0 ].P2 )
                                 },
                                 new Edge
                                 {
                                     P1 = ( 0.25f * coursePath[ 0 ].P1 ) + ( 0.75f * coursePath[ 0 ].P2 ),
                                     P2 = ( 0.75f * coursePath[ 1 ].P1 ) + ( 0.25f * coursePath[ 1 ].P2 )
                                 }
                             };

            for ( int i = 1; i < coursePath.Count - 1; i++ )
            {
                var currentEdge = coursePath[ i ];
                var nextEdge = coursePath[ i + 1 ];

                smoothPath.Add( new Edge
                {
                    P1 = ( 0.75f * currentEdge.P1 ) + ( 0.25f * currentEdge.P2 ),
                    P2 = ( 0.25f * currentEdge.P1 ) + ( 0.75f * currentEdge.P2 )
                } );
                smoothPath.Add( new Edge
                {
                    P1 = ( 0.25f * currentEdge.P1 ) + ( 0.75f * currentEdge.P2 ),
                    P2 = ( 0.75f * nextEdge.P1 ) + ( 0.25f * nextEdge.P2 ),
                } );
            }

            smoothPath.Add( new Edge
            {
                P1 = ( 0.25f * coursePath[ coursePath.Count - 1 ].P2 ) + ( 0.75f * coursePath[ coursePath.Count - 1 ].P1 ),
                P2 = coursePath[ coursePath.Count - 1 ].P2,
            } );

            return smoothPath;
        }

        private static Dictionary<IList<Edge>> AugmentDelaunyEdges( Dictionary<NeuralProcessDescription> neuralProcessDescriptions, Dictionary<IList<Edge>> delaunyEdges, Matrix volumeIndexToNormalizedVolumeCoordinates )
        {
            var augmentedDelaunyEdges = new Dictionary< IList< Edge > >
                                        {
                                            Internal =
                                                new ObservableDictionary< string, IList< Edge > >( delaunyEdges.Internal.ToDictionary( x => x.Key,
                                                                                                                                       x => x.Value ) )
                                        };

            foreach ( var neuralProcessDescription in neuralProcessDescriptions.Internal.Values )
            {
                foreach ( var breadcrumbDescription in neuralProcessDescription.BreadcrumbDescriptions )
                {
                    var tmpBreadcrumbDescription = breadcrumbDescription;

                    var nearestBreadcrumb =
                        neuralProcessDescription.BreadcrumbDescriptions
                        .ToList()
                        .Where( nearbyBreadcrumb => !nearbyBreadcrumb.Position.Equals( tmpBreadcrumbDescription.Position ) )
                        .ArgMin(
                            nearbyBreadcrumb =>
                            Vector3.Distance(
                                MathUtil.TransformAndHomogeneousDivide( tmpBreadcrumbDescription.Position, volumeIndexToNormalizedVolumeCoordinates ),
                                MathUtil.TransformAndHomogeneousDivide( nearbyBreadcrumb.Position, volumeIndexToNormalizedVolumeCoordinates ) ) );

                    var secondNearestBreadcrumb =
                        neuralProcessDescription.BreadcrumbDescriptions
                        .ToList()
                        .Where(
                            nearbyBreadcrumb =>
                            !nearbyBreadcrumb.Position.Equals( tmpBreadcrumbDescription.Position ) && !nearbyBreadcrumb.Position.Equals( nearestBreadcrumb.Position ) )
                        .ArgMin(
                            nearbyBreadcrumb =>
                            Vector3.Distance(
                                MathUtil.TransformAndHomogeneousDivide( tmpBreadcrumbDescription.Position, volumeIndexToNormalizedVolumeCoordinates ),
                                MathUtil.TransformAndHomogeneousDivide( nearbyBreadcrumb.Position, volumeIndexToNormalizedVolumeCoordinates ) ) );

                    augmentedDelaunyEdges.Get( neuralProcessDescription.Name ).Add( new Edge
                                                                                      {
                                                                                          P1 = breadcrumbDescription.Position,
                                                                                          P2 = nearestBreadcrumb.Position
                                                                                      } );

                    augmentedDelaunyEdges.Get( neuralProcessDescription.Name ).Add( new Edge
                                                                                      {
                                                                                          P1 = breadcrumbDescription.Position,
                                                                                          P2 = secondNearestBreadcrumb.Position
                                                                                      } );
                }
            }

            return augmentedDelaunyEdges;
        }
    }
}
