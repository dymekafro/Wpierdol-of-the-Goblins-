using UnityEngine;
using WPG.Core;

namespace WPG.Items
{
    // Mapuje id itemu na sprite z paczek projektu (Modern RPG icons, Fantasy Free GUI).
    // Fallback: kolor z ItemDefinition.iconColor na ramce GuiIconFrame.
    public static class ItemIconResolver
    {
        public static Sprite GetIcon(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            ItemDatabase.EnsureInitialized();

            switch (itemId)
            {
                case "health_potion":
                    return Load(GameAssetPaths.IconHeal, GameAssetPaths.ResIconHeal);
                case "flame_wand":
                case "shaman_totem_fragment":
                    return Load(GameAssetPaths.IconFireball, GameAssetPaths.ResIconFire);
                case "mana_amulet":
                    return Load(GameAssetPaths.IconAmulet, GameAssetPaths.ResIconAmulet)
                           ?? Load(GameAssetPaths.IconHeal, GameAssetPaths.ResIconHeal);
                case "goblin_tooth":
                case "iron_shard":
                    return Load(GameAssetPaths.IconMelee, GameAssetPaths.ResIconMelee);
                case "moss_clump":
                case "glow_mushroom":
                    return Load(GameAssetPaths.IconHerb, GameAssetPaths.ResIconHerb);
                default:
                    var def = ItemDatabase.Get(itemId);
                    if (def == null) return null;
                    switch (def.itemType)
                    {
                        case ItemType.Consumable:
                            return Load(GameAssetPaths.IconHeal, GameAssetPaths.ResIconHeal);
                        case ItemType.Weapon:
                            return Load(GameAssetPaths.IconMelee, GameAssetPaths.ResIconMelee)
                                   ?? Load(GameAssetPaths.IconFireball, GameAssetPaths.ResIconFire);
                        case ItemType.Relic:
                            return Load(GameAssetPaths.IconAmulet, GameAssetPaths.ResIconAmulet);
                        default:
                            return Load(GameAssetPaths.IconHerb, GameAssetPaths.ResIconHerb);
                    }
            }
        }

        public static Color GetTint(string itemId)
        {
            var def = ItemDatabase.Get(itemId);
            if (def != null) return def.iconColor;
            return Color.white;
        }

        private static Sprite Load(string[] paths, string resPath)
        {
            return GameAssetLoader.LoadSprite(paths, resPath);
        }
    }
}
