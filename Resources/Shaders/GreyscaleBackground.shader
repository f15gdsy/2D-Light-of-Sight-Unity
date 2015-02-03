Shader "LOS/Greyscale Background" {
 
    Properties {
        [HideInInspector]_MainTex ("Texture", 2D) = ""
    }
   
    SubShader {
        
        Tags { "Queue"="Transparent" }
	    ZWrite Off
	        
        Pass {
	        Blend SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct v2f {
                float4 position : SV_POSITION;
                float2 uv_mainTex : TEXCOORD;
                float4 c : COLOR;
            };
                      
            uniform float4 _MainTex_ST;
            v2f vert (float4 position : POSITION, float2 uv : TEXCOORD0, float4 c : COLOR) {
                v2f o;
                o.position = mul (UNITY_MATRIX_MVP, position);
                o.uv_mainTex = uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.c = c;
                return o;
            }
           
            uniform sampler2D _MainTex;
            fixed4 frag(v2f i) : COLOR {
                fixed4 mainTex = tex2D (_MainTex, i.uv_mainTex);
                fixed4 fragColor = mainTex * i.c;
                return fragColor;
            }
            ENDCG
        }
        
        Pass {
        	Blend Zero One, One Zero
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct v2f {
                float4 position : SV_POSITION;
                float2 uv_mainTex : TEXCOORD;
                float4 c : COLOR;
            };
                      
            uniform float4 _MainTex_ST;
            v2f vert (float4 position : POSITION, float2 uv : TEXCOORD0, float4 c : COLOR) {
                v2f o;
                o.position = mul (UNITY_MATRIX_MVP, position);
                o.uv_mainTex = uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.c = c;
                return o;
            }
           
            uniform sampler2D _MainTex;
            fixed4 frag(v2f i) : COLOR {
                fixed4 mainTex = tex2D (_MainTex, i.uv_mainTex) * i.c;
                float mainTexAlpha = (mainTex.r + mainTex.g + mainTex.b) / 3;
                fixed4 fragColor = fixed4(0, 0, 0, mainTexAlpha);
                return fragColor;
            }
            ENDCG
        }
    }
}