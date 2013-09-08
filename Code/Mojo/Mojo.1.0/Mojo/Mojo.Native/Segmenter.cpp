#include "Segmenter.hpp"

#include <fstream>
#include <iostream>
#include <queue>

#include <boost/filesystem.hpp>

#include <opencv2/core/core.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/video/tracking.hpp>
#include <opencv2/highgui/highgui.hpp>

#include "Mojo.Core/Assert.hpp"
#include "Mojo.Core/D3D11.hpp"
#include "Mojo.Core/Cuda.hpp"
#include "Mojo.Core/Thrust.hpp"
#include "Mojo.Core/SegmenterState.hpp"
#include "Mojo.Core/D3D11CudaTexture.hpp"
#include "Mojo.Core/ForEach.hpp"
#include "Mojo.Core/Printf.hpp"

#include "Mojo.Cuda/Mojo.Cuda.hpp"

namespace Mojo
{
namespace Native
{

Segmenter::Segmenter( ID3D11Device* d3d11Device, ID3D11DeviceContext* d3d11DeviceContext, Core::PrimitiveDictionary parameters ) :
    mD3D11Device       ( NULL ),
    mD3D11DeviceContext( NULL ),
    mSegmenterState    ( parameters )
{
    mD3D11Device = d3d11Device;
    mD3D11Device->AddRef();

    mD3D11DeviceContext = d3d11DeviceContext;
    mD3D11DeviceContext->AddRef();
}


Segmenter::~Segmenter(void)
{
    mD3D11DeviceContext->Release();
    mD3D11DeviceContext = NULL;

    mD3D11Device->Release();
    mD3D11Device = NULL;
}

void Segmenter::LoadVolume( Core::Dictionary< Core::VolumeDescription > volumeDescriptions )
{
    switch ( volumeDescriptions.Get( "SourceMap" ).dxgiFormat )
    {
        case DXGI_FORMAT_R8_UNORM:
            LoadVolumeInternal< uchar1 >( volumeDescriptions );
            break;

        case DXGI_FORMAT_R8G8B8A8_UNORM:
            LoadVolumeInternal< uchar4 >( volumeDescriptions );
            break;

        default:
            RELEASE_ASSERT( 0 );
            break;
    }
}

void Segmenter::UnloadVolume()
{
    UnloadVolumeInternal();
}

void Segmenter::LoadSegmentation( Core::Dictionary< Core::VolumeDescription > volumeDescriptions )
{
    InitializeSegmentation();
    InitializeConstraintMap();
    RedoLastChangeToCommittedSegmentation();

    int numVoxels = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

    Core::Thrust::MemcpyDeviceToHost( mUndoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mUndoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );

    Core::Thrust::MemcpyHostToDevice( mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), volumeDescriptions.Get( "IdMap"    ).data, numVoxels );
    Core::Thrust::MemcpyHostToDevice( mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), volumeDescriptions.Get( "ColorMap" ).data, numVoxels );

    Core::Thrust::MemcpyDeviceToHost( mRedoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mRedoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );
}

