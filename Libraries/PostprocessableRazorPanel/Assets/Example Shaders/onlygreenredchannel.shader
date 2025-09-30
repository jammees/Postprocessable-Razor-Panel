MODES
{
    Default();
}

CS
{
	#include "system.fxc"

	// IN
	Texture2D<float4> g_tRaw < Attribute( "RawTexture" ); >;

	// OUT
	RWTexture2D<float4> g_tProcessed < Attribute( "ProcessedTexture" ); >;

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
		// Get pixel at the current id
		float4 currentPixel = g_tRaw[id.xy];

		// Only write the red and green and alpha components of the
		// color
		g_tProcessed[id.xy] = float4(currentPixel.r, currentPixel.g, 0, currentPixel.a);
	}	
}