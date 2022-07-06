Shader "Custom/test_coordinate_transform"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vColor : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                // o.vertex = mul(unity_ObjectToWorld, v.vertex);
                // o.vertex = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, v.vertex));
                // o.vertex = UnityObjectToClipPos(v.vertex);
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vColor = float4(o.vertex.w, o.vertex.w, o.vertex.w, o.vertex.w) / 2;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(1, 1, 1, 1);
                return col;
            }
            ENDCG
        }
    }
}
