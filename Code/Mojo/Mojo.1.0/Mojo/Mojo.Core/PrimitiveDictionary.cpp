#include "PrimitiveDictionary.hpp"

namespace Mojo
{
namespace Core
{

void PrimitiveDictionary::Set( std::string key, float x, float y, float z, float w )
{
    mFloat4[ key ] = make_float4( x, y, z, w );
}

void PrimitiveDictionary::Set( std::string key, float x, float y )
{
    mFloat2[ key ] = make_float2( x, y );
}

void PrimitiveDictionary::Set( std::string key, unsigned char x, unsigned char y, unsigned char z, unsigned char w )
{
    mUChar4[ key ] = make_uchar4( x, y, z, w );
}

void PrimitiveDictionary::Set( std::string key, float x )
{
    mFloat[ key ] = x;
}

void PrimitiveDictionary::Set( std::string key, int x )
{
    mInt[ key ] = x;
}

void PrimitiveDictionary::Set( std::string key, bool x )
{
    mBool[ key ] = x;
}

}
}