void Segmenter::SaveSegmentationAs( Core::Dictionary< Core::VolumeDescription > volumeDescriptions )
{
    int numVoxels = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

    Core::Thrust::MemcpyDeviceToHost( volumeDescriptions.Get( "IdMap"    ).data, mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( volumeDescriptions.Get( "ColorMap" ).data, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );
}

void Segmenter::InitializeEdgeXYMap( Core::Dictionary< Core::VolumeDescription > volumeDescriptions )
{
    ::InitializeEdgeXYMap( &mSegmenterState, &volumeDescriptions );
}

void Segmenter::InitializeEdgeXYMapForSplitting( Core::Dictionary< Core::VolumeDescription > volumeDescriptions, int neuralProcessId )
{
    ::InitializeEdgeXYMapForSplitting( &mSegmenterState, &volumeDescriptions, neuralProcessId );
}

void Segmenter::InitializeConstraintMap()
{
    ::InitializeConstraintMap( &mSegmenterState );

    mSegmenterState.slicesWithForegroundConstraints.clear();
    mSegmenterState.slicesWithBackgroundConstraints.clear();
}

void Segmenter::InitializeConstraintMapFromIdMap( int neuralProcessId )
{
    ::InitializeConstraintMapFromIdMap( &mSegmenterState, neuralProcessId );
}

void Segmenter::InitializeConstraintMapFromIdMapForSplitting( int neuralProcessId )
{
    ::InitializeConstraintMapFromIdMapForSplitting( &mSegmenterState, neuralProcessId );
}

void Segmenter::InitializeConstraintMapFromPrimalMap()
{
    ::InitializeConstraintMapFromPrimalMap( &mSegmenterState );
}

void Segmenter::DilateConstraintMap()
{
    ::DilateConstraintMap( &mSegmenterState );
}

void Segmenter::InitializeSegmentation()
{
    ::InitializeSegmentation( &mSegmenterState );
}

void Segmenter::InitializeSegmentationAndRemoveFromCommittedSegmentation( int neuralProcessId )
{
    ::InitializeSegmentationAndRemoveFromCommittedSegmentation( &mSegmenterState, neuralProcessId );
}

void Segmenter::AddForegroundHardConstraint( int3 p, float radius, HardConstraintMode hardConstraintMode )
{
    AddHardConstraint( p,
                       p,
                       radius,
                       hardConstraintMode,
                       mSegmenterState.parameters.Get< float >( "CONSTRAINT_MAP_HARD_FOREGROUND_USER" ),
                       mSegmenterState.parameters.Get< float >( "PRIMAL_MAP_FOREGROUND" ) );

    if ( hardConstraintMode == HardConstraintMode_Scribble )
    {
        mSegmenterState.slicesWithForegroundConstraints.insert( p.z );
    }
}

void Segmenter::AddBackgroundHardConstraint( int3 p, float radius, HardConstraintMode hardConstraintMode )
{
    AddHardConstraint( p,
                       p,
                       radius,
                       hardConstraintMode,
                       mSegmenterState.parameters.Get< float >( "CONSTRAINT_MAP_HARD_BACKGROUND_USER" ),
                       mSegmenterState.parameters.Get< float >( "PRIMAL_MAP_BACKGROUND" ) );

    if ( hardConstraintMode == HardConstraintMode_Scribble )
    {
        mSegmenterState.slicesWithBackgroundConstraints.insert( p.z );
    }
}

void Segmenter::AddForegroundHardConstraint( int3 p1, int3 p2, float radius, HardConstraintMode hardConstraintMode )
{
    AddHardConstraint( p1,
                       p2,
                       radius,
                       hardConstraintMode,
                       mSegmenterState.parameters.Get< float >( "CONSTRAINT_MAP_HARD_FOREGROUND_USER" ),
                       mSegmenterState.parameters.Get< float >( "PRIMAL_MAP_FOREGROUND" ) );

    if ( hardConstraintMode == HardConstraintMode_Scribble )
    {
        mSegmenterState.slicesWithForegroundConstraints.insert( p1.z );
    }
}

void Segmenter::AddBackgroundHardConstraint( int3 p1, int3 p2, float radius, HardConstraintMode hardConstraintMode )
{
    AddHardConstraint( p1,
                       p2,
                       radius,
                       hardConstraintMode,
                       mSegmenterState.parameters.Get< float >( "CONSTRAINT_MAP_HARD_BACKGROUND_USER" ),
                       mSegmenterState.parameters.Get< float >( "PRIMAL_MAP_BACKGROUND" ) );

    if ( hardConstraintMode == HardConstraintMode_Scribble )
    {
        mSegmenterState.slicesWithBackgroundConstraints.insert( p1.z );
    }
}

bool Segmenter::AddHardConstraint( int3 p1, int3 p2, float radius, HardConstraintMode hardConstraintMode, float constraintValue, float primalValue )
{
    RELEASE_ASSERT( p1.z == p2.z );

    if ( p1.x < 0 || p1.y < 0 || p2.x < 0 || p2.y < 0 )
    {
        return false;
    }

    ::AddHardConstraint( &mSegmenterState, p1, p2, radius, constraintValue, primalValue );

    return true;
}

void Segmenter::Update2D( int numIterations, int zSlice )
{
#ifdef _DEBUG
    numIterations = 1;
#endif

    int   checkEnergyFrequency = numIterations - 1;
    float lambda               = 0.001f;
    float convergenceGap       = -1.0f;
    float L                    = 9.0f;
    float tau                  = 1.0f / sqrt( L );
    float sigma                = 1.0f / sqrt( L );

    // Start calculation
    for ( int i = 0; i < numIterations; i++ )
    {
        ::UpdatePrimalMap2D( &mSegmenterState, lambda, tau, zSlice );
        ::UpdateDualMap2D( &mSegmenterState, sigma, zSlice );

        if( ( i == 0 && checkEnergyFrequency == 0 ) || ( i % checkEnergyFrequency == 0 ) )
        {
            float primalEnergy = -1.0f;
            float dualEnergy   = -1.0f;

            ::CalculateDualEnergy2D( &mSegmenterState, lambda, zSlice, dualEnergy );
            ::CalculatePrimalEnergy2D( &mSegmenterState, lambda, zSlice, primalEnergy );

            float oldconvergenceGap             = mSegmenterState.convergenceGap;
            float newconvergenceGap             = primalEnergy - dualEnergy;
            float convergenceGapDelta           = abs( newconvergenceGap - oldconvergenceGap ) / (float)numIterations;

            mSegmenterState.convergenceGap      = newconvergenceGap;
            mSegmenterState.convergenceGapDelta = convergenceGapDelta;
        }
    }
}

void Segmenter::Update3D( int numIterations )
{
#ifdef _DEBUG
    numIterations = 1;
#endif

    int   checkEnergyFrequency = numIterations - 1;
    float lambda               = 0.001f;
    float convergenceGap       = -1.0f;
    float L                    = 9.0f;
    float tau                  = 1.0f / sqrt( L );
    float sigma                = 1.0f / sqrt( L );

    // Start calculation
    for ( int i = 0; i < numIterations; i++ )
    {
        ::UpdatePrimalMap3D( &mSegmenterState, lambda, tau );
        ::UpdateDualMap3D( &mSegmenterState, sigma );

        if( ( i == 0 && checkEnergyFrequency == 0 ) || ( i % checkEnergyFrequency == 0 ) )
        {
            float primalEnergy = -1.0f;
            float dualEnergy   = -1.0f;

            ::CalculateDualEnergy3D( &mSegmenterState, lambda, dualEnergy );
            ::CalculatePrimalEnergy3D( &mSegmenterState, lambda, primalEnergy );
                                    
            float oldconvergenceGap             = mSegmenterState.convergenceGap;
            float newconvergenceGap             = primalEnergy - dualEnergy;
            float convergenceGapDelta           = abs( newconvergenceGap - oldconvergenceGap ) / (float)numIterations;

            mSegmenterState.convergenceGap      = newconvergenceGap;
            mSegmenterState.convergenceGapDelta = convergenceGapDelta;
        }
    }
}

void Segmenter::VisualUpdate()
{
    cudaArray* cudaArray = NULL;
    mSegmenterState.d3d11CudaTextures.MapCudaArrays();

    cudaArray = mSegmenterState.d3d11CudaTextures.Get( "PrimalMap" )->GetMappedCudaArray();
    Core::Thrust::Memcpy3DToArray( cudaArray, mSegmenterState.deviceVectors.Get< float >( "PrimalMap" ), mSegmenterState.volumeDescription );

    cudaArray = mSegmenterState.d3d11CudaTextures.Get( "ConstraintMap" )->GetMappedCudaArray();
    Core::Thrust::Memcpy3DToArray( cudaArray, mSegmenterState.deviceVectors.Get< float >( "ConstraintMap" ), mSegmenterState.volumeDescription );

    mSegmenterState.d3d11CudaTextures.UnmapCudaArrays();
}

void Segmenter::UpdateCommittedSegmentation( int neuralProcessId, int4 neuralProcessColor )
{
    RedoLastChangeToCommittedSegmentation();

    uchar4 neuralProcessColorUChar4 = make_uchar4( neuralProcessColor.x, neuralProcessColor.y, neuralProcessColor.z, neuralProcessColor.w );
    int    numVoxels                = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

    Core::Thrust::MemcpyDeviceToHost( mUndoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mUndoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );

    ::UpdateCommittedSegmentation( &mSegmenterState, neuralProcessId, neuralProcessColorUChar4 );

    Core::Thrust::MemcpyDeviceToHost( mRedoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mRedoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );
}

void Segmenter::UpdateCommittedSegmentationDoNotRemove( int neuralProcessId, int4 neuralProcessColor )
{
    RedoLastChangeToCommittedSegmentation();

    uchar4 neuralProcessColorUChar4 = make_uchar4( neuralProcessColor.x, neuralProcessColor.y, neuralProcessColor.z, neuralProcessColor.w );
    int    numVoxels                = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

    Core::Thrust::MemcpyDeviceToHost( mUndoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mUndoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );

    ::UpdateCommittedSegmentationDoNotRemove( &mSegmenterState, neuralProcessId, neuralProcessColorUChar4 );

    Core::Thrust::MemcpyDeviceToHost( mRedoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mRedoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );
}

void Segmenter::ReplaceNeuralProcessInCommittedSegmentation2D( int oldId, int newId, int4 newColor, int slice )
{
    uchar4 newColorUChar4 = make_uchar4( newColor.x, newColor.y, newColor.z, newColor.w );
    int    numVoxels      = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

    Core::Thrust::MemcpyDeviceToHost( mUndoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mUndoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );

    ::ReplaceNeuralProcessInCommittedSegmentation2D( &mSegmenterState, oldId, newId, newColorUChar4, slice );

    Core::Thrust::MemcpyDeviceToHost( mRedoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mRedoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );
}

void Segmenter::ReplaceNeuralProcessInCommittedSegmentation3D( int oldId, int newId, int4 newColor )
{
    uchar4 newColorUChar4 = make_uchar4( newColor.x, newColor.y, newColor.z, newColor.w );
    int    numVoxels      = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

    Core::Thrust::MemcpyDeviceToHost( mUndoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mUndoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );

    ::ReplaceNeuralProcessInCommittedSegmentation3D( &mSegmenterState, oldId, newId, newColorUChar4 );

    Core::Thrust::MemcpyDeviceToHost( mRedoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mRedoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );
}

void Segmenter::ReplaceNeuralProcessInCommittedSegmentation2DConnectedComponentOnly( int oldId, int newId, int4 newColor, int slice, int2 seed )
{
    uchar4 newColorUChar4 = make_uchar4( newColor.x, newColor.y, newColor.z, newColor.w );
    int3   seedInt3       = make_int3( seed, slice );
    int    seedIndex1D    = Index3DToIndex1D( seedInt3, mSegmenterState.volumeDescription.numVoxels );
    int    numVoxels      = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

    Core::Thrust::MemcpyDeviceToHost( mUndoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mUndoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );

    
    //
    // perform a 2D flood fill on the CPU
    //
    Core::Thrust::MemcpyDeviceToHost( mFloodfillIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int >   ( "IdMap" ),    numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mFloodfillColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );

    memset( mFloodfillVisitedMapVolumeHost, 0, numVoxels * sizeof( char ) );

    RELEASE_ASSERT( mFloodfillIdMapVolumeHost[ seedIndex1D ] == oldId );

    std::queue< int3 > unprocessedElements;

    mFloodfillIdMapVolumeHost[ seedIndex1D ]      = newId;
    mFloodfillColorMapVolumeHost[ seedIndex1D ]   = newColorUChar4;
    mFloodfillVisitedMapVolumeHost[ seedIndex1D ] = 1;

    unprocessedElements.push( seedInt3 );

    while( !unprocessedElements.empty() )
    {
        int3 currentElement = unprocessedElements.front();
        unprocessedElements.pop();

        int3 adjacentElements[ 4 ];
        adjacentElements[ 0 ].x = currentElement.x - 1; adjacentElements[ 0 ].y = currentElement.y;     adjacentElements[ 0 ].z = currentElement.z;
        adjacentElements[ 1 ].x = currentElement.x + 1; adjacentElements[ 1 ].y = currentElement.y;     adjacentElements[ 1 ].z = currentElement.z;
        adjacentElements[ 2 ].x = currentElement.x;     adjacentElements[ 2 ].y = currentElement.y - 1; adjacentElements[ 2 ].z = currentElement.z;
        adjacentElements[ 3 ].x = currentElement.x;     adjacentElements[ 3 ].y = currentElement.y + 1; adjacentElements[ 3 ].z = currentElement.z;

        for( int i = 0; i < 4; i++ )
        {
            if ( adjacentElements[ i ].x < 0 || adjacentElements[ i ].x >= mSegmenterState.volumeDescription.numVoxels.x ||
                 adjacentElements[ i ].y < 0 || adjacentElements[ i ].y >= mSegmenterState.volumeDescription.numVoxels.y )
            {
                continue;
            }

            int adjacentIndex1D = Index3DToIndex1D( adjacentElements[ i ], mSegmenterState.volumeDescription.numVoxels );

            if ( mFloodfillVisitedMapVolumeHost[ adjacentIndex1D ] == 1 )
            {
                continue;
            }
                
            mFloodfillVisitedMapVolumeHost[ adjacentIndex1D ] = 1;

            if ( mUndoIdMapVolumeHost[ adjacentIndex1D ] == oldId )
            {
                mFloodfillIdMapVolumeHost[ adjacentIndex1D ]      = newId;
                mFloodfillColorMapVolumeHost[ adjacentIndex1D ]   = newColorUChar4;
                mFloodfillVisitedMapVolumeHost[ adjacentIndex1D ] = 1;

                unprocessedElements.push( adjacentElements[ i ] );
            }
        }
    }

    Core::Thrust::MemcpyHostToDevice( mSegmenterState.deviceVectors.Get< int >   ( "IdMap" ),    mFloodfillIdMapVolumeHost,    numVoxels );
    Core::Thrust::MemcpyHostToDevice( mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), mFloodfillColorMapVolumeHost, numVoxels );



    Core::Thrust::MemcpyDeviceToHost( mRedoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mRedoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );
}

