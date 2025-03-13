Shader "Aura 2/Sprites/Sprite" {
	Properties {
		_MainTex ("Sprite", 2D) = "white" {}
		[Toggle(_TWOSIDEDILLUMINATION_ON)] _TWOSIDEDILLUMINATION ("TWO SIDED ILLUMINATION", Float) = 1
		[Toggle(_FLIPFACE_ON)] _FLIPFACE ("FLIP FACE", Float) = 0
		_Cutoff ("Mask Clip Value", Float) = 0.5
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
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}