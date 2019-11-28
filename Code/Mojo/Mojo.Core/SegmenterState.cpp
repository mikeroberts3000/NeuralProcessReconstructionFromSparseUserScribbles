#include "SegmenterState.hpp"

#include "ForEach.hpp"
#include "ID3D11CudaTexture.hpp"

namespace Mojo
{
namespace Core
{

SegmenterState::SegmenterState( PrimitiveDictionary inParameters ) :
    parameters                     ( inParameters ),
    convergenceGap                 ( 0 ),
    convergenceGapDelta            ( 0 ),
    maxForegroundCostDelta         ( inParameters.Get< float >( "COST_MAP_INITIAL_MAX_FOREGROUND_COST_DELTA" ) )
{
}

}
}