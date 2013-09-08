#pragma once

#include <list>

#include "Cuda.hpp"
#include "DatasetDescription.hpp"
#include "Dictionary.hpp"

namespace Mojo
{
namespace Core
{

class BreadcrumberState
{
public:
    DatasetDescription                                     datasetDescription;
    Dictionary< std::list< std::pair< float3, float3 > > > delaunyEdges;
};

}
}