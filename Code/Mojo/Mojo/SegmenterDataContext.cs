using System;
using System.Collections.Generic;
using System.ComponentModel;
using Mojo.Interop;

namespace Mojo
{
    public class SegmenterDataContext : NotifyPropertyChanged, IDisposable
    {
        private readonly Segmenter mSegmenter;

        public string ToolbarString
        {
            get
            {
                if ( !mSegmenter.DatasetLoaded )
                {
                    return "No dataset loaded.";
                }

                switch ( mSegmenter.CurrentSegmenterToolMode )
                {
                    case SegmenterToolMode.Adjust:
                        return
                            mSegmenter.CurrentNeuralProcess == null ?
                            "Left mouse button picks a neural process" :
                            "Current Neural Process: " + mSegmenter.CurrentNeuralProcess.Name + " (" + mSegmenter.CurrentNeuralProcess.Color + "). Left mouse button indicates foreground. Right mouse button indicates background.";

                    case SegmenterToolMode.Merge:
                        return
                            mSegmenter.MergeSourceNeuralProcess == null ?
                            "Left mouse button picks source." :
                            "Merge Source: " + mSegmenter.MergeSourceNeuralProcess.Name + " (" + mSegmenter.MergeSourceNeuralProcess.Color + "). Right mouse button picks destination.";

                    case SegmenterToolMode.Split:
                        return
                            mSegmenter.SplitNeuralProcess == null ?
                            "Left mouse button selects process to split." :
                            "Process to split: " + mSegmenter.SplitNeuralProcess.Name + " (" + mSegmenter.SplitNeuralProcess.Color + "). Use mouse to paint within neural process.";

                    default:
                        Release.Assert( false );
                        return "";
                }
            } 
        }

        public bool EditMenuIsEnabled
        {
            get
            {
                return UndoLastCommitMenuItemIsEnabled || RedoLastCommitMenuItemIsEnabled;
            }
        }

        public bool UndoLastCommitMenuItemIsEnabled
        {
            get
            {
                return mSegmenter.DatasetLoaded && !mSegmenter.CommittedSegmentationEqualsUndoBuffer;
            }
        }

        public bool RedoLastCommitMenuItemIsEnabled
        {
            get
            {
                return mSegmenter.DatasetLoaded && !mSegmenter.CommittedSegmentationEqualsRedoBuffer && mSegmenter.CommittedSegmentationEqualsUndoBuffer;
            }
        }

        public bool AdjustSegmentationToolRadioButtonIsChecked
        {
            get
            {
                return mSegmenter.CurrentSegmenterToolMode == SegmenterToolMode.Adjust && mSegmenter.DatasetLoaded;
            }
            set
            {
                if ( value )
                {
                    if ( mSegmenter.CurrentSegmenterToolMode != SegmenterToolMode.Adjust )
                    {
                        mSegmenter.Interop.InitializeEdgeXYMap( mSegmenter.DatasetDescription.VolumeDescriptions );
                        mSegmenter.CommitSegmentation();
                    }

                    mSegmenter.CurrentSegmenterToolMode = SegmenterToolMode.Adjust;
                    Refresh();
                }
            }
        }

        public bool MergeSegmentationToolRadioButtonIsChecked
        {
            get
            {
                return mSegmenter.CurrentSegmenterToolMode == SegmenterToolMode.Merge && mSegmenter.DatasetLoaded;
            }
            set
            {
                if ( value )
                {
                    if ( mSegmenter.CurrentSegmenterToolMode != SegmenterToolMode.Merge )
                    {
                        mSegmenter.Interop.InitializeEdgeXYMap( mSegmenter.DatasetDescription.VolumeDescriptions );
                        mSegmenter.CommitSegmentation();
                    }

                    mSegmenter.CurrentSegmenterToolMode = SegmenterToolMode.Merge;
                    Refresh();
                }
            }
        }

