#pragma once

#include <hash_map>

#include "Cuda.hpp"

namespace Mojo
{
namespace Core
{

template< typename T >
class Dictionary
{
public:
    T                                   Get( std::string key );
    void                                Set( std::string key, T value );

    stdext::hash_map< std::string, T >& GetDictionary();

private:
    stdext::hash_map< std::string, T > mDictionary;
};

template< typename T >
inline T Dictionary< T >::Get( std::string key )
{
    RELEASE_ASSERT( mDictionary.find( key ) != mDictionary.end() );
    return mDictionary[ key ];
};

template< typename T >
inline void Dictionary< T >::Set( std::string key, T value )
{
    mDictionary[ key ] = value;
};

template< typename T >
inline stdext::hash_map< std::string, T >& Dictionary< T >::GetDictionary()
{
    return mDictionary;
};

}
}