using UnityEngine;

namespace LastNight.Interaction
{
    /// <summary>A grabbable crayon. Tip = an empty at the drawing end.</summary>
    public class Crayon : MonoBehaviour
    {
        [SerializeField] Transform tip;
        [SerializeField] Color color = new Color(1f, 0.85f, 0.2f);

        public Transform Tip => tip;
        public Color32 Color => color;
    }
}
