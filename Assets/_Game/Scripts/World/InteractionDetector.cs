using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WPG.World
{
    // Wyszukuje najbliższy IInteractable w zasięgu wokół gracza. UI subskrybuje OnPromptChanged.
    public class InteractionDetector : MonoBehaviour
    {
        public float radius = 3.2f;
        public LayerMask mask = ~0;

        public static event Action<string> OnPromptChanged;

        private IInteractable _current;
        private string _lastPrompt = "";

        public bool HasReadyInteractable => _current != null && _current.CanInteract(gameObject);

        private void Update()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, mask, QueryTriggerInteraction.Collide);
            IInteractable best = null;
            float bestDist = float.MaxValue;
            foreach (var h in hits)
            {
                var inter = h.GetComponentInParent<IInteractable>();
                if (inter == null) continue;
                string p = inter.GetPrompt(gameObject);
                if (string.IsNullOrEmpty(p)) continue;
                float d = Vector3.Distance(transform.position, h.transform.position);
                if (d < bestDist)
                {
                    best = inter;
                    bestDist = d;
                }
            }
            _current = best;

            string prompt = best != null ? best.GetPrompt(gameObject) : "";
            if (prompt != _lastPrompt)
            {
                _lastPrompt = prompt;
                OnPromptChanged?.Invoke(prompt);
            }

            var kb = Keyboard.current;
            bool fPressed = kb != null && kb.fKey.wasPressedThisFrame;
            bool ePressedAsInteract = kb != null && kb.eKey.wasPressedThisFrame && HasReadyInteractable;
            if ((fPressed || ePressedAsInteract) && _current != null && _current.CanInteract(gameObject))
            {
                _current.Interact(gameObject);
                string newP = _current.GetPrompt(gameObject);
                if (newP != _lastPrompt)
                {
                    _lastPrompt = newP;
                    OnPromptChanged?.Invoke(newP);
                }
            }
        }
    }
}
