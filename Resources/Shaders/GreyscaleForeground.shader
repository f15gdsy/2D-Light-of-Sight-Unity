Shader "LOS/Greyscale Foreground" {
	SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent+2"
            "RenderType"="Transparent"
        }
        Pass {        
            Blend DstAlpha Zero
            ZWrite Off
            Lighting Off
            Fog {Mode Off}
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            struct vIn {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };
            
            struct v2f {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };
            
            v2f vert (vIn v) {
                v2f o;
                o.color = v.color;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                return o;
            }
            
            fixed4 frag(v2f i) : COLOR {
                return i.color;
            }
            ENDCG
        }
    }
}
