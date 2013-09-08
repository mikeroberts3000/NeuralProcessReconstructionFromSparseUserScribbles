#define BLACK      float4( 0.0f, 0.0f, 0.0f, 1.0f )
#define WHITE      float4( 1.0f, 1.0f, 1.0f, 1.0f )
#define ERROR_PINK float4( 1.0f, 0.5f, 1.0f, 1.0f )

#define EQUALS( x, y, e )          ( abs( x - y ) < e )

Texture2D gCurrentTexture2D;
Texture3D gCurrentTexture3D;
Texture3D gSourceMapTexture3D;
Texture3D gColorMapTexture3D;
Texture3D gPrimalMapTexture3D;
Texture3D gConstraintMapTexture3D;

float     gPrimalMapThreshold;

float4    gSplitNeuralProcessColor;
float4    gCurrentNeuralProcessColor;
float     gCurrentSliceCoordinate;
int       gCurrentTextureIndex;
int       gCurrentTextureDimensions;
int       gRecordingMode;
int       gSplitMode;

sampler gTextureSampler = 
sampler_state
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;    
};

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
    
    output.position = input.position;
    output.texCoord = input.texCoord;
    
    return output;
}

float4 PS( PS_IN input ) : SV_Target
{
    if ( gCurrentTextureDimensions == 2 )
    {
        float4 returnColor = float4( gCurrentTexture2D.Sample( gTextureSampler, input.texCoord.xy ).xyz, 1.0f );
        return returnColor;
    }
    else
    if ( gCurrentTextureDimensions == 3 )
    {
        if ( gCurrentTextureIndex == -1 )
        {
            float4 sourceMap   = float4( gSourceMapTexture3D.Sample(     gTextureSampler, float3( input.texCoord.xy, gCurrentSliceCoordinate ) ).xyz, 1.0f );
            float4 returnColor = float4( sourceMap.x, sourceMap.x, sourceMap.x, 1.0f );

            return returnColor;
        }
        if ( gCurrentTextureIndex == 0 )
        {
            //
            // ColorMap
            //
            float4 sourceMap     = float4( gSourceMapTexture3D.Sample(     gTextureSampler, float3( input.texCoord.xy, gCurrentSliceCoordinate ) ).xyz, 1.0f );
            float4 colorMap      = float4( gColorMapTexture3D.Sample(      gTextureSampler, float3( input.texCoord.xy, gCurrentSliceCoordinate ) ).xyz, 1.0f );
            float4 primalMap     = float4( gPrimalMapTexture3D.Sample(     gTextureSampler, float3( input.texCoord.xy, gCurrentSliceCoordinate ) ).xyz, 1.0f );
            float4 constraintMap = float4( gConstraintMapTexture3D.Sample( gTextureSampler, float3( input.texCoord.xy, gCurrentSliceCoordinate ) ).xyz, 1.0f );

            sourceMap = float4( sourceMap.x, sourceMap.x, sourceMap.x, 1.0f );

            float4 returnColor = sourceMap;

			bool tintColorDueToSegmentation  = false;
			bool tintColorDueToConstraint    = false;
			bool overlayColorDueToConstraint = false;
			bool tintColorDueToSplit         = false;

			float4 segmentationTintColor  = BLACK;
			float4 constraintTintColor    = BLACK;
			float4 constraintOverlayColor = BLACK;
			float4 splitTintColor         = BLACK;

            if ( primalMap.x > gPrimalMapThreshold )
            {
				tintColorDueToSegmentation = true;
				segmentationTintColor      = gCurrentNeuralProcessColor;
            }

            if ( colorMap.x > 0.0f || colorMap.y > 0.0f || colorMap.z > 0.0f )
            {
				if ( gSplitMode )
				{
					if ( !EQUALS( colorMap.x, gSplitNeuralProcessColor.x, 0.05f ) ||
						 !EQUALS( colorMap.y, gSplitNeuralProcessColor.y, 0.05f ) ||
						 !EQUALS( colorMap.z, gSplitNeuralProcessColor.z, 0.05f ) )
					{
						tintColorDueToSegmentation = true;
						segmentationTintColor      = colorMap;						
					}
				}
				else
				{
					tintColorDueToSegmentation = true;
					segmentationTintColor      = colorMap;
				}
            }
			else
			{
				if ( gSplitMode )
				{
					tintColorDueToSplit = true;
					splitTintColor = BLACK;
				}
			}

            if ( gRecordingMode == 0 || gRecordingMode == 1 )
            {
				//
				// not recording, or recording with constraints visible
				//
                if ( constraintMap.x <= -1000.0f )
                {
					overlayColorDueToConstraint = true;
					constraintOverlayColor      = gCurrentNeuralProcessColor;
                }
                else
                if ( constraintMap.x >= 1000.0f )
                {
					tintColorDueToConstraint = true;
					constraintTintColor      = BLACK;
                }
            }
            else
            if ( gRecordingMode == 2 )
            {
				//
				// recording with constraints invisible
				//
                if ( constraintMap.x == -99999.0f )
                {
					tintColorDueToConstraint = true;
					constraintTintColor      = gCurrentNeuralProcessColor;
                }
				else
                if ( constraintMap.x <= -1000.0f )
                {
					overlayColorDueToConstraint = true;
					constraintOverlayColor      = gCurrentNeuralProcessColor;
                }
                else
                if ( constraintMap.x >= 1000.0f )
                {
					tintColorDueToConstraint = true;
					constraintTintColor      = BLACK;
                }
            }

			if ( tintColorDueToSegmentation )
			{
				returnColor = ( 0.65f * returnColor ) + ( 0.35f * segmentationTintColor );
			}

			if ( tintColorDueToConstraint )
			{
				returnColor = ( 0.65f * returnColor ) + ( 0.35f * constraintTintColor );
			}

			if ( overlayColorDueToConstraint )
			{
				returnColor = ( 0.15f * returnColor ) + ( 0.85f * constraintOverlayColor );
			}

			if ( tintColorDueToSplit )
			{
				returnColor = ( 0.65f * returnColor ) + ( 0.35f * splitTintColor );
			}

            returnColor.w = 1.0f;

            return returnColor;
        }
        else
        if ( gCurrentTextureIndex == 1 )
        {
            //
            // ConstraintMap
            //
            float4 constraintMap = float4( gCurrentTexture3D.Sample( gTextureSampler, float3( input.texCoord.xy, gCurrentSliceCoordinate ) ).xyz, 1.0f );
            float4 returnColor   = float4( 0.0f, 0.0f, 0.0f, 1 );

            if ( constraintMap.x < 0.0f )
                returnColor = float4( - constraintMap.x / 100000.0f, 0.0f, 0.0f, 1.0f );
            else if ( constraintMap.x > 0.0f )
                returnColor = float4( 0, 0, constraintMap.x / 100000, 1.0f );

            return returnColor;
        }
        else
        if ( gCurrentTextureIndex == 2 )
        {
            //
            // SourceMap
            //
            float4 sourceMap   = float4( gCurrentTexture3D.Sample( gTextureSampler, float3( input.texCoord.xy, gCurrentSliceCoordinate ) ).xyz, 1.0f );
            float4 primalMap   = float4( gPrimalMapTexture3D.Sample( gTextureSampler, float3( input.texCoord.xy, gCurrentSliceCoordinate ) ).xyz, 1.0f );

            sourceMap = float4( sourceMap.x, sourceMap.x, sourceMap.x, 1.0f );

            float4 returnColor = sourceMap;

            if ( primalMap.x > gPrimalMapThreshold )
            {
                returnColor.x = 1.0f;
            }

            return returnColor;
        }
        else
        if ( gCurrentTextureIndex == 3 )
        {
            //
            // CorrespondenceMap
            //
            float4 correspondenceMap = float4( gCurrentTexture2D.Sample( gTextureSampler, input.texCoord.xy ).xyz, 1.0f );

            return correspondenceMap;
		}
        else
        {
            return ERROR_PINK;
        }
    }
    else
    {
        return ERROR_PINK;
    }
}

RasterizerState gRasterizerState
{
    CullMode = NONE;
};

DepthStencilState gDepthStencilState
{
    DepthEnable = false;
};

technique11 Segmenter2D
{
    pass Segmenter2D
    {
        SetDepthStencilState( gDepthStencilState, 0x00000000 );
        SetRasterizerState( gRasterizerState );
        SetGeometryShader( 0 );
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetPixelShader( CompileShader( ps_5_0, PS() ) );
    }
}