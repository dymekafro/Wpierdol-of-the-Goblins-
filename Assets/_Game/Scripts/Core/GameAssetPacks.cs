using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WPG.Core
{
    /// <summary>
    /// Katalog paczek Asset Store — foldery, prefaby, powiązanie ze slotami GameAssetRegistry.
    /// </summary>
    public static class GameAssetPacks
    {
        public enum PackStatus
        {
            Imported,
            Missing
        }

        public struct PackEntry
        {
            public string DisplayName;
            public string[] FolderTokens;
            public string[] KeyPrefabPaths;
            public string UsedInCode;
            public GameAssetRegistry.Slot[] RegistrySlots;
        }

        public static readonly PackEntry[] All =
        {
            new PackEntry
            {
                DisplayName = "Fantasy Forest Environment Free Sample",
                FolderTokens = new[] { "fantasy forest" },
                KeyPrefabPaths = new[]
                {
                    "Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/tree_1.prefab",
                    "Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab",
                },
                UsedInCode = "WorldAssetPlacer — drzewa, trawa; GameAssetRegistry WorldTrees/Grass",
                RegistrySlots = new[] { GameAssetRegistry.Slot.TreeLarge, GameAssetRegistry.Slot.Grass },
            },
            new PackEntry
            {
                DisplayName = "Nature Starter Kit 2",
                FolderTokens = new[] { "nature starter", "naturestarter" },
                KeyPrefabPaths = GameAssetPaths.WorldTrees,
                UsedInCode = "WorldAssetPlacer — drzewa, krzaki, trawa",
                RegistrySlots = new[] { GameAssetRegistry.Slot.TreeLarge, GameAssetRegistry.Slot.Bush, GameAssetRegistry.Slot.Grass },
            },
            new PackEntry
            {
                DisplayName = "Rocks HD / Rock_pack",
                FolderTokens = new[] { "rock_pack", "rock pack", "rocks hd", "rockshd" },
                KeyPrefabPaths = GameAssetPaths.WorldRocks,
                UsedInCode = "WorldAssetPlacer.PlaceRock — GameAssetRegistry.PickWorldRock",
                RegistrySlots = new[] { GameAssetRegistry.Slot.Rock },
            },
            new PackEntry
            {
                DisplayName = "RPG Dungeon Pack",
                FolderTokens = new[] { "rpg dungeon", "dungeon pack", "dungeon kit" },
                KeyPrefabPaths = GameAssetPaths.WorldRuins,
                UsedInCode = "WorldAssetPlacer.PlaceRuin — obozy, PowerSite",
                RegistrySlots = new[] { GameAssetRegistry.Slot.Ruin },
            },
            new PackEntry
            {
                DisplayName = "Invector Third Person Controller LITE",
                FolderTokens = new[] { "invector", "invector-3rdpersoncontroller" },
                KeyPrefabPaths = GameAssetPaths.InvectorCharacter,
                UsedInCode = "GameAssetRegistry.InvectorCharacter — locomotion animator dla GanzSe druida",
                RegistrySlots = new[] { GameAssetRegistry.Slot.InvectorCharacter },
            },
            new PackEntry
            {
                DisplayName = "GanzSe FREE Modular Character (URP)",
                FolderTokens = new[] { "ganzse", "modular character", "urp ganzse" },
                KeyPrefabPaths = GameAssetPaths.DruidModel,
                UsedInCode = "PlayerBuilder → WorldAssetPlacer.CharacterModelKind.Druid",
                RegistrySlots = new[] { GameAssetRegistry.Slot.DruidModel },
            },
            new PackEntry
            {
                DisplayName = "Starter Assets Third Person (URP)",
                FolderTokens = new[] { "starter assets", "starterassets", "third person" },
                KeyPrefabPaths = new[]
                {
                    "Assets/Starter Assets/Runtime/ThirdPersonController/Prefabs/PlayerArmature.prefab",
                },
                UsedInCode = "PlayerBuilder — fallback druid (PlayerArmature)",
                RegistrySlots = new[] { GameAssetRegistry.Slot.DruidModel },
            },
            new PackEntry
            {
                DisplayName = "3D Stylized Goblin",
                FolderTokens = new[] { "3d stylized goblin", "stylized goblin" },
                KeyPrefabPaths = GameAssetPaths.GoblinModel,
                UsedInCode = "GoblinStormtrooper → GoblinBase → GoblinMelee",
                RegistrySlots = new[] { GameAssetRegistry.Slot.GoblinModel },
            },
            new PackEntry
            {
                DisplayName = "Stylized Goblins Archer & Warrior",
                FolderTokens = new[] { "stylized goblins", "goblins archer" },
                KeyPrefabPaths = GameAssetPaths.GoblinArcherModel,
                UsedInCode = "GoblinArcher, GoblinStormtrooper (warrior)",
                RegistrySlots = Array.Empty<GameAssetRegistry.Slot>(),
            },
            new PackEntry
            {
                DisplayName = "Fantasy Goblin",
                FolderTokens = new[] { "assets/goblin", "fantasy goblin", "/goblin/prefab" },
                KeyPrefabPaths = new[]
                {
                    GameAssetPaths.FantasyGoblinMeleePrefab,
                    GameAssetPaths.FantasyGoblinArcherPrefab,
                    GameAssetPaths.FantasyGoblinElitePrefab,
                },
                UsedInCode = "GoblinStormtrooper/Archer/ShamanElite → GoblinModel / GoblinArcher / GoblinElite",
                RegistrySlots = new[] { GameAssetRegistry.Slot.GoblinModel, GameAssetRegistry.Slot.GoblinElite },
            },
            new PackEntry
            {
                DisplayName = "Basic RPG Sounds",
                FolderTokens = new[] { "basic rpg sounds", "rpg sounds" },
                KeyPrefabPaths = GameAssetPaths.SfxHit,
                UsedInCode = "GameAudioManager, GoblinBase hit/death",
                RegistrySlots = new[] { GameAssetRegistry.Slot.SfxHit, GameAssetRegistry.Slot.SfxDeath, GameAssetRegistry.Slot.SfxCast },
            },
            new PackEntry
            {
                DisplayName = "Fantasy Free GUI",
                FolderTokens = new[] { "fantasy free gui", "fantasy gui" },
                KeyPrefabPaths = GameAssetPaths.GuiPanel,
                UsedInCode = "MainMenuBootstrap, PlayerHUD, UIFactory",
                RegistrySlots = new[] { GameAssetRegistry.Slot.UiBarFrame },
            },
            new PackEntry
            {
                DisplayName = "Modern RPG icons",
                FolderTokens = new[] { "modern rpg icons", "rpg icons" },
                KeyPrefabPaths = GameAssetPaths.IconMelee,
                UsedInCode = "PlayerHUD — ikony skilli",
                RegistrySlots = Array.Empty<GameAssetRegistry.Slot>(),
            },
            new PackEntry
            {
                DisplayName = "Cartoon FX Remaster / CFX",
                FolderTokens = new[] { "jmo assets", "cartoon fx", "cfxr" },
                KeyPrefabPaths = new[]
                {
                    "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Fire/CFXR Fireball.prefab",
                },
                UsedInCode = "WorldAssetPlacer.TrySpawnVfx — PlayerCombat",
                RegistrySlots = new[] { GameAssetRegistry.Slot.VfxFireball, GameAssetRegistry.Slot.VfxHeal },
            },
            new PackEntry
            {
                DisplayName = "Fantasy Effects Pack",
                FolderTokens = new[] { "fantasy effects", "fantasy effect" },
                KeyPrefabPaths = new[] { "Assets/Fantasy Effects Pack/Prefabs/Fireball.prefab" },
                UsedInCode = "WorldAssetPlacer VFX fallback",
                RegistrySlots = new[] { GameAssetRegistry.Slot.VfxFireball },
            },
            new PackEntry
            {
                DisplayName = "Celestial Cycles",
                FolderTokens = new[] { "celestial cycles", "celestial" },
                KeyPrefabPaths = Array.Empty<string>(),
                UsedInCode = "GoldenHourLighting (opcjonalnie skybox)",
                RegistrySlots = Array.Empty<GameAssetRegistry.Slot>(),
            },
        };

        public static PackStatus GetFolderStatus(PackEntry pack)
        {
#if UNITY_EDITOR
            return IsAnyFolderPresent(pack.FolderTokens) ? PackStatus.Imported : PackStatus.Missing;
#else
            return PackStatus.Missing;
#endif
        }

        public static bool IsAnyFolderPresent(string[] folderTokens)
        {
#if UNITY_EDITOR
            if (folderTokens == null || folderTokens.Length == 0) return false;
            if (!Directory.Exists("Assets")) return false;

            foreach (var dir in Directory.GetDirectories("Assets"))
            {
                string folderName = NormalizeToken(Path.GetFileName(dir));
                foreach (var token in folderTokens)
                {
                    if (string.IsNullOrEmpty(token)) continue;
                    if (folderName.Contains(NormalizeToken(token))) return true;
                }
            }
#endif
            return false;
        }

        public static string NormalizeToken(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var sb = new StringBuilder(value.Length);
            foreach (char c in value.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c)) sb.Append(c);
            }
            return sb.ToString();
        }

        public static void AppendPackReport(StringBuilder sb)
        {
            sb.AppendLine("[GameAssetRegistry] === Imported Packs (Assets/ top-level) ===");

            int imported = 0;
            foreach (var pack in All)
            {
                bool folderOk = GetFolderStatus(pack) == PackStatus.Imported;
                if (folderOk) imported++;

                string status = folderOk ? "FOUND" : "MISSING";
                sb.AppendLine($"  [{status,-7}] {pack.DisplayName}");

                if (folderOk)
                {
                    string resolvedFolder = FindFirstMatchingFolder(pack.FolderTokens);
                    if (!string.IsNullOrEmpty(resolvedFolder))
                        sb.AppendLine($"           folder: Assets/{resolvedFolder}");
                }
                else
                {
                    sb.AppendLine("           → Package Manager → My Assets → Download / Import into project");
                }
            }

            sb.AppendLine($"[GameAssetRegistry] Packs on disk: {imported}/{All.Length}");
        }

#if UNITY_EDITOR
        public static string FindFirstMatchingFolder(string[] folderTokens)
        {
            if (folderTokens == null) return null;
            foreach (var dir in Directory.GetDirectories("Assets"))
            {
                string name = Path.GetFileName(dir);
                string norm = NormalizeToken(name);
                foreach (var token in folderTokens)
                {
                    if (norm.Contains(NormalizeToken(token))) return name;
                }
            }
            return null;
        }

        public static bool PathMatchesPackFolder(string assetPath, string[] folderTokens)
        {
            if (string.IsNullOrEmpty(assetPath) || folderTokens == null) return false;
            string normPath = NormalizeToken(assetPath);
            foreach (var token in folderTokens)
            {
                if (normPath.Contains(NormalizeToken(token))) return true;
            }
            return false;
        }
#endif
    }
}
