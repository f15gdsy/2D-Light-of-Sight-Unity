Shader "LOS/Greyscale Background" {
 
    Properties {
        _MainTex ("Texture", 2D) = ""
    }
   
    SubShader {
        
        Tags { "Queue"="Transparent" }
	    ZWrite Off
	        
        Pass {
	        Blend SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
//	        Blend One One
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct v2f {
                float4 position : SV_POSITION;
                float2 uv_mainTex : TEXCOORD;
            };
                      
            uniform float4 _MainTex_ST;
            v2f vert (float4 position : POSITION, float2 uv : TEXCOORD0) {
                v2f o;
                o.position = mul (UNITY_MATRIX_MVP, position);
                o.uv_mainTex = uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
            }
           
            uniform sampler2D _MainTex;
            fixed4 frag(float2 uv_mainTex : TEXCOORD) : COLOR {
                fixed4 mainTex = tex2D (_MainTex, uv_mainTex);
                fixed4 fragColor = mainTex;
                return fragColor;
            }
            ENDCG
        }
        
        Pass {
        	Blend Zero One, One One
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct v2f {
                float4 position : SV_POSITION;
                float2 uv_mainTex : TEXCOORD;
            };
                      
            uniform float4 _MainTex_ST;
            v2f vert (float4 position : POSITION, float2 uv : TEXCOORD0) {
                v2f o;
                o.position = mul (UNITY_MATRIX_MVP, position);
                o.uv_mainTex = uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
            }
           
            uniform sampler2D _MainTex;
            fixed4 frag(float2 uv_mainTex : TEXCOORD) : COLOR {
                fixed4 mainTex = tex2D (_MainTex, uv_mainTex);
                float mainTexAlpha = (mainTex.r + mainTex.g + mainTex.b) / 3;
                fixed4 fragColor = fixed4(0, 0, 0, mainTexAlpha);
                return fragColor;
            }
            ENDCG
        }
    }
}