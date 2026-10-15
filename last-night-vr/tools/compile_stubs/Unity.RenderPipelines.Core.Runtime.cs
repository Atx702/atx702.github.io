// Compile-check stand-in for the members of com.unity.render-pipelines.core 17.0.4 that Last Night uses.
// Signatures copied from Runtime/Volume/Volume.cs. Not shipped: Unity uses the real package.
namespace UnityEngine.Rendering
{
    public class VolumeProfile : ScriptableObject { }

    public class Volume : MonoBehaviour
    {
        public bool isGlobal { get; set; }
        public float priority = 0f;
        public float blendDistance = 0f;
        public float weight = 1f;
        public VolumeProfile sharedProfile = null;
        public VolumeProfile profile { get; set; }
    }
}
