Shader "Custom/Spine_Skeleton_Glow"
{
    Properties{
        _Color("Tint Color", Color) = (1,1,1,1)
        _Black("Black Point", Color) = (0,0,0,0)
        [NoScaleOffset] _MainTex("MainTex", 2D) = "black" {}
        [Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
        //[Toggle(GLOW_ON)] _UseGlow("Use Glow", Int) = 1

        _Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.1
        _GlowPower("Glow Power", Range(1,3)) = 1
        [HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default
    }

        SubShader{
            Tags { "Queue" = "Transparent" "CanUseSpriteAtlas" = "True" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }

            Fog { Mode Off }
            Cull Off
            ZWrite Off
            Blend One OneMinusSrcAlpha
            Lighting Off

            Stencil {
                Ref[_StencilRef]
                Comp[_StencilComp]
                Pass Keep
                ReadMask 1
                WriteMask 0
            }

            Pass {
                CGPROGRAM
                #pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
                #pragma shader_feature GLOW_ON
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                sampler2D _MainTex;
                float4 _Color;
                float4 _Black;
                float _GlowPower;

                struct VertexInput {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float4 vertexColor : COLOR;
                };

                struct VertexOutput {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 vertexColor : COLOR;
                };

                VertexOutput vert(VertexInput v) {
                    VertexOutput o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;

                    //#if GLOW_ON
                    //o.vertexColor = v.vertexColor * float4(_Color.rgb * (_Color.a * _GlowPower), _Color.a); // Combine a PMA version of _Color with vertexColor.
                    //#else
                    //o.vertexColor = v.vertexColor * float4(_Color.rgb * _Color.a , _Color.a);
                    //#endif
                    o.vertexColor = v.vertexColor * float4(_Color.rgb * (_Color.a * _GlowPower), _Color.a);
                    return o;
                }

                float4 frag(VertexOutput i) : COLOR {
                    float4 texColor = tex2D(_MainTex, i.uv);

                    #if defined(_STRAIGHT_ALPHA_INPUT)
                    texColor.rgb *= texColor.a;
                    #endif

                    return (texColor * i.vertexColor) + float4(((1 - texColor.rgb) * _Black.rgb * texColor.a*_Color.a*i.vertexColor.a), 0);
                }
                ENDCG
            }

            Pass {
                Name "Caster"
                Tags { "LightMode" = "ShadowCaster" }
                Offset 1, 1
                ZWrite On
                ZTest LEqual

                Fog { Mode Off }
                Cull Off
                Lighting Off

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_shadowcaster
                #pragma fragmentoption ARB_precision_hint_fastest
                #include "UnityCG.cginc"
                sampler2D _MainTex;
                fixed _Cutoff;

                struct VertexOutput {
                    V2F_SHADOW_CASTER;
                    float4 uvAndAlpha : TEXCOORD1;
                };

                VertexOutput vert(appdata_base v, float4 vertexColor : COLOR) {
                    VertexOutput o;
                    o.uvAndAlpha = v.texcoord;
                    o.uvAndAlpha.a = vertexColor.a;
                    TRANSFER_SHADOW_CASTER(o)
                    return o;
                }

                float4 frag(VertexOutput i) : COLOR {
                    fixed4 texcol = tex2D(_MainTex, i.uvAndAlpha.xy);
                    clip(texcol.a * i.uvAndAlpha.a - _Cutoff);
                    SHADOW_CASTER_FRAGMENT(i)
                }
                ENDCG
            }
        }
}
