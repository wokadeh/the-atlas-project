﻿// shader that performs ray casting using a 3D texture
// adapted from a Cg example by Nvidia
// http://developer.download.nvidia.com/SDK/10/opengl/samples.html
// Gilles Ferrand, University of Manitoba / RIKEN, 2016–2017
// https://github.com/gillesferrand/Unity-RayTracing

Shader "Custom/Spherical Ray Casting" 
{

	Properties
	{
		// the data cube
		[NoScaleOffset] _Data("Data Texture", 3D) = "" {}
	//	_HistTex("Histogram Texture", 2D) = "white" {}
		_TFTex("Transfer Function Texture", 2D) = "white" {}
		_OuterSphereRadius("Outer Sphere Radius", Range(0.4,1)) = 0.5
		_InnerSphereRadius("Inner Sphere Radius", Range(0,0.4)) = 0.4
		_EarthRadius("Earth Radius", Float) = 0.5
		_LongitudeBounds("Longitude Bounds", Vector) = (-180, 180, 0, 0)
		_LatitudeBounds("Latitude Bounds", Vector) = (-90, 90, 0, 0)
		_AltitudeBounds("Altitude Bounds", Vector) = (0, 37, 0, 0)
		_DataChannel("Data Channel", Vector) = (0,0,0,1) // in which channel were the data value stored?
		// data slicing and thresholding (X, Y, Z are user coordinates)
		_SliceAxis1Min("Slice along axis X: min", Range(0,1)) = 0
		_SliceAxis1Max("Slice along axis X: max", Range(0,1)) = 1
		_SliceAxis2Min("Slice along axis Y: min", Range(0,1)) = 0
		_SliceAxis2Max("Slice along axis Y: max", Range(0,1)) = 1
		_SliceAxis3Min("Slice along axis Z: min", Range(0,1)) = 0
		_SliceAxis3Max("Slice along axis Z: max", Range(0,1)) = 1
		_DataMin("Data threshold: min", Range(0,1)) = 0
		_DataMax("Data threshold: max", Range(0,1)) = 1
		_StretchPower("Data stretch power", Range(0.1,3)) = 1  // increase it to highlight the highest data values
		// normalization of data intensity (has to be adjusted for each data set)
		_NormPerStep("Intensity normalization per step", Float) = 1
		_NormPerRay("Intensity normalization per ray" , Float) = 1
		_Steps("Max number of steps", Range(1,2048)) = 2048 // should ideally be as large as data resolution, strongly affects frame rate
		_LogFactor("Atmospheric Log Factor", Float) = 0.1
		_MaxPressure("Atmospheric pressure maximum", Float) = 1000
		_LogMaxPressure("Natural Log of Max Pressure Ln(MaxOressure)", Float) = 6.907755279
	}

		SubShader
		{

			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

			LOD 100

			Blend SrcAlpha OneMinusSrcAlpha

			Pass {
				Blend SrcAlpha OneMinusSrcAlpha
				Cull Off
				ZTest LEqual
				ZWrite Off
				Fog { Mode off }

				CGPROGRAM
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				sampler3D _Data;
				sampler2D _TFTex;

				float _OuterSphereRadius;
				float _InnerSphereRadius;
				float _EarthRadius;

				float2 _LongitudeBounds;
				float2 _LatitudeBounds;
				float2 _AltitudeBounds;
				float4 _DataChannel;

				float _SliceAxis1Min, _SliceAxis1Max;
				float _SliceAxis2Min, _SliceAxis2Max;
				float _SliceAxis3Min, _SliceAxis3Max;

				float _DataMin, _DataMax;

				float _StretchPower;
				float _NormPerStep;
				float _NormPerRay;
				float _Steps;
				float _MaxPressure;
				float _LogFactor;
				float _LogMaxPressure;

				bool IntersectSphere(float3 ray_o, float3 ray_d, float radius, out float tNear, out float tFar)
				{
					float3 origin = ray_o;
					float3 direction = ray_d;

					float b = 2. * (origin.x * direction.x + origin.y * direction.y + origin.z * direction.z);
					float c = origin.x * origin.x + origin.y * origin.y + origin.z * origin.z - radius * radius;

					float d = b * b - 4. * c;

					// we can bail here
					if (d < 0) 
					{
						return false;
					}

					float t0 = (-b - sqrt(d)) / 2.;
					float t1 = (-b + sqrt(d)) / 2.;

					if (t0 < t1) 
					{
						tNear = t0;
						tFar = t1;
					}
					else 
					{
						tNear = t1;
						tFar = t0;
					}

					return true;
				}


				// gets data value at a given position
				float4 GetDataFromTexture(float3 pos)
				{
					// sample texture (pos is normalized in [0,1])
					float3 posTex = pos;

					float z = 1 - pos.z;
					float newLogZ = 1 - (log(z * _MaxPressure) / _LogMaxPressure);

					posTex.z = pos.z;

					float4 data4 = tex3Dlod(_Data, float4(posTex, 0));
					float data = _DataChannel[0] * data4.r + _DataChannel[1] * data4.g + _DataChannel[2] * data4.b + _DataChannel[3] * data4.a;
					// slice and threshold
					data *= step(_SliceAxis1Min, posTex.x);
					data *= step(_SliceAxis2Min, posTex.y);
					data *= step(_SliceAxis3Min, posTex.z);
					data *= step(posTex.x, _SliceAxis1Max);
					data *= step(posTex.y, _SliceAxis2Max);
					data *= step(posTex.z, _SliceAxis3Max);
					data *= step(_DataMin, data);
					data *= step(data, _DataMax);
					// colourize
					float4 col = float4(data, data, data, data);
					return col;
				}

				float4 GetTransferFunction(float density)
				{
					return tex2Dlod(_TFTex, float4(density, 0, 0, 0));
				}

				float3 ConvertToSpherical(float3 pos)
				{
					float radius = _EarthRadius;
					float rho = sqrt(pos.x * pos.x + pos.y * pos.y + pos.z * pos.z);
					float longitude = atan2(pos.x, pos.y);
					float latitude = asin(pos.z / rho);
					float altitude = rho - radius;
					return float3(longitude, latitude, altitude);
				}

				float3 ConvertToTexture(float3 sphericalPos)
				{
					float u = (sphericalPos.x - _LongitudeBounds.x) / (_LongitudeBounds.y - _LongitudeBounds.x);
					float v = (sphericalPos.y - _LatitudeBounds.x) / (_LatitudeBounds.y - _LatitudeBounds.x);
					float s = 1 - ((sphericalPos.z - _AltitudeBounds.x) / (_AltitudeBounds.y - _AltitudeBounds.x));

					return float3(u, v, s);
				}

				struct vert_input 
				{
					float4 pos : POSITION;
				};

				struct frag_input 
				{
					float4 pos : SV_POSITION;
					float3 ray_o : TEXCOORD1; // ray origin
					float3 ray_d : TEXCOORD2; // ray direction
				};

				// ---------------------------------------- VERTEX SHADER ----------------------------------------
				// vertex program
				frag_input vert(vert_input i) 
				{
					frag_input o;

					// calculate eye ray in object space
					o.ray_d = -ObjSpaceViewDir(i.pos);
					o.ray_o = i.pos.xyz - o.ray_d;
					// calculate position on screen (unused)
					o.pos = UnityObjectToClipPos(i.pos);

					return o;
				}
				// ---------------------------------------- !VERTEX SHADER ---------------------------------------

				// ---------------------------------------- FRAGMENT SHADER ----------------------------------------
				// fragment program
				float4 frag(frag_input i) : COLOR 
				{
					i.ray_d = normalize(i.ray_d);

					// calculate eye ray intersection with sphere bounding box
					float tNear, tFar;
					bool hit = IntersectSphere(i.ray_o, i.ray_d, _OuterSphereRadius, tNear, tFar);
					if (!hit) discard;
					if (tNear < 0.0) tNear = 0.0;
					float3 pNear = i.ray_o + i.ray_d * tNear;
					float3 pFar = i.ray_o + i.ray_d * tFar;

					// march along ray inside the cube, accumulating color
					float3 ray_pos = pNear;
					float3 ray_dir = pFar - pNear;

					float3 ray_step = normalize(ray_dir) * sqrt(3) / _Steps;
					float4 ray_col = 0;
					int count = 0;

					[loop]
					for (int k = 0; k < _Steps; k++)
					{
						float3 sphericalPos = ConvertToSpherical(ray_pos);
						float3 tex_coord = ConvertToTexture(sphericalPos);
						float4 voxel_col = GetDataFromTexture(tex_coord);

						float density = voxel_col.r;
						if (density == 0) count = count + 1;
						if (density != 0) count = 0;
						if (count > 20) break;
						float4 tf_col = GetTransferFunction(density);

						tf_col.a = _NormPerStep * length(ray_step) * pow(tf_col.a,_StretchPower);

						ray_col.rgb = ray_col.rgb + (1 - ray_col.a) * tf_col.a * tf_col.rgb;
						ray_col.a = ray_col.a + (1 - ray_col.a) * tf_col.a;

						ray_pos += ray_step;
					}

					ray_col *= _NormPerRay;
					ray_col = clamp(ray_col,0,1);

					return ray_col;
				}
				// ---------------------------------------- !FRAGMENT SHADER ----------------------------------------
				ENDCG
			}
		}

		FallBack Off
}