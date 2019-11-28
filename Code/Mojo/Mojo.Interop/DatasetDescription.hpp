#pragma once

#include "Mojo.Core/DatasetDescription.hpp"

#include "NotifyPropertyChanged.hpp"
#include "Dictionary.hpp"
#include "VolumeDescription.hpp"
#include "NeuralProcessDescription.hpp"
#include "Edge.hpp"

#using <SlimDX.dll>
#using <ObservableDictionary.dll>

using namespace System;
using namespace System::Collections::Generic;
using namespace SlimDX;

namespace Mojo
{
namespace Interop
{

#pragma managed
public ref class DatasetDescription : public NotifyPropertyChanged
{
public:
    DatasetDescription();
    DatasetDescription( Core::DatasetDescription datasetDescription );
    ~DatasetDescription();

    Core::DatasetDescription ToCore();

    property Dictionary< VolumeDescription^ >^ VolumeDescriptions
    {
        Dictionary< VolumeDescription^ >^ get()
        {
            return mVolumeDescriptions;
        }
        
        void set( Dictionary< VolumeDescription^ >^ value )
        {
            mVolumeDescriptions = value;
            OnPropertyChanged( "VolumeDescriptions" );
        }
    }

    property ObservableDictionary< int, NeuralProcessDescription^ >^ NeuralProcessDescriptions
    {
        ObservableDictionary< int, NeuralProcessDescription^ >^ get()
        {
            return mNeuralProcessDescriptions;
        }
        
        void set( ObservableDictionary< int, NeuralProcessDescription^ >^ value )
        {
            mNeuralProcessDescriptions = value;
            OnPropertyChanged( "NeuralProcessDescriptions" );
        }
    }

private:
    Dictionary< VolumeDescription^ >^                       mVolumeDescriptions;
    ObservableDictionary< int, NeuralProcessDescription^ >^ mNeuralProcessDescriptions;

};

}
}
