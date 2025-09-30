MODES
{
    Default();
}

CS
{
	#include "system.fxc"

	// IN
	Texture2D<float4> g_tRaw < Attribute( "RawTexture" ); >;				// ref to the raw texture
	Texture2D<float> g_tNoise < Attribute( "NoiseTexture"; ) >;				// ref to the noise texture

	float g_fStrength < Attribute( "DissolveStrength" ); >;					// how much are we going to dissolve it

	// OUT
	RWTexture2D<float4> g_tProcessed < Attribute( "ProcessedTexture" ); >;	// save the texture here

	// CREDITS:
	// formula was taken and rewritten to work with hlsl
	// https://godotshaders.com/shader/simple-2d-dissolve/
	// source shader under CC0 by godotshaders
	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
		// load in the current raw texture's pixel we're going to
		// work with
		float4 currentPixel = g_tRaw[id.xy];

		// figure out the uv so we can somewhat safely sample
		// the noise texture without it needing to be the same
		// size as the raw texture
		float2 rawSize = TextureDimensions2D( g_tRaw, 0 );
		float2 uv = id.xy / rawSize;

		// sample the noise
		float noise = g_tNoise.SampleLevel( g_sPointClamp, uv, 0 );

		// magic
		currentPixel.a *= floor( g_fStrength + min(1, noise) );

		// save it inside of our processed texture
		g_tProcessed[id.xy] = currentPixel;
	}	
}