void Segmenter::ReplaceNeuralProcessInCommittedSegmentation3DConnectedComponentOnly( int oldId, int newId, int4 newColor, int3 seed )
{
    uchar4 newColorUChar4 = make_uchar4( newColor.x, newColor.y, newColor.z, newColor.w );
    int    seedIndex1D    = Index3DToIndex1D( seed, mSegmenterState.volumeDescription.numVoxels );
    int    numVoxels      = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

    Core::Thrust::MemcpyDeviceToHost( mUndoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mUndoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );

    

    //
    // perform a 3D flood fill on the CPU
    //
    Core::Thrust::MemcpyDeviceToHost( mFloodfillIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int >   ( "IdMap" ),    numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mFloodfillColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );

    memset( mFloodfillVisitedMapVolumeHost, 0, numVoxels * sizeof( char ) );

    RELEASE_ASSERT( mFloodfillIdMapVolumeHost[ seedIndex1D ] == oldId );

    std::queue< int3 > unprocessedElements;

    mFloodfillIdMapVolumeHost[ seedIndex1D ]      = newId;
    mFloodfillColorMapVolumeHost[ seedIndex1D ]   = newColorUChar4;
    mFloodfillVisitedMapVolumeHost[ seedIndex1D ] = 1;

    unprocessedElements.push( seed );

    while( !unprocessedElements.empty() )
    {
        int3 currentElement = unprocessedElements.front();
        unprocessedElements.pop();

        int3 adjacentElements[ 6 ];
        adjacentElements[ 0 ].x = currentElement.x - 1; adjacentElements[ 0 ].y = currentElement.y;     adjacentElements[ 0 ].z = currentElement.z;
        adjacentElements[ 1 ].x = currentElement.x + 1; adjacentElements[ 1 ].y = currentElement.y;     adjacentElements[ 1 ].z = currentElement.z;
        adjacentElements[ 2 ].x = currentElement.x;     adjacentElements[ 2 ].y = currentElement.y - 1; adjacentElements[ 2 ].z = currentElement.z;
        adjacentElements[ 3 ].x = currentElement.x;     adjacentElements[ 3 ].y = currentElement.y + 1; adjacentElements[ 3 ].z = currentElement.z;
        adjacentElements[ 4 ].x = currentElement.x;     adjacentElements[ 4 ].y = currentElement.y - 1; adjacentElements[ 4 ].z = currentElement.z - 1;
        adjacentElements[ 5 ].x = currentElement.x;     adjacentElements[ 5 ].y = currentElement.y + 1; adjacentElements[ 5 ].z = currentElement.z + 1;

        for( int i = 0; i < 6; i++ )
        {
            if ( adjacentElements[ i ].x < 0 || adjacentElements[ i ].x >= mSegmenterState.volumeDescription.numVoxels.x ||
                 adjacentElements[ i ].y < 0 || adjacentElements[ i ].y >= mSegmenterState.volumeDescription.numVoxels.y || 
                 adjacentElements[ i ].z < 0 || adjacentElements[ i ].z >= mSegmenterState.volumeDescription.numVoxels.z )
            {
                continue;
            }

            int adjacentIndex1D = Index3DToIndex1D( adjacentElements[ i ], mSegmenterState.volumeDescription.numVoxels );

            if ( mFloodfillVisitedMapVolumeHost[ adjacentIndex1D ] == 1 )
            {
                continue;
            }
                
            mFloodfillVisitedMapVolumeHost[ adjacentIndex1D ] = 1;

            if ( mUndoIdMapVolumeHost[ adjacentIndex1D ] == oldId )
            {
                mFloodfillIdMapVolumeHost[ adjacentIndex1D ]      = newId;
                mFloodfillColorMapVolumeHost[ adjacentIndex1D ]   = newColorUChar4;
                mFloodfillVisitedMapVolumeHost[ adjacentIndex1D ] = 1;

                unprocessedElements.push( adjacentElements[ i ] );
            }
        }
    }

    Core::Thrust::MemcpyHostToDevice( mSegmenterState.deviceVectors.Get< int >   ( "IdMap" ),    mFloodfillIdMapVolumeHost,    numVoxels );
    Core::Thrust::MemcpyHostToDevice( mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), mFloodfillColorMapVolumeHost, numVoxels );



    Core::Thrust::MemcpyDeviceToHost( mRedoIdMapVolumeHost,    mSegmenterState.deviceVectors.Get< int    >( "IdMap"    ), numVoxels );
    Core::Thrust::MemcpyDeviceToHost( mRedoColorMapVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );
}

