Shader "UI/HL Grab Blur"
{
    Properties
    {
        _BlurSize ("Blur Size", Range(0, 20)) = 5.0
        _Color ("Tint", Color) = (1,1,1,1)
        
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]

        // 화면 캡처
        GrabPass
        {
            "_BackgroundTexture"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _BackgroundTexture;
            float4 _BackgroundTexture_TexelSize;
            float _BlurSize;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.grabPos.xy / i.grabPos.w;
                float2 texelSize = _BackgroundTexture_TexelSize.xy * _BlurSize;
                
                fixed4 col = fixed4(0, 0, 0, 0);
                
                // 5x5 샘플링으로 더 부드러운 블러
                for(int x = -2; x <= 2; x++)
                {
                    for(int y = -2; y <= 2; y++)
                    {
                        float2 offset = float2(x, y) * texelSize;
                        col += tex2D(_BackgroundTexture, uv + offset);
                    }
                }
                
                col /= 25.0;
                col *= i.color;
                
                return col;
            }
            ENDCG
        }
    }
}