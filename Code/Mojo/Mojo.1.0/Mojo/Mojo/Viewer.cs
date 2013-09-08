using System;

namespace Mojo
{
    public class Viewer : IDisposable
    {
        public RenderingPane RenderingPane { get; set; }
        public IUserInputHandler UserInputHandler { get; set; }

        public void Dispose()
        {
            RenderingPane.Dispose();
        }
    }
}
