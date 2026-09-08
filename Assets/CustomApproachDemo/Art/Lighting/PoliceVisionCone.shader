Shader "CustomApproachDemo/Police Vision Cone"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.66, 0.3, 0.22)
        _Alpha ("Alpha", Range(0, 1)) = 0.22
        _Range ("Range", Float) = 12
        _ConeAngle ("Cone Angle", Float) = 90
        _NearFade ("Near Fade", Range(0.001, 0.5)) = 0.08
        _FarFadePower ("Far Fade Power", Range(0.25, 4)) = 1.6
        _EdgeSoftness ("Edge Softness", Range(0.05, 2)) = 0.55
        _CenterBoost ("Center Boost", Range(0, 2)) = 0.8
        _NoiseScale ("Noise Scale", Range(0.1, 12)) = 3.5
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.22
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+20"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Alpha;
            float _Range;
            float _ConeAngle;
            float _NearFade;
            float _FarFadePower;
            float _EdgeSoftness;
            float _CenterBoost;
            float _NoiseScale;
            float _NoiseStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 localPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                fixed4 color : COLOR;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.localPos = input.vertex.xyz;
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            float Hash(float2 value)
            {
                return frac(sin(dot(value, float2(127.1, 311.7))) * 43758.5453);
            }

            float ValueNoise(float2 value)
            {
                float2 cell = floor(value);
                float2 fraction = frac(value);
                fraction = fraction * fraction * (3.0 - 2.0 * fraction);

                float a = Hash(cell);
                float b = Hash(cell + float2(1.0, 0.0));
                float c = Hash(cell + float2(0.0, 1.0));
                float d = Hash(cell + float2(1.0, 1.0));

                return lerp(lerp(a, b, fraction.x), lerp(c, d, fraction.x), fraction.y);
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float length01 = saturate(input.uv.y);
                float angleRadians = radians(max(_ConeAngle, 1.0) * 0.5);
                float maxRadius = max(tan(angleRadians) * max(_Range, 0.01) * max(length01, 0.01), 0.01);
                float radial01 = saturate(length(input.localPos.xy) / maxRadius);

                float nearFade = smoothstep(0.0, _NearFade, length01);
                float farFade = pow(saturate(1.0 - length01), _FarFadePower);
                float edgeFade = pow(saturate(1.16 - radial01), _EdgeSoftness);
                float centerGlow = lerp(1.0, saturate(1.0 - radial01 * 0.65), _CenterBoost);

                float softNoise = ValueNoise(float2(input.uv.x * _NoiseScale, length01 * _NoiseScale * 1.7));
                float noise = lerp(1.0, 0.72 + softNoise * 0.56, _NoiseStrength);

                float alpha = _Alpha * input.color.a * nearFade * farFade * edgeFade * centerGlow * noise;
                float3 color = _Color.rgb * (0.8 + centerGlow * 0.35);

                return fixed4(color, saturate(alpha));
            }
            ENDCG
        }
    }
}
