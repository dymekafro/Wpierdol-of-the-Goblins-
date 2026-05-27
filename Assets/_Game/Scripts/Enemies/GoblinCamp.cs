using System;
using System.Collections.Generic;
using UnityEngine;
using WPG.Core;
using WPG.World;

namespace WPG.Enemies
{
    public class GoblinCamp : MonoBehaviour
    {
        public string campId = "goblin_camp_unnamed";
        public string displayName = "Obóz Goblinów";
        public Color activeLightColor = new Color(1f, 0.3f, 0.1f);
        public Color clearedLightColor = new Color(0.5f, 0.5f, 0.5f);
        public Color capturedLightColor = new Color(0.3f, 1f, 0.6f);

        public int ring = 1;
        public List<GoblinBase> Goblins { get; } = new List<GoblinBase>();
        public Totem totem;
        public CampState State { get; private set; } = CampState.Active;

        public Light campFireLight;
        public ParticleSystem smokeSystem;
        public Renderer[] palisadeRenderers;
        public Transform campFireRoot;

        public string captureBonusDescription = "";
        public int captureBonusType; // 0 = mana regen, 1 = max HP +10, 2 = + nature crystal

        public event Action<CampState> OnStateChanged;

        public void Initialize(CampState initialState)
        {
            ApplyState(initialState, fireEvent: false);
        }

        public void RegisterGoblin(GoblinBase g)
        {
            if (g == null) return;
            Goblins.Add(g);
            g.camp = this;
            g.OnDeath += HandleGoblinDeath;
        }

        public void RegisterTotem(Totem t)
        {
            totem = t;
            if (t != null) t.OnDestroyed += HandleTotemDestroyed;
        }

        private void HandleTotemDestroyed()
        {
            CheckCleared();
        }

        private void HandleGoblinDeath(GoblinBase g)
        {
            CheckCleared();
        }

        private void CheckCleared()
        {
            if (State != CampState.Active) return;
            bool totemDead = totem == null || totem.IsDead;
            bool allDead = true;
            foreach (var g in Goblins)
            {
                if (g != null && !g.IsDead) { allDead = false; break; }
            }
            if (totemDead && allDead)
            {
                ApplyState(CampState.Cleared);
            }
        }

        public bool TryCapture(WPG.Player.PlayerStats playerStats)
        {
            if (State != CampState.Cleared) return false;
            ApplyState(CampState.Captured);
            if (playerStats != null)
            {
                playerStats.Heal(20);
                playerStats.RestoreMana(20);
                ApplyCaptureBonus(playerStats);
            }
            return true;
        }

        private void ApplyCaptureBonus(WPG.Player.PlayerStats playerStats)
        {
            if (playerStats == null || playerStats.attributes == null) return;
            switch (captureBonusType)
            {
                case 0:
                    playerStats.manaRegenBonus += 1.5f;
                    captureBonusDescription = "Regeneracja many +1.5/s";
                    break;
                case 1:
                    playerStats.attributes.endurance += 1;
                    playerStats.Heal(10);
                    captureBonusDescription = "Wytrzymałość +1 (więcej HP)";
                    break;
                case 2:
                    playerStats.attributes.intelligence += 1;
                    captureBonusDescription = "Intelekt +1 (silniejsze czary)";
                    break;
            }
        }

        public void ApplyState(CampState newState, bool fireEvent = true)
        {
            State = newState;
            if (GameManager.Instance != null)
                GameManager.Instance.SetCampState(campId, newState);

            switch (newState)
            {
                case CampState.Active:
                    if (campFireLight != null) campFireLight.color = activeLightColor;
                    SetSmoke(true, new Color(0.3f, 0.2f, 0.1f));
                    break;
                case CampState.Cleared:
                    if (campFireLight != null)
                    {
                        campFireLight.color = clearedLightColor;
                        campFireLight.intensity *= 0.5f;
                    }
                    SetSmoke(false);
                    if (totem != null && !totem.IsDead) totem.DimLight();
                    break;
                case CampState.Captured:
                    if (campFireLight != null)
                    {
                        campFireLight.color = capturedLightColor;
                        campFireLight.intensity *= 1.5f;
                    }
                    SetSmoke(true, new Color(0.4f, 1f, 0.6f));
                    // Zmień kolor palisad na lekko zielony
                    if (palisadeRenderers != null)
                    {
                        var m = MaterialFactory.Get(new Color(0.25f, 0.4f, 0.22f), 0.2f, new Color(0.1f, 0.4f, 0.15f), 0.2f);
                        foreach (var r in palisadeRenderers) if (r != null) r.sharedMaterial = m;
                    }
                    // Wyłącz potem żywych goblinów (gracz podbił "cleared" więc i tak są martwi)
                    break;
            }

            if (fireEvent) OnStateChanged?.Invoke(newState);
        }

        private void SetSmoke(bool on, Color? color = null)
        {
            if (smokeSystem == null) return;
            var main = smokeSystem.main;
            if (on)
            {
                if (color.HasValue) main.startColor = new ParticleSystem.MinMaxGradient(color.Value);
                if (!smokeSystem.isPlaying) smokeSystem.Play();
            }
            else
            {
                if (smokeSystem.isPlaying) smokeSystem.Stop();
            }
        }
    }
}
