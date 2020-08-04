#ifndef FLATKIT_LIGHT_PASS_DR_INCLUDED
#define FLATKIT_LIGHT_PASS_DR_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Lighting_DR.hlsl"

// TODO: Set these from the editor script.
#define _NORMALMAP
//#define BUMP_SCALE_NOT_SUPPORTED

// TODO: Makes everything darker.
// #define _MAIN_LIGHT_SHADOWS_CASCADE

// TODO: Remove.
#ifndef _ADDITIONAL_LIGHTS
#define _ADDITIONAL_LIGHTS
#endif

#define _ADDITIONAL_LIGHT_SHADOWS
//#define _SHADOWS_SOFT
#define _MIXED_LIGHTING_SUBTRACTIVE

#undef _RECEIVE_SHADOWS_OFF
//#define _MAIN_LIGHT_SHADOWS

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 uv           : TEXCOORD0;
    float2 lightmapUV   : TEXCOORD1;
    // float4 color        : COLOR;  // TODO: Vertex color.
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

// #ifdef _ADDITIONAL_LIGHTS
    float3 positionWS               : TEXCOORD2;
// #endif

#ifdef _NORMALMAP
    half4 normalWS                  : TEXCOORD3;    // xyz: normal, w: viewDir.x
    half4 tangentWS                 : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    half4 bitangentWS               : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
#else
    half3 normalWS                  : TEXCOORD3;
    half3 viewDirWS                 : TEXCOORD4;
#endif

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#ifdef _MAIN_LIGHT_SHADOWS
    float4 shadowCoord              : TEXCOORD7;
#endif

    float4 positionCS               : SV_POSITION;

    // float4 VertexColor              : COLOR;  // TODO: Vertex color.

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

/// ---------------------------------------------------------------------------

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

#ifdef _ADDITIONAL_LIGHTS
    inputData.positionWS = input.positionWS;
#endif

#ifdef _NORMALMAP
    half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
    inputData.normalWS = TransformTangentToWorld(normalTS,
        half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz));
#else
    half3 viewDirWS = input.viewDirWS;
    inputData.normalWS = input.normalWS;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    viewDirWS = SafeNormalize(viewDirWS);

    inputData.viewDirectionWS = viewDirWS;
#if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
    inputData.shadowCoord = input.shadowCoord;
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif
    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
}

Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.uv, _MainTex);

#ifdef _NORMALMAP
    output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
    output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
    output.viewDirWS = viewDirWS;
#endif
    
    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#ifdef _ADDITIONAL_LIGHTS
    output.positionWS = vertexInput.positionWS;
#endif

#if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    output.positionCS = vertexInput.positionCS;

    // TODO: DR_VERTEX_COLORS_ON
    // output.VertexColor = input.color;  // TODO: Vertex color.

    return output;
}

half4 LitPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);

#if defined(_NORMALMAP)
    half3 normalWS = TransformTangentToWorld(surfaceData.normalTS,
        half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz));
    normalWS = normalize(normalWS + input.normalWS.xyz);
#else
    half3 normalWS = input.normalWS;
#endif
    normalWS = normalize(normalWS);

#ifdef LIGHTMAP_ON
    // Normal is required in case Directional lightmaps are baked
    half3 bakedGI = SampleLightmap(input.lightmapUV, normalWS);
#else
    // Samples SH fully per-pixel. SampleSHVertex and SampleSHPixel functions
    // are also defined in case you want to sample some terms per-vertex.
    half3 bakedGI = SampleSH(normalWS);
#endif

    float3 positionWS = input.positionWS;
    half3 viewDirectionWS = SafeNormalize(GetCameraPositionWS() - positionWS);

    // BRDFData holds energy conserving diffuse and specular material reflections and its roughness.
    // It's easy to plugin your own shading fuction. You just need replace LightingPhysicallyBased function
    // below with your own.
    BRDFData brdfData;
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

    // Light struct is provide by LWRP to abstract light shader variables.
    // It contains light direction, color, distanceAttenuation and shadowAttenuation.
    // LWRP take different shading approaches depending on light and platform.
    // You should never reference light shader variables in your shader, instead use the GetLight
    // funcitons to fill this Light struct.
#ifdef _MAIN_LIGHT_SHADOWS
    // Main light is the brightest directional light.
    // It is shaded outside the light loop and it has a specific set of variables and shading path
    // so we can be as fast as possible in the case when there's only a single directional light
    // You can pass optionally a shadowCoord (computed per-vertex). If so, shadowAttenuation will be
    // computed.
    Light mainLight = GetMainLight(input.shadowCoord);
#else
    Light mainLight = GetMainLight();
#endif

    // Mix diffuse GI with environment reflections.
    half3 color = GlobalIllumination(brdfData, bakedGI, surfaceData.occlusion, normalWS, viewDirectionWS);

    // LightingPhysicallyBased_DSTRM computes direct light contribution.
    color += LightingPhysicallyBased_DSTRM(brdfData, mainLight, normalWS, viewDirectionWS, positionWS);
    // color += LightingPhysicallyBased(brdfData, mainLight, normalWS, viewDirectionWS);

    {
        half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
        #if defined(_TEXTUREBLENDINGMODE_ADD)
            color += lerp(half4(0.0, 0.0, 0.0, 0.0), tex, _TextureImpact).rgb;
        #else  // _TEXTUREBLENDINGMODE_MULTIPLY
            // This is the default blending mode for compatibility with the v.1 of the asset.
            color *= lerp(half4(1.0, 1.0, 1.0, 1.0), tex, _TextureImpact).rgb;
        #endif
    }

    // Additional lights loop
#ifdef _ADDITIONAL_LIGHTS
    // Returns the amount of lights affecting the object being renderer.
    // These lights are culled per-object in the forward renderer
    int additionalLightsCount = GetAdditionalLightsCount();
    for (int i = 0; i < additionalLightsCount; ++i)
    {
        // Similar to GetMainLight, but it takes a for-loop index. This figures out the
        // per-object light index and samples the light buffer accordingly to initialized the
        // Light struct. If _ADDITIONAL_LIGHT_SHADOWS is defined it will also compute shadows.
        Light light = GetAdditionalLight(i, positionWS);

        // Same functions used to shade the main light.
        color += LightingPhysicallyBased_DSTRM(brdfData, light, normalWS, viewDirectionWS, positionWS);
    }
#endif

    color += surfaceData.emission;
    
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    return half4(color, surfaceData.alpha);
}

#endif // FLATKIT_LIGHT_PASS_DR_INCLUDED
