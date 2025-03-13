Shader "KriptoFX/RFX1/Distortion" {
	Properties {
		[Header(Main Settings)] [Toggle(USE_MAINTEX)] _UseMainTex ("Use Main Texture", Float) = 0
		[HDR] _TintColor ("Tint Color", Vector) = (1,1,1,1)
		_MainTex ("Main Texture", 2D) = "black" {}
		[Header(Main Settings)] [Normal] _NormalTex ("Normal(RG) Alpha(A)", 2D) = "bump" {}
		[HDR] _MainColor ("Main Color", Vector) = (1,1,1,1)
		_Distortion ("Distortion", Float) = 100
		[Toggle(USE_REFRACTIVE)] _UseRefractive ("Use Refractive Distort", Float) = 0
		_RefractiveStrength ("Refractive Strength", Range(-1, 1)) = 0
		[Toggle(USE_SOFT_PARTICLES)] _UseSoft ("Use Soft Particles", Float) = 0
		_InvFade ("Soft Particles Factor", Float) = 3
		[Space] [Header(Height Settings)] [Toggle(USE_HEIGHT)] _UseHeight ("Use Height Map", Float) = 0
		_HeightTex ("Height Tex", 2D) = "white" {}
		_Height ("_Height", Float) = 0.1
		_HeightUVScrollDistort ("Height UV Scroll(XY)", Vector) = (8,12,0,0)
		[Space] [Header(Fresnel)] [Toggle(USE_FRESNEL)] _UseFresnel ("Use Fresnel", Float) = 0
		[HDR] _FresnelColor ("Fresnel Color", Vector) = (0.5,0.5,0.5,1)
		_FresnelPow ("Fresnel Pow", Float) = 5
		_FresnelR0 ("Fresnel R0", Float) = 0.04
		_FresnelDistort ("Fresnel Distort", Float) = 1500
		[Space] [Header(Cutout)] [Toggle(USE_CUTOUT)] _UseCutout ("Use Cutout", Float) = 0
		_CutoutTex ("Cutout Tex", 2D) = "white" {}
		_Cutout ("Cutout", Range(0, 1)) = 1
		[HDR] _CutoutColor ("Cutout Color", Vector) = (1,1,1,1)
		_CutoutThreshold ("Cutout Threshold", Range(0, 1)) = 0.015
		[Space] [Header(Rendering)] [Toggle] _ZWriteMode ("ZWrite On?", Float) = 0
		[Enum(Off,0,Front,1,Back,2)] _CullMode ("Culling", Float) = 2
		[Toggle(USE_ALPHA_CLIPING)] _UseAlphaCliping ("Use Alpha Cliping", Float) = 0
		_AlphaClip ("Alpha Clip Threshold", Float) = 10
		[Toggle(USE_BLENDING)] _UseBlending ("Use Blending", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	//CustomEditor "CustomShaderGUI"
}