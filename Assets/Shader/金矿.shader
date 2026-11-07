Shader "TianKengHuiXiang/Gold Ore"
{
    Properties
    {
        _RockColor("Rock", Color) = (0.31, 0.33, 0.35, 1)
        _GoldColor("Native Gold", Color) = (1.0, 0.73, 0.29, 1)
        _OreThreshold("Gold Coverage Threshold", Range(0, 1)) = 0.645
        _RockSmoothness("Rock Smoothness", Range(0, 1)) = 0.18
        _GoldSmoothness("Gold Smoothness", Range(0, 1)) = 0.68
        [HideInInspector] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _BaseMap("Base Map", 2D) = "white" {}
        [HideInInspector] _Cull("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "GoldOreForward"
            Tags { "LightMode"="UniversalForwardOnly" }
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex OreVertex
            #pragma fragment OreFragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _RockColor;
                half4 _GoldColor;
                half _OreThreshold;
                half _RockSmoothness;
                half _GoldSmoothness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half2 mask : TEXCOORD2;
                half4 fogAndLights : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
            };

            Varyings OreVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.mask = input.color.rg;
                output.fogAndLights = half4(ComputeFogFactor(position.positionCS.z),
                    VertexLighting(position.positionWS, output.normalWS));
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    output.shadowCoord = GetShadowCoord(position);
                #endif
                return output;
            }

            half4 OreFragment(Varyings input) : SV_Target
            {
                half ore = smoothstep(_OreThreshold - 0.014h, _OreThreshold + 0.014h, input.mask.r);
                half3 rock = _RockColor.rgb * lerp(0.56h, 1.34h, input.mask.g);
                half3 gold = _GoldColor.rgb * lerp(0.70h, 1.08h, input.mask.g);
                SurfaceData surface = (SurfaceData)0;
                surface.albedo = lerp(rock, gold, ore);
                surface.metallic = ore * 0.92h;
                surface.smoothness = lerp(_RockSmoothness, _GoldSmoothness, ore);
                surface.normalTS = half3(0, 0, 1);
                surface.occlusion = 1;
                surface.alpha = 1;

                InputData data = (InputData)0;
                data.positionWS = input.positionWS;
                data.positionCS = input.positionCS;
                data.normalWS = NormalizeNormalPerPixel(input.normalWS);
                data.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                data.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                data.bakedGI = SampleSH(data.normalWS);
                data.shadowMask = half4(1, 1, 1, 1);
                data.vertexLighting = input.fogAndLights.yzw;
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    data.shadowCoord = input.shadowCoord;
                #else
                    data.shadowCoord = TransformWorldToShadowCoord(data.positionWS);
                #endif
                half4 color = UniversalFragmentPBR(data, surface);
                color.rgb = MixFog(color.rgb, input.fogAndLights.x);
                return color;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
    FallBack Off
}
