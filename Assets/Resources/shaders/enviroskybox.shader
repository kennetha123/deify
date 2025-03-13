Shader "Enviro/Standard/Skybox" {
	Properties {
		_Stars ("Stars Cubemap", Cube) = "black" {}
		_StarsTwinklingNoise ("Stars Noise", Cube) = "black" {}
		_Galaxy ("Galaxy Cubemap", Cube) = "black" {}
		_SatTex ("Satellites Tex", 2D) = "black" {}
		_MoonTex ("Moon Tex", 2D) = "black" {}
		_GlowTex ("Glow Tex", 2D) = "black" {}
		_DitheringTex ("Dithering Tex", 2D) = "black" {}
		_Aurora_Layer_1 ("Aurora Layer 1", 2D) = "black" {}
		_Aurora_Layer_2 ("Aurora Layer 2", 2D) = "black" {}
		_Aurora_Colorshift ("Aurora Color Shift", 2D) = "black" {}
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