using Mojo.Interop;

namespace Mojo
{
    public class SegmenterImageStackSaveDescription
    {        
        public Dictionary< string > Directories { get; set; }
        public Dictionary<VolumeDescription> VolumeDescriptions { get; set; }
    }
}
