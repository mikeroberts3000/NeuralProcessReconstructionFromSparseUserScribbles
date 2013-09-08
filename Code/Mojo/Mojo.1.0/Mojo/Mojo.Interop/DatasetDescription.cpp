#include "DatasetDescription.hpp"

namespace Mojo
{
namespace Interop
{

DatasetDescription::DatasetDescription()
{
    VolumeDescriptions        = nullptr;
    NeuralProcessDescriptions = nullptr;
}

DatasetDescription::DatasetDescription( Core::DatasetDescription datasetDescription )
{
    VolumeDescriptions        = gcnew Dictionary< VolumeDescription^ >();
    NeuralProcessDescriptions = gcnew ObservableDictionary< int, NeuralProcessDescription^ >();

    MOJO_INTEROP_DICTIONARY_FROM_CORE( datasetDescription.volumeDescriptions, Core::VolumeDescription, VolumeDescriptions, VolumeDescription );

    MOJO_FOR_EACH_KEY_VALUE( int key, Core::NeuralProcessDescription value, datasetDescription.neuralProcessDescriptions )
    {
        NeuralProcessDescriptions->Add( key, gcnew NeuralProcessDescription( value ) );
    }
}

DatasetDescription::~DatasetDescription()
{
    delete VolumeDescriptions;
    delete NeuralProcessDescriptions;
}

Core::DatasetDescription DatasetDescription::ToCore()
{
    Core::DatasetDescription datasetDescription;

    MOJO_INTEROP_DICTIONARY_TO_CORE( VolumeDescriptions, VolumeDescription^, datasetDescription.volumeDescriptions );

    for each ( System::Collections::Generic::KeyValuePair< int, NeuralProcessDescription^ > keyValuePair in NeuralProcessDescriptions )
    {
        datasetDescription.neuralProcessDescriptions.insert( std::pair< int, Core::NeuralProcessDescription >( keyValuePair.Key, keyValuePair.Value->ToCore() ) );
    }

    return datasetDescription;
}

}
}