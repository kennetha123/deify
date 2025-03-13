Shader "KriptoFX/RFX1/LightningParticles" {
	Properties {
		[HDR] _TintColor ("Tint Color", Vector) = (0.5,0.5,0.5,0.5)
		_MainTex ("Main Texture", 2D) = "white" {}
		_DistortTex1 ("Distort Texture1", 2D) = "white" {}
		_DistortTex2 ("Distort Texture2", 2D) = "white" {}
		_DistortSpeed ("Distort Speed Scale (xy/zw)", Vector) = (1,0.1,1,0.1)
		_Offset ("Offset", Float) = 0
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