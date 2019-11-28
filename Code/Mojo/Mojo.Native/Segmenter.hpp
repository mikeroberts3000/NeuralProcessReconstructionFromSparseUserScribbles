#pragma once

#include "Mojo.Core/Dictionary.hpp"
#include "Mojo.Core/SegmenterState.hpp"

struct ID3D11Device;
struct ID3D11DeviceContext;

namespace Mojo
{
namespace Native
{

enum HardConstraintMode
{
    HardConstraintMode_Breadcrumb,
    HardConstraintMode_Scribble
};

class Segmenter
{
public:
    Segmenter( ID3D11Device* d3d11Device, ID3D11DeviceContext* d3d11DeviceContext, Core::PrimitiveDictionary parameters );
    ~Segmenter();

    Core::SegmenterState* GetSegmenterState();

    void LoadVolume( Core::Dictionary< Core::VolumeDescription > volumeDescriptions );
    void UnloadVolume();

    void LoadSegmentation( Core::Dictionary< Core::VolumeDescription > volumeDescriptions );
    void SaveSegmentationAs( Core::Dictionary< Core::VolumeDescription > volumeDescriptions );

    void InitializeEdgeXYMap( Core::Dictionary< Core::VolumeDescription > volumeDescriptions );
    void InitializeEdgeXYMapForSplitting( Core::Dictionary< Core::VolumeDescription > volumeDescriptions, int neuralProcessId );

    void InitializeSegmentation();
    void InitializeSegmentationAndRemoveFromCommittedSegmentation( int neuralProcessId );

    void InitializeConstraintMap();
    void InitializeConstraintMapFromIdMap( int neuralProcessId );
    void InitializeConstraintMapFromIdMapForSplitting( int neuralProcessId );
    void InitializeConstraintMapFromPrimalMap();
    void DilateConstraintMap();

    void AddForegroundHardConstraint( int3 p, float radius, HardConstraintMode hardConstraintMode );
    void AddBackgroundHardConstraint( int3 p, float radius, HardConstraintMode hardConstraintMode );
    void AddForegroundHardConstraint( int3 p1, int3 p2, float radius, HardConstraintMode hardConstraintMode );
    void AddBackgroundHardConstraint( int3 p1, int3 p2, float radius, HardConstraintMode hardConstraintMode );

    void Update2D( int numIterations, int zSlice );
    void Update3D( int numIterations );
    void VisualUpdate();

    void UpdateCommittedSegmentation( int neuralProcessId, int4 neuralProcessColor );
    void UpdateCommittedSegmentationDoNotRemove( int neuralProcessId, int4 neuralProcessColor );

    void ReplaceNeuralProcessInCommittedSegmentation2D( int oldId, int newId, int4 newColor, int slice );
    void ReplaceNeuralProcessInCommittedSegmentation3D( int oldId, int newId, int4 newColor );
    void ReplaceNeuralProcessInCommittedSegmentation2DConnectedComponentOnly( int oldId, int newId, int4 newColor, int slice, int2 seed );
    void ReplaceNeuralProcessInCommittedSegmentation3DConnectedComponentOnly( int oldId, int newId, int4 newColor, int3 seed );

    void UndoLastChangeToCommittedSegmentation();
    void RedoLastChangeToCommittedSegmentation();
    void VisualUpdateColorMap();

    void InitializeCostMap();
    void InitializeCostMapFromPrimalMap();
    void IncrementCostMapFromPrimalMapForward();
    void IncrementCostMapFromPrimalMapBackward();
    void FinalizeCostMapFromPrimalMap();

    void UpdateConstraintMapAndPrimalMapFromCostMap();

    int   GetNeuralProcessId( int3 p );
    int4  GetNeuralProcessColor( int3 p );
    float GetPrimalValue( int3 p );

    void DumpIntermediateData();

    void DebugInitialize();
    void DebugUpdate();
    void DebugTerminate();

private:
    template < typename TCudaType >
    void LoadVolumeInternal( Core::Dictionary< Core::VolumeDescription > volumeDescriptions );
    void UnloadVolumeInternal();

    bool AddHardConstraint( int3 p1, int3 p2, float radius, HardConstraintMode hardConstraintMode, float constraintValue, float primalValue );

    int  Index3DToIndex1D( int3 index3D, int3 numVoxels ); 

    Core::SegmenterState      mSegmenterState;
    ID3D11Device*             mD3D11Device;
    ID3D11DeviceContext*      mD3D11DeviceContext;

    float*                    mScratchpadVolumeHost;
    uchar4*                   mUndoColorMapVolumeHost;
    uchar4*                   mRedoColorMapVolumeHost;
    uchar4*                   mFloodfillColorMapVolumeHost;
    int*                      mUndoIdMapVolumeHost;
    int*                      mRedoIdMapVolumeHost;
    int*                      mFloodfillIdMapVolumeHost;
    char*                     mFloodfillVisitedMapVolumeHost;
};

}
}