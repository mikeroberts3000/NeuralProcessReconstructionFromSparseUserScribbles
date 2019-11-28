using System.Windows.Forms;

namespace Mojo.Wpf
{
    class RenderingPaneHwndHost : UserControl
    {
        public IUserInputHandler UserInputHandler { get; set; }

        protected override void OnPaintBackground( PaintEventArgs e )
        {
        }

        protected override void OnMouseDown( MouseEventArgs e )
        {
            UserInputHandler.OnMouseDown( e, Width, Height );
        }

        protected override void OnMouseUp( MouseEventArgs e )
        {
            UserInputHandler.OnMouseUp( e, Width, Height );
        }

        protected override void OnMouseMove( MouseEventArgs e )
        {
            UserInputHandler.OnMouseMove( e, Width, Height );
        }

        protected override void OnMouseWheel( MouseEventArgs e )
        {
            UserInputHandler.OnMouseWheel( e );
        }
    }
}
