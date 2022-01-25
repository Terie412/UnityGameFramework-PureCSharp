// 实现 UI 帧序列动画
Shader "Custom/UI/shader_ui_frame_anim"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _CurrentFrame("Current Frame", Float) = 0
        _RowCount("Row Count", Float) = 4
        _ColumCount("Colum Count", Float) = 4
        _Speed("Speed", Float) = 4
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _CurrentFrame;
            float _RowCount;
            float _ColumCount;
            float _Speed;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                o.vertex = UnityObjectToClipPos(v.vertex);
                float row = floor((_CurrentFrame * _Time.x * 2 * _Speed) / _ColumCount) % _RowCount;
                float colum = floor((_CurrentFrame * _Time.x * 2 * _Speed) % _ColumCount);
                float2 texcoord = v.uv / float2(_ColumCount, _RowCount);
                texcoord += float2(colum / _ColumCount, (_RowCount - 1 - row) / _RowCount);
                o.uv = texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 col = tex2D(_MainTex, IN.uv);
                return col;
            }
            ENDCG
        }
    }
}