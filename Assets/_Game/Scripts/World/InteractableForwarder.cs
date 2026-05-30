using UnityEngine;

namespace WPG.World
{
    // Mały pośrednik - SphereCollider trigger ma swój IInteractable rzutowany na cel.
    public class InteractableForwarder : MonoBehaviour, IInteractable
    {
        public MonoBehaviour target; // np. PowerSite

        public string GetPrompt(GameObject player)
        {
            return target is IInteractable i ? i.GetPrompt(player) : "";
        }

        public bool CanInteract(GameObject player)
        {
            return target is IInteractable i && i.CanInteract(player);
        }

        public void Interact(GameObject player)
        {
            (target as IInteractable)?.Interact(player);
        }
    }
}
