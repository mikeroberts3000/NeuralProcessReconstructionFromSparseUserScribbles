#pragma once

#include <cuda_runtime.h>

#include "D3D11.hpp"

namespace Mojo
{
namespace Core
{

class VolumeDescription
{
public:
    VolumeDescription();

    DXGI_FORMAT dxgiFormat;
    void*       data;
    int         numBytesPerVoxel;
    int3        numVoxels;
    bool        isSigned;
};

}
}