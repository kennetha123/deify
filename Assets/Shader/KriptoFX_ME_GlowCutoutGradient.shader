Shader "KriptoFX/ME/GlowCutoutGradient" {
	Properties {
		[HDR] _TintColor ("Tint Color", Vector) = (0.5,0.5,0.5,1)
		_GradientStrength ("Gradient Strength", Float) = 0.5
		_TimeScale ("Time Scale", Vector) = (1,1,1,1)
		_MainTex ("Noise Texture", 2D) = "white" {}
		_BorderScale ("Border Scale (XY) Offset (Z)", Vector) = (0.5,0.05,1,1)
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
}