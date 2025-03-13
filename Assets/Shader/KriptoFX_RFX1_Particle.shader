Shader "KriptoFX/RFX1/Particle" {
	Properties {
		[HDR] _TintColor ("Tint Color", Vector) = (0.5,0.5,0.5,1)
		_MainTex ("Particle Texture", 2D) = "white" {}
		[HideInInspector] _Cutout ("_Cutout", Float) = 0.2
		[HideInInspector] _InvFade ("Soft Particles Factor", Float) = 1
		[HideInInspector] _FresnelStr ("Fresnel Strength", Float) = 1
		[HideInInspector] SrcMode ("SrcMode", Float) = 5
		[HideInInspector] DstMode ("DstMode", Float) = 10
		[HideInInspector] CullMode ("Cull Mode", Float) = 2
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
	//CustomEditor "RFX1_CustomMaterialInspectorParticle"
}