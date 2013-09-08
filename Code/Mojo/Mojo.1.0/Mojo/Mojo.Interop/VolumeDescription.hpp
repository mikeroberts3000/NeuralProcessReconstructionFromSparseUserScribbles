#pragma once

#include "Mojo.Core/VolumeDescription.hpp"

#using <SlimDX.dll>

using namespace System;
using namespace SlimDX;
using namespace SlimDX::DXGI;

namespace Mojo
{
namespace Interop
{

#pragma managed
public ref class VolumeDescription
{
public:
    VolumeDescription();
    VolumeDescription( Core::VolumeDescription volumeDescription );

    Core::VolumeDescription ToCore();

    property Format      DxgiFormat;
    property DataStream^ DataStream;
    property IntPtr      Data;
    property int         NumBytesPerVoxel;
    property int         NumVoxelsX;
    property int         NumVoxelsY;
    property int         NumVoxelsZ;
    property bool        IsSigned;
};

}
}