        public bool SplitSegmentationToolRadioButtonIsChecked
        {
            get
            {
                return mSegmenter.CurrentSegmenterToolMode == SegmenterToolMode.Split && mSegmenter.DatasetLoaded;
            }
            set
            {
                if ( value )
                {
                    if ( mSegmenter.CurrentSegmenterToolMode != SegmenterToolMode.Split )
                    {
                        mSegmenter.Interop.InitializeEdgeXYMap( mSegmenter.DatasetDescription.VolumeDescriptions );
                        mSegmenter.CommitSegmentation();
                    }

                    mSegmenter.CurrentSegmenterToolMode = SegmenterToolMode.Split;
                    Refresh();
                }
            }
        }

        public bool NotMergeModeAndDatasetLoaded
        {
            get
            {
                return mSegmenter.CurrentSegmenterToolMode != SegmenterToolMode.Merge && mSegmenter.DatasetLoaded;
            }
        }

        public bool MergeModeAndDatasetLoaded
        {
            get
            {
                return mSegmenter.CurrentSegmenterToolMode == SegmenterToolMode.Merge && mSegmenter.DatasetLoaded;
            }
        }

        public object CurrentNeuralProcess
        {
            get
            {
                return mSegmenter.CurrentNeuralProcess == null
                           ? (object)null
                           : new KeyValuePair< int, NeuralProcessDescription >( mSegmenter.CurrentNeuralProcess.Id, mSegmenter.CurrentNeuralProcess );
            }
            set
            {
                if ( mSegmenter.CurrentSegmenterToolMode == SegmenterToolMode.Split )
                {
                    if ( mSegmenter.CurrentNeuralProcess != null )
                    {
                        mSegmenter.Interop.UpdateCommittedSegmentationDoNotRemove( mSegmenter.CurrentNeuralProcess.Id, mSegmenter.CurrentNeuralProcess.Color );
                        mSegmenter.Interop.InitializeConstraintMap();
                        mSegmenter.Interop.InitializeSegmentation();
                        mSegmenter.Interop.InitializeEdgeXYMapForSplitting( mSegmenter.DatasetDescription.VolumeDescriptions, mSegmenter.SplitNeuralProcess.Id );
                        mSegmenter.Interop.InitializeConstraintMapFromIdMapForSplitting( mSegmenter.SplitNeuralProcess.Id );
                        mSegmenter.Interop.VisualUpdate();
                        mSegmenter.Interop.VisualUpdateColorMap();

                        mSegmenter.CommittedSegmentationEqualsUndoBuffer = false;
                        mSegmenter.CommittedSegmentationEqualsRedoBuffer = false;
                    }
                }

                mSegmenter.SelectNeuralProcess( ((KeyValuePair<int, NeuralProcessDescription>)value).Key );
                Refresh(); 
            }
        }

        public SegmenterDataContext( Segmenter segmenter )
        {
            mSegmenter = segmenter;
            mSegmenter.PropertyChanged += OnPropertyChangedInner;
            mSegmenter.Interop.PropertyChanged += OnPropertyChangedInner;
        }

        public void Dispose()
        {
            mSegmenter.Interop.PropertyChanged -= OnPropertyChangedInner;
            mSegmenter.PropertyChanged -= OnPropertyChangedInner;
        }

        private void OnPropertyChangedInner( object sender, PropertyChangedEventArgs e )
        {
            Refresh();
        }

        private void Refresh()
        {
            OnPropertyChanged( "EditMenuIsEnabled" );
            OnPropertyChanged( "UndoLastCommitMenuItemIsEnabled" );
            OnPropertyChanged( "RedoLastCommitMenuItemIsEnabled" );
            OnPropertyChanged( "AdjustSegmentationToolRadioButtonIsChecked" );
            OnPropertyChanged( "MergeSegmentationToolRadioButtonIsChecked" );
            OnPropertyChanged( "SplitSegmentationToolRadioButtonIsChecked" );
            OnPropertyChanged( "MergeModeAndDatasetLoaded" );
            OnPropertyChanged( "NotMergeModeAndDatasetLoaded" );
            OnPropertyChanged( "CurrentNeuralProcess" );
            OnPropertyChanged( "ToolbarString" );
        }
    }
}
