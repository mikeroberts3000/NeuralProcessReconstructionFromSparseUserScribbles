#include "Mojo.Core/Assert.hpp"

namespace Mojo
{
namespace Cuda
{

extern "C" void Dummy()
{
    RELEASE_ASSERT( 0 );
}

}
}