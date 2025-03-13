Shader "KriptoFX/RFX1/Decal" {
	Properties {
		[Header(Main Settings)] [Space] [PerRendererData] [HDR] _TintColor ("Tint Color", Vector) = (1,1,1,1)
		_MainTex ("Main Texture", 2D) = "white" {}
		[Toggle(USE_ALPHA_POW)] _UseAlphaPow ("Use Alpha Pow", Float) = 0
		_AlphaPow ("Alpha pow", Float) = 1
		[Space] [Header(Light)] [Space] [Header(Noise Distortion)] [Toggle(USE_NOISE_DISTORTION)] _UseNoiseDistortion ("Use Noise Distortion", Float) = 0
		_NoiseTex ("Noise Texture (RG)", 2D) = "gray" {}
		_DistortSpeed ("Distort Speed", Float) = 1
		_DistortScale ("Distort Scale", Float) = 0.1
		[Space] [Header(Cutout)] [Toggle(USE_CUTOUT)] _UseCutout ("Use Cutout", Float) = 0
		[PerRendererData] _Cutout ("Cutout", Range(0, 1)) = 1
		_CutoutAlphaMul ("Alpha multiplier", Float) = 1
		[Toggle(USE_CUTOUT_TEX)] _UseCutoutTex ("Use Cutout Texture", Float) = 0
		_CutoutTex ("Cutout Tex", 2D) = "white" {}
		[Toggle(USE_CUTOUT_THRESHOLD)] _UseCutoutThreshold ("Use Cutout Threshold", Float) = 0
		[HDR] _CutoutColor ("Cutout Color", Vector) = (1,1,1,1)
		[Space] [Header(Rendering)] [Toggle(USE_WORLD_SPACE_UV)] _UseWorldSpaceUV ("Use World Space UV", Float) = 0
		[Toggle(USE_FRAME_BLENDING)] _UseFrameBlending ("Use Frame Blending", Float) = 0
		[KeywordEnum(Add, Blend, Mul)] _BlendMode ("Blend Mode", Float) = 1
		_SrcMode ("SrcMode", Float) = 5
		_DstMode ("DstMode", Float) = 10
		_ZTest1 ("_ZTest1", Float) = 5
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
	//CustomEditor "RFX1_UberDecalGUI"
}