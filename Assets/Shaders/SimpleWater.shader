Shader "Custom/SimpleWater"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", COLOR) = (1,1,1,1)
		_Tint("Tint", COLOR) = (1,1,1,1)
		_EdgeBlend("Edge Blend", float) = 0.0
		_EdgeColor("Edge Color", COLOR) = (1,1,1,1)
		_FlowDirection("Flow direction", Vector) = (0, 0, 0, 0)
		_FlowMap("Flow map", 2D) = "white" {}
	}
		SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent-10" }
		LOD 100

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On
		//Cull Off

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag						

		#include "UnityCG.cginc"
		#include "UnityLightingCommon.cginc"

		struct v2f
		{
			float4 vertex : SV_POSITION;

			float2 uv : TEXCOORD0;
			float4 projPos : TEXCOORD1;
			float2 uvOriginal : TEXCOORD2;
			fixed4 diff : TEXCOORD3;

			float4 color : COLOR;
		};

		sampler2D _MainTex;
		sampler2D _FlowMap;
		float4 _MainTex_ST;
		fixed4 _Color;
		fixed4 _Tint;
		fixed4 _EdgeColor;
		float _EdgeBlend;
		uniform sampler2D _CameraDepthTexture;
		half4 _FlowDirection;


		v2f vert(appdata_full v)
		{
			v2f o;
			float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

			v.vertex.y += sin(worldPos.x * 0.1 + worldPos.z * 0.1 + _Time.z) * 0.01f;
			o.vertex = UnityObjectToClipPos(v.vertex);


			half2 worldUv = (worldPos.xz + worldPos.y * 0.05);
			o.uv = TRANSFORM_TEX(worldUv, _MainTex);
			o.uvOriginal = v.texcoord.xy;

			half3 worldNormal = UnityObjectToWorldNormal(v.normal);
			half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
			o.diff = nl * _LightColor0;
			o.diff.rgb += ShadeSH9(half4(worldNormal, 1));

			o.projPos = ComputeScreenPos(o.vertex);
			o.color = v.color.r * (1 - v.color.g) * (1 - v.color.b);

			return o;
		}

		#define DEBUG_VAL(x) return fixed4(x, x, x, 1);

		fixed4 frag(v2f i) : SV_Target
		{
			half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos));
			depth = LinearEyeDepth(depth);
			half edge = 1 - saturate(_EdgeBlend * (depth - i.projPos.w));

			fixed flow = tex2D(_FlowMap, i.uvOriginal).r;



			float2 flowDir = tex2D(_FlowMap, i.uvOriginal).r * _FlowDirection.xz;


			float phase0 = frac(_Time[1] * 0.5f + 0.5f);
			float phase1 = frac(_Time[1] * 0.5f + 1.0f);

			fixed4 tex0 = tex2D(_MainTex, i.uv + flowDir.xy * phase0);
			fixed4 tex1 = tex2D(_MainTex, i.uv + flowDir.xy * phase1);

			float flowLerp = abs((0.5f - phase0) / 0.5f);
			fixed4 flowColor = lerp(tex0, tex1, flowLerp) * _Tint;



			flow = fmod(_Time.x, 1) * flow;
			fixed4 col = (flowColor + edge * _EdgeColor) * i.diff;
			col.a = pow(col.b, 3) * _Color.r + _Color.g;
			col.a = (1 - edge) * col.a + edge * _EdgeColor.a;


			return col;
		}
		ENDCG
	}
	}
}