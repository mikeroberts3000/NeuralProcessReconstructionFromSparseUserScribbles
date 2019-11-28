#include "SegmenterImageStackLoader.hpp"

#include <string>

#include <msclr/marshal_cppstd.h>

#include <opencv2/core/core.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/video/tracking.hpp>
#include <opencv2/highgui/highgui.hpp>

#include "Mojo.Core/Assert.hpp"
#include "Mojo.Core/Printf.hpp"

#include "Mojo.Cuda/Index.cuh"

namespace Mojo
{
namespace Interop
{
void SegmenterImageStackLoader::SaveIdImages( VolumeDescription^ inVolumeDescription, System::String^ path )
{
    Core::VolumeDescription volumeDescription = inVolumeDescription->ToCore();

    RELEASE_ASSERT( volumeDescription.numVoxels.z < 10000 );

    System::IO::Directory::CreateDirectory( path );

    for ( int z = 0; z < volumeDescription.numVoxels.z; z++ )
    {
        cv::Mat idMapSlice( volumeDescription.numVoxels.y, volumeDescription.numVoxels.x, CV_16UC1 );

        for ( int y = 0; y < volumeDescription.numVoxels.y; y++ )
        {
            for ( int x = 0; x < volumeDescription.numVoxels.x; x++ )
            {
                int index1D    = Index3DToIndex1D( make_int3( x, y, z ), volumeDescription.numVoxels );
                int idMapValue = ( (int*)volumeDescription.data )[ index1D ];

                idMapSlice.at< unsigned short >( y, x ) = (unsigned short)idMapValue;
            }
        }

        cv::imwrite(
            msclr::interop::marshal_as< std::string >( System::IO::Path::Combine( path, System::String::Format( "{0:0000}.png", z ) ) ).c_str(),
            idMapSlice );
    }
}

}
}