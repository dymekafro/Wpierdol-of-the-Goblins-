using System.Collections.Generic;
using UnityEngine;

namespace WPG.Items
{
    // Runtime registry MVP itemów. W MVP itemy są hardcode'owane tu (ScriptableObject
    // tworzone w pamięci); docelowo można je zastąpić assetami z Resources/ScriptableObjects.
    public static class ItemDatabase
    {
        private static readonly Dictionary<string, ItemDefinition> _byId = new Dictionary<string, ItemDefinition>();
        private static readonly List<ItemDefinition> _all = new List<ItemDefinition>();
        private static readonly List<ItemDefinition> _recipes = new List<ItemDefinition>();
        private static bool _initialized;

        public static IReadOnlyList<ItemDefinition> All
        {
            get { EnsureInitialized(); return _all; }
        }

        // Itemy z niepustym craftInputs — przepisy MVP.
        public static IReadOnlyList<ItemDefinition> Recipes
        {
            get { EnsureInitialized(); return _recipes; }
        }

        public static ItemDefinition Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            EnsureInitialized();
            return _byId.TryGetValue(id, out var def) ? def : null;
        }

        public static string DisplayName(string id)
        {
            var def = Get(id);
            return def != null ? def.displayNamePL : id;
        }

        public static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            // --- Materiały ---
            Register("moss_clump", "Kępa Mchu",
                "Wilgotny mech z dna lasu. Podstawowy składnik mikstur.",
                ItemType.Material, ItemRarity.Common, 99, new Color(0.4f, 0.6f, 0.3f));

            Register("goblin_tooth", "Ząb Goblinów",
                "Wyszczerbiony kieł. Goblini wierzą, że niesie szczęście.",
                ItemType.Material, ItemRarity.Common, 99, new Color(0.85f, 0.82f, 0.7f));

            Register("glow_mushroom", "Świecący Grzyb",
                "Bioluminescencyjny grzyb z głębi lasu. Pulsuje magią.",
                ItemType.Material, ItemRarity.Uncommon, 99, new Color(0.4f, 0.8f, 0.9f));

            Register("iron_shard", "Odłamek Żelaza",
                "Kawałek zardzewiałego żelaza. Przyda się przy zbrojeniu.",
                ItemType.Material, ItemRarity.Common, 99, new Color(0.6f, 0.62f, 0.66f));

            Register("shaman_totem_fragment", "Fragment Totemu",
                "Odłamek goblińskiego totemu, wciąż drży od pierwotnej mocy.",
                ItemType.Material, ItemRarity.Rare, 99, new Color(0.8f, 0.5f, 0.25f));

            // --- Mikstura Życia (consumable) — moss_clump ×2 ---
            var potion = Register("health_potion", "Mikstura Życia",
                "Leśny wywar. Przywraca +40 HP.",
                ItemType.Consumable, ItemRarity.Uncommon, 20, new Color(0.9f, 0.25f, 0.3f));
            potion.healAmount = 40;
            potion.craftInputs = new[] { new ItemAmount("moss_clump", 2) };
            potion.craftOutputAmount = 1;

            // --- Amulet Leśnej Many (relic) — goblin_tooth ×3 + glow_mushroom ×1 ---
            var amulet = Register("mana_amulet", "Amulet Leśnej Many",
                "Splot korzeni i kłów. Przyspiesza regenerację many o 2/s.",
                ItemType.Relic, ItemRarity.Rare, 1, new Color(0.45f, 0.7f, 1f));
            amulet.manaRegenBonus = 2f;
            amulet.craftInputs = new[]
            {
                new ItemAmount("goblin_tooth", 3),
                new ItemAmount("glow_mushroom", 1)
            };
            amulet.craftOutputAmount = 1;

            // --- Różdżka Żaru (weapon) — iron_shard ×2 + shaman_totem_fragment ×1 ---
            var wand = Register("flame_wand", "Różdżka Żaru",
                "Różdżka tląca się żarem. Obniża koszt many Ognistego Ciosu o 3.",
                ItemType.Weapon, ItemRarity.Epic, 1, new Color(1f, 0.5f, 0.15f));
            wand.fireballManaDiscount = 3;
            wand.craftInputs = new[]
            {
                new ItemAmount("iron_shard", 2),
                new ItemAmount("shaman_totem_fragment", 1)
            };
            wand.craftOutputAmount = 1;

            _recipes.Clear();
            foreach (var def in _all)
                if (def.IsCraftable) _recipes.Add(def);
        }

        private static ItemDefinition Register(string id, string namePL, string descPL,
            ItemType type, ItemRarity rarity, int stackMax, Color iconColor)
        {
            var def = ItemDefinition.Create(id, namePL, descPL, type, rarity, stackMax, iconColor);
            _byId[id] = def;
            _all.Add(def);
            return def;
        }
    }
}
