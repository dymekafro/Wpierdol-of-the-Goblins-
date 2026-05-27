using System;
using UnityEngine;
using WPG.Character;
using WPG.Core;

namespace WPG.Player
{
    public class PlayerStats : MonoBehaviour, IDamageReceiver
    {
        public PlayerAttributes attributes;

        public int currentHealth;
        public float currentMana;

        public event Action<int, int> OnHealthChanged;   // current, max
        public event Action<int, int> OnManaChanged;     // current, max
        public event Action OnDied;

        public bool IsDead { get; private set; }

        public float manaRegenBonus = 0f; // np. obozy Captured

        private float _baseRegenTimer;

        public void Init(PlayerAttributes attrs, int? hp = null, int? mana = null)
        {
            attributes = attrs ?? PlayerAttributes.CreateDruidBase();
            currentHealth = hp ?? attributes.MaxHealth;
            currentMana = mana ?? attributes.MaxMana;
            IsDead = false;
            OnHealthChanged?.Invoke(currentHealth, attributes.MaxHealth);
            OnManaChanged?.Invoke(Mathf.RoundToInt(currentMana), attributes.MaxMana);
        }

        private void Update()
        {
            if (IsDead) return;
            if (currentMana < attributes.MaxMana)
            {
                currentMana += (attributes.ManaRegenPerSecond + manaRegenBonus) * Time.deltaTime;
                if (currentMana > attributes.MaxMana) currentMana = attributes.MaxMana;
                OnManaChanged?.Invoke(Mathf.RoundToInt(currentMana), attributes.MaxMana);
            }
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                IsDead = true;
                OnHealthChanged?.Invoke(currentHealth, attributes.MaxHealth);
                OnDied?.Invoke();
                return;
            }
            OnHealthChanged?.Invoke(currentHealth, attributes.MaxHealth);
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            currentHealth = Mathf.Min(currentHealth + amount, attributes.MaxHealth);
            OnHealthChanged?.Invoke(currentHealth, attributes.MaxHealth);
        }

        public void RestoreMana(int amount)
        {
            currentMana = Mathf.Min(currentMana + amount, attributes.MaxMana);
            OnManaChanged?.Invoke(Mathf.RoundToInt(currentMana), attributes.MaxMana);
        }

        public bool TrySpendMana(int amount)
        {
            if (currentMana < amount) return false;
            currentMana -= amount;
            OnManaChanged?.Invoke(Mathf.RoundToInt(currentMana), attributes.MaxMana);
            return true;
        }

        public void FullRestore()
        {
            currentHealth = attributes.MaxHealth;
            currentMana = attributes.MaxMana;
            IsDead = false;
            OnHealthChanged?.Invoke(currentHealth, attributes.MaxHealth);
            OnManaChanged?.Invoke(Mathf.RoundToInt(currentMana), attributes.MaxMana);
        }

        public void ReviveAt(Vector3 position)
        {
            // CharacterController nie pozwala teleportować przez transform.position bezpośrednio
            // gdy jest aktywny. Tymczasowo wyłączamy.
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            transform.position = position;
            if (cc != null) cc.enabled = true;
            FullRestore();
        }

        public void ReceiveDamage(int amount, Vector3 hitPoint)
        {
            TakeDamage(amount);
        }
    }
}
