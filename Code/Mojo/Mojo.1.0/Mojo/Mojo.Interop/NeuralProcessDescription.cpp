#include <msclr/marshal_cppstd.h>

#include "NeuralProcessDescription.hpp"

#include "Mojo.Core/ForEach.hpp"

#include "BreadcrumbDescription.hpp"

using namespace msclr::interop;

#define MOJO_CLI_LIST_TO_STD( interopList, interopListType, stdList ) \
    for each( interopListType^ value in interopList )                 \
    {                                                                 \
        stdList.push_back( value->ToCore() );                         \
    }                                                                 \

#define MOJO_CLI_LIST_FROM_STD( stdList, stdListType, interopList, interopListType ) \
    MOJO_FOR_EACH( stdListType value, stdList )                                      \
    {                                                                                \
        interopList->Add( gcnew interopListType( value ) );                          \
    }                                                                                \

namespace Mojo
{
namespace Interop
{

NeuralProcessDescription::NeuralProcessDescription( int id )
{
    Id                     = id;
    Name                   = nullptr;
    BreadcrumbDescriptions = nullptr;
    Branches               = nullptr;
}

NeuralProcessDescription::NeuralProcessDescription( Core::NeuralProcessDescription neuralProcessDescription )
{
    Id                     = neuralProcessDescription.id;
    Name                   = marshal_as< String^ >( neuralProcessDescription.name );
    Color                  = Vector3( (float)neuralProcessDescription.color.x, (float)neuralProcessDescription.color.y, (float)neuralProcessDescription.color.z );
    BreadcrumbDescriptions = gcnew System::Collections::Generic::List< BreadcrumbDescription^ >();

    MOJO_CLI_LIST_FROM_STD( neuralProcessDescription.breadcrumbDescriptions, Core::BreadcrumbDescription, BreadcrumbDescriptions, BreadcrumbDescription );
}

NeuralProcessDescription::~NeuralProcessDescription()
{
    delete Name;
    delete BreadcrumbDescriptions;
}

Core::NeuralProcessDescription NeuralProcessDescription::ToCore()
{
    Core::NeuralProcessDescription neuralProcessDescription( Id );

    neuralProcessDescription.name  = marshal_as< std::string >( Name );
    neuralProcessDescription.color = make_int3( (int)Color.X, (int)Color.Y, (int)Color.Z );

    MOJO_CLI_LIST_TO_STD( BreadcrumbDescriptions, BreadcrumbDescription, neuralProcessDescription.breadcrumbDescriptions )

    return neuralProcessDescription;
}

}
}