using System;
using UnityEngine;

namespace WPG.World
{
    // Strefa o nazwie wyświetlanej w HUDzie kiedy gracz w niej jest.
    public class WorldZone : MonoBehaviour
    {
        public string zoneName = "Magiczny Las";
        public string defaultOuterName = "Magiczny Las";
        public bool restoreDefaultOnExit = true;

        public static event Action<string> OnZoneEntered;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            OnZoneEntered?.Invoke(zoneName);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!restoreDefaultOnExit) return;
            if (!other.CompareTag("Player")) return;
            OnZoneEntered?.Invoke(defaultOuterName);
        }

        public static void RaiseExternal(string name)
        {
            OnZoneEntered?.Invoke(name);
        }
    }
}
