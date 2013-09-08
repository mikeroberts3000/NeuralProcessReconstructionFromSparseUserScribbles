#pragma once

#include "Mojo.Native/Breadcrumber.hpp"

#include "DatasetDescription.hpp"

namespace Mojo
{
namespace Interop
{

#pragma managed
public ref class Breadcrumber
{
public:
    Breadcrumber();
    ~Breadcrumber();

    void LoadDataset( Mojo::Interop::DatasetDescription^ datasetDescription );
    void UnloadDataset();

    property DatasetDescription^                                          DatasetDescription;
    property Dictionary< System::Collections::Generic::IList< Edge^ >^ >^ DelaunyEdges;

private:
    Native::Breadcrumber* mBreadcrumber;
};

}
}