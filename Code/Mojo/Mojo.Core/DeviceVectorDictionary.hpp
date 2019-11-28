#pragma once

#include <hash_map>

#include "Thrust.hpp"

namespace Mojo
{
namespace Core
{

class DeviceVectorDictionary
{
public:
    template< typename TCudaType >
    thrust::device_vector< TCudaType >& Get( std::string key );

    void Set( std::string key, thrust::device_vector< float4 >& value );
    void Set( std::string key, thrust::device_vector< float2 >& value );
    void Set( std::string key, thrust::device_vector< uchar4 >& value );
    void Set( std::string key, thrust::device_vector< float >&  value );
    void Set( std::string key, thrust::device_vector< int >&    value );

    template< typename TCudaType >
    stdext::hash_map< std::string, thrust::device_vector< TCudaType > >& GetDictionary();

private:
    stdext::hash_map< std::string, thrust::device_vector< float4 > > mFloat4;
    stdext::hash_map< std::string, thrust::device_vector< float2 > > mFloat2;
    stdext::hash_map< std::string, thrust::device_vector< uchar4 > > mUChar4;
    stdext::hash_map< std::string, thrust::device_vector< float > >  mFloat;
    stdext::hash_map< std::string, thrust::device_vector< int > >    mInt;
};

template< typename TCudaType >
inline thrust::device_vector< TCudaType >& DeviceVectorDictionary::Get( std::string key )
{
    RELEASE_ASSERT( 0 );
    thrust::device_vector< TCudaType > dummy;
    return dummy;
}

template<>
inline thrust::device_vector< float4 >& DeviceVectorDictionary::Get( std::string key )
{
    RELEASE_ASSERT( mFloat4.find( key ) != mFloat4.end() );
    return mFloat4[ key ];
}

template<>
inline thrust::device_vector< float2 >& DeviceVectorDictionary::Get( std::string key )
{
    RELEASE_ASSERT( mFloat2.find( key ) != mFloat2.end() );
    return mFloat2[ key ];
}

template<>
inline thrust::device_vector< uchar4 >& DeviceVectorDictionary::Get( std::string key )
{
    RELEASE_ASSERT( mUChar4.find( key ) != mUChar4.end() );
    return mUChar4[ key ];
}

template<>
inline thrust::device_vector< float >& DeviceVectorDictionary::Get( std::string key )
{
    RELEASE_ASSERT( mFloat.find( key ) != mFloat.end() );
    return mFloat[ key ];
}

template<>
inline thrust::device_vector< int >& DeviceVectorDictionary::Get( std::string key )
{
    RELEASE_ASSERT( mInt.find( key ) != mInt.end() );
    return mInt[ key ];
}

template< typename TCudaType >
inline stdext::hash_map< std::string, thrust::device_vector< TCudaType > >& DeviceVectorDictionary::GetDictionary()
{
    RELEASE_ASSERT( 0 );
    stdext::hash_map< std::string, thrust::device_vector< TCudaType > > dummy;
    return dummy;
}

template<>
inline stdext::hash_map< std::string, thrust::device_vector< float4 > >& DeviceVectorDictionary::GetDictionary()
{
    return mFloat4;
}

template<>
inline stdext::hash_map< std::string, thrust::device_vector< float2 > >& DeviceVectorDictionary::GetDictionary()
{
    return mFloat2;
}

template<>
inline stdext::hash_map< std::string, thrust::device_vector< uchar4 > >& DeviceVectorDictionary::GetDictionary()
{
    return mUChar4;
}

template<>
inline stdext::hash_map< std::string, thrust::device_vector< float > >&  DeviceVectorDictionary::GetDictionary()
{
    return mFloat;
}

template<>
inline stdext::hash_map< std::string, thrust::device_vector< int > >&  DeviceVectorDictionary::GetDictionary()
{
    return mInt;
}

}
}