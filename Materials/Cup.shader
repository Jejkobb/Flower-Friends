Shader "Custom/UnlitFresnelSolidColorsWithYThreshold"
{
    Properties
    {
        _MainColor("Inner Color", Color) = (1, 1, 1, 1) // Inner color of the object
        _FresnelColor("Fresnel Edge Color", Color) = (1, 0, 0, 1) // Fresnel edge color
        _FresnelThreshold("Fresnel Threshold", Range(0, 1)) = 0.5 // Threshold for switching between inner and edge color
        _YThreshold("Y ScreenSpace Threshold", Range(0, 1)) = 0.5 // Y-axis threshold in screen space
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Properties
            fixed4 _MainColor;
            fixed4 _FresnelColor;
            float _FresnelThreshold;
            float _YThreshold; // Y-axis screen-space threshold

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 viewDir : TEXCOORD0;
                float4 screenPos : TEXCOORD1; // Screen position for Y threshold
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Calculate view direction in orthographic mode
                o.viewDir = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));

                // Get screen-space position (clip space)
                o.screenPos = o.pos;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Fresnel term for orthographic view
                float fresnelFactor = 1.0 - abs(dot(i.viewDir, float3(0, 0, 1)));

            // Apply Y-axis screen space threshold
            float screenY = i.screenPos.y / i.screenPos.w; // Convert to normalized device coordinates
            screenY = (screenY * 0.5) + 0.5; // Map from [-1, 1] to [0, 1]

            // Only apply the Y threshold in areas where the inner color would be applied (Fresnel inner part)
            if (fresnelFactor <= _FresnelThreshold && screenY > _YThreshold)
            {
                return _MainColor; // Inner color
            }
            else if (fresnelFactor > _FresnelThreshold)
            {
                return _FresnelColor; // Fresnel color
            }

            return _FresnelColor; // Default fallback
        }
        ENDCG
    }
    }
        FallBack "Unlit"
}
