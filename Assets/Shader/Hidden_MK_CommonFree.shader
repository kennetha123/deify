Shader "Hidden/MK/CommonFree" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,0.1)
		_MainTex ("Color (RGB)", 2D) = "white" {}
		_MainTint ("Main Tint", Range(0, 2)) = 0
		[Toggle] _AlbedoMap ("Color source map", Float) = 0
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_Distortion ("Distortion", Range(0, 1)) = 0.3
		_Shininess ("Shininess", Range(0.01, 1)) = 0.275
		_SpecColor ("Specular Color", Vector) = (1,1,1,0.5)
		_SpecularIntensity ("Intensity", Range(0, 2)) = 0.5
		_EmissionColor ("Emission Color", Vector) = (0,0,0,1)
		_RimColor ("Rim Color", Vector) = (1,1,1,1)
		_RimSize ("Rim Size", Range(0, 5)) = 2.3
		_RimIntensity ("Intensity", Range(0, 1)) = 0.3
		[HideInInspector] _MKEditorShowMainBehavior ("Main Behavior", Float) = 1
		[HideInInspector] _MKEditorShowRenderBehavior ("Render Behavior", Float) = 0
		[HideInInspector] _MKEditorShowSpecularBehavior ("Specular Behavior", Float) = 0
		[HideInInspector] _MKEditorShowRimBehavior ("Rim Behavior", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}