void Segmenter::UndoLastChangeToCommittedSegmentation()
{
    int numVoxels = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

    Core::Thrust::MemcpyHostToDevice( mSegmenterState.deviceVectors.Get< int    >( "IdMap" ),    mUndoIdMapVolumeHost,    numVoxels );
    Core::Thrust::MemcpyHostToDevice( mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), mUndoColorMapVolumeHost, numVoxels );
}

void Segmenter::RedoLastChangeToCommittedSegmentation()
{
    int numVoxels = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

    Core::Thrust::MemcpyHostToDevice( mSegmenterState.deviceVectors.Get< int    >( "IdMap" ),    mRedoIdMapVolumeHost,    numVoxels );
    Core::Thrust::MemcpyHostToDevice( mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), mRedoColorMapVolumeHost, numVoxels );
}

void Segmenter::VisualUpdateColorMap()
{
    cudaArray* cudaArray = NULL;
    mSegmenterState.d3d11CudaTextures.MapCudaArrays();

    cudaArray = mSegmenterState.d3d11CudaTextures.Get( "ColorMap" )->GetMappedCudaArray();
    Core::Thrust::Memcpy3DToArray( cudaArray, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), mSegmenterState.volumeDescription );

    mSegmenterState.d3d11CudaTextures.UnmapCudaArrays();
}

