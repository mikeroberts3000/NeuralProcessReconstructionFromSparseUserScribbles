#include "SegmenterState.hpp"

#include "ForEach.hpp"
#include "ID3D11CudaTexture.hpp"

namespace Mojo
{
namespace Core
{

void DeviceVectorDictionary::Set( std::string key, thrust::device_vector< float4 >& value )
{
    mFloat4[ key ] = value;
}

void DeviceVectorDictionary::Set( std::string key, thrust::device_vector< float2 >& value )
{
    mFloat2[ key ] = value;
}

void DeviceVectorDictionary::Set( std::string key, thrust::device_vector< uchar4 >& value )
{
    mUChar4[ key ] = value;
}

void DeviceVectorDictionary::Set( std::string key, thrust::device_vector< float >& value )
{
    mFloat[ key ] = value;
}

void DeviceVectorDictionary::Set( std::string key, thrust::device_vector< int >& value )
{
    mInt[ key ] = value;
}

}
}