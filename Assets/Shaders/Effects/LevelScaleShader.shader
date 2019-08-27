Shader "Unlit/LevelScaleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Levels ("Levels", int) = 37
		_MaxPressure ("Atmospheric pressure maximum", Float) = 1000
		_LogMaxPressure("Natural Log of Max Pressure Ln(MaxOressure)", Float) = 6.907755279
		_LineThickness("Thickness of lines", Float) = 0.001
    }
    SubShader
    {
		Blend SrcAlpha OneMinusSrcAlpha
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZTest LEqual
			ZWrite Off
			Fog { Mode off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Levels;
			float _MaxPressure;
			float _LogMaxPressure;
			float _LineThickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f input) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, input.uv);
				col.a = 0;

				// draw the level lines
				for(int i = 0; i < _Levels; i++)
				{
					float linIdx = (float) i / _Levels;
					float logIdx = ( log( linIdx * _MaxPressure ) / _LogMaxPressure );

					// if is bad
					if(input.uv.x <= ( logIdx + _LineThickness ) && input.uv.x >= ( logIdx - _LineThickness ) )
					{
						// white pixel line
						col = fixed4(1,1,1,1);
					}
				}

                return col;
            }
            ENDCG
        }
    }
}
