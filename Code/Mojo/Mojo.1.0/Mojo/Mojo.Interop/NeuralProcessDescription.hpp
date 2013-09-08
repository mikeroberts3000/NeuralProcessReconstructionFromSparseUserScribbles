#pragma once

#include "Mojo.Core/NeuralProcessDescription.hpp"

#include "BreadcrumbDescription.hpp"
#include "Edge.hpp"

#using <SlimDX.dll>

using namespace System;
using namespace System::Collections::Generic;
using namespace SlimDX;

namespace Mojo
{
namespace Interop
{

#pragma managed
public ref class NeuralProcessDescription
{
public:
    NeuralProcessDescription( int id );
    NeuralProcessDescription( Core::NeuralProcessDescription NeuralProcessDescription );
    ~NeuralProcessDescription();

    Core::NeuralProcessDescription ToCore();

    property int                                                            Id;
    property String^                                                        Name;
    property Vector3                                                        Color;
    property System::Collections::Generic::IList< BreadcrumbDescription^ >^ BreadcrumbDescriptions;
    property System::Collections::Generic::IList< Edge^ >^                  Branches;
};

}
}
