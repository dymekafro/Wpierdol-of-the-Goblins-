using System;
using UnityEngine;

namespace WPG.Items
{
    public enum ItemType
    {
        Material,
        Consumable,
        Weapon,
        Relic,
        Quest,
        NaturePoint
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }

    // Składnik przepisu (id itemu + ilość). Używane też ogólnie do par item/ilość.
    [Serializable]
    public struct ItemAmount
    {
        public string itemId;
        public int amount;

        public ItemAmount(string itemId, int amount)
        {
            this.itemId = itemId;
            this.amount = amount;
        }
    }

    // Definicja itemu. ScriptableObject — w MVP tworzona runtime przez ItemDatabase,
    // ale gotowa do zapisania jako asset w Assets/_Game/ScriptableObjects/Items/.
    public class ItemDefinition : ScriptableObject
    {
        public string id;
        public string displayNamePL;
        [TextArea] public string descriptionPL;

        public ItemType itemType = ItemType.Material;
        public ItemRarity rarity = ItemRarity.Common;
        public int stackMax = 99;

        // Placeholder ikony — kolor kafelka w UI (brak gwarantowanych sprite'ów ikon w MVP).
        public Color iconColor = Color.white;

        [Header("Efekt — Consumable")]
        public int healAmount;          // +HP przy użyciu

        [Header("Efekt — Relic")]
        public float manaRegenBonus;    // +mana/s gdy posiadany

        [Header("Efekt — Weapon")]
        public int fireballManaDiscount; // obniżka kosztu fireballa gdy posiadany

        [Header("Crafting (niepuste = przepis produkujący ten item)")]
        public ItemAmount[] craftInputs;
        public int craftOutputAmount = 1;

        public bool IsCraftable => craftInputs != null && craftInputs.Length > 0;

        public static ItemDefinition Create(
            string id,
            string displayNamePL,
            string descriptionPL,
            ItemType itemType,
            ItemRarity rarity,
            int stackMax,
            Color iconColor)
        {
            var def = CreateInstance<ItemDefinition>();
            def.name = id;
            def.id = id;
            def.displayNamePL = displayNamePL;
            def.descriptionPL = descriptionPL;
            def.itemType = itemType;
            def.rarity = rarity;
            def.stackMax = stackMax;
            def.iconColor = iconColor;
            return def;
        }

        public static Color RarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Uncommon: return new Color(0.45f, 0.85f, 0.4f);
                case ItemRarity.Rare: return new Color(0.4f, 0.6f, 1f);
                case ItemRarity.Epic: return new Color(0.75f, 0.45f, 0.95f);
                default: return new Color(0.78f, 0.78f, 0.78f);
            }
        }
    }
}
