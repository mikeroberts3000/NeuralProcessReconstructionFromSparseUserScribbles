#pragma once

#include "Mojo.Core/BreadcrumbDescription.hpp"

#using <SlimDX.dll>

using namespace SlimDX;

namespace Mojo
{
namespace Interop
{

#pragma managed
public ref class BreadcrumbDescription
{
public:
    BreadcrumbDescription();
    BreadcrumbDescription( Core::BreadcrumbDescription BreadcrumbDescription );

    Core::BreadcrumbDescription ToCore();

    property Vector3 Position;
};

}
}
