Shader "LOS/Greyscale Foreground" {
	SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent+1"
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

            uniform sampler2D _MainTex; 
         	uniform float _intensity;
            
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
            	_intensity = 1;
                return fixed4(i.color.rgb * _intensity, i.color.a);
            }
            ENDCG
        }
    }
}
