using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LastNight.Interaction
{
    /// <summary>
    /// A group of XR sockets that must all be filled (boat parts on the table, timetable tiles,
    /// mugs on the table). Fires onEachFilled per socket and onAllFilled once.
    /// Restrict what fits each socket with the socket's Interaction Layer Mask.
    /// </summary>
    public class SocketSet : MonoBehaviour
    {
        [SerializeField] XRSocketInteractor[] sockets;
        [SerializeField] bool lockWhenFilled = true;
        [SerializeField] UnityEvent<int> onEachFilled;
        [SerializeField] UnityEvent onAllFilled;

        bool done;

        public int FilledCount
        {
            get
            {
                int n = 0;
                foreach (var s in sockets) if (s.hasSelection) n++;
                return n;
            }
        }

        void OnEnable()
        {
            for (int i = 0; i < sockets.Length; i++)
            {
                int index = i;
                sockets[i].selectEntered.AddListener(_ => OnFilled(index));
            }
        }

        void OnDisable()
        {
            foreach (var s in sockets) s.selectEntered.RemoveAllListeners();
        }

        void OnFilled(int index)
        {
            onEachFilled?.Invoke(index);
            if (lockWhenFilled)
            {
                // Make the placed item stay put: stop it from being grabbed out again.
                var item = sockets[index].firstInteractableSelected;
                if (item is Component c && c.TryGetComponent<Collider>(out var col)) col.enabled = false;
            }
            if (!done && FilledCount == sockets.Length)
            {
                done = true;
                onAllFilled?.Invoke();
            }
        }
    }
}
