using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace Mojo.Segmenter2D
{
    internal class UserInputHandler : IUserInputHandler
    {
        private readonly Segmenter mSegmenter;

        public UserInputHandler( Segmenter segmenter )
        {
            mSegmenter = segmenter;
        }

        public void OnKeyDown( System.Windows.Input.KeyEventArgs keyEventArgs )
        {
            switch ( keyEventArgs.Key )
            {
                case Key.F2:
                    mSegmenter.InitializeSegmentation2D();
                    break;

                case Key.F3:
                    mSegmenter.InitializeSegmentation3D();
                    break;

                case Key.C:
                    mSegmenter.InitializeCostMap();
                    break;

                case Key.D:
                    mSegmenter.Interop.DumpIntermediateData();
                    break;

                case Key.S:
                    mSegmenter.ToggleShowSegmentation();
                    break;

                case Key.OemPlus:
                    mSegmenter.IncrementMaxForegroundCostDelta();
                    break;

                case Key.OemMinus:
                    mSegmenter.DecrementMaxForegroundCostDelta();
                    break;

                case Key.Escape:
                    mSegmenter.CommitSegmentation();
                    break;

                case Key.Back:
                    mSegmenter.ClearSegmentationAndCostMap();
                    break;

                case Key.Delete:
                    mSegmenter.ClearSegmentation();
                    break;

                case Key.Left:
                    mSegmenter.DecrementCurrentSlice();
                    break;

                case Key.Right:
                    mSegmenter.IncrementCurrentSlice();
                    break;

                case Key.Up:
                    mSegmenter.IncrementCurrentTexture();
                    break;

                case Key.Down:
                    mSegmenter.DecrementCurrentTexture();
                    break;
            }
        }

        public void OnMouseDown( System.Windows.Forms.MouseEventArgs mouseEventArgs, int width, int height )
        {
            if ( mSegmenter.DatasetLoaded )
            {
                var x = (int)Math.Floor( ( (float)mouseEventArgs.X / width ) * mSegmenter.Interop.VolumeDescription.NumVoxelsX );
                var y = (int)Math.Floor( ( (float)mouseEventArgs.Y / height ) * mSegmenter.Interop.VolumeDescription.NumVoxelsY );

                switch ( mSegmenter.CurrentSegmenterToolMode )
                {
                    case SegmenterToolMode.Adjust:
                        mSegmenter.BeginScribble( x, y );

                        if ( Keyboard.IsKeyDown( Key.LeftAlt ) )
                        {
                            if ( mouseEventArgs.Button == MouseButtons.Left )
                            {
                                mSegmenter.SelectNeuralProcessOrScribble( x, y, ConstraintType.Foreground, Constants.MAX_BRUSH_WIDTH );
                            }

                            if ( mouseEventArgs.Button == MouseButtons.Right )
                            {
                                mSegmenter.SelectNeuralProcessOrScribble( x, y, ConstraintType.Background, Constants.MAX_BRUSH_WIDTH );
                            }
                        }
                        else
                        {
                            if ( mouseEventArgs.Button == MouseButtons.Left )
                            {
                                mSegmenter.SelectNeuralProcessOrScribble( x, y, ConstraintType.Foreground, Constants.MIN_BRUSH_WIDTH );
                            }

                            if ( mouseEventArgs.Button == MouseButtons.Right )
                            {
                                mSegmenter.SelectNeuralProcessOrScribble( x, y, ConstraintType.Background, Constants.MIN_BRUSH_WIDTH );
                            }
                        }
                        break;

                    case SegmenterToolMode.Merge:
                        if ( mouseEventArgs.Button == MouseButtons.Left )
                        {
                            mSegmenter.SelectMergeSourceNeuralProcess( x, y );
                        }

                        if ( mouseEventArgs.Button == MouseButtons.Right )
                        {
                            if ( mSegmenter.MergeSourceNeuralProcess != null )
                            {
                                mSegmenter.SelectMergeDestinationNeuralProcess( x, y );                                
                            }
                        }
                        break;

                    case SegmenterToolMode.Split:
                        mSegmenter.BeginScribble( x, y );

                        if ( Keyboard.IsKeyDown( Key.LeftAlt ) )
                        {
                            if ( mouseEventArgs.Button == MouseButtons.Left )
                            {
                                mSegmenter.SelectSplitNeuralProcessOrScribble( x, y, ConstraintType.Foreground, Constants.MAX_BRUSH_WIDTH );
                            }

                            if ( mouseEventArgs.Button == MouseButtons.Right )
                            {
                                mSegmenter.SelectSplitNeuralProcessOrScribble( x, y, ConstraintType.Background, Constants.MAX_BRUSH_WIDTH );
                            }
                        }
                        else
                        {
                            if ( mouseEventArgs.Button == MouseButtons.Left )
                            {
                                mSegmenter.SelectSplitNeuralProcessOrScribble( x, y, ConstraintType.Foreground, Constants.MIN_BRUSH_WIDTH );
                            }

                            if ( mouseEventArgs.Button == MouseButtons.Right )
                            {
                                mSegmenter.SelectSplitNeuralProcessOrScribble( x, y, ConstraintType.Background, Constants.MIN_BRUSH_WIDTH );
                            }
                        }
                        break;

                    default:
                        Release.Assert( false );
                        break;
                }
            }
        }

        public void OnMouseUp( System.Windows.Forms.MouseEventArgs mouseEventArgs, int width, int height )
        {
            if ( mSegmenter.DatasetLoaded )
            {
                switch ( mSegmenter.CurrentSegmenterToolMode )
                {
                    case SegmenterToolMode.Adjust:
                        mSegmenter.EndScribble();
                        break;

                    case SegmenterToolMode.Merge:
                        break;

                    case SegmenterToolMode.Split:
                        break;

                    default:
                        Release.Assert( false );
                        break;
                }
            }
        }

        public void OnMouseMove( System.Windows.Forms.MouseEventArgs mouseEventArgs, int width, int height )
        {
            if ( mSegmenter.DatasetLoaded )
            {
                var x = (int)Math.Floor( ( (float)mouseEventArgs.X / width ) * mSegmenter.Interop.VolumeDescription.NumVoxelsX );
                var y = (int)Math.Floor( ( (float)mouseEventArgs.Y / height ) * mSegmenter.Interop.VolumeDescription.NumVoxelsY ); 
                
                switch ( mSegmenter.CurrentSegmenterToolMode )
                {
                    case SegmenterToolMode.Adjust:
                        if ( Keyboard.IsKeyDown( Key.LeftAlt ) )
                        {
                            if ( mouseEventArgs.Button == MouseButtons.Left )
                            {
                                mSegmenter.Scribble( x, y, ConstraintType.Foreground, Constants.MAX_BRUSH_WIDTH );
                            }

                            if ( mouseEventArgs.Button == MouseButtons.Right )
                            {
                                mSegmenter.Scribble( x, y, ConstraintType.Background, Constants.MAX_BRUSH_WIDTH );
                            }
                        }
                        else
                        {
                            if ( mouseEventArgs.Button == MouseButtons.Left )
                            {
                                mSegmenter.Scribble( x, y, ConstraintType.Foreground, Constants.MIN_BRUSH_WIDTH );
                            }

                            if ( mouseEventArgs.Button == MouseButtons.Right )
                            {
                                mSegmenter.Scribble( x, y, ConstraintType.Background, Constants.MIN_BRUSH_WIDTH );
                            }
                        }
                        break;

                    case SegmenterToolMode.Merge:
                        break;

                    case SegmenterToolMode.Split:
                        if ( mSegmenter.SplitNeuralProcess != null )
                        {
                            if ( Keyboard.IsKeyDown( Key.LeftAlt ) )
                            {
                                if ( mouseEventArgs.Button == MouseButtons.Left )
                                {
                                    mSegmenter.Scribble( x, y, ConstraintType.Foreground, Constants.MAX_BRUSH_WIDTH );
                                }

                                if ( mouseEventArgs.Button == MouseButtons.Right )
                                {
                                    mSegmenter.Scribble( x, y, ConstraintType.Background, Constants.MAX_BRUSH_WIDTH );
                                }
                            }
                            else
                            {
                                if ( mouseEventArgs.Button == MouseButtons.Left )
                                {
                                    mSegmenter.Scribble( x, y, ConstraintType.Foreground, Constants.MIN_BRUSH_WIDTH );
                                }

                                if ( mouseEventArgs.Button == MouseButtons.Right )
                                {
                                    mSegmenter.Scribble( x, y, ConstraintType.Background, Constants.MIN_BRUSH_WIDTH );
                                }
                            }                            
                        }
                        break;

                    default:
                        Release.Assert( false );
                        break;
                }
            }
        }

        public void OnMouseWheel( System.Windows.Forms.MouseEventArgs mouseEventArgs )
        {
            if ( mouseEventArgs.Delta > 0 )
            {
                mSegmenter.IncrementCurrentSlice();
            }
            else
            if ( mouseEventArgs.Delta < 0 )
            {
                mSegmenter.DecrementCurrentSlice();
            }

        }
    }
}
