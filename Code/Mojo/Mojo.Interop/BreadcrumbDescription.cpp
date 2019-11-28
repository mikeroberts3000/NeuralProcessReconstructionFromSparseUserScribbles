#include "BreadcrumbDescription.hpp"

namespace Mojo
{
namespace Interop
{

BreadcrumbDescription::BreadcrumbDescription()
{
}

BreadcrumbDescription::BreadcrumbDescription( Core::BreadcrumbDescription breadcrumbDescription )
{
    Position = Vector3( (float)breadcrumbDescription.position.x, (float)breadcrumbDescription.position.y, (float)breadcrumbDescription.position.z );
}

Core::BreadcrumbDescription BreadcrumbDescription::ToCore()
{
    Core::BreadcrumbDescription breadcrumbDescription;

    breadcrumbDescription.position = make_float3( Position.X, Position.Y, Position.Z );

    return breadcrumbDescription;
}

}
}