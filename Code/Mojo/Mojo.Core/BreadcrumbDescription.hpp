#pragma once

#include <cuda_runtime.h>

namespace Mojo
{
namespace Core
{

class BreadcrumbDescription
{
public:
    BreadcrumbDescription();

    float3 position;
};

}
}