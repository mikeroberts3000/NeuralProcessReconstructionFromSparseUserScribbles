#include "PrimitiveDictionary.hpp"

#include <msclr/marshal_cppstd.h>

#include "Mojo.Core/ForEach.hpp"
#include "Mojo.Core/SegmenterState.hpp"
#include "Mojo.Core/ID3D11CudaTexture.hpp"

#using <SlimDX.dll>

using namespace msclr::interop;
using namespace System;
using namespace SlimDX;

namespace Mojo
{
namespace Interop
{

PrimitiveDictionary::PrimitiveDictionary()
{
    mFloat4 = gcnew System::Collections::Generic::Dictionary< String^, Vector4 >();
    mFloat2 = gcnew System::Collections::Generic::Dictionary< String^, Vector2 >();
    mUChar4 = gcnew System::Collections::Generic::Dictionary< String^, Vector4 >();
    mFloat  = gcnew System::Collections::Generic::Dictionary< String^, float >();
    mInt    = gcnew System::Collections::Generic::Dictionary< String^, int >();
    mBool   = gcnew System::Collections::Generic::Dictionary< String^, bool >();
}

PrimitiveDictionary::~PrimitiveDictionary()
{
    delete mFloat4;
    delete mFloat2;
    delete mUChar4;
    delete mFloat;
    delete mInt;
    delete mBool;
}

Collections::IEnumerator^ PrimitiveDictionary::GetEnumerator()
{
    RELEASE_ASSERT( 0 && "Iterating through instances of the PrimitiveDictionary class is not supported." );
    return nullptr;
}

void PrimitiveDictionary::Add( PrimitiveType type, String^ key, Vector4 value )
{
    switch( type )
    {
    case PrimitiveType::Float4:
        SetFloat4( key, value );
        break;
        
    case PrimitiveType::UChar4:
        SetUChar4( key, value );
        break;

    default:
        RELEASE_ASSERT( 0 );
    }
}

void PrimitiveDictionary::Add( PrimitiveType type, String^ key, Vector2 value )
{
    RELEASE_ASSERT( type == PrimitiveType::Float2 );
    SetFloat2( key, value );
}

void PrimitiveDictionary::Add( PrimitiveType type, String^ key, float value )
{
    RELEASE_ASSERT( type == PrimitiveType::Float );
    SetFloat( key, value );
}

void PrimitiveDictionary::Add( PrimitiveType type, String^ key, int value )
{
    RELEASE_ASSERT( type == PrimitiveType::Int );
    SetInt( key, value );
}

void PrimitiveDictionary::Add( PrimitiveType type, String^ key, bool value )
{
    RELEASE_ASSERT( type == PrimitiveType::Bool );
    SetBool( key, value );
}

Core::PrimitiveDictionary PrimitiveDictionary::ToCore()
{
    Core::PrimitiveDictionary primitiveDictionary;

    for each ( System::Collections::Generic::KeyValuePair< String^, Vector4 > keyValuePair in mFloat4 )
    {
        primitiveDictionary.Set( marshal_as< std::string >( keyValuePair.Key ), keyValuePair.Value.X, keyValuePair.Value.Y, keyValuePair.Value.Z, keyValuePair.Value.W );
    }

    for each ( System::Collections::Generic::KeyValuePair< String^, Vector2 > keyValuePair in mFloat2 )
    {
        primitiveDictionary.Set( marshal_as< std::string >( keyValuePair.Key ), keyValuePair.Value.X, keyValuePair.Value.Y );
    }

    for each ( System::Collections::Generic::KeyValuePair< String^, Vector4 > keyValuePair in mUChar4 )
    {
        primitiveDictionary.Set( marshal_as< std::string >( keyValuePair.Key ), (unsigned char)keyValuePair.Value.X, (unsigned char)keyValuePair.Value.Y, (unsigned char)keyValuePair.Value.Z, (unsigned char)keyValuePair.Value.W );
    }

    for each ( System::Collections::Generic::KeyValuePair< String^, float > keyValuePair in mFloat )
    {
        primitiveDictionary.Set( marshal_as< std::string >( keyValuePair.Key ), keyValuePair.Value );
    }

    for each ( System::Collections::Generic::KeyValuePair< String^, int > keyValuePair in mInt )
    {
        primitiveDictionary.Set( marshal_as< std::string >( keyValuePair.Key ), keyValuePair.Value );
    }

    for each ( System::Collections::Generic::KeyValuePair< String^, bool > keyValuePair in mBool )
    {
        primitiveDictionary.Set( marshal_as< std::string >( keyValuePair.Key ), keyValuePair.Value );
    }

    return primitiveDictionary;
}

Vector4 PrimitiveDictionary::GetFloat4( String^ key )
{
    RELEASE_ASSERT( mFloat4->ContainsKey( key ) );
    return mFloat4[ key ];
}

Vector2 PrimitiveDictionary::GetFloat2( String^ key )
{
    RELEASE_ASSERT( mFloat2->ContainsKey( key ) );
    return mFloat2[ key ];
}

Vector4 PrimitiveDictionary::GetUChar4( String^ key )
{
    RELEASE_ASSERT( mUChar4->ContainsKey( key ) );
    return mUChar4[ key ];
}

float PrimitiveDictionary::GetFloat( String^ key )
{
    RELEASE_ASSERT( mFloat->ContainsKey( key ) );
    return mFloat[ key ];
}

int PrimitiveDictionary::GetInt( String^ key )
{
    RELEASE_ASSERT( mInt->ContainsKey( key ) );
    return mInt[ key ];
}

bool PrimitiveDictionary::GetBool( String^ key )
{
    RELEASE_ASSERT( mBool->ContainsKey( key ) );
    return mBool[ key ];
}

void PrimitiveDictionary::SetFloat4( String^ key, Vector4 value )
{
    mFloat4[ key ] = value;
}

void PrimitiveDictionary::SetFloat2( String^ key, Vector2 value )
{
    mFloat2[ key ] = value;
}

void PrimitiveDictionary::SetUChar4( String^ key, Vector4 value )
{
    mUChar4[ key ] = value;
}

void PrimitiveDictionary::SetFloat( String^ key, float value )
{
    mFloat[ key ] = value;
}

void PrimitiveDictionary::SetInt( String^ key, int value )
{
    mInt[ key ] = value;
}

void PrimitiveDictionary::SetBool( String^ key, bool value )
{
    mBool[ key ] = value;
}

System::Collections::Generic::IDictionary< String^, Vector4 >^ PrimitiveDictionary::GetDictionaryFloat4()
{
    return mFloat4;
}

System::Collections::Generic::IDictionary< String^, Vector2 >^ PrimitiveDictionary::GetDictionaryFloat2()
{
    return mFloat2;
}

System::Collections::Generic::IDictionary< String^, Vector4 >^ PrimitiveDictionary::GetDictionaryUChar4()
{
    return mUChar4;
}

System::Collections::Generic::IDictionary< String^, float >^ PrimitiveDictionary::GetDictionaryFloat()
{
    return mFloat;
}

System::Collections::Generic::IDictionary< String^, int >^ PrimitiveDictionary::GetDictionaryInt()
{
    return mInt;
}

System::Collections::Generic::IDictionary< String^, bool >^ PrimitiveDictionary::GetDictionaryBool()
{
    return mBool;
}

}
}