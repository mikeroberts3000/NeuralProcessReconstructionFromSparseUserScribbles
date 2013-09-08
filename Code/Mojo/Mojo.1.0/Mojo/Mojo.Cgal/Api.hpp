#pragma once

#ifdef MOJO_CGAL_API_EXPORT
#define MOJO_CGAL_API __declspec( dllexport )
#else
#define MOJO_CGAL_API __declspec( dllimport )
#endif