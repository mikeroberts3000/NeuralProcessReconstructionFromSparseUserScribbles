using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Mojo.Interop;

namespace Mojo.Wpf
{
    public partial class Application : System.Windows.Application
    {
        private MainWindow mMainWindow;
        private Engine mEngine;
        private DispatcherTimer mUpdateTimer;
        private DispatcherTimer mAutoSaveTimer;

        protected override void OnStartup( StartupEventArgs e )
        {
            base.OnStartup( e );

            mMainWindow = new MainWindow();

            var windowDescriptions = new Dictionary< RenderingPaneHwndDescription >
                                     {
                                         {
                                             "Segmenter2D",
                                             new RenderingPaneHwndDescription
                                             {
                                                 Handle = mMainWindow.Segmenter2DViewerWpfContext.RenderingPaneHwndHost.Handle,
                                                 Width = mMainWindow.Segmenter2DViewerWpfContext.RenderingPaneHwndHost.Width,
                                                 Height = mMainWindow.Segmenter2DViewerWpfContext.RenderingPaneHwndHost.Height
                                             }
                                             },
                                     };

            mEngine = new Engine( windowDescriptions );

            mMainWindow.Engine = mEngine;
            mMainWindow.DataContext = new EngineDataContext
                                      {
                                          Engine = mEngine,
                                          SegmenterDataContext = new SegmenterDataContext( mEngine.Segmenter )
                                      };
            mMainWindow.Show();

            mUpdateTimer = new DispatcherTimer( DispatcherPriority.Input ) { Interval = TimeSpan.FromMilliseconds( 0 ) };
            mUpdateTimer.Tick += Update;
            mUpdateTimer.Start();

            mAutoSaveTimer = new DispatcherTimer( DispatcherPriority.Input ) { Interval = TimeSpan.FromSeconds( Settings.Default.AutoSaveSegmentationFrequencySeconds ) };
            mAutoSaveTimer.Tick += AutoSave;
            mAutoSaveTimer.Start();

            if ( Settings.Default.AutoSaveSegmentation )
            {
                Console.WriteLine( "Auto-saving turned on. Auto-saving every {0} seconds...", Settings.Default.AutoSaveSegmentationFrequencySeconds );
            }
        }

        protected override void OnExit( ExitEventArgs e )
        {
            mAutoSaveTimer.Stop();
            mAutoSaveTimer.Tick -= AutoSave;

            mUpdateTimer.Stop();
            mUpdateTimer.Tick -= Update;

            Settings.Default.Save();

            mEngine.Dispose();
            mMainWindow.Dispose();
            base.OnExit( e );
        }

        public void Update( object sender, EventArgs eventArgs )
        {
            mEngine.Update();
        }

        public void AutoSave( object sender, EventArgs eventArgs )
        {
            if ( Settings.Default.AutoSaveSegmentation && mEngine.Segmenter.DatasetLoaded )
            {
                var dateTimeString = String.Format("{0:s}", DateTime.Now ).Replace( ':', '-' );

                Console.WriteLine( "Auto-saving segmentation: " + dateTimeString );

                var segmenterImageStackSaveDescription = new SegmenterImageStackSaveDescription
                                                         {
                                                             Directories = new Dictionary<string>
                                                                           {
                                                                               { "ColorMap", Directory.GetCurrentDirectory() + @"\" + Settings.Default.AutoSaveSegmentationPath + @"\" + dateTimeString + @"\Colors" },
                                                                               { "IdMap", Directory.GetCurrentDirectory() + @"\" + Settings.Default.AutoSaveSegmentationPath + @"\" + dateTimeString + @"\Ids" }
                                                                           },
                                                         };

                mEngine.Segmenter.SaveSegmentationAs( segmenterImageStackSaveDescription );
            }
        }    
    }
}
