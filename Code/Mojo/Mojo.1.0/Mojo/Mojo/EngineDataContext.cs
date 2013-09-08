using System;

namespace Mojo
{
    public class EngineDataContext : IDisposable
    {
        public Engine Engine { get; set; }
        public SegmenterDataContext SegmenterDataContext { get; set; }

        public void Dispose()
        {
            SegmenterDataContext.Dispose();
        }
    }
}