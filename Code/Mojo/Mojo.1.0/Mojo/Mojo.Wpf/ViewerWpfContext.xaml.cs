using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mojo.Wpf
{
    /// <summary>
    ///   Interaction logic for ViewerWpfContext.xaml
    /// </summary>
    public partial class ViewerWpfContext : UserControl, IUserInputHandler
    {



        // Viewer Dependency Property
        public Viewer Viewer
        {
            get
            {
                return GetValue( ViewerProperty ) as Viewer;
            }
            set
            {
                SetValue( ViewerProperty, value );
            }
        }

// ReSharper disable InconsistentNaming
        public static readonly DependencyProperty ViewerProperty = DependencyProperty.Register(
            "Viewer",
            typeof ( Viewer ),
            typeof ( ViewerWpfContext ),
            new FrameworkPropertyMetadata() );
// ReSharper restore InconsistentNaming



        public ViewerWpfContext()
        {
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
            Unloaded += OnUnloaded;

            InitializeComponent();
        }

        private void OnLoaded( object sender, RoutedEventArgs e )
        {
            RenderingPaneHwndHost.UserInputHandler = this;

            SetSize( new Size( ActualWidth, ActualHeight ) );
            AquireKeyboardFocusAndLogicalFocus();
        }

        public void OnUnloaded( object sender, RoutedEventArgs e )
        {
            RenderingPaneHwndHost.UserInputHandler = null;

            Loaded -= OnLoaded;
            SizeChanged -= OnSizeChanged;
            Unloaded -= OnUnloaded;
        }

        public void AquireKeyboardFocusAndLogicalFocus()
        {
            WindowsFormsHost.TabInto( new TraversalRequest( FocusNavigationDirection.First ) );
            Keyboard.Focus( this );
        }

        public void OnMouseUp( System.Windows.Forms.MouseEventArgs e, int width, int height )
        {
            if ( Viewer != null )
            {
                Viewer.UserInputHandler.OnMouseUp( e, width, height );
                AquireKeyboardFocusAndLogicalFocus();
            }
        }

        public void OnMouseDown( System.Windows.Forms.MouseEventArgs e, int width, int height )
        {
            if ( Viewer != null )
            {
                Viewer.UserInputHandler.OnMouseDown( e, width, height );
                AquireKeyboardFocusAndLogicalFocus();
            }
        }

        public void OnMouseMove( System.Windows.Forms.MouseEventArgs e, int width, int height )
        {
            if ( Viewer != null )
            {
                Viewer.UserInputHandler.OnMouseMove( e, width, height );
                AquireKeyboardFocusAndLogicalFocus();
            }
        }

        public void OnMouseWheel( System.Windows.Forms.MouseEventArgs e )
        {
            if ( Viewer != null )
            {
                Viewer.UserInputHandler.OnMouseWheel( e );
                AquireKeyboardFocusAndLogicalFocus();
            }
        }

        void IUserInputHandler.OnKeyDown( KeyEventArgs e )
        {
            Release.Assert( false );
        }

        protected override void OnKeyDown( KeyEventArgs e )
        {
            if ( Viewer != null )
            {
                Viewer.UserInputHandler.OnKeyDown( e );
                AquireKeyboardFocusAndLogicalFocus();
                e.Handled = true;
            }
        }

        private void OnSizeChanged( object sender, SizeChangedEventArgs e )
        {
            SetSize( e.NewSize );
            AquireKeyboardFocusAndLogicalFocus();
        }

        private void SetSize( Size size )
        {
            if ( WindowsFormsHost != null )
            {
                WindowsFormsHost.Width = size.Width;
                WindowsFormsHost.Height = size.Height;

                if ( RenderingPaneHwndHost != null )
                {
                    RenderingPaneHwndHost.Width = (int)size.Width;
                    RenderingPaneHwndHost.Height = (int)size.Height;
                }

                if ( Viewer != null )
                {
                    Viewer.RenderingPane.SetSize( new Size( size.Width, size.Height ) );
                }
            }
        }
    }
}
