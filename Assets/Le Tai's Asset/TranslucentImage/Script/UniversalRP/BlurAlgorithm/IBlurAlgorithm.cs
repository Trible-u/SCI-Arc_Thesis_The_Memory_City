using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;

namespace LeTai.Asset.TranslucentImage.UniversalRP
{
[MovedFrom("UnityEditor.Rendering.LWRP")]
public enum BlurAlgorithmType
{
    ScalableBlur
}

[MovedFrom("UnityEditor.Rendering.LWRP")]
public interface IBlurAlgorithm
{
    void Init(BlurConfig config);

    void Blur(CommandBuffer          cmd,
              RenderTargetIdentifier src,
              Rect                   srcCropRegion,
              RenderTexture          target);
}
}
