Shader "SyntyStudios/Prototype_Global_URP"
{
    Properties
    {
        _BaseColor ("BaseColor", Color) = (0.06228374,0.8320726,0.9411765,1)
        _Grid ("Grid", 2D) = "white" {}
        _GridScale ("GridScale", Float) = 5
        _Falloff ("Falloff", Float) = 50
        _OverlayAmount ("OverlayAmount", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        // ===== Forward / Lit pass =====
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Enable shadows & additional lights (if needed)
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldTangent : TEXCOORD2;
                float3 worldBitangent : TEXCOORD3;
                float2 uv           : TEXCOORD4;

                #if defined(_MAIN_LIGHT_SHADOWS)
                float4 shadowCoord : TEXCOORD5;
                #endif
            };

            // Uniforms / material properties
            float4 _BaseColor;
            sampler2D _Grid;
            float _GridScale;
            float _Falloff;
            float _OverlayAmount;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Transform position
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS);

                // Normals/tangent space
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normalOS);
                float3 tangentWS = TransformObjectToWorldDir(IN.tangentOS.xyz);
                float tangentSign = IN.tangentOS.w * TransformObjectToWorldScale().w;
                OUT.worldBitangent = cross(OUT.worldNormal, tangentWS) * tangentSign;
                OUT.worldTangent = tangentWS;

                OUT.uv = IN.uv;

                #if defined(_MAIN_LIGHT_SHADOWS)
                {
                    // get shadow coordinate
                    VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS);
                    OUT.shadowCoord = GetShadowCoord(vpi);
                }
                #endif

                return OUT;
            }

            // Triplanar sampling function
            float4 TriplanarSamplingCF(
                sampler2D topTex, sampler2D midTex, sampler2D botTex,
                float3 worldPos, float3 worldNormal,
                float falloff, float tiling)
            {
                float3 absN = abs(worldNormal);
                float3 proj = pow(absN, falloff);
                float sum = proj.x + proj.y + proj.z;
                proj /= sum;

                float3 nsign = sign(worldNormal);
                float negProjY = max(0, proj.y * -nsign.y);
                proj.y = max(0, proj.y * nsign.y);

                float4 xNorm = SAMPLE_TEXTURE2D(midTex, sampler2D(midTex), tiling * worldPos.zy * float2(nsign.x, 1.0));
                float4 yNorm = SAMPLE_TEXTURE2D(topTex, sampler2D(topTex), tiling * worldPos.xz * float2(nsign.y, 1.0));
                float4 yNormN = SAMPLE_TEXTURE2D(botTex, sampler2D(botTex), tiling * worldPos.xz * float2(nsign.y, 1.0));
                float4 zNorm = SAMPLE_TEXTURE2D(midTex, sampler2D(midTex), tiling * worldPos.xy * float2(-nsign.z, 1.0));

                return xNorm * proj.x + yNorm * proj.y + yNormN * negProjY + zNorm * proj.z;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Normalize world normal
                float3 normWS = normalize(IN.worldNormal);

                // Triplanar sampling
                float4 triSample = TriplanarSamplingCF(_Grid, _Grid, _Grid, IN.worldPos, normWS, _Falloff, _GridScale);
                float4 lerpRes = lerp(float4(1,1,1,1), triSample, _OverlayAmount);
                float3 baseCol = _BaseColor.rgb * lerpRes.rgb;

                // Setup surface data for lighting
                SurfaceData surface;
                surface.normalWS = normWS;
                surface.tangentWS = IN.worldTangent;
                surface.bitangentWS = IN.worldBitangent;
                surface.baseColor = baseCol;
                surface.emissive = 0;
                surface.metallic = 0;
                surface.smoothness = 0;
                surface.ambientOcclusion = 1;
                surface.alpha = 1;

                // Main light
                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;
                float3 lightCol = mainLight.color * mainLight.distanceAttenuation;

                float3 lighting = LightingLambert(lightCol, lightDir, surface.normalWS);

                // Shadow (if enabled)
                #if defined(_MAIN_LIGHT_SHADOWS)
                {
                    float shadowAtt = MainLightRealtimeShadow(IN.shadowCoord);
                    lighting *= shadowAtt;
                }
                #endif

                // Additional lights
                #if defined(_ADDITIONAL_LIGHTS)
                {
                    int cnt = GetAdditionalLightsCount();
                    for (int i = 0; i < cnt; i++)
                    {
                        Light l = GetAdditionalLight(i, IN.worldPos);
                        lighting += LightingLambert(l.color * l.distanceAttenuation, l.direction, surface.normalWS);
                    }
                }
                #endif

                float3 finalColor = lighting * surface.baseColor;

                return float4(finalColor, surface.alpha);
            }

            ENDHLSL
        } // end ForwardLit pass

        // ===== ShadowCaster Pass =====
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex ShadowCasterVert
            #pragma fragment ShadowCasterFrag
            #pragma multi_compile_instancing

            // If your shader supports alpha test, also:
            //#pragma shader_feature _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                // include normal / tangent if you offset vertices or need them for shadows
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings ShadowCasterVert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                return OUT;
            }

            half4 ShadowCasterFrag(Varyings IN) : SV_Target
            {
                // The ShadowCasterPass handles most of the logic for depth only
                return 0;
            }

            ENDHLSL
        }

    } // end SubShader

    FallBack "Universal Forward"
}
