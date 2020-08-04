using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Scripting.APIUpdating;


namespace LeTai.Asset.TranslucentImage.UniversalRP
{
[MovedFrom("UnityEditor.Rendering.LWRP")]
public class TranslucentImageBlurSource : ScriptableRendererFeature
{
    public RenderPassEvent interceptionPoint = RenderPassEvent.AfterRenderingTransparents;

    readonly Dictionary<Camera, TranslucentImageSource> tisCache = new Dictionary<Camera, TranslucentImageSource>();

    TranslucentImageBlurRenderPass pass;
    IBlurAlgorithm                 blurAlgorithm;


    /// <summary>
    /// When adding new Translucent Image Source to existing Camera at run time, the new Source must be registered here
    /// </summary>
    /// <param name="source"></param>
    public void RegisterSource(TranslucentImageSource source)
    {
        tisCache.Add(source.GetComponent<Camera>(), source);
    }

    public override void Create()
    {
        ShaderProperties.Init(32); //hack for now

        blurAlgorithm = new ScalableBlur();
        pass          = new TranslucentImageBlurRenderPass();

        tisCache.Clear();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                         ref RenderingData  renderingData)
    {
        var tis = GetTIS(renderingData.cameraData.camera);

        if (tis == null || !tis.shouldUpdateBlur())
            return;

        blurAlgorithm.Init(tis.BlurConfig);
        var passData = new TISPassData {
            cameraColorTarget = renderer.cameraColorTarget,
            blurAlgorithm     = blurAlgorithm, //hack for now
            blurSource        = tis,
            isPreviewing      = tis.preview
        };

        bool requiresFinalPostProcessPass = renderingData.cameraData.postProcessEnabled &&
                                            renderingData.cameraData.antialiasing ==
                                            AntialiasingMode.FastApproximateAntialiasing;

        pass.renderPassEvent = requiresFinalPostProcessPass
                              ? RenderPassEvent.AfterRenderingPostProcessing
                              : RenderPassEvent.AfterRendering;

        pass.Setup(passData);

        renderer.EnqueuePass(pass);
    }

    TranslucentImageSource GetTIS(Camera camera)
    {
        if (!tisCache.ContainsKey(camera))
        {
            tisCache.Add(camera, camera.GetComponent<TranslucentImageSource>());
        }

        return tisCache[camera];
    }
}
}
