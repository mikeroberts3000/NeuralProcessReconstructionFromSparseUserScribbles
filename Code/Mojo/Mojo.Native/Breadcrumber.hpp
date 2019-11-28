#pragma once

#include "Mojo.Core/BreadcrumberState.hpp"
#include "Mojo.Core/DatasetDescription.hpp"

#include "Mojo.Cgal/Triangulate.hpp"

namespace Mojo
{
namespace Native
{

class Breadcrumber
{
public:
    Breadcrumber();
    ~Breadcrumber();

    Core::BreadcrumberState* GetBreadcrumberState();

    void LoadDataset( Core::DatasetDescription datasetDescription );
    void UnloadDataset();

private:
    Core::BreadcrumberState mBreadcrumberState;
};

}
}