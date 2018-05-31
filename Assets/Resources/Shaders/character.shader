// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

///
///  Reference: http://prideout.net/blog/?p=22
/// 
Shader "Miyoo/Character" 
{
	Properties 
    {
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Main Tex", 2D)  = "white" {}
		_Outline ("Outline", Range(0,10)) = 0.1
		_OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
		
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
        
		//_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
		//_RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
		//_RimMin ("Rim Min", Range(0,1)) = 0.5
		//_RimMax ("Rim Max", Range(0,1)) = 1.0
        
		//Curved World
		[CurvedWorldLabel] V_CW_Label_UnityDefaults("Curved World Optionals", float) = 0
		[Toggle] V_CW_PARTICLE_SYSTEM ("Use With Particle System", Float) = 0			
	}
	SubShader 
    {
		Tags { "RenderType"="Opaque" "Reflection" = "RenderReflectionOpaque" }
		LOD 200
		
		Pass 
        {
			NAME "OUTLINE"
			
			Cull Front
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#pragma multi_compile V_CW_PARTICLE_SYSTEM_OFF V_CW_PARTICLE_SYSTEM_ON
			float _Outline;
			fixed4 _OutlineColor;
			
			struct v2f 
            {
				float4 pos : SV_POSITION;
			};
			
			v2f vert (appdata_base v) 
            {
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o); 
				//V_CW_TransformPoint(v.vertex);	
				float4 pos = mul(UNITY_MATRIX_MV, v.vertex); 
				float3 normal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);  
				normal.z = -0.5;

				//Camera-independent outline size
				//float dist = distance(_WorldSpaceCameraPos, mul(_Object2World, v.vertex));
				//pos = pos + float4(normalize(normal), 0) * _Outline * 0.01 * dist;

				pos = pos + float4(normalize(normal), 0) * _Outline * 0.01;
				o.pos = mul(UNITY_MATRIX_P, pos);
				
				return o;
			}
			
			float4 frag(v2f i) : SV_Target 
            {
				return float4(_OutlineColor.rgb, 1);               
			}
			
			ENDCG
		}
 
		Pass
		{
			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"
                #pragma multi_compile V_CW_PARTICLE_SYSTEM_OFF V_CW_PARTICLE_SYSTEM_ON
				struct v2f
				{
					float4 pos	: SV_POSITION;
					half2 uv 	: TEXCOORD0;
					half2 cap	: TEXCOORD1;
				};
				
				fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				//float4 _RimColor;
				//fixed _RimMin;
				//fixed _RimMax;
				//fixed _RimPower;
				
				v2f vert (appdata_base v)
				{
					v2f o;
					//V_CW_TransformPoint(v.vertex);	
					o.pos = UnityObjectToClipPos (v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					
					float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
					worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
					o.cap.xy = worldNorm.xy * 0.5 + 0.5;
					
					
					/*float3 viewdir = ObjSpaceViewDir(v.vertex);
					half rim = 1.0 - saturate(dot (normalize(viewdir), v.normal));
					rim = smoothstep(_RimMin, _RimMax, rim);
					rim = pow(rim, _RimPower);
					o.cap.z = rim;*/
                    
					return o;
				}
				
				uniform sampler2D _MainTex;
				uniform sampler2D _MatCap;
				
				fixed4 frag (v2f i) : COLOR
				{
					half4 tex = tex2D(_MainTex, i.uv);
					half4 mc = tex2D(_MatCap, i.cap.xy);

					half4 diff = tex * tex.a + tex * (1 - tex.a) * 0.9;
					half4 matcap = (mc * 2.0 - 1.0) * tex.a;
					//half4 rim = half4((_RimColor.rgb * i.cap.z) * _RimColor.a, 1);

					//return diff + matcap + rim;

					return (diff + matcap) * (_TintColor * 2.0f);
				}
			ENDCG
		}
	}  
    
    SubShader 
    {
		Tags { "RenderType"="Opaque" "Reflection" = "RenderReflectionOpaque" }
		LOD 150
				 
		Pass
		{
			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"
				struct v2f
				{
					float4 pos	: SV_POSITION;
					half2 uv 	: TEXCOORD0;
					half2 cap	: TEXCOORD1;
				};
				
				fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				
				v2f vert (appdata_base v)
				{
					v2f o;
					//V_CW_TransformPoint(v.vertex);	
					o.pos = UnityObjectToClipPos (v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					
					float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
					worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
					o.cap.xy = worldNorm.xy * 0.5 + 0.5;
					
					
					return o;
				}
				
				uniform sampler2D _MainTex;
				uniform sampler2D _MatCap;
				
				fixed4 frag (v2f i) : COLOR
				{
					half4 tex = tex2D(_MainTex, i.uv);
					half4 mc = tex2D(_MatCap, i.cap.xy);

					half4 diff = tex * tex.a + tex * (1 - tex.a) * 0.9;
					half4 matcap = (mc * 2.0 - 1.0) * tex.a;

					return (diff + matcap) * (_TintColor * 2.0f);
				}
			ENDCG
		}
	}  
    
    SubShader 
    {
		Tags { "RenderType"="Opaque" "Reflection" = "RenderReflectionOpaque" }
		LOD 100
				 
		Pass
		{
			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"
                //#include "../../VacuumShaders/Curved World/Shaders/cginc/CurvedWorld_Base.cginc"						
				struct v2f
				{
					float4 pos	: SV_POSITION;
					half2 uv 	: TEXCOORD0;
				};
				
				fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				
				v2f vert (appdata_base v)
				{
					v2f o;
					//V_CW_TransformPoint(v.vertex);	
					o.pos = UnityObjectToClipPos (v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					
					return o;
				}
				
				uniform sampler2D _MainTex;
				
				fixed4 frag (v2f i) : COLOR
				{
					half4 tex = tex2D(_MainTex, i.uv);
					half4 diff = tex * tex.a + tex * (1 - tex.a) * 0.9;
					return (diff) * (_TintColor * 2.0f);
				}
			ENDCG
		}
	} 
}
