// Compile-check stand-in for the URP 17.0.4 members Last Night's editor setup uses.
// Signatures copied from Runtime/Data/UniversalRenderPipelineAsset.cs. Not shipped.
namespace UnityEngine.Rendering.Universal
{
    public enum RendererType { Custom, UniversalRenderer, _2DRenderer }

    public abstract class ScriptableRendererData : ScriptableObject { }

    public class UniversalRendererData : ScriptableRendererData { }

    public class UniversalRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline() => null;
        public static UniversalRenderPipelineAsset Create(ScriptableRendererData rendererData = null) => null;
        public ScriptableRendererData LoadBuiltinRendererData(RendererType type = RendererType.UniversalRenderer) => null;
        public bool supportsCameraDepthTexture { get; set; }
        public bool supportsCameraOpaqueTexture { get; set; }
        public bool supportsHDR { get; set; }
        public int msaaSampleCount { get; set; }
        public float renderScale { get; set; }
        public int mainLightShadowmapResolution { get; set; }
        public int maxAdditionalLightsCount { get; set; }
        public float shadowDistance { get; set; }
        public int shadowCascadeCount { get; set; }
        public bool useSRPBatcher { get; set; }
    }
}
