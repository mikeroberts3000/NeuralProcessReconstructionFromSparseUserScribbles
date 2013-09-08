Texture3D gSourceMapTexture3D;

sampler gTextureSampler = 
sampler_state
{
    Filter = MIN_MAG_LINEAR_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;
};

float4x4 gPositionTransform;
float4x4 gTexCoordTransform;

struct VS_IN
{
    float4 position : POSITION;
    float4 texCoord : TEXCOORD0;
};

struct PS_IN
{
    float4 position : SV_POSITION;
    float4 texCoord : TEXCOORD0;
};

PS_IN VS( VS_IN input )
{
    PS_IN output = (PS_IN)0;
    
    output.position = mul( input.position, gPositionTransform );
    output.texCoord = mul( input.texCoord, gTexCoordTransform );
    
    return output;
}

float4 PS( PS_IN input ) : SV_Target
{
    float4 sourceMap = float4( gSourceMapTexture3D.Sample( gTextureSampler, input.texCoord.xyz ).xyz, 1.0f );
    float4 returnColor = float4( sourceMap.x, sourceMap.x, sourceMap.x, 1.0f );
    return returnColor;
}

RasterizerState gRasterizerState
{
    CullMode = None;
};

DepthStencilState gDepthStencilState
{
    DepthEnable = true;
    DepthWriteMask = All;

    StencilEnable = false;
};

technique11 T0
{
    pass P0
    {
        SetDepthStencilState( gDepthStencilState, 0x00000000 );
        SetRasterizerState( gRasterizerState );
        SetGeometryShader( 0 );
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetPixelShader( CompileShader( ps_5_0, PS() ) );
    }
}