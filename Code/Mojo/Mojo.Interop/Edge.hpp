#pragma once

#using <SlimDX.dll>

using namespace SlimDX;

namespace Mojo
{
namespace Interop
{

#pragma managed
public ref class Edge
{
public:
    property Vector3 P1;
    property Vector3 P2;
};

}
}
