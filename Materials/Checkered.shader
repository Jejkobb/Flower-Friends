Shader "Custom/ScrollingCheckeredForPlane"
{
    Properties
    {
        _MainColor("Main Color", Color) = (1, 1, 1, 1) // Color of the white squares
        _SecondaryColor("Secondary Color", Color) = (0, 0, 0, 1) // Color of the black squares
        _TileSize("Tile Size", Float) = 10.0 // Size of the checkered pattern
        _ScrollSpeed("Scroll Speed", Float) = 0.1 // Speed of the scrolling
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
            fixed4 _SecondaryColor;
            float _TileSize;
            float _ScrollSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Scrolling UVs based on time and scroll speed
                float scrollOffset = _ScrollSpeed * _Time.y;

                // Adjust the UV coordinates with scrolling offset
                o.uv = v.uv + float2(scrollOffset, scrollOffset);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Create checkered pattern based on the UV coordinates and tile size
                float checkerPattern = frac(i.uv.x * _TileSize) < 0.5 ^ frac(i.uv.y * _TileSize) < 0.5 ? 1.0 : 0.0;

            // Return main color for one square and secondary color for the other
            return lerp(_SecondaryColor, _MainColor, checkerPattern);
        }
        ENDCG
    }
    }
        FallBack "Unlit"
}
