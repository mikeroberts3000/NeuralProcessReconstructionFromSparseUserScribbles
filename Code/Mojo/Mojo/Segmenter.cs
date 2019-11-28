using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DrWPF.Windows.Data;
using Emgu.CV;
using Emgu.CV.Structure;
using Mojo.Interop;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace Mojo
{
    public enum ConstraintType
    {
        Foreground,
        Background
    }

    public enum InteractionMode
    {
        HighLatency,
        LowLatency
    }

    public enum DimensionMode
    {
        Two,
        Three
    }

    public enum InitializeCostMapMode
    {
        Idle,
        Initialize,
        Forward,
        Backward,
        Finalize
    }

    public enum SegmenterToolMode
    {
        Adjust,
        Merge,
        Split
    }

    public class Segmenter : NotifyPropertyChanged
    {
        private const int NUM_BYTES_PER_COLOR_MAP_PIXEL = 4;
        private const int NUM_BYTES_PER_ID_MAP_PIXEL = 4;

        private readonly Random mRandom = new Random();
        private int mMousePreviousX;
        private int mMousePreviousY;

        public Interop.Segmenter Interop { get; set; }

        public IEnumerator<KeyValuePair<int, NeuralProcessDescription>> NeuralProcessEnumerator { get; set; }
        public IEnumerator<KeyValuePair<string, ShaderResourceView>> D3D11CudaTextureEnumerator { get; set; }

        public InteractionMode InteractionMode { get; set; }
        public DimensionMode DimensionMode { get; set; }
        public InitializeCostMapMode InitializeCostMapMode { get; set; }

        public int CurrentSlice { get; set; }
        public int CurrentD3D11CudaTextureIndex { get; set; }
        public int CurrentBrushWidth { get; set; }

        private bool mDatasetLoaded;
        public bool DatasetLoaded
        {
            get
            {
                return mDatasetLoaded;
            }
            set
            {
                mDatasetLoaded = value;
                OnPropertyChanged( "DatasetLoaded" );
            }
        }

        private bool mSegmentationLoaded;
        public bool SegmentationLoaded
        {
            get
            {
                return mSegmentationLoaded;
            }
            set
            {
                mSegmentationLoaded = value;
                OnPropertyChanged( "SegmentationLoaded" );
            }
        }

        private NeuralProcessDescription mCurrentNeuralProcess;
        public NeuralProcessDescription CurrentNeuralProcess
        {
            get
            {
                return mCurrentNeuralProcess;
            }
            set
            {
                mCurrentNeuralProcess = value;
                OnPropertyChanged( "CurrentNeuralProcess" );
            }
        }

        private NeuralProcessDescription mMergeSourceNeuralProcess;
        public NeuralProcessDescription MergeSourceNeuralProcess
        {
            get
            {
                return mMergeSourceNeuralProcess;
            }
            set
            {
                mMergeSourceNeuralProcess = value;
                OnPropertyChanged( "MergeSourceNeuralProcess" );
            }
        }

        private NeuralProcessDescription mMergeDestinationNeuralProcess;
        public NeuralProcessDescription MergeDestinationNeuralProcess
        {
            get
            {
                return mMergeDestinationNeuralProcess;
            }
            set
            {
                mMergeDestinationNeuralProcess = value;
                OnPropertyChanged( "MergeDestinationNeuralProcess" );
            }
        }

        private NeuralProcessDescription mSplitNeuralProcess;
        public NeuralProcessDescription SplitNeuralProcess
        {
            get
            {
                return mSplitNeuralProcess;
            }
            set
            {
                mSplitNeuralProcess = value;
                OnPropertyChanged( "SplitNeuralProcess" );
            }
        }

        private SegmenterToolMode mCurrentSegmenterToolMode;
        public SegmenterToolMode CurrentSegmenterToolMode
        {
            get
            {
                return mCurrentSegmenterToolMode;
            }
            set
            {
                mCurrentSegmenterToolMode = value;
                OnPropertyChanged( "CurrentSegmenterToolMode" );
            }
        }

        private DatasetDescription mDatasetDescription;
        public DatasetDescription DatasetDescription
        {
            get
            {
                return mDatasetDescription;
            }
            set
            {
                mDatasetDescription = value;
                OnPropertyChanged( "DatasetDescription" );
            }
        }

        private bool mCommittedSegmentationEqualsUndoBuffer = true;
        public bool CommittedSegmentationEqualsUndoBuffer
        {
            get
            {
                return mCommittedSegmentationEqualsUndoBuffer;
            }
            set
            {
                mCommittedSegmentationEqualsUndoBuffer = value;
                OnPropertyChanged( "CommittedSegmentationEqualsUndoBuffer" );
            }
        }

        private bool mCommittedSegmentationEqualsRedoBuffer = true;
        public bool CommittedSegmentationEqualsRedoBuffer
        {
            get
            {
                return mCommittedSegmentationEqualsRedoBuffer;
            }
            set
            {
                mCommittedSegmentationEqualsRedoBuffer = value;
                OnPropertyChanged( "CommittedSegmentationEqualsRedoBuffer" );
            }
        }

        private bool mConstrainSegmentationMergeToCurrentSlice = true;
        public bool ConstrainSegmentationMergeToCurrentSlice
        {
            get
            {
                return mConstrainSegmentationMergeToCurrentSlice;
            }
            set
            {
                mConstrainSegmentationMergeToCurrentSlice = value;
                OnPropertyChanged( "ConstrainSegmentationMergeToCurrentSlice" );
            }
        }

        private bool mConstrainSegmentationMergeToConnectedComponent = true;
        public bool ConstrainSegmentationMergeToConnectedComponent
        {
            get
            {
                return mConstrainSegmentationMergeToConnectedComponent;
            }
            set
            {
                mConstrainSegmentationMergeToConnectedComponent = value;
                OnPropertyChanged( "ConstrainSegmentationMergeToConnectedComponent" );
            }
        }

        private bool mShowSegmentation = true;
        public bool ShowSegmentation
        {
            get
            {
                return mShowSegmentation;
            }
            set
            {
                mShowSegmentation = value;
                OnPropertyChanged( "ShowSegmentation" );
            }
        }

        public void Dispose()
        {
            UnloadDataset();

            Interop.Dispose();
        }

        public void LoadDataset( SegmenterImageStackLoadDescription segmenterImageStackLoadDescription, BreadcrumbXmlLoadDescription breadcrumbXmlLoadDescription )
        {
            var volumeDescriptions = SegmenterImageStackLoader.LoadDataset( segmenterImageStackLoadDescription );

            var breadcrumbXmlLoader = new BreadcrumbXmlLoader();
            var neuralProcessDescriptions = breadcrumbXmlLoader.LoadDataset( breadcrumbXmlLoadDescription );

            neuralProcessDescriptions.Add( Constants.DEFAULT_NEURAL_PROCESS.Id, Constants.DEFAULT_NEURAL_PROCESS );

            var datasetDescription = new DatasetDescription
            {
                NeuralProcessDescriptions = neuralProcessDescriptions,
                VolumeDescriptions = volumeDescriptions
            };

            LoadDataset( datasetDescription );
        }

        public void LoadDataset( SegmenterImageStackLoadDescription segmenterImageStackLoadDescription )
        {
            var volumeDescriptions = SegmenterImageStackLoader.LoadDataset( segmenterImageStackLoadDescription );

            var neuralProcessDescriptions = new ObservableDictionary<int, NeuralProcessDescription>
                                            {
                                                { Constants.DEFAULT_NEURAL_PROCESS.Id, Constants.DEFAULT_NEURAL_PROCESS }
                                            };

            var datasetDescription = new DatasetDescription
            {
                NeuralProcessDescriptions = neuralProcessDescriptions,
                VolumeDescriptions = volumeDescriptions
            };

            LoadDataset( datasetDescription );
        }


// ReSharper disable InconsistentNaming
        public void LoadDataset( DatasetDescription datasetDescription )
        {
            UnloadDataset();

            Interop.LoadVolume( datasetDescription.VolumeDescriptions );
            Interop.VisualUpdate();
            Interop.VisualUpdateColorMap();

            var neuralProcessEnumerator = datasetDescription.NeuralProcessDescriptions.GetEnumerator() as IEnumerator<KeyValuePair<int, NeuralProcessDescription>>;
            neuralProcessEnumerator.MoveNext();
            var d3d11CudaTextureEnumerator = Interop.D3D11CudaTextures.Internal.GetEnumerator() as IEnumerator<KeyValuePair<string, ShaderResourceView>>;
            d3d11CudaTextureEnumerator.MoveNext();

            DatasetDescription = datasetDescription;
            NeuralProcessEnumerator = neuralProcessEnumerator;
            D3D11CudaTextureEnumerator = d3d11CudaTextureEnumerator;
            CurrentNeuralProcess = NeuralProcessEnumerator.Current.Value;
            CurrentD3D11CudaTextureIndex = 0;
            CurrentSegmenterToolMode = SegmenterToolMode.Adjust;
            CommittedSegmentationEqualsUndoBuffer = true;
            CommittedSegmentationEqualsRedoBuffer = true;
            DatasetLoaded = true;

            D3D11CudaTextureEnumerator.MoveNext();
            CurrentD3D11CudaTextureIndex++;

            D3D11CudaTextureEnumerator.MoveNext();
            CurrentD3D11CudaTextureIndex++;
        }
// ReSharper restore InconsistentNaming

        public void UnloadDataset()
        {
            if ( DatasetLoaded )
            {
                Interop.UnloadVolume();
                DatasetLoaded = false;
            }
        }

        public void UnloadSegmentation()
        {
            if ( SegmentationLoaded )
            {
                DatasetDescription.NeuralProcessDescriptions.Clear();
                SegmentationLoaded = false;
            }
        }

        public void LoadSegmentation( SegmenterImageStackLoadDescription segmenterImageStackLoadDescription )
        {
            UnloadSegmentation();

            var volumeDescriptions = SegmenterImageStackLoader.LoadSegmentation( segmenterImageStackLoadDescription );

            Interop.LoadSegmentation( volumeDescriptions );
            Interop.VisualUpdate();
            Interop.VisualUpdateColorMap();

            volumeDescriptions.Get( "ColorMap" ).DataStream.Seek( 0, SeekOrigin.Begin );
            volumeDescriptions.Get( "IdMap" ).DataStream.Seek( 0, SeekOrigin.Begin );

            var uniqueIds = new Dictionary< int, Rgb >();

            for ( var z = 0; z < volumeDescriptions.Get( "IdMap" ).NumVoxelsZ; z++ )
            {
                for ( var y = 0; y < volumeDescriptions.Get( "IdMap" ).NumVoxelsY; y++ )
                {
                    for ( var x = 0; x < volumeDescriptions.Get( "IdMap" ).NumVoxelsX; x++ )
                    {
                        var id = volumeDescriptions.Get( "IdMap" ).DataStream.Read<int>();
                        var colorMapValue = volumeDescriptions.Get( "ColorMap" ).DataStream.Read<int>();

                        var r = ( colorMapValue & ( 0x0000ff << 0 ) ) >> 0;
                        var g = ( colorMapValue & ( 0x0000ff << 8 ) ) >> 8;
                        var b = ( colorMapValue & ( 0x0000ff << 16 ) ) >> 16;



                        if ( id == 0 )
                        {
                            if ( !( r == 0 && g == 0 && b == 0 ) )
                            {
                                Console.WriteLine( "WARNING: x = {0}, y = {1}, z = {2}, id = {3}, color = {4}", x, y, z, id, new Rgb( r, g, b ).ToString() );
                            }
                        }

                        if ( uniqueIds.ContainsKey( id ) )
                        {
                            if ( !uniqueIds[ id ].Equals( new Rgb( r, g, b ) ) )
                            {
                                Console.WriteLine( "WARNING: x = {0}, y = {1}, z = {2}, id = {3}, existing color = {4}, new color = {5}", x, y, z, id, uniqueIds[ id ].ToString(), new Rgb( r, g, b ).ToString() );
                            }
                        }



                        if ( !uniqueIds.ContainsKey( id ) )
                        {
                            uniqueIds[ id ] = new Rgb( r, g, b );                            
                        }
                    }
                }                
            }

            uniqueIds.Remove( Constants.NULL_NEURAL_PROCESS.Id );

            uniqueIds.ToList().ForEach( keyValuePair =>
                                        DatasetDescription.NeuralProcessDescriptions.Add( keyValuePair.Key,
                                                                                          new NeuralProcessDescription( keyValuePair.Key )
                                                                                          {
                                                                                              Name = "Autogenerated Neural Process (ID " + keyValuePair.Key + ") " + keyValuePair.Value,
                                                                                              Color =
                                                                                                  new Vector3( (float)keyValuePair.Value.Red,
                                                                                                               (float)keyValuePair.Value.Green,
                                                                                                               (float)keyValuePair.Value.Blue),
                                                                                              BreadcrumbDescriptions = new List<BreadcrumbDescription>()
                                                                                          } ) );

            CurrentNeuralProcess = null;
            MergeSourceNeuralProcess = null;
            MergeDestinationNeuralProcess = null;
            SplitNeuralProcess = null;
            CommittedSegmentationEqualsUndoBuffer = false;
            CommittedSegmentationEqualsRedoBuffer = false;
            SegmentationLoaded = true;
        }

        public void SaveSegmentationAs( SegmenterImageStackSaveDescription segmenterImageStackSaveDescription )
        {
            CommitSegmentation();

            var colorMapDataStream =
                new DataStream(
                    Interop.VolumeDescription.NumVoxelsX * Interop.VolumeDescription.NumVoxelsY * Interop.VolumeDescription.NumVoxelsZ * NUM_BYTES_PER_COLOR_MAP_PIXEL,
                    true,
                    true );
            var idMapDataStream =
                new DataStream(
                    Interop.VolumeDescription.NumVoxelsX * Interop.VolumeDescription.NumVoxelsY * Interop.VolumeDescription.NumVoxelsZ * NUM_BYTES_PER_COLOR_MAP_PIXEL,
                    true,
                    true );

            var volumeDescriptions = new Dictionary< VolumeDescription >
                                     {
                                         {
                                             "ColorMap", new VolumeDescription
                                                         {
                                                             DxgiFormat = Format.R8G8B8A8_UNorm,
                                                             DataStream = colorMapDataStream,
                                                             Data = colorMapDataStream.DataPointer,
                                                             NumBytesPerVoxel = NUM_BYTES_PER_COLOR_MAP_PIXEL,
                                                             NumVoxelsX = Interop.VolumeDescription.NumVoxelsX,
                                                             NumVoxelsY = Interop.VolumeDescription.NumVoxelsY,
                                                             NumVoxelsZ = Interop.VolumeDescription.NumVoxelsZ,
                                                             IsSigned = false
                                                         }
                                             },
                                         {
                                             "IdMap", new VolumeDescription
                                                      {
                                                          DxgiFormat = Format.R32_UInt,
                                                          DataStream = idMapDataStream,
                                                          Data = idMapDataStream.DataPointer,
                                                          NumBytesPerVoxel = NUM_BYTES_PER_ID_MAP_PIXEL,
                                                          NumVoxelsX = Interop.VolumeDescription.NumVoxelsX,
                                                          NumVoxelsY = Interop.VolumeDescription.NumVoxelsY,
                                                          NumVoxelsZ = Interop.VolumeDescription.NumVoxelsZ,
                                                          IsSigned = false
                                                      }
                                             },
                                     };

            Interop.SaveSegmentationAs( volumeDescriptions );

            segmenterImageStackSaveDescription.VolumeDescriptions = volumeDescriptions;

            SegmenterImageStackLoader.SaveSegmentation( segmenterImageStackSaveDescription );
        }

        public void AddNeuralProcess( string neuralProcessName )
        {
            var trimmedNeuralProcessName = neuralProcessName;
            trimmedNeuralProcessName.Trim();
            var tmpNeuralProcessName = trimmedNeuralProcessName;

            if ( DatasetDescription.NeuralProcessDescriptions.Any( neuralProcessDescription => neuralProcessDescription.Value.Name.Equals( tmpNeuralProcessName ) ) )
            {
                Console.WriteLine( "The name " + trimmedNeuralProcessName + " is already being used" );
                return;
            }

            var neuralProcessId = DatasetDescription.NeuralProcessDescriptions.Keys.Max() + 1;
            var randomColor = GetRandomColor();

            if ( trimmedNeuralProcessName.Length == 0 )
            {
                tmpNeuralProcessName = "Autogenerated Neural Process (ID " + neuralProcessId + ") " + randomColor;
            }
                    
            if ( CurrentSegmenterToolMode == SegmenterToolMode.Split )
            {
                if ( CurrentNeuralProcess != null )
                {
                    Interop.UpdateCommittedSegmentationDoNotRemove( CurrentNeuralProcess.Id, CurrentNeuralProcess.Color );
                    Interop.InitializeSegmentation();
                    Interop.InitializeEdgeXYMapForSplitting( DatasetDescription.VolumeDescriptions, SplitNeuralProcess.Id );
                    Interop.InitializeConstraintMap();
                    Interop.InitializeConstraintMapFromIdMapForSplitting( SplitNeuralProcess.Id );
                    Interop.VisualUpdate();
                    Interop.VisualUpdateColorMap();

                    CommittedSegmentationEqualsUndoBuffer = false;
                    CommittedSegmentationEqualsRedoBuffer = false;
                }
            }
            else
            {
                CommitSegmentation();
            }

            DatasetDescription.NeuralProcessDescriptions.Add( neuralProcessId,
                                           new NeuralProcessDescription( neuralProcessId )
                                           {
                                               Name = tmpNeuralProcessName,
                                               Color = new Vector3( (float)randomColor.Red, (float)randomColor.Green, (float)randomColor.Blue ),
                                               BreadcrumbDescriptions = new List<BreadcrumbDescription>()
                                           } );

            CurrentNeuralProcess = DatasetDescription.NeuralProcessDescriptions[ neuralProcessId ];

            return;
        }

        public void RemoveNeuralProcess( string neuralProcessName )
        {
            try
            {
                var neuralProcess = (from neuralProcessDescription in DatasetDescription.NeuralProcessDescriptions.Values
                                     where neuralProcessDescription.Name.Equals(neuralProcessName)
                                     select neuralProcessDescription).First();

                SelectNeuralProcess( neuralProcess.Id );
                ClearSegmentationAndCostMap();

                DatasetDescription.NeuralProcessDescriptions.Remove( neuralProcess.Id );
            }
            catch
            {
                Console.WriteLine( "There is no neural process with the name " + neuralProcessName );
            }
        }

        public void Update()
        {
            if ( DatasetLoaded )
            {
                if ( InitializeCostMapMode != InitializeCostMapMode.Idle )
                {
                    switch ( InitializeCostMapMode )
                    {
                        case InitializeCostMapMode.Initialize:
                            Interop.InitializeCostMapFromPrimalMap();
                            InitializeCostMapMode = InitializeCostMapMode.Forward;
                            break;

                        case InitializeCostMapMode.Forward:
                            Interop.IncrementCostMapFromPrimalMapForward();
                            InitializeCostMapMode = InitializeCostMapMode.Backward;
                            break;

                        case InitializeCostMapMode.Backward:
                            Interop.IncrementCostMapFromPrimalMapBackward();
                            InitializeCostMapMode = InitializeCostMapMode.Finalize;
                            break;

                        case InitializeCostMapMode.Finalize:
                            Interop.FinalizeCostMapFromPrimalMap();
                            InitializeCostMapMode = InitializeCostMapMode.Idle;
                            break;
                    }
                }
                else if ( Interop.ConvergenceGap > Constants.CONVERGENCE_GAP_THRESHOLD &&
                            Interop.ConvergenceGapDelta > Constants.CONVERGENCE_DELTA_THRESHOLD )
                {
                    if ( DimensionMode == DimensionMode.Two )
                    {
                        if ( !Constants.Parameters.GetBool( "DIRECT_SCRIBBLE_PROPAGATION" ) )
                        {
                            Interop.Update2D( InteractionMode == InteractionMode.HighLatency
                                                                ? Constants.NUM_ITERATIONS_PER_VISUAL_UPDATE_HIGH_LATENCY_2D
                                                                : Constants.NUM_ITERATIONS_PER_VISUAL_UPDATE_LOW_LATENCY_2D,
                                                            CurrentSlice );
                        }
                    }
                    else
                    {
                        Interop.Update3D( InteractionMode == InteractionMode.HighLatency
                                                            ? Constants.NUM_ITERATIONS_PER_VISUAL_UPDATE_HIGH_LATENCY_3D
                                                            : Constants.NUM_ITERATIONS_PER_VISUAL_UPDATE_LOW_LATENCY_3D );
                    }

                    Interop.VisualUpdate();
                }
            }
        }

        public void InitializeConstraintMapAndSegmentationFromBreadcrumbs()
        {
            if ( DatasetDescription.NeuralProcessDescriptions.Count > 0 )
            {
                Release.Assert( CurrentNeuralProcess != null );

                var backgroundBreadcrumbs = from neuralProcessDescription in DatasetDescription.NeuralProcessDescriptions.Values
                                            from breadcrumb in neuralProcessDescription.BreadcrumbDescriptions
                                            where !neuralProcessDescription.Color.Equals( CurrentNeuralProcess.Color )
                                            select breadcrumb;


                backgroundBreadcrumbs.ToList().ForEach(
                    breadcrumb => Interop.AddBackgroundHardConstraint( breadcrumb.Position,
                                                                       Constants.MIN_BRUSH_WIDTH,
                                                                       HardConstraintMode.Breadcrumb ) );

                CurrentNeuralProcess.BreadcrumbDescriptions.ToList().ForEach(
                    breadcrumb => Interop.AddForegroundHardConstraint( breadcrumb.Position,
                                                                       Constants.MIN_BRUSH_WIDTH,
                                                                       HardConstraintMode.Breadcrumb ) );
            }
        }

        public void InitializeSegmentation2D()
        {
            DimensionMode = Constants.Parameters.GetBool( "DIRECT_ANISOTROPIC_TV" ) ? DimensionMode.Three : DimensionMode.Two;
            Interop.ConvergenceGap = Constants.Parameters.GetFloat( "MAX_CONVERGENCE_GAP" );
            Interop.ConvergenceGapDelta = Constants.Parameters.GetFloat( "MAX_CONVERGENCE_GAP_DELTA" );
        }

        public void InitializeSegmentation3D()
        {
            Interop.InitializeConstraintMapFromPrimalMap();
            Interop.UpdateConstraintMapAndPrimalMapFromCostMap();
            Interop.VisualUpdate();

            DimensionMode = DimensionMode.Three;
            Interop.ConvergenceGap = Constants.Parameters.GetFloat( "MAX_CONVERGENCE_GAP" );
            Interop.ConvergenceGapDelta = Constants.Parameters.GetFloat( "MAX_CONVERGENCE_GAP_DELTA" );
        }

        public void InitializeCostMap()
        {
            InitializeCostMapMode = InitializeCostMapMode.Initialize;
        }

        public void IncrementMaxForegroundCostDelta()
        {
            Interop.MaxForegroundCostDelta = Interop.MaxForegroundCostDelta + 5;
            Console.WriteLine( "MaxForegroundCostDelta = {0}", Interop.MaxForegroundCostDelta );
        }

        public void DecrementMaxForegroundCostDelta()
        {
            Interop.MaxForegroundCostDelta = Interop.MaxForegroundCostDelta - 5;
            Console.WriteLine( "MaxForegroundCostDelta = {0}", Interop.MaxForegroundCostDelta );
        }

        public void CommitSegmentation()
        {
            if ( CurrentNeuralProcess != null )
            {
                if (CurrentSegmenterToolMode == SegmenterToolMode.Split)
                {
                    Interop.UpdateCommittedSegmentationDoNotRemove(CurrentNeuralProcess.Id, CurrentNeuralProcess.Color);
                }
                else
                {
                    Interop.UpdateCommittedSegmentation(CurrentNeuralProcess.Id, CurrentNeuralProcess.Color);
                }
            }

            Interop.InitializeConstraintMap();
            Interop.InitializeSegmentation();
            Interop.VisualUpdate();
            Interop.VisualUpdateColorMap();

            Interop.ConvergenceGap = 0;
            Interop.ConvergenceGapDelta = 0;

            DimensionMode = DimensionMode.Two;
            CurrentNeuralProcess = null;
            MergeSourceNeuralProcess = null;
            MergeDestinationNeuralProcess = null;
            SplitNeuralProcess = null;
            CommittedSegmentationEqualsUndoBuffer = false;
            CommittedSegmentationEqualsRedoBuffer = false;
        }

        public void CancelSegmentation()
        {
            Interop.InitializeCostMap();
            Interop.InitializeConstraintMap();
            Interop.InitializeSegmentation();
            Interop.VisualUpdate();

            RedoLastCommit();
        }

        public void UndoLastCommit()
        {
            Interop.InitializeCostMap();
            Interop.InitializeConstraintMap();
            Interop.InitializeSegmentation();
            Interop.RedoLastChangeToCommittedSegmentation();
            Interop.UndoLastChangeToCommittedSegmentation();
            Interop.VisualUpdate();
            Interop.VisualUpdateColorMap();

            DimensionMode = DimensionMode.Two;
            CurrentNeuralProcess = null;
            MergeSourceNeuralProcess = null;
            MergeDestinationNeuralProcess = null;
            SplitNeuralProcess = null;

            CommittedSegmentationEqualsUndoBuffer = true;
            CommittedSegmentationEqualsRedoBuffer = false;
        }

        public void RedoLastCommit()
        {
            Interop.InitializeCostMap();
            Interop.InitializeConstraintMap();
            Interop.InitializeSegmentation();
            Interop.RedoLastChangeToCommittedSegmentation();
            Interop.VisualUpdate();
            Interop.VisualUpdateColorMap();

            DimensionMode = DimensionMode.Two;
            CurrentNeuralProcess = null;
            MergeSourceNeuralProcess = null;
            MergeDestinationNeuralProcess = null;
            SplitNeuralProcess = null;

            CommittedSegmentationEqualsUndoBuffer = false;
            CommittedSegmentationEqualsRedoBuffer = true;
        }

        public void ClearSegmentationAndCostMap()
        {
            Interop.InitializeCostMap();
            Interop.InitializeConstraintMap();
            Interop.InitializeSegmentation();

            if ( CurrentSegmenterToolMode == SegmenterToolMode.Split )
            {
                Interop.InitializeEdgeXYMapForSplitting( DatasetDescription.VolumeDescriptions, SplitNeuralProcess.Id );
                Interop.InitializeConstraintMapFromIdMapForSplitting( SplitNeuralProcess.Id );                
            }
            else
            {
                InitializeNewNeuralProcess();                
            }

            Interop.VisualUpdate();
            
            DimensionMode = DimensionMode.Two;
            Interop.ConvergenceGap = 0;
            Interop.ConvergenceGapDelta = 0;
        }

        public void ClearSegmentation()
        {
            Interop.InitializeConstraintMap();
            Interop.InitializeSegmentation();

            InitializeNewNeuralProcess(); 
            
            Interop.VisualUpdate();

            DimensionMode = DimensionMode.Two;
            Interop.ConvergenceGap = 0;
            Interop.ConvergenceGapDelta = 0;
        }

        public void IncrementCurrentSlice()
        {
            if ( Interop.VolumeDescription.NumVoxelsZ > 1 )
            {
                CurrentSlice++;
                if ( CurrentSlice > Interop.VolumeDescription.NumVoxelsZ - 1 )
                {
                    CurrentSlice = Interop.VolumeDescription.NumVoxelsZ - 1;
                }
            }
        }

        public void DecrementCurrentSlice()
        {
            if ( Interop.VolumeDescription.NumVoxelsZ > 1 )
            {
                CurrentSlice--;
                if ( CurrentSlice < 0 )
                {
                    CurrentSlice = 0;
                }
            }
        }

        public void IncrementCurrentTexture()
        {
            CurrentD3D11CudaTextureIndex++;
            D3D11CudaTextureEnumerator.MoveNext();

            if ( CurrentD3D11CudaTextureIndex > Interop.D3D11CudaTextures.Internal.Count - 1 )
            {
                CurrentD3D11CudaTextureIndex = 0;
                D3D11CudaTextureEnumerator.Reset();
                D3D11CudaTextureEnumerator.MoveNext();
            }
        }

        public void DecrementCurrentTexture()
        {
            CurrentD3D11CudaTextureIndex--;

            if ( CurrentD3D11CudaTextureIndex < 0 )
            {
                CurrentD3D11CudaTextureIndex = Interop.D3D11CudaTextures.Internal.Count - 1;
            }

            D3D11CudaTextureEnumerator.Reset();
            D3D11CudaTextureEnumerator.MoveNext();

            for ( var i = 0; i < CurrentD3D11CudaTextureIndex; i++ )
            {
                D3D11CudaTextureEnumerator.MoveNext();
            }
        }

        public void ToggleShowSegmentation()
        {
            ShowSegmentation = !ShowSegmentation;
        }

        public void BeginScribble( int x, int y )
        {
            mMousePreviousX = x;
            mMousePreviousY = y;

            InteractionMode = InteractionMode.LowLatency;

            if ( !Constants.Parameters.GetBool( "DIRECT_ANISOTROPIC_TV" ) )
            {
                DimensionMode = DimensionMode.Two;
            }
        }

        public void EndScribble()
        {
            InteractionMode = InteractionMode.HighLatency;
        }

        public void SelectNeuralProcessOrScribble( int x, int y, ConstraintType constraintType, int brushWidth )
        {
            mMousePreviousX = x;
            mMousePreviousY = y;

            var newNeuralProcessId = Interop.GetNeuralProcessId( new Vector3( x, y, CurrentSlice ) );

            if ( newNeuralProcessId != Constants.NULL_NEURAL_PROCESS.Id )
            {
                SelectNeuralProcess( newNeuralProcessId );
            }
            else
            {
                Scribble( x, y, constraintType, brushWidth );
            }
        }

        public void Scribble( int x, int y, ConstraintType constraintType, int brushWidth )
        {
            if ( CurrentNeuralProcess != null )
            {
                Interop.ConvergenceGap = Constants.Parameters.GetFloat( "MAX_CONVERGENCE_GAP" );
                Interop.ConvergenceGapDelta = Constants.Parameters.GetFloat( "MAX_CONVERGENCE_GAP_DELTA" );

                switch ( constraintType )
                {
                    case ConstraintType.Foreground:
                        Interop.AddForegroundHardConstraint(
                            new Vector3( x, y, CurrentSlice ),
                            new Vector3( mMousePreviousX, mMousePreviousY, CurrentSlice ),
                            brushWidth,
                            HardConstraintMode.Scribble );
                        break;

                    case ConstraintType.Background:
                        Interop.AddBackgroundHardConstraint(
                            new Vector3( x, y, CurrentSlice ),
                            new Vector3( mMousePreviousX, mMousePreviousY, CurrentSlice ),
                            brushWidth,
                            HardConstraintMode.Scribble );
                        break;

                    default:
                        Release.Assert( false );
                        break;
                }

                mMousePreviousX = x;
                mMousePreviousY = y;                
            }
        }

        public void SelectNeuralProcess( int id )
        {
            switch ( CurrentSegmenterToolMode )
            {
                case SegmenterToolMode.Adjust:

                    SaveOldNeuralProcess();
                    CurrentNeuralProcess = DatasetDescription.NeuralProcessDescriptions[ id ];
                    InitializeNewNeuralProcess();
                    break;

                case SegmenterToolMode.Merge:
                    Release.Assert( false );
                    break;

                case SegmenterToolMode.Split:
                    CurrentNeuralProcess = DatasetDescription.NeuralProcessDescriptions[ id ];
                    break;

                default:
                    Release.Assert( false );
                    break;
            }
        }

        public void SelectMergeSourceNeuralProcess( int x, int y )
        {
            var neuralProcessId = Interop.GetNeuralProcessId( new Vector3( x, y, CurrentSlice ) );

            if ( neuralProcessId != Constants.NULL_NEURAL_PROCESS.Id )
            {
                MergeSourceNeuralProcess = DatasetDescription.NeuralProcessDescriptions[ neuralProcessId ];
            }
        }

        public void SelectMergeDestinationNeuralProcess( int x, int y )
        {
            var clickCoordinates = new Vector3( x, y, CurrentSlice );
            var neuralProcessId = Interop.GetNeuralProcessId( clickCoordinates );

            if ( neuralProcessId != Constants.NULL_NEURAL_PROCESS.Id )
            {
                MergeDestinationNeuralProcess = DatasetDescription.NeuralProcessDescriptions[ neuralProcessId ];

                if ( MergeSourceNeuralProcess != null && MergeDestinationNeuralProcess != null )
                {
                    if ( ConstrainSegmentationMergeToCurrentSlice && ConstrainSegmentationMergeToConnectedComponent )
                    {
                        Interop.ReplaceNeuralProcessInCommittedSegmentation2DConnectedComponentOnly( MergeDestinationNeuralProcess.Id, MergeSourceNeuralProcess.Id, MergeSourceNeuralProcess.Color, CurrentSlice, clickCoordinates );                        
                    }
                    else
                    if ( ConstrainSegmentationMergeToCurrentSlice )
                    {
                        Interop.ReplaceNeuralProcessInCommittedSegmentation2D( MergeDestinationNeuralProcess.Id, MergeSourceNeuralProcess.Id, MergeSourceNeuralProcess.Color, CurrentSlice );                        
                    }
                    else
                    if ( ConstrainSegmentationMergeToConnectedComponent )
                    {
                        Interop.ReplaceNeuralProcessInCommittedSegmentation3DConnectedComponentOnly( MergeDestinationNeuralProcess.Id, MergeSourceNeuralProcess.Id, MergeSourceNeuralProcess.Color, clickCoordinates );
                    }
                    else
                    {
                        Interop.ReplaceNeuralProcessInCommittedSegmentation3D( MergeDestinationNeuralProcess.Id, MergeSourceNeuralProcess.Id, MergeSourceNeuralProcess.Color );                        
                    }

                    Interop.VisualUpdateColorMap();

                    CommittedSegmentationEqualsUndoBuffer = false;
                    CommittedSegmentationEqualsRedoBuffer = true;
                }
            }
        }

        public void SelectSplitNeuralProcessOrScribble( int x, int y, ConstraintType constraintType, int brushWidth )
        {
            if ( SplitNeuralProcess == null )
            {
                var neuralProcessId = Interop.GetNeuralProcessId( new Vector3( x, y, CurrentSlice ) );

                if ( neuralProcessId != Constants.NULL_NEURAL_PROCESS.Id )
                {
                    SplitNeuralProcess = DatasetDescription.NeuralProcessDescriptions[ neuralProcessId ];
 
                    if ( SplitNeuralProcess != null )
                    {
                        InitializeSplit();
                    }
                }
            }
            else
            {
                Scribble( x, y, constraintType, brushWidth );
            }
        }

        void InitializeNewNeuralProcess()
        {
            if ( CurrentNeuralProcess != null )
            {
                Interop.InitializeConstraintMap();
                Interop.InitializeConstraintMapFromIdMap( CurrentNeuralProcess.Id );

                for ( var i = 0; i < Constants.NUM_CONSTRAINT_MAP_DILATION_PASSES_INITIALIZE_NEW_PROCESS; i++ )
                {
                    Interop.VisualUpdate();
                    Interop.DilateConstraintMap();
                }

                InitializeConstraintMapAndSegmentationFromBreadcrumbs();

                Interop.InitializeSegmentationAndRemoveFromCommittedSegmentation( CurrentNeuralProcess.Id );
                Interop.VisualUpdateColorMap();
                Interop.VisualUpdate();

                CommittedSegmentationEqualsUndoBuffer = false;
                CommittedSegmentationEqualsRedoBuffer = false;
            }
        }

        void SaveOldNeuralProcess()
        {
            if ( CurrentNeuralProcess != null )
            {
                if ( CurrentSegmenterToolMode == SegmenterToolMode.Split )
                {
                    Interop.UpdateCommittedSegmentationDoNotRemove( CurrentNeuralProcess.Id, CurrentNeuralProcess.Color );                    
                }
                else
                {
                    Interop.UpdateCommittedSegmentation( CurrentNeuralProcess.Id, CurrentNeuralProcess.Color );
                }
                Interop.VisualUpdateColorMap();

                CommittedSegmentationEqualsUndoBuffer = false;
                CommittedSegmentationEqualsRedoBuffer = false;
            }
        }

        void InitializeSplit()
        {
            if ( SplitNeuralProcess != null )
            {
                Interop.InitializeEdgeXYMapForSplitting( DatasetDescription.VolumeDescriptions, SplitNeuralProcess.Id );
                Interop.InitializeConstraintMap();
                Interop.InitializeConstraintMapFromIdMapForSplitting( SplitNeuralProcess.Id );
                Interop.InitializeSegmentation();
                Interop.VisualUpdate();
                Interop.VisualUpdateColorMap();

                CommittedSegmentationEqualsUndoBuffer = false;
                CommittedSegmentationEqualsRedoBuffer = false;
            }            
        }

        Rgb GetRandomColor()
        {
            var hsvImage = new Image<Hsv, byte>( 1, 1 );
            hsvImage[ 0, 0 ] = new Hsv( mRandom.Next( 0, 179 ), 255, 255 );

            var bmp = hsvImage.ToBitmap();

            var rgbImage = new Image<Rgb, byte>( bmp );
            return rgbImage[ 0, 0 ];
        }
    }
}
