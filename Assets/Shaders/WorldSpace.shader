Shader "WorldSpace/Unlit"{
	//show values to edit in inspector
	Properties{
		_Color("Tint", Color) = (0, 0, 0, 1)
		_MainTex("Texture", 2D) = "white" {}
		[PerRendererData]_Rotation("Rotation", Range(0,360.0)) = 0
		_Scale("Scale", float) = 1
	}

		SubShader{
		//the material is completely non-transparent and is rendered at the same time as the other opaque geometry
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

		Blend SrcAlpha OneMinusSrcAlpha

		Pass{
		CGPROGRAM

		//include useful shader functions
#include "UnityCG.cginc"

		//define vertex and fragment shader
#pragma alpha:blend
#pragma vertex vert
#pragma fragment frag

		//texture and transforms of the texture
		sampler2D _MainTex;
	float4 _MainTex_ST;

	//tint of the texture
	fixed4 _Color;
	float _Rotation;
	float _Scale;

	

	//the object data that's put into the vertex shader
	struct appdata {
		float4 vertex : POSITION;
	};

	//the data that's used to generate fragments and can be read by the fragment shader
	struct v2f {
		float4 position : SV_POSITION;
		float3 worldPos : TEXCOORD0;
	};

	//the vertex shader
	v2f vert(appdata v) {
		v2f o;
		//convert the vertex positions from object space to clip space so they can be rendered
		float4x4 rotMat = float4x4(
			cos(radians(_Rotation)), -sin(radians(_Rotation)), 0, 0,
			sin(radians(_Rotation)), cos(radians(_Rotation)), 0, 0,
			0, 0, 1, 0,
			0, 0, 0, 1);
		o.position = UnityObjectToClipPos(v.vertex);
		o.worldPos = mul(rotMat, mul(unity_ObjectToWorld, v.vertex));
		return o;
	}

	//the fragment shader
	fixed4 frag(v2f i) : SV_TARGET{
		float2 textureCoordinate = i.worldPos.xy * _Scale;
		textureCoordinate = TRANSFORM_TEX(textureCoordinate, _MainTex);
		fixed4 col = tex2D(_MainTex, textureCoordinate);
		col *= _Color;
		return col;
	}

		ENDCG
	}
	}
}