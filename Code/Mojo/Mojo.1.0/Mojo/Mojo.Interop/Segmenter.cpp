#include "Segmenter.hpp"

#include <string>

#include <msclr/marshal_cppstd.h>

#include "Mojo.Core/ForEach.hpp"
#include "Mojo.Core/ID3D11CudaTexture.hpp"

namespace Mojo
{
namespace Interop
{

Segmenter::Segmenter( SlimDX::Direct3D11::Device^ d3d11Device, SlimDX::Direct3D11::DeviceContext^ d3d11DeviceContext, PrimitiveDictionary^ parameters )
{
    mSegmenter = new Native::Segmenter(
        reinterpret_cast< ID3D11Device* >( d3d11Device->ComPointer.ToPointer() ),
        reinterpret_cast< ID3D11DeviceContext* >( d3d11DeviceContext->ComPointer.ToPointer() ),
        parameters->ToCore() );

    VolumeDescription = gcnew Mojo::Interop::VolumeDescription( mSegmenter->GetSegmenterState()->volumeDescription );
    D3D11CudaTextures = gcnew Dictionary< ShaderResourceView^ >;

    MOJO_FOR_EACH_KEY_VALUE( std::string key, Core::ID3D11CudaTexture* d3d11CudaTexture, mSegmenter->GetSegmenterState()->d3d11CudaTextures.GetDictionary() )
    {
        RELEASE_ASSERT( d3d11CudaTexture->GetD3D11ShaderResourceView() != NULL );

        String^ string = msclr::interop::marshal_as< String^ >( key );
        ShaderResourceView^ shaderResourceView = ShaderResourceView::FromPointer( IntPtr( d3d11CudaTexture->GetD3D11ShaderResourceView() ) );

        D3D11CudaTextures->Set( string, shaderResourceView );
    }

    ConvergenceGap                  = mSegmenter->GetSegmenterState()->convergenceGap;
    ConvergenceGapDelta             = mSegmenter->GetSegmenterState()->convergenceGapDelta;
    MaxForegroundCostDelta          = mSegmenter->GetSegmenterState()->maxForegroundCostDelta;
}

Segmenter::~Segmenter()
{
    delete mSegmenter;
}

void Segmenter::LoadVolume( Dictionary< Mojo::Interop::VolumeDescription^ >^ inVolumeDescriptions )
{
    Core::Dictionary< Core::VolumeDescription > volumeDescriptions;
    MOJO_INTEROP_DICTIONARY_TO_CORE( inVolumeDescriptions, Mojo::Interop::VolumeDescription^, volumeDescriptions );

    mSegmenter->LoadVolume( volumeDescriptions );

    delete D3D11CudaTextures;
    delete VolumeDescription;

    VolumeDescription = gcnew Mojo::Interop::VolumeDescription( mSegmenter->GetSegmenterState()->volumeDescription );
    D3D11CudaTextures = gcnew Dictionary< ShaderResourceView^ >;

    MOJO_FOR_EACH_KEY_VALUE( std::string key, Core::ID3D11CudaTexture* d3d11CudaTexture, mSegmenter->GetSegmenterState()->d3d11CudaTextures.GetDictionary() )
    {
        RELEASE_ASSERT( d3d11CudaTexture->GetD3D11ShaderResourceView() != NULL );

        String^ string = msclr::interop::marshal_as< String^ >( key );
        ShaderResourceView^ shaderResourceView = ShaderResourceView::FromPointer( IntPtr( d3d11CudaTexture->GetD3D11ShaderResourceView() ) );

        D3D11CudaTextures->Set( string, shaderResourceView );
    }

    ConvergenceGap                  = mSegmenter->GetSegmenterState()->convergenceGap;
    ConvergenceGapDelta             = mSegmenter->GetSegmenterState()->convergenceGapDelta;
    MaxForegroundCostDelta          = mSegmenter->GetSegmenterState()->maxForegroundCostDelta;
}

void Segmenter::UnloadVolume()
{
    mSegmenter->UnloadVolume();

    for each ( System::Collections::Generic::KeyValuePair< String^, ShaderResourceView^ > keyValuePair in D3D11CudaTextures )
    {
        delete keyValuePair.Value;
    }

    delete D3D11CudaTextures;
    delete VolumeDescription;

    VolumeDescription = gcnew Mojo::Interop::VolumeDescription( mSegmenter->GetSegmenterState()->volumeDescription );
    D3D11CudaTextures = gcnew Dictionary< ShaderResourceView^ >;

    MOJO_FOR_EACH_KEY_VALUE( std::string key, Core::ID3D11CudaTexture* d3d11CudaTexture, mSegmenter->GetSegmenterState()->d3d11CudaTextures.GetDictionary() )
    {
        RELEASE_ASSERT( d3d11CudaTexture->GetD3D11ShaderResourceView() != NULL );

        String^ string = msclr::interop::marshal_as< String^ >( key );
        ShaderResourceView^ shaderResourceView = ShaderResourceView::FromPointer( IntPtr( d3d11CudaTexture->GetD3D11ShaderResourceView() ) );

        D3D11CudaTextures->Set( string, shaderResourceView );
    }

    ConvergenceGap                  = mSegmenter->GetSegmenterState()->convergenceGap;
    ConvergenceGapDelta             = mSegmenter->GetSegmenterState()->convergenceGapDelta;
    MaxForegroundCostDelta          = mSegmenter->GetSegmenterState()->maxForegroundCostDelta;
}

void Segmenter::LoadSegmentation( Dictionary< Mojo::Interop::VolumeDescription^ >^ inVolumeDescriptions )
{
    Core::Dictionary< Core::VolumeDescription > volumeDescriptions;
    MOJO_INTEROP_DICTIONARY_TO_CORE( inVolumeDescriptions, Mojo::Interop::VolumeDescription^, volumeDescriptions );

    mSegmenter->LoadSegmentation( volumeDescriptions );
}

void Segmenter::SaveSegmentationAs( Dictionary< Mojo::Interop::VolumeDescription^ >^ inVolumeDescriptions )
{
    Core::Dictionary< Core::VolumeDescription > volumeDescriptions;
    MOJO_INTEROP_DICTIONARY_TO_CORE( inVolumeDescriptions, Mojo::Interop::VolumeDescription^, volumeDescriptions );

    mSegmenter->SaveSegmentationAs( volumeDescriptions );
}

void Segmenter::InitializeEdgeXYMap( Dictionary< Mojo::Interop::VolumeDescription^ >^ inVolumeDescriptions )
{
    Core::Dictionary< Core::VolumeDescription > volumeDescriptions;
    MOJO_INTEROP_DICTIONARY_TO_CORE( inVolumeDescriptions, Mojo::Interop::VolumeDescription^, volumeDescriptions );

    mSegmenter->InitializeEdgeXYMap( volumeDescriptions );
}

void Segmenter::InitializeEdgeXYMapForSplitting( Dictionary< Mojo::Interop::VolumeDescription^ >^ inVolumeDescriptions, int neuralProcessId )
{
    Core::Dictionary< Core::VolumeDescription > volumeDescriptions;
    MOJO_INTEROP_DICTIONARY_TO_CORE( inVolumeDescriptions, Mojo::Interop::VolumeDescription^, volumeDescriptions );

    mSegmenter->InitializeEdgeXYMapForSplitting( volumeDescriptions, neuralProcessId );
}

void Segmenter::InitializeConstraintMap()
{
    mSegmenter->InitializeConstraintMap();
}

void Segmenter::InitializeConstraintMapFromIdMap( int neuralProcessId )
{
    mSegmenter->InitializeConstraintMapFromIdMap( neuralProcessId );
}

void Segmenter::InitializeConstraintMapFromIdMapForSplitting( int neuralProcessId )
{
    mSegmenter->InitializeConstraintMapFromIdMapForSplitting( neuralProcessId );
}

void Segmenter::InitializeConstraintMapFromPrimalMap()
{
    mSegmenter->InitializeConstraintMapFromPrimalMap();
}

void Segmenter::DilateConstraintMap()
{
    mSegmenter->DilateConstraintMap();
}

void Segmenter::InitializeSegmentation()
{
    mSegmenter->InitializeSegmentation();
}

void Segmenter::InitializeSegmentationAndRemoveFromCommittedSegmentation( int neuralProcessId )
{
    mSegmenter->InitializeSegmentationAndRemoveFromCommittedSegmentation( neuralProcessId );
}

void Segmenter::AddForegroundHardConstraint( Vector3^ p, float radius, HardConstraintMode hardConstraintMode )
{
    int3 pInt3 = make_int3( (int)p->X, (int)p->Y, (int)p->Z );

    mSegmenter->AddForegroundHardConstraint( pInt3, radius, (Native::HardConstraintMode)hardConstraintMode );
}

void Segmenter::AddBackgroundHardConstraint( Vector3^ p, float radius, HardConstraintMode hardConstraintMode )
{
    int3 pInt3 = make_int3( (int)p->X, (int)p->Y, (int)p->Z );

    mSegmenter->AddBackgroundHardConstraint( pInt3, radius, (Native::HardConstraintMode)hardConstraintMode );
}

void Segmenter::AddForegroundHardConstraint( Vector3^ p1, Vector3^ p2, float radius, HardConstraintMode hardConstraintMode )
{
    int3 p1Int3 = make_int3( (int)p1->X, (int)p1->Y, (int)p1->Z );
    int3 p2Int3 = make_int3( (int)p2->X, (int)p2->Y, (int)p2->Z );

    mSegmenter->AddForegroundHardConstraint( p1Int3, p2Int3, radius, (Native::HardConstraintMode)hardConstraintMode );
}

void Segmenter::AddBackgroundHardConstraint( Vector3^ p1, Vector3^ p2, float radius, HardConstraintMode hardConstraintMode )
{
    int3 p1Int3 = make_int3( (int)p1->X, (int)p1->Y, (int)p1->Z );
    int3 p2Int3 = make_int3( (int)p2->X, (int)p2->Y, (int)p2->Z );

    mSegmenter->AddBackgroundHardConstraint( p1Int3, p2Int3, radius, (Native::HardConstraintMode)hardConstraintMode );
}

void Segmenter::Update2D( int numIterations, int zSlice )
{
    mSegmenter->Update2D( numIterations, zSlice );

    ConvergenceGap      = mSegmenter->GetSegmenterState()->convergenceGap;
    ConvergenceGapDelta = mSegmenter->GetSegmenterState()->convergenceGapDelta;
}

void Segmenter::Update3D( int numIterations )
{
    mSegmenter->Update3D( numIterations );

    ConvergenceGap      = mSegmenter->GetSegmenterState()->convergenceGap;
    ConvergenceGapDelta = mSegmenter->GetSegmenterState()->convergenceGapDelta;
}

void Segmenter::VisualUpdate()
{
    mSegmenter->VisualUpdate();
}

void Segmenter::UpdateCommittedSegmentation( int neuralProcessId, Vector3^ neuralProcessColor )
{
    int4 neuralProcessColorInt4 = make_int4( (int)neuralProcessColor->X, (int)neuralProcessColor->Y, (int)neuralProcessColor->Z, 255 );

    mSegmenter->UpdateCommittedSegmentation( neuralProcessId, neuralProcessColorInt4 );
}

void Segmenter::UpdateCommittedSegmentationDoNotRemove( int neuralProcessId, Vector3^ neuralProcessColor )
{
    int4 neuralProcessColorInt4 = make_int4( (int)neuralProcessColor->X, (int)neuralProcessColor->Y, (int)neuralProcessColor->Z, 255 );

    mSegmenter->UpdateCommittedSegmentationDoNotRemove( neuralProcessId, neuralProcessColorInt4 );
}

void Segmenter::ReplaceNeuralProcessInCommittedSegmentation2D( int oldId, int newId, Vector3^ newColor, int slice )
{
    int4 newColorInt4 = make_int4( (int)newColor->X, (int)newColor->Y, (int)newColor->Z, 255 );

    mSegmenter->ReplaceNeuralProcessInCommittedSegmentation2D( oldId, newId, newColorInt4, slice );
}

void Segmenter::ReplaceNeuralProcessInCommittedSegmentation3D( int oldId, int newId, Vector3^ newColor )
{
    int4 newColorInt4 = make_int4( (int)newColor->X, (int)newColor->Y, (int)newColor->Z, 255 );

    mSegmenter->ReplaceNeuralProcessInCommittedSegmentation3D( oldId, newId, newColorInt4 );
}

void Segmenter::ReplaceNeuralProcessInCommittedSegmentation2DConnectedComponentOnly( int oldId, int newId, Vector3^ newColor, int slice, Vector3^ seed )
{
    int4 newColorInt4 = make_int4( (int)newColor->X, (int)newColor->Y, (int)newColor->Z, 255 );
    int2 seedInt2     = make_int2( (int)seed->X,     (int)seed->Y );

    mSegmenter->ReplaceNeuralProcessInCommittedSegmentation2DConnectedComponentOnly( oldId, newId, newColorInt4, slice, seedInt2 );
}

void Segmenter::ReplaceNeuralProcessInCommittedSegmentation3DConnectedComponentOnly( int oldId, int newId, Vector3^ newColor, Vector3^ seed )
{
    int4 newColorInt4 = make_int4( (int)newColor->X, (int)newColor->Y, (int)newColor->Z, 255 );
    int3 seedInt3     = make_int3( (int)seed->X,     (int)seed->Y,     (int)seed->Z );

    mSegmenter->ReplaceNeuralProcessInCommittedSegmentation3DConnectedComponentOnly( oldId, newId, newColorInt4, seedInt3 );
}

void Segmenter::UndoLastChangeToCommittedSegmentation()
{
    mSegmenter->UndoLastChangeToCommittedSegmentation();
}

void Segmenter::RedoLastChangeToCommittedSegmentation()
{
    mSegmenter->RedoLastChangeToCommittedSegmentation();
}

void Segmenter::VisualUpdateColorMap()
{
    mSegmenter->VisualUpdateColorMap();
}

void Segmenter::InitializeCostMap()
{
    mSegmenter->InitializeCostMap();
}

void Segmenter::InitializeCostMapFromPrimalMap()
{
    mSegmenter->InitializeCostMapFromPrimalMap();
}

void Segmenter::IncrementCostMapFromPrimalMapForward()
{
    mSegmenter->IncrementCostMapFromPrimalMapForward();
}

void Segmenter::IncrementCostMapFromPrimalMapBackward()
{
    mSegmenter->IncrementCostMapFromPrimalMapBackward();
}

void Segmenter::FinalizeCostMapFromPrimalMap()
{
    mSegmenter->FinalizeCostMapFromPrimalMap();
}

void Segmenter::UpdateConstraintMapAndPrimalMapFromCostMap()
{
    mSegmenter->GetSegmenterState()->maxForegroundCostDelta = MaxForegroundCostDelta;

    mSegmenter->UpdateConstraintMapAndPrimalMapFromCostMap();
}

int Segmenter::GetNeuralProcessId( Vector3^ p )
{
    int3 p1Int3 = make_int3( (int)p->X, (int)p->Y, (int)p->Z );
    return mSegmenter->GetNeuralProcessId( p1Int3 );
}

Vector3 Segmenter::GetNeuralProcessColor( Vector3^ p )
{
    int3    p1Int3                    = make_int3( (int)p->X, (int)p->Y, (int)p->Z );
    int4    neuralProcessColor        = mSegmenter->GetNeuralProcessColor( p1Int3 );
    Vector3 neuralProcessColorVector3 = Vector3( (float)neuralProcessColor.x, (float)neuralProcessColor.y, (float)neuralProcessColor.z );

    return neuralProcessColorVector3;
}

float Segmenter::GetPrimalValue( Vector3^ p )
{
    int3 p1Int3 = make_int3( (int)p->X, (int)p->Y, (int)p->Z );

    return mSegmenter->GetPrimalValue( p1Int3 );
}

void Segmenter::DumpIntermediateData()
{
    mSegmenter->DumpIntermediateData();
}

void Segmenter::DebugInitialize()
{
    mSegmenter->DebugInitialize();
}

void Segmenter::DebugUpdate()
{
    mSegmenter->DebugUpdate();
}

void Segmenter::DebugTerminate()
{
    mSegmenter->DebugTerminate();
}

}
}