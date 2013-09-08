#include "Triangulate.hpp"

#include <list>
#include <hash_map>

#include <CGAL/Simple_cartesian.h>
#include <CGAL/Triangulation_3.h>

#include "Mojo.Core/Assert.hpp"
#include "Mojo.Core/ForEach.hpp"
#include "Mojo.Core/Printf.hpp"
#include "Mojo.Core/NeuralProcessDescription.hpp"

typedef CGAL::Triangulation_3< CGAL::Simple_cartesian< float > > CgalTriangulation;

namespace Mojo
{
namespace Cgal
{

MOJO_CGAL_API void TriangulateNeuralProcesss( Core::BreadcrumberState* breadcrumberState )
{
    stdext::hash_map< std::string, std::list< CgalTriangulation::Point > > breadcrumbPointsForAllTrails;

    MOJO_FOR_EACH_KEY_VALUE( int neuralProcessId, Core::NeuralProcessDescription neuralProcessDescription, breadcrumberState->datasetDescription.neuralProcessDescriptions )
    {
        std::list< CgalTriangulation::Point > breadcrumbPoints;

        MOJO_FOR_EACH( Core::BreadcrumbDescription breadcrumbDescription, neuralProcessDescription.breadcrumbDescriptions )
        {
            breadcrumbPoints.push_front( CgalTriangulation::Point( breadcrumbDescription.position.x, breadcrumbDescription.position.y, breadcrumbDescription.position.z ) );
        }

        breadcrumbPointsForAllTrails[ neuralProcessDescription.name ] = breadcrumbPoints;
    }

    MOJO_FOR_EACH_KEY_VALUE( int neuralProcessId, Core::NeuralProcessDescription neuralProcessDescription, breadcrumberState->datasetDescription.neuralProcessDescriptions )
    {
        CgalTriangulation triangulation( breadcrumbPointsForAllTrails[ neuralProcessDescription.name ].begin(), breadcrumbPointsForAllTrails[ neuralProcessDescription.name ].end() );

        RELEASE_ASSERT( triangulation.is_valid() );

        std::list< std::pair< float3, float3 > > edges;
        for ( CgalTriangulation::Finite_edges_iterator edge = triangulation.finite_edges_begin(); edge != triangulation.finite_edges_end(); edge++ )
        {
            CgalTriangulation::Point pt0 = edge->first->vertex( edge->second )->point();
            CgalTriangulation::Point pt1 = edge->first->vertex( edge->third )->point(); 

            edges.push_back( std::pair< float3, float3 >(
                make_float3( (float)pt0.x(), (float)pt0.y(), (float)pt0.z() ),
                make_float3( (float)pt1.x(), (float)pt1.y(), (float)pt1.z() ) ) );            
        }

        breadcrumberState->delaunyEdges.Set( neuralProcessDescription.name, edges );
    }
}

}
}