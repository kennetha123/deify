Shader "Enviro/Standard/EnviroFogRenderingSimple" {
	Properties {
		_EnviroVolumeLightingTex ("Volume Lighting Tex", any) = "" {}
		_Source ("Source", 2D) = "black" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
}