Shader "Invector/EmissionWithMask" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,0)
		_Difuse ("Difuse", 2D) = "white" {}
		_Metallic ("Metallic", Float) = 0
		_Smoothness ("Smoothness", Float) = 0
		_Normal ("Normal", 2D) = "bump" {}
		_NormalPower ("NormalPower", Float) = 0
		_EmissionColor ("EmissionColor", Vector) = (0,0,0,0)
		_EmissionMask ("Emission Mask", 2D) = "white" {}
		_Emission ("Emission", 2D) = "white" {}
		_Emissionpower ("Emission power", Float) = 1
		_EmissionTransition ("EmissionTransition", Range(0, 1)) = 100
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[HideInInspector] __dirty ("", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}