void Segmenter::InitializeCostMap()
{
    ::InitializeCostMap( &mSegmenterState );
}

void Segmenter::InitializeCostMapFromPrimalMap()
{
    ::InitializeCostMapFromPrimalMap( &mSegmenterState );
}

void Segmenter::IncrementCostMapFromPrimalMapForward()
{
    ::IncrementCostMapFromPrimalMapForward( &mSegmenterState );
}

void Segmenter::IncrementCostMapFromPrimalMapBackward()
{
    ::IncrementCostMapFromPrimalMapBackward( &mSegmenterState );
}

void Segmenter::FinalizeCostMapFromPrimalMap()
{
    ::FinalizeCostMapFromPrimalMap( &mSegmenterState );
}


void Segmenter::UpdateConstraintMapAndPrimalMapFromCostMap()
{
    ::UpdateConstraintMapAndPrimalMapFromCostMap( &mSegmenterState );
}

int Segmenter::GetNeuralProcessId( int3 p )
{
    int    numVoxelsX      = mSegmenterState.volumeDescription.numVoxels.x;
    int    numVoxelsY      = mSegmenterState.volumeDescription.numVoxels.y;
    int    numVoxelsZ      = mSegmenterState.volumeDescription.numVoxels.z;
    int    neuralProcessId = mSegmenterState.deviceVectors.Get< int >( "IdMap" )[ ( numVoxelsX * numVoxelsY * p.z ) + ( numVoxelsX * p.y ) + p.x ];

    return neuralProcessId;
}

int4 Segmenter::GetNeuralProcessColor( int3 p )
{
    int    numVoxelsX            = mSegmenterState.volumeDescription.numVoxels.x;
    int    numVoxelsY            = mSegmenterState.volumeDescription.numVoxels.y;
    int    numVoxelsZ            = mSegmenterState.volumeDescription.numVoxels.z;
    uchar4 neuralProcessColor    = mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" )[ ( numVoxelsX * numVoxelsY * p.z ) + ( numVoxelsX * p.y ) + p.x ];
    int4  neuralProcessColorInt4 = make_int4( neuralProcessColor.x, neuralProcessColor.y, neuralProcessColor.z, neuralProcessColor.w );

    return neuralProcessColorInt4;
}

float Segmenter::GetPrimalValue( int3 p )
{
    int   numVoxelsX  = mSegmenterState.volumeDescription.numVoxels.x;
    int   numVoxelsY  = mSegmenterState.volumeDescription.numVoxels.y;
    int   numVoxelsZ  = mSegmenterState.volumeDescription.numVoxels.z;
    float primalValue = mSegmenterState.deviceVectors.Get< float >( "PrimalMap" )[ ( numVoxelsX * numVoxelsY * p.z ) + ( numVoxelsX * p.y ) + p.x ];

    return primalValue;
}

Core::SegmenterState* Segmenter::GetSegmenterState()
{
    return &mSegmenterState;
}

