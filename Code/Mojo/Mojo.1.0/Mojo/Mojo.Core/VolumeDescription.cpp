#include "VolumeDescription.hpp"

#include <cstdlib>

namespace Mojo
{
namespace Core
{

VolumeDescription::VolumeDescription() :
    dxgiFormat      ( DXGI_FORMAT_UNKNOWN ),
    data            ( NULL ),
    numBytesPerVoxel( -1 ),
    numVoxels       ( make_int3( -1, -1, -1 ) ),
    isSigned        ( false )
{
}

}
}