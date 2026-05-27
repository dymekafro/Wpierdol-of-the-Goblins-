using UnityEngine;
using WPG.Core;
using WPG.Enemies;
using WPG.Player;

namespace WPG.World
{
    // Interakcja przy ognisku obozu (E - oczyść / podbij). Zachowanie zależy od stanu.
    public class CampInteractable : MonoBehaviour, IInteractable
    {
        public GoblinCamp camp;

        public string GetPrompt(GameObject player)
        {
            if (camp == null) return "";
            switch (camp.State)
            {
                case CampState.Active: return $"Pokonaj goblinów i totem ({camp.displayName})";
                case CampState.Cleared: return $"E aby PODBIĆ obóz: {camp.displayName}";
                case CampState.Captured: return $"{camp.displayName} (Podbity){(string.IsNullOrEmpty(camp.captureBonusDescription) ? "" : " - " + camp.captureBonusDescription)}";
            }
            return "";
        }

        public bool CanInteract(GameObject player)
        {
            return camp != null && camp.State == CampState.Cleared;
        }

        public void Interact(GameObject player)
        {
            if (camp == null) return;
            var stats = player != null ? player.GetComponent<PlayerStats>() : null;
            camp.TryCapture(stats);
        }
    }
}
