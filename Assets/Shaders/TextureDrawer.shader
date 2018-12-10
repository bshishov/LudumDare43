Shader "Hidden/TextureDrawer"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_X("X", float) = 0.5
		_Y("Y", float) = 0.5
		_Color("Color", COLOR) = (0, 0, 0, 0)
		_Radius("Radius", float) = 0.1
		_Strength("Strength", float) = 0.5
	}
	SubShader
	{		
		Cull Off 
		ZWrite Off 
		ZTest Always
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag			

			#include "UnityCG.cginc"			

			sampler2D _MainTex;
			float _X;
			float _Y;
			float4 _Color;
			float _Radius;
			float _Strength;

			float4 frag(v2f_img i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
				float d = length(i.uv - float2(_X, _Y));
				float x = d / _Radius;
				float k = saturate(1 - pow(x, _Strength)) * _Color.a;

				return col * (1 - k) + k * _Color;
			}
			ENDCG
		}
	}
}
