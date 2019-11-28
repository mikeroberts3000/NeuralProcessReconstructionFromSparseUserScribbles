#pragma once

#include <set>
#include <hash_map>

#include "VolumeDescription.hpp"
#include "Dictionary.hpp"
#include "PrimitiveDictionary.hpp"
#include "DeviceVectorDictionary.hpp"
#include "D3D11CudaTextureDictionary.hpp"

namespace Mojo
{
namespace Core
{

class ID3D11CudaTexture;

class SegmenterState
{
public:
    SegmenterState( PrimitiveDictionary parameters );

    D3D11CudaTextureDictionary                          d3d11CudaTextures;
    Dictionary< cudaArray* >                            cudaArrays;
    DeviceVectorDictionary                              deviceVectors;
    PrimitiveDictionary                                 parameters;

    std::set< int >                                     slicesWithForegroundConstraints;
    std::set< int >                                     slicesWithBackgroundConstraints;
    stdext::hash_map< int, float >                      minCostsPerSlice;

    VolumeDescription                                   volumeDescription;

    float                                               convergenceGap;
    float                                               convergenceGapDelta;
    float                                               maxForegroundCostDelta;
};

}
}