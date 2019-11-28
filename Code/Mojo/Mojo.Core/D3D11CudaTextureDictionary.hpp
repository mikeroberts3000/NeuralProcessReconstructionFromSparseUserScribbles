#pragma once

#include <string>
#include <vector>
#include <hash_map>

struct cudaGraphicsResource;

namespace Mojo
{
namespace Core
{

class ID3D11CudaTexture;

class D3D11CudaTextureDictionary
{
public:
    ID3D11CudaTexture*                                   Get( std::string key );
    void                                                 Set( std::string key, ID3D11CudaTexture* value );

    stdext::hash_map< std::string, ID3D11CudaTexture* >& GetDictionary();

    void                                                 MapCudaArrays();
    void                                                 UnmapCudaArrays();

private:
    stdext::hash_map< std::string, ID3D11CudaTexture* > mD3D11CudaTextures;
    std::vector< cudaGraphicsResource* >                mCudaGraphicsResources;
};

}
}