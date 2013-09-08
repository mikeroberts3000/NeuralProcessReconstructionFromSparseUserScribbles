#include "VolumeDescription.hpp"

namespace Mojo
{
namespace Interop
{

VolumeDescription::VolumeDescription()
{
    DxgiFormat       = SlimDX::DXGI::Format::Unknown;
    DataStream       = nullptr;
    Data             = System::IntPtr( nullptr );
    NumBytesPerVoxel = -1;
    NumVoxelsX       = -1;
    NumVoxelsY       = -1;
    NumVoxelsZ       = -1;
    IsSigned         = false;
}

VolumeDescription::VolumeDescription( Core::VolumeDescription volumeDescription )
{
    DxgiFormat       = (SlimDX::DXGI::Format)volumeDescription.dxgiFormat;
    DataStream       = nullptr;
    Data             = System::IntPtr( volumeDescription.data );
    NumBytesPerVoxel = volumeDescription.numBytesPerVoxel;
    NumVoxelsX       = volumeDescription.numVoxels.x;
    NumVoxelsY       = volumeDescription.numVoxels.y;
    NumVoxelsZ       = volumeDescription.numVoxels.z;
    IsSigned         = volumeDescription.isSigned;
}

Core::VolumeDescription VolumeDescription::ToCore()
{
    Core::VolumeDescription volumeDescription;

    volumeDescription.dxgiFormat       = (DXGI_FORMAT)this->DxgiFormat;
    volumeDescription.data             = this->Data.ToPointer();
    volumeDescription.numBytesPerVoxel = this->NumBytesPerVoxel;
    volumeDescription.numVoxels.x      = this->NumVoxelsX;
    volumeDescription.numVoxels.y      = this->NumVoxelsY;
    volumeDescription.numVoxels.z      = this->NumVoxelsZ;
    volumeDescription.isSigned         = this->IsSigned;

    return volumeDescription;
}

}
}