#include "Breadcrumber.hpp"

#include "Mojo.Core/Printf.hpp"

#include "Mojo.Cgal/Triangulate.hpp"

namespace Mojo
{
namespace Native
{

Breadcrumber::Breadcrumber()
{
}


Breadcrumber::~Breadcrumber(void)
{
}

Core::BreadcrumberState* Breadcrumber::GetBreadcrumberState()
{
    return &mBreadcrumberState;
}

void Breadcrumber::LoadDataset( Core::DatasetDescription datasetDescription )
{
    mBreadcrumberState.datasetDescription = datasetDescription;

    Cgal::TriangulateNeuralProcesss( &mBreadcrumberState );
}

void Breadcrumber::UnloadDataset()
{
    mBreadcrumberState.datasetDescription = Core::DatasetDescription();
}

}
}