void Segmenter::DumpIntermediateData()
{
    if ( mSegmenterState.parameters.Get< bool >( "DUMP_PRIMAL_MAP" ) )
    {
        int numVoxels = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

        Core::Thrust::MemcpyDeviceToHost( mScratchpadVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "PrimalMap" ), numVoxels );
        
        std::ofstream file( "PrimalMap.raw", std::ios::binary );
        file.write( (char*)mScratchpadVolumeHost, numVoxels * sizeof( float ) );
        file.close();
    }

    if ( mSegmenterState.parameters.Get< bool >( "DUMP_EDGE_XY_MAP" ) )
    {
        int numVoxels = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

        Core::Thrust::MemcpyDeviceToHost( mScratchpadVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "EdgeXYMap" ), numVoxels );

        std::ofstream file( "EdgeXYMap.raw", std::ios::binary );
        file.write( (char*)mScratchpadVolumeHost, numVoxels * sizeof( float ) );
        file.close();
    }

    if ( mSegmenterState.parameters.Get< bool >( "DUMP_EDGE_Z_MAP" ) )
    {
        RELEASE_ASSERT( mSegmenterState.parameters.Get< bool >( "DIRECT_ANISOTROPIC_TV" ) );

        int numVoxels = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

        Core::Thrust::MemcpyDeviceToHost( mScratchpadVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "EdgeZMap" ), numVoxels );

        std::ofstream file( "EdgeZMap.raw", std::ios::binary );
        file.write( (char*)mScratchpadVolumeHost, numVoxels * sizeof( float ) );
        file.close();
    }

    if ( mSegmenterState.parameters.Get< bool >( "DUMP_CONSTRAINT_MAP" ) )
    {
        int numVoxels = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

        Core::Thrust::MemcpyDeviceToHost( mScratchpadVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ConstraintMap" ), numVoxels );

        std::ofstream file( "ConstraintMap.raw", std::ios::binary );
        file.write( (char*)mScratchpadVolumeHost, numVoxels * sizeof( float ) );
        file.close();
    }

    if ( mSegmenterState.parameters.Get< bool >( "DUMP_ID_MAP" ) )
    {
        int numVoxels = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

        Core::Thrust::MemcpyDeviceToHost( mScratchpadVolumeHost, mSegmenterState.deviceVectors.Get< int >( "IdMap" ), numVoxels );

        std::ofstream file( "IdMap.raw", std::ios::binary );
        file.write( (char*)mScratchpadVolumeHost, numVoxels * sizeof( int ) );
        file.close();
    }

    if ( mSegmenterState.parameters.Get< bool >( "DUMP_COLOR_MAP" ) )
    {
        int numVoxels = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;

        Core::Thrust::MemcpyDeviceToHost( mScratchpadVolumeHost, mSegmenterState.deviceVectors.Get< uchar4 >( "ColorMap" ), numVoxels );

        std::ofstream file( "ColorMap.raw", std::ios::binary );
        file.write( (char*)mScratchpadVolumeHost, numVoxels * sizeof( uchar4 ) );
        file.close();
    }
}

void Segmenter::DebugInitialize()
{
    ::DebugInitialize( &mSegmenterState );
}

void Segmenter::DebugTerminate()
{
    ::DebugTerminate( &mSegmenterState );
}

void Segmenter::DebugUpdate()
{
    ::DebugUpdate( &mSegmenterState );
}

