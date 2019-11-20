// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/HighlightLocation"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _TillRadius ("TillRadius", Float) = 1.0
        _TillMaxRadius ("TillMaxRadius", Float) = 0.5
        _Target_Pos ("_Target_Pos", Vector) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Cull off

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        float _TillRadius;
        float _TillMaxRadius;
        float4 _Target_Pos;
        float powerForPos(float4 pos, float2 nearVertex);

        struct Input
        {
            float2 uv_MainTex;
            float2 location;
        };

        void vert(inout appdata_full vertexData, out Input outData) {
            float4 pos = UnityObjectToClipPos(vertexData.vertex);
            float4 posWorld = mul(unity_ObjectToWorld, vertexData.vertex);
            outData.uv_MainTex = vertexData.texcoord;
            outData.location = posWorld.xy;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 baseColor = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            float alpha = (1.0 - powerForPos(_Target_Pos, IN.location));
            o.Albedo = baseColor.rgb;
            o.Alpha = alpha;
        }

        float powerForPos(float4 pos, float2 nearVertex) {
            float atten = (_TillRadius - length(pos.xy - nearVertex.xy));
            return atten / _TillRadius;
        }

        ENDCG
    }
    FallBack "Diffuse"
}

// 왜 안되는거야 으아아아아아아아아아아아아아아