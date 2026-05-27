using System;
using UnityEngine;
using WPG.Core;
using WPG.Player;

namespace WPG.World
{
    public class PowerSite : MonoBehaviour, IInteractable
    {
        public string siteId = "power_site_stone_circle";
        public string displayName = "Kamienny Krąg";
        public string bonusDescription = "+1 INT (jednorazowo)";

        private bool _used;

        public bool Used => _used;

        public void MarkUsedSilent()
        {
            _used = true;
            // Stłumiona emisja - zużyte
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    var c = r.sharedMaterial.GetColor("_EmissionColor");
                    r.sharedMaterial.SetColor("_EmissionColor", c * 0.3f);
                }
            }
        }

        public string GetPrompt(GameObject player)
        {
            if (_used) return $"{displayName} (zużyte)";
            return $"E aby aktywować: {displayName}\n({bonusDescription})";
        }

        public bool CanInteract(GameObject player)
        {
            return !_used;
        }

        public void Interact(GameObject player)
        {
            if (_used) return;
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null && stats.attributes != null)
            {
                stats.attributes.intelligence += 1;
                stats.RestoreMana(stats.attributes.MaxMana);
            }
            _used = true;
            if (GameManager.Instance != null) GameManager.Instance.visitedPowerSites.Add(siteId);

            // Wizualnie: zwiększamy świecenie raz
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    var c = r.sharedMaterial.GetColor("_EmissionColor");
                    r.sharedMaterial.SetColor("_EmissionColor", c * 1.5f);
                }
            }
        }
    }

    public interface IInteractable
    {
        string GetPrompt(GameObject player);
        bool CanInteract(GameObject player);
        void Interact(GameObject player);
    }
}
