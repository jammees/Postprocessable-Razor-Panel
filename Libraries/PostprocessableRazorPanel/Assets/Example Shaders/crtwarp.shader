MODES
{
    Default();
}

CS
{
	#include "common.fxc"

	// IN
	Texture2D<float4> g_tRaw < Attribute( "RawTexture" ); >;
	float g_fCurvature < Attribute( "Curvature" ); Default( 7.0 ); >;

	// OUT
	RWTexture2D<float4> g_tProcessed < Attribute( "ProcessedTexture" ); >;

	// Used https://www.youtube.com/watch?v=E401x98N6iA
	// Making a CRT Shader Effect - Using Godot Engine
	// MIT License

	// Copyright (c) 2024 DevPoodle

	// Permission is hereby granted, free of charge, to any person obtaining a copy
	// of this software and associated documentation files (the "Software"), to deal
	// in the Software without restriction, including without limitation the rights
	// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	// copies of the Software, and to permit persons to whom the Software is
	// furnished to do so, subject to the following conditions:

	// The above copyright notice and this permission notice shall be included in all
	// copies or substantial portions of the Software.

	// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	// SOFTWARE.
	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
		// Calculate the texture uv
		float2 rawSize = TextureDimensions2D( g_tRaw, 0 );
		float2 uv = id.xy / rawSize;

		float2 centeredUv = uv * 2.0 - 1.0;
		float2 uvOffset = centeredUv.yx / g_fCurvature;
		float2 warpedUv = centeredUv + centeredUv * uvOffset * uvOffset;
		float2 finalUv = (warpedUv + 1.0) / 2.0;

		// Sample the raw texture with the modified uv and
		// write it to the result
		float4 result = g_tRaw.SampleLevel( g_sTrilinearClamp, finalUv, 0 );
		g_tProcessed[id.xy] = result;
	}	
}