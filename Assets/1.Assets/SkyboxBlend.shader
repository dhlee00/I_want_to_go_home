Shader "SkyboxBlend" {
    Properties {
        _Tint ("Tint Color", Color) = (.5, .5, .5, 1)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Night Rotation", Range(0, 360)) = 0
        _Blend ("Night Blend", Range(0, 1)) = 0

        [NoScaleOffset] _DayCube ("Day Cubemap", Cube) = "_Skybox" {}
        [NoScaleOffset] _NightCube ("Night Cubemap", Cube) = "_Skybox" {}
    }

    SubShader {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            samplerCUBE _DayCube;
            samplerCUBE _NightCube;
            half4 _Tint;
            half _Exposure, _Rotation, _Blend;

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 viewDirDay : TEXCOORD0;
                float3 viewDirNight : TEXCOORD1;
            };

            // 세로 회전(X축 기준) 함수
            float3 RotateX(float3 v, float degree) {
                float a = degree * (3.14159265 / 180.0);
                float s, c;
                sincos(a, s, c);
                // X축은 고정하고 Y, Z 평면을 회전시켜 세로 회전을 구현합니다.
                return float3(v.x, c * v.y - s * v.z, s * v.y + c * v.z);
            }

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // 낮은 회전 없이 원래의 방향 벡터 사용
                o.viewDirDay = v.vertex.xyz;
                
                // 밤은 X축 기준으로 회전된 방향 벡터 사용
                o.viewDirNight = RotateX(v.vertex.xyz, _Rotation);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 각각의 방향 벡터로 낮/밤 큐브맵 샘플링
                half4 dayCol = texCUBE(_DayCube, i.viewDirDay);
                half4 nightCol = texCUBE(_NightCube, i.viewDirNight);

                // 설정한 _Blend 값에 따라 두 하늘을 섞음
                half4 res = lerp(dayCol, nightCol, _Blend);
                
                // 틴트와 노출 적용
                res.rgb *= _Tint.rgb * _Exposure * 2.0;
                return res;
            }
            ENDCG
        }
    }
    Fallback Off
}