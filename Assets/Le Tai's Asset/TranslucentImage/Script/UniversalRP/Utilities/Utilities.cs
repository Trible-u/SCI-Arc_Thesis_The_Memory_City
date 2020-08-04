using UnityEngine.Scripting.APIUpdating;

namespace LeTai.Asset.TranslucentImage.UniversalRP
{
[MovedFrom("UnityEditor.Rendering.LWRP")]
public static class Utilities
{
    public static int SimplePingPong(int t, int max)
    {
        if (t > max)
            return 2 * max - t;
        return t;
    }
}
}
