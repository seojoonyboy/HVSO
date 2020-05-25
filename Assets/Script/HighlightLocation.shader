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
        _MaskTex("Mask Texture (RGB)", 2D) = "white" {}
        _CutOff("_CutOff", Range(0,5)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="False" "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Cull off
        Lighting On

        CGPROGRAM
        #pragma surface surf Lambert alpha
        #pragma target 3.0

        sampler2D _MainTex;
        half4 _Color;
        sampler2D _MaskTex;
        float _CutOff;

        float _TillRadius;
        float _TillMaxRadius;
        float4 _Target_Pos;
        float powerForPos(float4 pos, float2 nearVertex);

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            half4 base = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Emission = base.rgb;
            o.Alpha = base.a - (tex2D(_MaskTex, IN.uv_MainTex).a * _CutOff);
/*
            float4 pos;
            pos.x = _Target_Pos.x - _TillRadius;
            pos.y = _Target_Pos.y - _TillRadius;
            o.Alpha = base.a - ((pos.x + _TillRadius) * (pos.y + _TillRadius)* _CutOff);*/
        }


        ENDCG
    }
    FallBack "Diffuse"
}

// 왜 안되는거야 으아아아아아아아아아아아아아아