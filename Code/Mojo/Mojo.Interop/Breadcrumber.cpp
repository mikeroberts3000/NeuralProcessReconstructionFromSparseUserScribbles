#include "Breadcrumber.hpp"

#include <msclr/marshal_cppstd.h>

#include "Mojo.Core/Dictionary.hpp"
#include "Mojo.Core/NeuralProcessDescription.hpp"

#include "Mojo.Native/Breadcrumber.hpp"

using namespace msclr::interop;

namespace Mojo
{
namespace Interop
{

Breadcrumber::Breadcrumber()
{
    mBreadcrumber = new Native::Breadcrumber();
}

Breadcrumber::~Breadcrumber()
{
    delete mBreadcrumber;
}

void Breadcrumber::LoadDataset( Mojo::Interop::DatasetDescription^ datasetDescription )
{
    mBreadcrumber->LoadDataset( datasetDescription->ToCore() );

    DatasetDescription = gcnew Mojo::Interop::DatasetDescription( mBreadcrumber->GetBreadcrumberState()->datasetDescription );
    DelaunyEdges       = gcnew Dictionary< System::Collections::Generic::IList< Edge^ >^ >();

    MOJO_FOR_EACH_KEY_VALUE( std::string neuralProcessName, MOJO_PROTECT_COMMAS( std::list< std::pair< float3, float3 > > edges ), mBreadcrumber->GetBreadcrumberState()->delaunyEdges.GetDictionary() )
    {
        String^        cliNeuralProcessName = marshal_as< System::String^ >( neuralProcessName );
        List< Edge^ >^ cliEdges               = gcnew List< Edge^ >();

        MOJO_FOR_EACH( MOJO_PROTECT_COMMAS( std::pair< float3, float3 > edge ), edges )
        {
            Edge^ cliEdge = gcnew Edge();
            cliEdge->P1 = Vector3( edge.first.x,  edge.first.y,  edge.first.z );
            cliEdge->P2 = Vector3( edge.second.x, edge.second.y, edge.second.z );

            cliEdges->Add( cliEdge );
        }

        DelaunyEdges->Set( cliNeuralProcessName, cliEdges );
    }
}

void Breadcrumber::UnloadDataset()
{
    delete DelaunyEdges;
    delete DatasetDescription;

    mBreadcrumber->UnloadDataset();
}

}
}