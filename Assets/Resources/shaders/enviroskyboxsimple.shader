Shader "Enviro/Lite/SkyboxSimple" {
	Properties {
		_SkyColor ("Sky Color", Vector) = (0,0,0,0)
		_HorizonColor ("Horizon Color", Vector) = (0,0,0,0)
		_SunColor ("Sun Color", Vector) = (0,0,0,0)
		_Stars ("StarsMap", Cube) = "white" {}
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
	Fallback "None"
}