template < typename TCudaType >
void Segmenter::LoadVolumeInternal( Core::Dictionary< Core::VolumeDescription > volumeDescriptions )
{
    mSegmenterState.volumeDescription = volumeDescriptions.Get( "SourceMap" );

    int numElements                   = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y * mSegmenterState.volumeDescription.numVoxels.z;
    int numElementsPerSlice           = mSegmenterState.volumeDescription.numVoxels.x * mSegmenterState.volumeDescription.numVoxels.y;


    mScratchpadVolumeHost          = new float [ numElements ];
    mUndoColorMapVolumeHost        = new uchar4[ numElements ];
    mRedoColorMapVolumeHost        = new uchar4[ numElements ];
    mFloodfillColorMapVolumeHost   = new uchar4[ numElements ];
    mUndoIdMapVolumeHost           = new int   [ numElements ];
    mRedoIdMapVolumeHost           = new int   [ numElements ];
    mFloodfillIdMapVolumeHost      = new int   [ numElements ];
    mFloodfillVisitedMapVolumeHost = new char  [ numElements ];


    Core::Printf(
        "\nLoading dataset. Size in bytes = ",
        volumeDescriptions.Get( "SourceMap" ).numVoxels.x, " * ",
        volumeDescriptions.Get( "SourceMap" ).numVoxels.y, " * ",
        volumeDescriptions.Get( "SourceMap" ).numVoxels.z, " * ", 
        volumeDescriptions.Get( "SourceMap" ).numBytesPerVoxel, " = ",
        ( numElements * volumeDescriptions.Get( "SourceMap" ).numBytesPerVoxel ) / ( 1024 * 1024 ), " MBytes." );


    unsigned int freeMemory, totalMemory;
    CUresult     memInfoResult;
    memInfoResult = cuMemGetInfo( &freeMemory, &totalMemory );
    RELEASE_ASSERT( memInfoResult == CUDA_SUCCESS );
    Core::Printf( "\nBefore allocating GPU memory:\n",
                  "    Free memory:  ", freeMemory  / ( 1024 * 1024 ), " MBytes.\n",
                  "    Total memory: ", totalMemory / ( 1024 * 1024 ), " MBytes.\n" );


    D3D11_TEXTURE3D_DESC textureDesc3D;
    ZeroMemory( &textureDesc3D, sizeof( D3D11_TEXTURE3D_DESC ) );

    textureDesc3D.Width     = mSegmenterState.volumeDescription.numVoxels.x;
    textureDesc3D.Height    = mSegmenterState.volumeDescription.numVoxels.y;
    textureDesc3D.Depth     = mSegmenterState.volumeDescription.numVoxels.z;
    textureDesc3D.MipLevels = 1;
    textureDesc3D.Usage     = D3D11_USAGE_DEFAULT;
    textureDesc3D.BindFlags = D3D11_BIND_SHADER_RESOURCE;

    textureDesc3D.Format = volumeDescriptions.Get( "SourceMap" ).dxgiFormat;
    mSegmenterState.d3d11CudaTextures.Set( "SourceMap",          new Core::D3D11CudaTexture< ID3D11Texture3D, TCudaType >( mD3D11Device, mD3D11DeviceContext, textureDesc3D, volumeDescriptions.Get( "SourceMap" ) ) );

    textureDesc3D.Format = DXGI_FORMAT_R32_FLOAT;
    mSegmenterState.d3d11CudaTextures.Set( "ConstraintMap",      new Core::D3D11CudaTexture< ID3D11Texture3D, float >( mD3D11Device, mD3D11DeviceContext, textureDesc3D ) );
    mSegmenterState.d3d11CudaTextures.Set( "PrimalMap",          new Core::D3D11CudaTexture< ID3D11Texture3D, float >( mD3D11Device, mD3D11DeviceContext, textureDesc3D ) );

    textureDesc3D.Format = DXGI_FORMAT_R8G8B8A8_UNORM;          
    mSegmenterState.d3d11CudaTextures.Set( "ColorMap",           new Core::D3D11CudaTexture< ID3D11Texture3D, uchar4 >( mD3D11Device, mD3D11DeviceContext, textureDesc3D ) );

    mSegmenterState.cudaArrays.Set( "SourceMap",                 Core::Cuda::MallocArray3D< TCudaType >( mSegmenterState.volumeDescription ) );
    mSegmenterState.cudaArrays.Set( "TempScratchpadMap",         Core::Cuda::MallocArray2D< float     >( mSegmenterState.volumeDescription ) );

    mSegmenterState.deviceVectors.Set( "DualMap",                thrust::device_vector< float4 >( numElements, mSegmenterState.parameters.Get< float4 >( "DUAL_MAP_INITIAL_VALUE"  ) ) );
    mSegmenterState.deviceVectors.Set( "ColorMap",               thrust::device_vector< uchar4 >( numElements, mSegmenterState.parameters.Get< uchar4 >( "COLOR_MAP_INITIAL_VALUE" ) ) );
    mSegmenterState.deviceVectors.Set( "EdgeXYMap",              thrust::device_vector< float  >( numElements, mSegmenterState.parameters.Get< float  >( "EDGE_MAP_INITIAL_VALUE"  ) ) );
    
    if ( mSegmenterState.parameters.Get< bool >( "DIRECT_ANISOTROPIC_TV" ) )
    {
        mSegmenterState.deviceVectors.Set( "EdgeZMap",           thrust::device_vector< float >( numElements, mSegmenterState.parameters.Get< float >( "EDGE_MAP_INITIAL_VALUE" ) ) );
    }

    mSegmenterState.deviceVectors.Set( "ConstraintMap",          thrust::device_vector< float >( numElements, mSegmenterState.parameters.Get< float >( "CONSTRAINT_MAP_INITIAL_VALUE" ) ) );
    mSegmenterState.deviceVectors.Set( "OldPrimalMap",           thrust::device_vector< float >( numElements, mSegmenterState.parameters.Get< float >( "OLD_PRIMAL_MAP_INITIAL_VALUE" ) ) );
    mSegmenterState.deviceVectors.Set( "CostForwardMap",         thrust::device_vector< float >( numElements, mSegmenterState.parameters.Get< float >( "COST_MAP_INITIAL_VALUE"       ) ) );
    mSegmenterState.deviceVectors.Set( "CostBackwardMap",        thrust::device_vector< float >( numElements, mSegmenterState.parameters.Get< float >( "COST_MAP_INITIAL_VALUE"       ) ) );
    mSegmenterState.deviceVectors.Set( "PrimalMap",              thrust::device_vector< float >( numElements, mSegmenterState.parameters.Get< float >( "PRIMAL_MAP_INITIAL_VALUE"     ) ) );
    mSegmenterState.deviceVectors.Set( "IdMap",                  thrust::device_vector< int   >( numElements, mSegmenterState.parameters.Get< int >  ( "ID_MAP_INITIAL_VALUE"         ) ) );

    mSegmenterState.deviceVectors.Set( "ScratchpadMap",          thrust::device_vector< float >( numElements, mSegmenterState.parameters.Get< float >( "SCRATCHPAD_MAP_INITIAL_VALUE" ) ) );

    mSegmenterState.deviceVectors.Set( "ScratchpadMap",          thrust::device_vector< float4 >( numElements, mSegmenterState.parameters.Get< float4 >( "SCRATCHPAD_MAP_INITIAL_VALUE" ) ) );

    Core::Cuda::MemcpyHostToArray3D( mSegmenterState.cudaArrays.Get( "SourceMap" ), volumeDescriptions.Get( "SourceMap" ) );

    ::InitializeSegmentation( &mSegmenterState );
    ::InitializeCommittedSegmentation( &mSegmenterState );
    ::InitializeConstraintMap( &mSegmenterState );
    ::InitializeCostMap( &mSegmenterState );
    ::InitializeEdgeXYMap( &mSegmenterState, &volumeDescriptions );

    if (  mSegmenterState.volumeDescription.numVoxels.z > 1 )
    {
        mSegmenterState.deviceVectors.Set( "OpticalFlowForwardMap",  thrust::device_vector< float2 >( numElements, mSegmenterState.parameters.Get< float2 >( "OPTICAL_FLOW_MAP_INITIAL_VALUE" ) ) );
        mSegmenterState.deviceVectors.Set( "OpticalFlowBackwardMap", thrust::device_vector< float2 >( numElements, mSegmenterState.parameters.Get< float2 >( "OPTICAL_FLOW_MAP_INITIAL_VALUE" ) ) );

        int numElementsOpticalFlow = volumeDescriptions.Get( "OpticalFlowForwardMap" ).numVoxels.x * volumeDescriptions.Get( "OpticalFlowForwardMap" ).numVoxels.y * volumeDescriptions.Get( "OpticalFlowForwardMap" ).numVoxels.z;

        Core::Thrust::MemcpyHostToDevice( mSegmenterState.deviceVectors.Get< float2 >( "OpticalFlowForwardMap"  )[ 0 ],                   volumeDescriptions.Get( "OpticalFlowForwardMap"  ).data, numElementsOpticalFlow );
        Core::Thrust::MemcpyHostToDevice( mSegmenterState.deviceVectors.Get< float2 >( "OpticalFlowBackwardMap" )[ numElementsPerSlice ], volumeDescriptions.Get( "OpticalFlowBackwardMap" ).data, numElementsOpticalFlow );

        if ( mSegmenterState.parameters.Get< bool >( "DIRECT_ANISOTROPIC_TV" ) )
        {
            ::InitializeEdgeZMap( &mSegmenterState, &volumeDescriptions );
        }
    }

    memInfoResult = cuMemGetInfo( &freeMemory, &totalMemory );
    RELEASE_ASSERT( memInfoResult == CUDA_SUCCESS );
    Core::Printf( "After allocating GPU memory:\n",
                  "    Free memory:  ", freeMemory  / ( 1024 * 1024 ), " MBytes.\n",
                  "    Total memory: ", totalMemory / ( 1024 * 1024 ), " MBytes.\n" );
}

