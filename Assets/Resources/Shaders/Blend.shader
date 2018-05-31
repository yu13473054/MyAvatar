// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Miyoo/AlphaBlend" {
	Properties {
		_MainTex ("Base", 2D) = "white" {}
		_TintColor ("TintColor", Color) = (1.0, 1.0, 1.0, 0.2)
		//Curved World
		[CurvedWorldLabel] V_CW_Label_UnityDefaults("Curved World Optionals", float) = 0
		[Toggle] V_CW_PARTICLE_SYSTEM ("Use With Particle System", Float) = 0		
	}
	
	CGINCLUDE
		#include "UnityCG.cginc"
		#pragma multi_compile V_CW_PARTICLE_SYSTEM_OFF V_CW_PARTICLE_SYSTEM_ON

		sampler2D _MainTex;
		fixed4 _TintColor;
		half4 _MainTex_ST;
		struct v2f {
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
            fixed4 color : COLOR;
		};

		v2f vert(appdata_full v) {
			v2f o;			
			UNITY_INITIALIZE_OUTPUT(v2f,o); 
			//V_CW_TransformPoint(v.vertex);

			o.pos = UnityObjectToClipPos (v.vertex);	
			o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
            o.color = v.color;
			return o; 
		}
		
		fixed4 frag( v2f i ) : COLOR {	
			return tex2D (_MainTex, i.uv.xy) * _TintColor * i.color;
		}
	
	ENDCG
	
	SubShader {
		Tags { "RenderType" = "Transparent" "Reflection" = "RenderReflectionTransparentBlend" "Queue" = "Transparent"}
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		
	Pass {
	
		CGPROGRAM
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		 
		}
				
	} 
	FallBack Off
}
