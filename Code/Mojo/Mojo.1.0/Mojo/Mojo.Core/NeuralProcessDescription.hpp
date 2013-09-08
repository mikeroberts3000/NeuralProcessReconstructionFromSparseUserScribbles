#pragma once

#include <string>
#include <list>

#include "BreadcrumbDescription.hpp"

namespace Mojo
{
namespace Core
{

class NeuralProcessDescription
{
public:
    NeuralProcessDescription( int id );

    int                                id;
    std::string                        name;
    int3                               color;
    std::list< BreadcrumbDescription > breadcrumbDescriptions;
};

}
}