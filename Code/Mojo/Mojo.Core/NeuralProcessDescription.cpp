#include "NeuralProcessDescription.hpp"

namespace Mojo
{
namespace Core
{

NeuralProcessDescription::NeuralProcessDescription( int inId ) :
    id   ( inId ),
    color( make_int3( -1, -1, -1 ) )
{
}

}
}