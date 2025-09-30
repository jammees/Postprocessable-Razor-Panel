MODES
{
    Default();
}

CS
{
	#include "common.fxc"

	// IN
	Texture2D<float4> g_tRaw < Attribute( "RawTexture" ); >;

	// OUT
	RWTexture2D<float4> g_tProcessed < Attribute( "ProcessedTexture" ); >;

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
		// Calculate the texture uv
		float2 rawSize = TextureDimensions2D( g_tRaw, 0 );
		float2 uv = id.xy / rawSize;

		// Apply a sin to the y component of the uv, while
		// advancing it by g_flTime. The uv.x is there to
		// make sure the sin wave is the same for a vertical line
		uv.y += sin( g_flTime * 2 + ( 0.1 + uv.x ) * 20 ) * 0.01;

		// Sample the raw texture with the modified uv and
		// write it to the result
		float4 result = g_tRaw.SampleLevel( g_sTrilinearWrap, uv, 0 );
		g_tProcessed[id.xy] = result;
	}	
}