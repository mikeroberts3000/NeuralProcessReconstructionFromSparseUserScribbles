#pragma once

#include <string>
#include <hash_map>

#include "Cuda.hpp"

namespace Mojo
{
namespace Core
{

class PrimitiveDictionary
{
public:
    template< typename TCudaType >
    TCudaType Get( std::string key );

    void Set( std::string key, float x, float y, float z, float w );
    void Set( std::string key, float x, float y );
    void Set( std::string key, unsigned char x, unsigned char y, unsigned char z, unsigned char w );
    void Set( std::string key, float x );
    void Set( std::string key, int x );
    void Set( std::string key, bool x );

    template< typename TCudaType >
    stdext::hash_map< std::string, TCudaType >& GetDictionary();

private:
    stdext::hash_map< std::string, float4 > mFloat4;
    stdext::hash_map< std::string, float2 > mFloat2;
    stdext::hash_map< std::string, uchar4 > mUChar4;
    stdext::hash_map< std::string, float >  mFloat;
    stdext::hash_map< std::string, int >    mInt;
    stdext::hash_map< std::string, bool >   mBool;
};

template< typename TCudaType >
inline TCudaType PrimitiveDictionary::Get( std::string key )
{
    RELEASE_ASSERT( 0 );
    TCudaType dummy;
    return dummy;
};

template<>
inline float4 PrimitiveDictionary::Get( std::string key )
{
    RELEASE_ASSERT( mFloat4.find( key ) != mFloat4.end() );
    return mFloat4[ key ];
};

template<>
inline float2 PrimitiveDictionary::Get( std::string key )
{
    RELEASE_ASSERT( mFloat2.find( key ) != mFloat2.end() );
    return mFloat2[ key ];
};

template<>
inline uchar4 PrimitiveDictionary::Get( std::string key )
{
    RELEASE_ASSERT( mUChar4.find( key ) != mUChar4.end() );
    return mUChar4[ key ];
};

template<>
inline float PrimitiveDictionary::Get( std::string key )
{
    RELEASE_ASSERT( mFloat.find( key ) != mFloat.end() );
    return mFloat[ key ];
};

template<>
inline int PrimitiveDictionary::Get( std::string key )
{
    RELEASE_ASSERT( mInt.find( key ) != mInt.end() );
    return mInt[ key ];
};

template<>
inline bool  PrimitiveDictionary::Get( std::string key )
{
    RELEASE_ASSERT( mBool.find( key ) != mBool.end() );
    return mBool[ key ];
};

template< typename TCudaType >
inline stdext::hash_map< std::string, TCudaType >& PrimitiveDictionary::GetDictionary()
{
    RELEASE_ASSERT( 0 );
    stdext::hash_map< std::string, TCudaType > dummy;
    return dummy;
};

template<>
inline stdext::hash_map< std::string, float4 >& PrimitiveDictionary::GetDictionary()
{
    return mFloat4;
};

template<>
inline stdext::hash_map< std::string, float2 >& PrimitiveDictionary::GetDictionary()
{
    return mFloat2;
};

template<>
inline stdext::hash_map< std::string, uchar4 >& PrimitiveDictionary::GetDictionary()
{
    return mUChar4;
};

template<>
inline stdext::hash_map< std::string, float >&  PrimitiveDictionary::GetDictionary()
{
    return mFloat;
};

template<>
inline stdext::hash_map< std::string, int >&  PrimitiveDictionary::GetDictionary()
{
    return mInt;
};

template<>
inline stdext::hash_map< std::string, bool >&   PrimitiveDictionary::GetDictionary()
{
    return mBool;
};

}
}