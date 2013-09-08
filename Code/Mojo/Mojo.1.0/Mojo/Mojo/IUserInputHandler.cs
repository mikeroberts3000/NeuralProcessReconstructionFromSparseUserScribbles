using System.Windows.Input;

namespace Mojo
{
    public interface IUserInputHandler
    {
        void OnKeyDown( KeyEventArgs keyEventArgs );
        void OnMouseUp( System.Windows.Forms.MouseEventArgs mouseEventArgs, int width, int height );
        void OnMouseDown( System.Windows.Forms.MouseEventArgs mouseEventArgs, int width, int height );
        void OnMouseMove( System.Windows.Forms.MouseEventArgs mouseEventArgs, int width, int height );
        void OnMouseWheel( System.Windows.Forms.MouseEventArgs mouseEventArgs );
    }
}
