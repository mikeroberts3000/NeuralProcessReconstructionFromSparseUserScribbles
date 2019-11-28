using System.Collections.Generic;
using Mojo.Interop;

namespace Mojo
{
    public class ShortestPathDescription
    {
        public Edge Branch { get; set; }
        public IList< Edge > ShortestPath { get; set; }
        public IList<Edge> SmoothPath { get; set; }
    }
}
