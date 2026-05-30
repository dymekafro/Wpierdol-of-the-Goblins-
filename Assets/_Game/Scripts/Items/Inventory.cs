using System;
using System.Collections.Generic;
using UnityEngine;
using WPG.Core;
using WPG.Player;

namespace WPG.Items
{
    // Ekwipunek gracza: 16 slotów, stackowanie wg ItemDefinition.stackMax,
    // crafting, pasywne bonusy relikwii/broni oraz serializacja do SaveData.
    public class Inventory : MonoBehaviour
    {
        public const int SlotCount = 16;
        public const int HotbarCount = 4;

        private readonly InventorySlot[] _slots = new InventorySlot[SlotCount];

        // Wywoływane po każdej zmianie zawartości (UI nasłuchuje).
        public event Action OnChanged;

        private PlayerStats _stats;
        private PlayerCombat _combat;

        private void Awake()
        {
            ItemDatabase.EnsureInitialized();
            for (int i = 0; i < SlotCount; i++)
                _slots[i] = new InventorySlot();
        }

        public void Bind(PlayerStats stats, PlayerCombat combat)
        {
            _stats = stats;
            _combat = combat;
            RecomputeEquipmentBonuses();
        }

        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= SlotCount) return null;
            return _slots[index];
        }

        public int Count(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return 0;
            int total = 0;
            foreach (var slot in _slots)
                if (!slot.IsEmpty && slot.itemId == itemId)
                    total += slot.quantity;
            return total;
        }

        public bool HasItem(string itemId, int amount = 1) => Count(itemId) >= amount;

        // Dodaje itemy; wypełnia istniejące stacki, potem puste sloty.
        // Zwraca true gdy całość się zmieściła.
        public bool TryAdd(string itemId, int amount = 1)
        {
            var def = ItemDatabase.Get(itemId);
            if (def == null || amount <= 0) return false;

            int remaining = amount;
            int stackMax = Mathf.Max(1, def.stackMax);

            // 1) dopełnij istniejące stacki
            foreach (var slot in _slots)
            {
                if (remaining <= 0) break;
                if (slot.IsEmpty || slot.itemId != itemId) continue;
                int space = stackMax - slot.quantity;
                if (space <= 0) continue;
                int add = Mathf.Min(space, remaining);
                slot.quantity += add;
                remaining -= add;
            }

            // 2) zajmij puste sloty
            foreach (var slot in _slots)
            {
                if (remaining <= 0) break;
                if (!slot.IsEmpty) continue;
                int add = Mathf.Min(stackMax, remaining);
                slot.Set(itemId, add);
                remaining -= add;
            }

            bool fitAll = remaining <= 0;
            if (remaining != amount)
            {
                RecomputeEquipmentBonuses();
                OnChanged?.Invoke();
            }
            return fitAll;
        }

        // Usuwa amount sztuk danego itemu z dowolnych slotów. Zwraca true gdy było wystarczająco.
        public bool TryRemove(string itemId, int amount = 1)
        {
            if (!HasItem(itemId, amount)) return false;

            int remaining = amount;
            foreach (var slot in _slots)
            {
                if (remaining <= 0) break;
                if (slot.IsEmpty || slot.itemId != itemId) continue;
                int take = Mathf.Min(slot.quantity, remaining);
                slot.quantity -= take;
                remaining -= take;
                if (slot.quantity <= 0) slot.Clear();
            }

            RecomputeEquipmentBonuses();
            OnChanged?.Invoke();
            return true;
        }

        // --- Crafting ---

        public bool CanCraft(ItemDefinition recipe)
        {
            if (recipe == null || !recipe.IsCraftable) return false;
            foreach (var input in recipe.craftInputs)
                if (Count(input.itemId) < input.amount) return false;
            return true;
        }

        public bool TryCraft(ItemDefinition recipe)
        {
            if (!CanCraft(recipe)) return false;

            foreach (var input in recipe.craftInputs)
                TryRemove(input.itemId, input.amount);

            TryAdd(recipe.id, Mathf.Max(1, recipe.craftOutputAmount));
            GameAudioManager.EnsureExists()?.PlayUIClick();
            return true;
        }

        // --- Użycie consumable ---

        public bool UseSlot(int index)
        {
            var slot = GetSlot(index);
            if (slot == null || slot.IsEmpty) return false;
            var def = slot.Definition;
            if (def == null || def.itemType != ItemType.Consumable) return false;

            if (_stats == null || _stats.IsDead) return false;
            if (def.healAmount > 0 && _stats.currentHealth >= _stats.attributes.MaxHealth)
                return false; // pełne HP — nie marnuj mikstury

            if (def.healAmount > 0) _stats.Heal(def.healAmount);

            TryRemove(def.id, 1);
            return true;
        }

        // --- Bonusy ekwipunku (MVP: aktywne gdy item jest w ekwipunku) ---

        public void RecomputeEquipmentBonuses()
        {
            float relicMana = 0f;
            int fireballDiscount = 0;

            foreach (var slot in _slots)
            {
                if (slot.IsEmpty) continue;
                var def = slot.Definition;
                if (def == null) continue;
                if (def.itemType == ItemType.Relic)
                    relicMana += def.manaRegenBonus * slot.quantity;
                if (def.itemType == ItemType.Weapon)
                    fireballDiscount = Mathf.Max(fireballDiscount, def.fireballManaDiscount);
            }

            if (_stats != null) _stats.relicManaRegenBonus = relicMana;
            if (_combat != null) _combat.fireballManaDiscount = fireballDiscount;
        }

        // Materiały startowe — pozwalają od razu przetestować wszystkie 3 przepisy.
        public void GiveStarterItems()
        {
            TryAdd("moss_clump", 4);
            TryAdd("goblin_tooth", 3);
            TryAdd("glow_mushroom", 1);
            TryAdd("iron_shard", 2);
            TryAdd("shaman_totem_fragment", 1);
        }

        public bool IsEmpty()
        {
            foreach (var slot in _slots)
                if (!slot.IsEmpty) return false;
            return true;
        }

        // --- Save / Load ---

        public List<ItemStackSave> ToSaveList()
        {
            var list = new List<ItemStackSave>();
            foreach (var slot in _slots)
                if (!slot.IsEmpty)
                    list.Add(new ItemStackSave { itemId = slot.itemId, quantity = slot.quantity });
            return list;
        }

        public void LoadFromSave(List<ItemStackSave> saved)
        {
            foreach (var slot in _slots) slot.Clear();
            if (saved != null)
            {
                foreach (var entry in saved)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.itemId)) continue;
                    TryAdd(entry.itemId, entry.quantity);
                }
            }
            RecomputeEquipmentBonuses();
            OnChanged?.Invoke();
        }
    }
}
