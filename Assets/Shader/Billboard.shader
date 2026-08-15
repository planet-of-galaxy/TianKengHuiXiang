// ============================================================
// 用途
//   世界空间 Billboard：让 uGUI 元素（World Space Canvas 下的 Image / RawImage）
//   始终正对相机，并配合透视相机做距离补偿——几何尺寸随距离变大，
//   抵消透视的近大远小，视觉上无论远近屏幕大小恒定。
//
// 适用场景
//   漂浮标签、头顶标记、路径点图标、NPC 头顶血条等 3D 世界中的 UI 元素。
//   仅用于 Render Mode 为 World Space 的 Canvas；不适用 Screen Space 的 Canvas。
//
// 使用方法
//   1. 创建材质：Project 右键 → Create → Material，Shader 选 Custom/UI/Billboard。
//   2. Canvas 的 Render Mode 设为 World Space，把 Canvas 摆放到锚点位置。
//   3. 在 Canvas 下创建 Image / RawImage，把材质赋给元素（Graphic 的 Material），
//      元素大小由 RectTransform 决定。
//   注意：元素的 RectTransform 需放在 Canvas 原点（anchoredPosition 为 0,0），
//   即元素中心对准 Canvas 锚点，shader 会以该点为中心朝向相机。
//   无需 C# 脚本，静态使用即可。
//
// 距离与大小
//   视觉大小 = 元素世界尺寸 ×（视线深度 / _RefDistance）
//   —— 距离主相机越近几何上越小、越远几何上越大，从而屏幕上的视觉大小恒定；
//   在距主相机 _RefDistance 时，屏幕大小恰好等于元素真实世界大小。
//
// 参数
//   _BaseMap     贴图
//   _BaseColor   颜色（含透明度）
//   _RefDistance 参考距离：元素在距主相机该距离时，屏幕大小 = 真实世界大小
//   _ZTest       深度测试：4=会被遮挡，8=始终绘制在最上层
// ============================================================
Shader "Custom/UI/Billboard"
{
    Properties
    {
        [MainTexture] _BaseMap ("Texture", 2D) = "white" {}
        [MainColor]   _BaseColor ("Color", Color) = (1, 1, 1, 1)

        // 参考距离：在距离主相机该距离时，元素屏幕大小 = 元素真实世界大小
        _RefDistance ("Reference Distance (world units)", Range(0.01, 500.0)) = 10.0

        // 深度测试；4 = LEqual（会被遮挡），8 = Always（始终绘制在最上层）
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Z Test", Float) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest [_ZTest]
        Cull Off

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                half4  color      : COLOR;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                float  _RefDistance;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                // 锚点（Canvas 原点）和当前顶点的世界坐标
                float3 centerWS = TransformObjectToWorld(float3(0, 0, 0));
                float3 vertexWS = TransformObjectToWorld(input.positionOS);
                float3 offsetWS = vertexWS - centerWS;

                // Canvas 本地 X/Y 轴在世界空间的方向，用于把偏移分解到元素平面
                float3 localRight = normalize(TransformObjectToWorldDir(float3(1, 0, 0)));
                float3 localUp    = normalize(TransformObjectToWorldDir(float3(0, 1, 0)));

                // 顶点相对锚点、沿元素自身的偏移量（单位：世界长度）
                float u = dot(offsetWS, localRight);
                float v = dot(offsetWS, localUp);

                // 锚点到相机的视线方向深度（相机看向 -Z，取 -z 得到正深度）
                float depth = max(-TransformWorldToView(centerWS).z, 1e-4);
                // 距离补偿：越近越小、越远越大，抵消透视近大远小
                float distScale = depth / _RefDistance;

                // 相机在世界空间的正交基
                float3 camRight = UNITY_MATRIX_V[0].xyz; // 相机右
                float3 camUp    = UNITY_MATRIX_V[1].xyz; // 相机上

                // 以锚点为中心、用相机 right/up 重新铺出 quad，并按距离缩放
                float3 worldPos = centerWS + (camRight * u + camUp * v) * distScale;

                output.positionCS = TransformWorldToHClip(worldPos);
                output.uv = input.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                output.color = input.color * _BaseColor;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * input.color;
                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
