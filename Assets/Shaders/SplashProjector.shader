Shader "Custom/SplashProjector" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _ShadowTex ("Cookie", 2D) = "white" {}
    }
    Subshader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Pass {
            ZWrite Off
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha
            Offset -1, -1

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata_t {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 uvShadow : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
            };
            
            float4x4 unity_Projector;
            
            v2f vert (appdata_t v) {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                o.uvShadow = mul (unity_Projector, v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            
            sampler2D _ShadowTex;
            fixed4 _Color;
            
            fixed4 frag (v2f i) : SV_Target {
                // Khử hiện tượng "chảy sọc" trên tường:
                // Nếu pháp tuyến của bề mặt (mặt phẳng) không hướng thẳng lên trời (mặt bên hoặc mặt dưới)
                // thì không hiển thị vết bóng này!
                if (i.worldNormal.y < 0.5) discard;

                // Không chiếu ngược ra phía sau projector
                if (i.uvShadow.w <= 0.0) discard;
                
                float2 uv = i.uvShadow.xy / i.uvShadow.w;
                // Cắt phần ảnh nằm ngoài viền texture
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) discard;
                
                fixed4 texS = tex2D(_ShadowTex, uv);
                return texS * _Color;
            }
            ENDCG
        }
    }
}
