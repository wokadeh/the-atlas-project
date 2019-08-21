Shader "UCLA Game Lab/Wireframe/Single-Sided" 
{
	Properties 
	{
		_Color ("Line Color", Color) = (1,1,1,1)
		_MainTex ("Main Texture", 2D) = "white" {}
		_Thickness ("Thickness", Float) = 1
		_Radius ("Radius", Range(0.001, 500)) = 10
	}

	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Transparent" "Queue"="Transparent" }

			Blend SrcAlpha OneMinusSrcAlpha 
			ZWrite Off
			LOD 200
			
			CGPROGRAM
				#pragma target 5.0
				#include "UnityCG.cginc"
				#include "UCLA GameLab Wireframe Functions.cginc"
				#pragma vertex vert
				#pragma fragment frag
				#pragma geometry geom

				// Vertex Shader
				UCLAGL_v2g vert(appdata_base v)
				{
					return UCLAGL_vert(v);
				}

				
				// Geometry Shader
				[maxvertexcount(3)]
				void geom(triangle UCLAGL_v2g p[3], inout TriangleStream<UCLAGL_g2f> triStream)
				{
					UCLAGL_geom( p, triStream);
				}

				float _Radius;
				
				// Fragment Shader
				float4 frag(UCLAGL_g2f input) : SV_Target
				{	
					return UCLAGL_frag(input, _Radius);
				}
			
			ENDCG
		}
	} 
}
