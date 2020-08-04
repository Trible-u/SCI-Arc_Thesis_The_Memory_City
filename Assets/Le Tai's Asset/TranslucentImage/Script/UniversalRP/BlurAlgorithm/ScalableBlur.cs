using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;

namespace LeTai.Asset.TranslucentImage.UniversalRP
{
[MovedFrom("UnityEditor.Rendering.LWRP")]
public class ScalableBlur : IBlurAlgorithm
{
    Shader             shader;
    Material           material;
    ScalableBlurConfig config;

    const int BLUR_PASS      = 0;
    const int CROP_BLUR_PASS = 1;

    Material Material
    {
        get
        {
            if (material == null)
                Material = new Material(Shader.Find("Hidden/EfficientBlur_UniversalRP"));

            return material;
        }
        set => material = value;
    }

    public void Init(BlurConfig config)
    {
        this.config = (ScalableBlurConfig) config;
    }

    public void Blur(CommandBuffer          cmd,
                     RenderTargetIdentifier src,
                     Rect                   srcCropRegion,
                     RenderTexture          target)
    {
        float radius = ScaleWithResolution(config.Radius,
                                           target.width * srcCropRegion.width,
                                           target.height * srcCropRegion.height);
        ConfigMaterial(radius, srcCropRegion.ToMinMaxVector());

        int firstDownsampleFactor = config.Iteration > 0 ? 1 : 0;
        int stepCount             = Mathf.Max(config.Iteration * 2 - 1, 1);

        int firstIRT = ShaderProperties.intermediateRT[0];
        CreateTempRenderTextureFrom(cmd, firstIRT, target, firstDownsampleFactor);
        cmd.BlitFullscreenTriangle(src, firstIRT, Material, CROP_BLUR_PASS);


        for (var i = 1; i < stepCount; i++)
        {
            BlurAtDepth(cmd, i, target);
        }

        cmd.BlitFullscreenTriangle(ShaderProperties.intermediateRT[stepCount - 1], target, Material, BLUR_PASS);

        CleanupIntermediateRT(cmd, stepCount);
    }

    void CreateTempRenderTextureFrom(CommandBuffer cmd,
                                     int           nameId,
                                     RenderTexture src,
                                     int           downsampleFactor)
    {
        int w = src.width >> downsampleFactor; //= width / 2^downsample
        int h = src.height >> downsampleFactor;

        cmd.GetTemporaryRT(nameId, w, h, 0, FilterMode.Bilinear);
    }

    protected virtual void BlurAtDepth(CommandBuffer cmd, int depth, RenderTexture baseTexture)
    {
        int sizeLevel = Utilities.SimplePingPong(depth, config.Iteration - 1) + 1;
        sizeLevel = Mathf.Min(sizeLevel, config.MaxDepth);
        CreateTempRenderTextureFrom(cmd, ShaderProperties.intermediateRT[depth], baseTexture, sizeLevel);

        cmd.BlitFullscreenTriangle(ShaderProperties.intermediateRT[depth - 1],
                                   ShaderProperties.intermediateRT[depth],
                                   Material,
                                   0);
    }

    private void CleanupIntermediateRT(CommandBuffer cmd, int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            cmd.ReleaseTemporaryRT(ShaderProperties.intermediateRT[i]);
        }
    }

    ///<summary>
    /// Relative blur size to maintain same look across multiple resolution
    /// </summary>
    float ScaleWithResolution(float baseRadius, float width, float height)
    {
        float scaleFactor = Mathf.Min(width, height) / 1080f;
        scaleFactor = Mathf.Clamp(scaleFactor, .5f, 2f); //too much variation cause artifact
        return baseRadius * scaleFactor;
    }

    protected void ConfigMaterial(float radius, Vector4 cropRegion)
    {
        Material.SetFloat(ShaderProperties.blurRadius, radius);
        Material.SetVector(ShaderProperties.blurTextureCropRegion, cropRegion);
    }
}
}
