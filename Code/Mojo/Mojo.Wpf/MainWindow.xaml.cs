using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.Structure;
using Mojo.Interop;

namespace Mojo.Wpf
{
    public partial class MainWindow : IDisposable
    {
        public Engine Engine { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            NeuralProcessesListBox.SelectionChanged += OnNeuralProcessListBoxSelectionChanged;
        }

        public void Dispose()
        {
            NeuralProcessesListBox.SelectionChanged -= OnNeuralProcessListBoxSelectionChanged;
        }

        private void LoadDataset( object sender, RoutedEventArgs e )
        {
            var dialog = new LoadDatasetDialog();
            var result = dialog.ShowDialog();

            if ( result == true )
            {
                if ( !Directory.Exists( dialog.SourceImages.Text ) )
                {
                    Console.WriteLine( "SourceImages directory does not exist." );
                    return;
                }
                if ( !Directory.Exists( dialog.FilteredImages.Text ) )
                {
                    Console.WriteLine( "FilteredImages directory does not exist." );
                    return;
                }
                if ( dialog.OpticalFlowBackwardImages.Text.Length > 0 && !Directory.Exists( dialog.OpticalFlowForwardImages.Text ) )
                {
                    Console.WriteLine( "OpticalFlowForwardImages directory does not exist." );
                    return;
                }
                if ( dialog.OpticalFlowBackwardImages.Text.Length > 0 &&!Directory.Exists( dialog.OpticalFlowBackwardImages.Text ) )
                {
                    Console.WriteLine( "OpticalFlowBackwardImages directory does not exist." );
                    return;
                }

                var imageFileNames = from fileInfo in new DirectoryInfo( dialog.SourceImages.Text ).GetFiles( "*.*" ) select fileInfo.Name;
                var imageFilePaths = from imageFileName in imageFileNames select Path.Combine( dialog.SourceImages.Text, imageFileName );

                using ( var image = new Image< Gray, Byte >( imageFilePaths.First() ) )
                {
                    if ( ( image.Width % 4 != 0 ) || ( image.Height % 4 != 0 ) )
                    {
                        Console.WriteLine( "Image dimensions must each be a multiple of 4." );
                        return;
                    }
                }

                var segmenterImageStackLoadDescription = new SegmenterImageStackLoadDescription
                                                         {
                                                             Directories = new Dictionary< string >
                                                                           {
                                                                               { "SourceMap", dialog.SourceImages.Text },
                                                                               { "FilteredSourceMap", dialog.FilteredImages.Text },
                                                                               { "OpticalFlowForwardMap", dialog.OpticalFlowForwardImages.Text },
                                                                               { "OpticalFlowBackwardMap", dialog.OpticalFlowBackwardImages.Text }
                                                                           },
                                                         };

                Engine.Segmenter.LoadDataset( segmenterImageStackLoadDescription );
            }
        }

        private void LoadSegmentation( object sender, RoutedEventArgs e )
        {
            if ( !Engine.Segmenter.DatasetLoaded )
            {
                Console.WriteLine( "No dataset loaded." );
                return;
            }

            var dialog = new LoadSaveSegmentationDialog( "Load Segmentation", Settings.Default, "ColorImagesLoad", Settings.Default, "IdImagesLoad" );
            var result = dialog.ShowDialog();

            if ( result == true )
            {
                if ( !Directory.Exists( dialog.ColorImages.Text ) )
                {
                    Console.WriteLine( "ColorImages directory does not exist." );
                    return;
                }
                if ( !Directory.Exists( dialog.IdImages.Text ) )
                {
                    Console.WriteLine( "IdImages directory does not exist." );
                    return;
                }

                var segmenterImageStackLoadDescription = new SegmenterImageStackLoadDescription
                                                         {
                                                             Directories = new Dictionary< string >
                                                                           {
                                                                               { "ColorMap", dialog.ColorImages.Text },
                                                                               { "IdMap", dialog.IdImages.Text }
                                                                           },
                                                         };

                Engine.Segmenter.LoadSegmentation( segmenterImageStackLoadDescription );
            }
        }

        private void SaveSegmentationAs( object sender, RoutedEventArgs e )
        {
            if ( !Engine.Segmenter.DatasetLoaded )
            {
                Console.WriteLine( "No dataset loaded." );
                return;
            }

            var dialog = new LoadSaveSegmentationDialog( "Save Segmentation As", Settings.Default, "ColorImagesSaveAs", Settings.Default, "IdImagesSaveAs" );
            var result = dialog.ShowDialog();

            if ( result == true )
            {
                var segmenterImageStackSaveDescription = new SegmenterImageStackSaveDescription
                                                         {
                                                             Directories = new Dictionary< string >
                                                                           {
                                                                               { "ColorMap", dialog.ColorImages.Text },
                                                                               { "IdMap", dialog.IdImages.Text }
                                                                           },
                                                         };

                Engine.Segmenter.SaveSegmentationAs( segmenterImageStackSaveDescription );
            }
        }

        private void CreateNeuralProcessClick( object sender, RoutedEventArgs e )
        {
            Engine.Segmenter.AddNeuralProcess( CreateNeuralProcessName.Text );
        }

        private void RemoveNeuralProcessClick( object sender, RoutedEventArgs e )
        {
            var neuralProcess =
                ( (KeyValuePair< string, NeuralProcessDescription >)( DataContext as EngineDataContext ).SegmenterDataContext.CurrentNeuralProcess );

            Engine.Segmenter.RemoveNeuralProcess( neuralProcess.Key );
        }

        private void PrecomputeSegmentationClick( object sender, RoutedEventArgs e )
        {
            Engine.Segmenter.InitializeCostMap();
        }

        private void ComputeSegmentationClick( object sender, RoutedEventArgs e )
        {
            Engine.Segmenter.InitializeSegmentation3D();
        }

        private void CommitSegmentationClick( object sender, RoutedEventArgs e )
        {
            Engine.Segmenter.CommitSegmentation();
        }

        private void CancelSegmentationClick( object sender, RoutedEventArgs e )
        {
            Engine.Segmenter.CancelSegmentation();
        }

        private void UndoLastCommit( object sender, RoutedEventArgs e )
        {
            Engine.Segmenter.UndoLastCommit();
        }

        private void RedoLastCommit( object sender, RoutedEventArgs e )
        {
            Engine.Segmenter.RedoLastCommit();
        }

        private void OnNeuralProcessListBoxSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            NeuralProcessesListBox.ScrollIntoView( NeuralProcessesListBox.SelectedItem );
        }
    }
}
