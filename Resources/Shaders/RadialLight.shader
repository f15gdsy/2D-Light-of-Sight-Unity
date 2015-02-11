Shader "LOS/Radial Light" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "radial" {}
	}
	SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent+1"
            "RenderType"="Transparent"
        }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="Always"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            Fog {Mode Off}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma target 3.0

            uniform sampler2D _MainTex; 
         	uniform float _intensity;
            
            struct vIn {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 color : COLOR;
            };
            
            v2f vert (vIn v) {
                v2f o;
                o.uv0 = v.texcoord0;
                o.color = v.color;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                return o;
            }
            
            fixed4 frag(v2f i) : COLOR {
            	_intensity = 1;
                float4 _MainTex_var = tex2D(_MainTex, i.uv0);
                float texRGBAverage = (_MainTex_var.r + _MainTex_var.g + _MainTex_var.b) / 3;
                return fixed4(i.color.rgb * _intensity, texRGBAverage * i.color.a);
            }
            ENDCG
        }
    }
}
