#pragma once

#include "Dictionary.hpp"
#include "NeuralProcessDescription.hpp"
#include "VolumeDescription.hpp"

namespace Mojo
{
namespace Core
{

class DatasetDescription
{
public:
    Dictionary< VolumeDescription >                   volumeDescriptions;
    stdext::hash_map< int, NeuralProcessDescription > neuralProcessDescriptions;
};

}
}