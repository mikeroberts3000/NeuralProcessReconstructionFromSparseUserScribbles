#include "Mojo.Core/D3D11.hpp"
#include "Mojo.Core/Cuda.hpp"
#include "Mojo.Core/Thrust.hpp"
#include "Mojo.Core/SegmenterState.hpp"
#include "Mojo.Core/ID3D11CudaTexture.hpp"
#include "Mojo.Core/ForEach.hpp"

#include "Index.cuh"

extern "C" void InitializeCommittedSegmentation( Mojo::Core::SegmenterState* segmenterState )
{
    MOJO_THRUST_SAFE(
        Mojo::Core::Thrust::Fill(
            segmenterState->deviceVectors.Get< uchar4 >( "ColorMap" ).begin(),
            segmenterState->deviceVectors.Get< uchar4 >( "ColorMap" ).end(),
            segmenterState->parameters.Get< uchar4 >( "COLOR_MAP_INITIAL_VALUE" ),
            segmenterState->deviceVectors.Get< float >( "ScratchpadMap" ) ) );

    MOJO_THRUST_SAFE(
        Mojo::Core::Thrust::Fill(
            segmenterState->deviceVectors.Get< int >( "IdMap" ).begin(),
            segmenterState->deviceVectors.Get< int >( "IdMap" ).end(),
            segmenterState->parameters.Get< int >( "ID_MAP_INITIAL_VALUE" ),
            segmenterState->deviceVectors.Get< float >( "ScratchpadMap" ) ) );
}

extern "C" void InitializeSegmentation( Mojo::Core::SegmenterState* segmenterState )
{   
    MOJO_THRUST_SAFE(
        Mojo::Core::Thrust::Fill(
            segmenterState->deviceVectors.Get< float >( "PrimalMap" ).begin(),
            segmenterState->deviceVectors.Get< float >( "PrimalMap" ).end(),
            segmenterState->parameters.Get< float >( "PRIMAL_MAP_INITIAL_VALUE" ),
            segmenterState->deviceVectors.Get< float >( "ScratchpadMap" ) ) );

    MOJO_THRUST_SAFE(
        Mojo::Core::Thrust::Fill(
            segmenterState->deviceVectors.Get< float >( "OldPrimalMap" ).begin(),
            segmenterState->deviceVectors.Get< float >( "OldPrimalMap" ).end(),
            segmenterState->parameters.Get< float >( "PRIMAL_MAP_INITIAL_VALUE" ),
            segmenterState->deviceVectors.Get< float >( "ScratchpadMap" ) ) );

    MOJO_THRUST_SAFE(
        Mojo::Core::Thrust::Fill(
            segmenterState->deviceVectors.Get< float4 >( "DualMap" ).begin(),
            segmenterState->deviceVectors.Get< float4 >( "DualMap" ).end(),
            segmenterState->parameters.Get< float4 >( "DUAL_MAP_INITIAL_VALUE" ),
            segmenterState->deviceVectors.Get< float4 >( "ScratchpadMap" ) ) );
}

extern "C" void InitializeConstraintMap( Mojo::Core::SegmenterState* segmenterState )
{
    MOJO_THRUST_SAFE(
        Mojo::Core::Thrust::Fill(
            segmenterState->deviceVectors.Get< float >( "ConstraintMap" ).begin(),
            segmenterState->deviceVectors.Get< float >( "ConstraintMap" ).end(),
            segmenterState->parameters.Get< float >( "CONSTRAINT_MAP_INITIAL_VALUE" ),
            segmenterState->deviceVectors.Get< float >( "ScratchpadMap" ) ) );
}

extern "C" void InitializeScratchpad( Mojo::Core::SegmenterState* segmenterState )
{
    MOJO_THRUST_SAFE(
        Mojo::Core::Thrust::Fill(
            segmenterState->deviceVectors.Get< float >( "ScratchpadMap" ).begin(),
            segmenterState->deviceVectors.Get< float >( "ScratchpadMap" ).end(),
            segmenterState->parameters.Get< float >( "SCRATCHPAD_MAP_INITIAL_VALUE" ),
            segmenterState->deviceVectors.Get< float >( "ScratchpadMap" ) ) );
}

extern "C" void InitializeCostMap( Mojo::Core::SegmenterState* segmenterState )
{
    MOJO_THRUST_SAFE(
        Mojo::Core::Thrust::Fill(
            segmenterState->deviceVectors.Get< float >( "CostForwardMap" ).begin(),
            segmenterState->deviceVectors.Get< float >( "CostForwardMap" ).end(),
            segmenterState->parameters.Get< float >( "COST_MAP_INITIAL_VALUE" ),
            segmenterState->deviceVectors.Get< float >( "ScratchpadMap" ) ) );

    MOJO_THRUST_SAFE(
        Mojo::Core::Thrust::Fill(
            segmenterState->deviceVectors.Get< float >( "CostBackwardMap" ).begin(),
            segmenterState->deviceVectors.Get< float >( "CostBackwardMap" ).end(),
            segmenterState->parameters.Get< float >( "COST_MAP_INITIAL_VALUE" ),
            segmenterState->deviceVectors.Get< float >( "ScratchpadMap" ) ) );
}