#pragma once

#include "Mojo.Native/Segmenter.hpp"

#include "Dictionary.hpp"
#include "PrimitiveDictionary.hpp"
#include "VolumeDescription.hpp"

 

namespace Mojo
{
namespace Interop
{

#pragma managed
public enum class HardConstraintMode : System::Int32
{
    Breadcrumb = Native::HardConstraintMode_Breadcrumb,
    Scribble   = Native::HardConstraintMode_Scribble
};

#pragma managed
public ref class Segmenter : public NotifyPropertyChanged
{
public:
    Segmenter( SlimDX::Direct3D11::Device^ d3d11Device, SlimDX::Direct3D11::DeviceContext^ d3d11DeviceContext, PrimitiveDictionary^ parameters );
    ~Segmenter();

    void LoadVolume( Dictionary< VolumeDescription^ >^ volumeDescriptions );
    void UnloadVolume();

    void LoadSegmentation( Dictionary< VolumeDescription^ >^ volumeDescriptions );
    void SaveSegmentationAs( Dictionary< VolumeDescription^ >^ volumeDescriptions );

    void InitializeEdgeXYMap( Dictionary< VolumeDescription^ >^ volumeDescriptions );
    void InitializeEdgeXYMapForSplitting( Dictionary< VolumeDescription^ >^ volumeDescriptions, int neuralProcessId );

    void InitializeSegmentation();
    void InitializeSegmentationAndRemoveFromCommittedSegmentation( int neuralProcessId );

    void InitializeConstraintMap();
    void InitializeConstraintMapFromIdMap( int neuralProcessId );
    void InitializeConstraintMapFromIdMapForSplitting( int neuralProcessId );
    void InitializeConstraintMapFromPrimalMap();
    void DilateConstraintMap();

    void AddForegroundHardConstraint( Vector3^ p, float radius, HardConstraintMode hardConstraintMode );
    void AddBackgroundHardConstraint( Vector3^ p, float radius, HardConstraintMode hardConstraintMode );

    void AddForegroundHardConstraint( Vector3^ p1, Vector3^ p2, float radius, HardConstraintMode hardConstraintMode );
    void AddBackgroundHardConstraint( Vector3^ p1, Vector3^ p2, float radius, HardConstraintMode hardConstraintMode );

    void Update2D( int numIterations, int zSlice );
    void Update3D( int numIterations );
    void VisualUpdate();

    void UpdateCommittedSegmentation( int neuralProcessId, Vector3^ neuralProcessColor );
    void UpdateCommittedSegmentationDoNotRemove( int neuralProcessId, Vector3^ neuralProcessColor );

    void ReplaceNeuralProcessInCommittedSegmentation2D( int oldId, int newId, Vector3^ newColor, int slice );
    void ReplaceNeuralProcessInCommittedSegmentation3D( int oldId, int newId, Vector3^ newColor );
    void ReplaceNeuralProcessInCommittedSegmentation2DConnectedComponentOnly( int oldId, int newId, Vector3^ newColor, int slice, Vector3^ seed );
    void ReplaceNeuralProcessInCommittedSegmentation3DConnectedComponentOnly( int oldId, int newId, Vector3^ newColor, Vector3^ seed );

    void UndoLastChangeToCommittedSegmentation();
    void RedoLastChangeToCommittedSegmentation();
    void VisualUpdateColorMap();

    void InitializeCostMap();

    void InitializeCostMapFromPrimalMap();
    void IncrementCostMapFromPrimalMapForward();
    void IncrementCostMapFromPrimalMapBackward();
    void FinalizeCostMapFromPrimalMap();

    void UpdateConstraintMapAndPrimalMapFromCostMap();

    int     GetNeuralProcessId( Vector3^ p );
    Vector3 GetNeuralProcessColor( Vector3^ p );
    float   GetPrimalValue( Vector3^ p );

    void DumpIntermediateData();

    void DebugInitialize();
    void DebugUpdate();
    void DebugTerminate();

    property VolumeDescription^                 VolumeDescription;
    property Dictionary< ShaderResourceView^ >^ D3D11CudaTextures;
    property float                              ConvergenceGap;
    property float                              ConvergenceGapDelta;
    property float                              MaxForegroundCostDelta;

private:
    Native::Segmenter* mSegmenter;
};

}
}