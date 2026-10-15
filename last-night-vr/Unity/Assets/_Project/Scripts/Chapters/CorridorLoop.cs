using System.Collections.Generic;
using LastNight.World;
using UnityEngine;

namespace LastNight.Chapters
{
    /// <summary>
    /// Chapter 5's endless corridor. The player walks through looping modules. Five drawings are
    /// shuffled across the walls each loop; looking at them in story order (house, lantern, boat,
    /// bird, door 7) breaks the loop. A wrong order resets with a long hum and the player is
    /// quietly teleported back to the loop start (seamless: the modules are identical).
    /// </summary>
    public class CorridorLoop : MonoBehaviour
    {
        [Tooltip("The five story drawings in the correct order.")]
        [SerializeField] DrawingCollectible[] storyOrder;
        [SerializeField] Transform[] drawingSlots;          // wall positions they get shuffled into
        [SerializeField] Collider loopEndTrigger;           // at the end of the looping section
        [SerializeField] Transform loopStart, loopEnd;      // identical points in two modules
        [SerializeField] GameObject exitModule;             // enabled when solved
        [SerializeField] GameObject[] extraModules;         // enabled by Lengthen() after coffee
        [SerializeField] AudioSource resetHum;

        readonly List<int> seenOrder = new List<int>();
        readonly HashSet<DrawingCollectible> counted = new HashSet<DrawingCollectible>();
        bool running;

        public bool Solved { get; private set; }
        public int Resets { get; private set; }

        public void Begin()
        {
            running = true;
            Shuffle();
        }

        void Update()
        {
            if (!running || Solved) return;
            for (int i = 0; i < storyOrder.Length; i++)
            {
                var d = storyOrder[i];
                if (d.Found && counted.Add(d)) seenOrder.Add(i);
            }
            if (seenOrder.Count < storyOrder.Length) return;

            bool inOrder = true;
            for (int i = 0; i < seenOrder.Count; i++) if (seenOrder[i] != i) { inOrder = false; break; }
            if (inOrder)
            {
                Solved = true;
                exitModule.SetActive(true);
                loopEndTrigger.enabled = false;
            }
        }

        /// <summary>Called by a trigger relay at the loop end.</summary>
        public void OnReachedLoopEnd()
        {
            if (Solved) return;
            Resets++;
            if (resetHum != null) resetHum.Play();
            // Seamless teleport: shift the rig by the offset between two identical points.
            var origin = FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (origin != null) origin.transform.position += loopStart.position - loopEnd.position;
            // The order only counts within one pass; drawings already collected still count for the gallery.
            seenOrder.Clear();
            counted.Clear();
            foreach (var d in storyOrder) d.ResetForLoop();
            Shuffle();
        }

        public void Lengthen(int modules)
        {
            for (int i = 0; i < Mathf.Min(modules, extraModules.Length); i++) extraModules[i].SetActive(true);
        }

        void Shuffle()
        {
            var slots = new List<Transform>(drawingSlots);
            foreach (var d in storyOrder)
            {
                int i = Random.Range(0, slots.Count);
                d.transform.SetPositionAndRotation(slots[i].position, slots[i].rotation);
                slots.RemoveAt(i);
            }
        }
    }
}