void Segmenter::UnloadVolumeInternal()
{
    unsigned int freeMemory, totalMemory;
    CUresult memInfoResult;

    mSegmenterState.convergenceGap      = 0.0f;
    mSegmenterState.convergenceGapDelta = 0.0f;

    mSegmenterState.slicesWithForegroundConstraints.clear();
    mSegmenterState.slicesWithBackgroundConstraints.clear();

    Core::Printf( "\nUnloading dataset.\n" );

    memInfoResult = cuMemGetInfo( &freeMemory, &totalMemory );
    RELEASE_ASSERT( memInfoResult == CUDA_SUCCESS );
    Core::Printf( "Before freeing GPU memory:\n",
                  "    Free memory:  ", freeMemory  / ( 1024 * 1024 ), " MBytes.\n",
                  "    Total memory: ", totalMemory / ( 1024 * 1024 ), " MBytes.\n" );

    mSegmenterState.deviceVectors.GetDictionary< float4 >().clear();
    mSegmenterState.deviceVectors.GetDictionary< float2 >().clear();
    mSegmenterState.deviceVectors.GetDictionary< uchar4 >().clear();
    mSegmenterState.deviceVectors.GetDictionary< float >().clear();

    MOJO_FOR_EACH_KEY( std::string key, mSegmenterState.d3d11CudaTextures.GetDictionary() )
    {
        delete mSegmenterState.d3d11CudaTextures.GetDictionary()[ key ];
    }
    mSegmenterState.d3d11CudaTextures.GetDictionary().clear();

    MOJO_FOR_EACH_KEY( std::string key, mSegmenterState.cudaArrays.GetDictionary() )
    {
        MOJO_CUDA_SAFE( cudaFreeArray( mSegmenterState.cudaArrays.GetDictionary()[ key ] ) );
    }
    mSegmenterState.d3d11CudaTextures.GetDictionary().clear();

    memInfoResult = cuMemGetInfo( &freeMemory, &totalMemory );
    RELEASE_ASSERT( memInfoResult == CUDA_SUCCESS );
    Core::Printf( "After freeing GPU memory:\n",
                  "    Free memory:  ", freeMemory  / ( 1024 * 1024 ), " MBytes.\n",
                  "    Total memory: ", totalMemory / ( 1024 * 1024 ), " MBytes.\n" );

    delete[] mScratchpadVolumeHost;
    delete[] mUndoColorMapVolumeHost;
    delete[] mRedoColorMapVolumeHost;
    delete[] mFloodfillColorMapVolumeHost;
    delete[] mUndoIdMapVolumeHost;
    delete[] mRedoIdMapVolumeHost;
    delete[] mFloodfillIdMapVolumeHost;
    delete[] mFloodfillVisitedMapVolumeHost;
}

int Segmenter::Index3DToIndex1D( int3 index3D, int3 numVoxels )
{
    return ( numVoxels.x * numVoxels.y * index3D.z ) + ( numVoxels.x * index3D.y ) + index3D.x;
}


}
}