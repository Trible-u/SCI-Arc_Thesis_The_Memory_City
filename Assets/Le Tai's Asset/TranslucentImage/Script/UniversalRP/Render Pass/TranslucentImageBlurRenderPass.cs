using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Scripting.APIUpdating;


namespace LeTai.Asset.TranslucentImage.UniversalRP
{
[MovedFrom("UnityEditor.Rendering.LWRP")]
struct TISPassData
{
    public RenderTargetIdentifier cameraColorTarget;
    public TranslucentImageSource blurSource;
    public IBlurAlgorithm         blurAlgorithm;
    public bool                   isPreviewing;
}

[MovedFrom("UnityEditor.Rendering.LWRP")]
public class TranslucentImageBlurRenderPass : ScriptableRenderPass
{
    private const string PROFILER_TAG = "Translucent Image Source";

    RenderTargetHandle afterPostProcessTexture = new RenderTargetHandle();

    private TISPassData currentPassData;

    readonly Material previewMaterial;

    public TranslucentImageBlurRenderPass()
    {
        previewMaterial = new Material(Shader.Find("Hidden/FillCrop_UniversalRP"));

        //Fragile!!! Should request Unity for access
        afterPostProcessTexture.Init("_AfterPostProcessTexture");
    }

    internal void Setup(TISPassData passData)
    {
        currentPassData = passData;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get(PROFILER_TAG);
        var source = renderingData.cameraData.postProcessEnabled
                         ? afterPostProcessTexture.Identifier()
                         : currentPassData.cameraColorTarget;

        currentPassData.blurAlgorithm.Blur(cmd,
                                           source,
                                           currentPassData.blurSource.BlurRegion,
                                           currentPassData.blurSource.BlurredScreen);

        if (currentPassData.isPreviewing)
        {
            previewMaterial.SetVector(ShaderProperties.blurTextureCropRegion,
                                      currentPassData.blurSource.BlurRegion.ToMinMaxVector());
            cmd.BlitFullscreenTriangle(currentPassData.blurSource.BlurredScreen,
                                       source,
                                       previewMaterial,
                                       0);
        }

        //Have to manually copy postfx to screen
        if (renderPassEvent == RenderPassEvent.AfterRendering)
        {
            cmd.Blit(source, currentPassData.cameraColorTarget);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
}
