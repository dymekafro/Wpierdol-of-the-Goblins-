using System;
using UnityEngine;
using WPG.Core;
using WPG.Player;

namespace WPG.World
{
    // Trigger bezpiecznej strefy: heal + save + spawn point.
    public class DruidBase : MonoBehaviour
    {
        public Vector3 spawnPoint;
        public string zoneName = "Sady Ostatniego Strażnika";
        public float healInterval = 1f;
        public int healAmountPerTick = 8;
        public int manaPerTick = 6;
        public float saveCooldown = 5f;

        private float _nextHealAt;
        private float _nextSaveAt;
        private bool _playerInside;
        private PlayerStats _playerStats;
        private Transform _playerT;

        public static event Action OnGameSaved;
        public static event Action OnPlayerEnter;
        public static event Action OnPlayerExit;

        public bool IsPlayerInside => _playerInside;
        public static bool IsPlayerInsideStatic { get; private set; }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInside = true;
            IsPlayerInsideStatic = true;
            _playerStats = other.GetComponent<PlayerStats>();
            _playerT = other.transform;
            WorldZone.RaiseExternal(zoneName);
            // Natychmiastowy heal i save przy wejściu
            if (_playerStats != null)
            {
                _playerStats.Heal(_playerStats.attributes.MaxHealth);
                _playerStats.RestoreMana(_playerStats.attributes.MaxMana);
            }
            TrySave();
            OnPlayerEnter?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInside = false;
            IsPlayerInsideStatic = false;
            _playerStats = null;
            _playerT = null;
            WorldZone.RaiseExternal("Magiczny Las");
            OnPlayerExit?.Invoke();
        }

        private void Update()
        {
            if (!_playerInside || _playerStats == null) return;
            if (Time.time >= _nextHealAt)
            {
                _nextHealAt = Time.time + healInterval;
                _playerStats.Heal(healAmountPerTick);
                _playerStats.RestoreMana(manaPerTick);
            }
            if (Time.time >= _nextSaveAt)
            {
                _nextSaveAt = Time.time + saveCooldown;
                TrySave();
            }
        }

        private void TrySave()
        {
            if (GameManager.Instance == null || _playerStats == null || _playerT == null) return;
            var data = GameManager.Instance.BuildSaveData(
                _playerT.position,
                _playerStats.currentHealth,
                Mathf.RoundToInt(_playerStats.currentMana),
                zoneName);
            SaveSystem.Save(data);
            OnGameSaved?.Invoke();
        }
    }
}
