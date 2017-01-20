//Required:
//
//1. #define OUTPUT_DEPTH_TO_ALPHA
//
//2. #pragma vertex:depth_vert
//
//3. Input:
//		#ifdef OUTPUT_DEPTH_TO_ALPHA
//		half depth : TEXCOORD6;
//		#endif
//
//4. Surf:
//		#ifdef OUTPUT_DEPTH_TO_ALPHA
//		OUT.Alpha = IN.depth;
//		#endif
//
//5. Tags:
//		"OUTPUT_DEPTH_TO_ALPHA" = "True"
//
//6. #include "DepthAlpha.cginc"		(after input)

#ifndef DEPTH_ALPHA_CGINC
#define DEPTH_ALPHA_CGINC

half _OneOverDepthFar;
half _OneOverDepthScale;

#include "UnityCG.cginc"

#ifndef DEPTH_VERT_DEFINED
void depth_vert (inout appdata_full v, out Input o) {
	UNITY_INITIALIZE_OUTPUT(Input, o);
	#ifdef OUTPUT_DEPTH_TO_ALPHA
//	o.depth = (mul(UNITY_MATRIX_MVP, v.vertex) ).z * _OneOverDepthFar * _OneOverDepthScale;
	o.depth = COMPUTE_DEPTH_01 * _OneOverDepthScale;
//	#define COMPUTE_DEPTH_01 -(mul( UNITY_MATRIX_MV, v.vertex ).z * _ProjectionParams.w)
//	o.depth = Linear01Depth( mul(UNITY_MATRIX_MVP, v.vertex).z ) ;
	#endif
}
#endif

#endif