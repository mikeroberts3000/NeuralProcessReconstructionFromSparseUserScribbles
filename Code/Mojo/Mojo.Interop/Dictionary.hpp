#pragma once

#include <string>

#include <msclr/marshal_cppstd.h>

#include "Mojo.Core/Assert.hpp"
#include "Mojo.Core/ForEach.hpp"
#include "Mojo.Core/Dictionary.hpp"

#include "NotifyPropertyChanged.hpp"

#using <ObservableDictionary.dll>

using namespace System;
using namespace System::Collections;
using namespace DrWPF::Windows::Data;

#define MOJO_INTEROP_DICTIONARY_TO_CORE( interopDictionary, interopDictionaryType, coreDictionary )                                       \
    for each ( System::Collections::Generic::KeyValuePair< String^, interopDictionaryType > keyValuePair in interopDictionary->Internal ) \
    {                                                                                                                                     \
        coreDictionary.Set( msclr::interop::marshal_as< std::string >( keyValuePair.Key ), keyValuePair.Value->ToCore() );                \
    }                                                                                                                                     \

#define MOJO_INTEROP_DICTIONARY_FROM_CORE( coreDictionary, coreDictionaryType, interopDictionary, interopDictionaryType ) \
    MOJO_FOR_EACH_KEY_VALUE( std::string key, coreDictionaryType value, coreDictionary.GetDictionary() )                  \
    {                                                                                                                     \
        System::String^ string = msclr::interop::marshal_as< System::String^ >( key );                                    \
        interopDictionary->Set( string, gcnew interopDictionaryType( value ) );                                           \
    }                                                                                                                     \

namespace Mojo
{
namespace Interop
{

void DictionaryError();

#pragma managed
generic < typename T >
public ref class Dictionary : public NotifyPropertyChanged, Collections::IEnumerable
{
public:
    Dictionary();
    ~Dictionary();

    void                              Add( String^ key, T value );
    virtual Collections::IEnumerator^ GetEnumerator();

    T    Get( String^ key );
    void Set( String^ key, T value );

    property ObservableDictionary< String^, T >^ Internal
    {
        ObservableDictionary< String^, T >^ get()
        {
            return mInternal;
        }
        
        void set( ObservableDictionary< String^, T >^ value )
        {
            mInternal = value;
            OnPropertyChanged( "Internal" );
        }
    }

private:
    ObservableDictionary< String^, T >^ mInternal;

};

generic < typename T >
inline Dictionary< T >::Dictionary()
{
    Internal = gcnew ObservableDictionary< String^, T >();
}

generic < typename T >
inline Dictionary< T >::~Dictionary()
{
    delete Internal;
}

generic < typename T >
inline void Dictionary< T >::Add( String^ key, T value )
{
    Set( key, value );
}

generic < typename T >
inline Collections::IEnumerator^ Dictionary< T >::GetEnumerator()
{
    return Internal->GetEnumerator();
}

generic< typename T >
inline T Dictionary< T >::Get(String^ key )
{
    if ( !Internal->ContainsKey( key ) )
    {
        DictionaryError();                                                                                                                                                      
    }

    return Internal[ key ];
}

generic< typename T >
inline void Dictionary< T >::Set(String^ key, T value)
{
    Internal[ key ] = value;
}

}
}
