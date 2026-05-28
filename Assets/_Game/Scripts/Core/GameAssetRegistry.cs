using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WPG.Core
{
    /// <summary>
    /// Centralny rejestr prefabów / clipów — skan Assets/, cache, raport przy starcie.
    /// </summary>
    public static class GameAssetRegistry
    {
        public enum Slot
        {
            TreeLarge,
            TreeSmall,
            Bush,
            Grass,
            Rock,
            Ruin,
            DruidModel,
            GoblinModel,
            GoblinElite,
            Totem,
            CampFire,
            VfxFireball,
            VfxHeal,
            SfxHit,
            SfxDeath,
            SfxCast,
            UiBarFrame,
            InvectorCharacter,
            InvectorController,
            InvectorCamera,
            TexturePath,
            TextureGrass,
            SkyboxMaterial,
        }

        static readonly Dictionary<Slot, string> PrimaryPaths = new Dictionary<Slot, string>
        {
            { Slot.TreeLarge, "Assets/_Game/Prefabs/World/TreeLarge.prefab" },
            { Slot.TreeSmall, "Assets/_Game/Prefabs/World/TreeSmall.prefab" },
            { Slot.Bush, "Assets/_Game/Prefabs/World/Bush.prefab" },
            { Slot.Grass, "Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab" },
            { Slot.Rock, "Assets/_Game/Prefabs/World/Rock.prefab" },
            { Slot.Ruin, "Assets/_Game/Prefabs/World/Ruin.prefab" },
            { Slot.DruidModel, GameAssetPaths.GanzSeDruidPrefab },
            { Slot.GoblinModel, GameAssetPaths.FantasyGoblinMeleePrefab },
            { Slot.GoblinElite, GameAssetPaths.FantasyGoblinElitePrefab },
            { Slot.Totem, "Assets/_Game/Prefabs/Enemies/Totem.prefab" },
            { Slot.CampFire, "Assets/_Game/Prefabs/World/CampFire.prefab" },
            { Slot.VfxFireball, "Assets/_Game/Prefabs/VFX/VfxFireball.prefab" },
            { Slot.VfxHeal, "Assets/_Game/Prefabs/VFX/VfxHeal.prefab" },
            { Slot.UiBarFrame, "Assets/_Game/Prefabs/UI/UiBarFrame.prefab" },
            { Slot.InvectorCharacter, GameAssetPaths.InvectorCharacterPrefab },
            { Slot.InvectorController, GameAssetPaths.InvectorThirdPersonPrefab },
            { Slot.InvectorCamera, "Assets/Invector-3rdPersonController_LITE/Prefabs/vThirdPersonCamera_LITE.prefab" },
            { Slot.TexturePath, GameAssetPaths.TexturePath[0] },
            { Slot.TextureGrass, GameAssetPaths.TextureGrass[0] },
            { Slot.SkyboxMaterial, GameAssetPaths.SkyboxMaterial[0] },
        };

        static readonly Dictionary<Slot, string[]> FallbackTokens = new Dictionary<Slot, string[]>
        {
            { Slot.TreeLarge, new[] { "treelarge", "tree_large", "tree_1", "tree1", "pine", "fir", "oak" } },
            { Slot.TreeSmall, new[] { "treesmall", "tree_small", "tree_2", "tree2", "sapling" } },
            { Slot.Bush, new[] { "bush", "shrub", "fern", "plant" } },
            { Slot.Grass, new[] { "grass01", "grass_01", "grass_mesh", "grassmesh", "grass" } },
            { Slot.Rock, new[] { "rock", "stone", "boulder", "cliff" } },
            { Slot.Ruin, new[] { "ruin", "ruins", "altar", "shrine", "pillar", "column", "wall", "arch" } },
            { Slot.DruidModel, new[] { "druidmodel", "druid_model", "player_druid", "ganzse", "ganzse free modular", "modular character", "modular_character", "urp ganzse", "starter_armature", "playerarmature", "invector", "third person", "basic locomotion", "thirdpersoncontroller", "vbot" } },
            { Slot.InvectorCharacter, new[] { "invector", "third person", "basic locomotion", "thirdpersoncontroller", "thirdpersoncontroller_lite", "vbot", "vbot2", "invector@basiclocomotion" } },
            { Slot.InvectorController, new[] { "thirdpersoncontroller_lite", "thirdpersoncontroller", "invector@basiclocomotion", "invectorcontroller", "invector" } },
            { Slot.InvectorCamera, new[] { "vthirdpersoncamera_lite", "vthirdpersoncamera", "invectorcamera" } },
            { Slot.GoblinModel, new[] { "goblin_stonesword", "skin1", "fantasy goblin", "fantasy_goblin", "assets/goblin", "goblinmodel", "goblin_model", "goblin_storm", "goblin_warrior", "goblin warrior", "stylized goblin", "stylized_goblin", "goblin_melee", "goblin" } },
            { Slot.GoblinElite, new[] { "skin3", "skin2", "goblinelite", "goblin_elite", "goblin_shaman", "shaman", "fantasy goblin", "fantasy_goblin" } },
            { Slot.Totem, new[] { "totem" } },
            { Slot.CampFire, new[] { "campfire", "camp_fire" } },
            { Slot.VfxFireball, new[] { "vfxfireball", "fireball", "vfx_fireball", "cfxrfireball", "cfxfireball", "cfxr fireball", "cfx_fireball" } },
            { Slot.VfxHeal, new[] { "vfxheal", "healfx", "vfx_heal", "cfxr2healing", "cfxr2 healing", "cfx_magic", "cfxmagicstars", "healing" } },
            { Slot.UiBarFrame, new[] { "uibarframe", "bar_frame", "icon_frame", "iconframe", "slot" } },
        };

        static readonly Dictionary<Slot, UnityEngine.Object> Cache = new Dictionary<Slot, UnityEngine.Object>();
        static readonly Dictionary<Slot, string> ResolvedPaths = new Dictionary<Slot, string>();
        static readonly List<GameObject> WorldTreePool = new List<GameObject>();
        static readonly List<GameObject> WorldBushPool = new List<GameObject>();
        static readonly List<GameObject> WorldGrassPool = new List<GameObject>();
        static readonly List<GameObject> WorldRockPool = new List<GameObject>();
        static readonly List<GameObject> WorldRuinPool = new List<GameObject>();
        static bool _initialized;
        static bool _worldPoolsBuilt;

        public static GameObject TreeLarge => GetPrefab(Slot.TreeLarge);
        public static GameObject TreeSmall => GetPrefab(Slot.TreeSmall);
        public static GameObject Bush => GetPrefab(Slot.Bush);
        public static GameObject Grass => GetPrefab(Slot.Grass);
        public static GameObject Rock => GetPrefab(Slot.Rock);
        public static GameObject Ruin => GetPrefab(Slot.Ruin);
        public static GameObject DruidModel => GetPrefab(Slot.DruidModel);
        public static GameObject GoblinModel => GetPrefab(Slot.GoblinModel);
        public static GameObject GoblinElite => GetPrefab(Slot.GoblinElite);
        public static GameObject Totem => GetPrefab(Slot.Totem);
        public static GameObject CampFire => GetPrefab(Slot.CampFire);
        public static GameObject VfxFireball => GetPrefab(Slot.VfxFireball);
        public static GameObject VfxHeal => GetPrefab(Slot.VfxHeal);
        public static AudioClip SfxHit => GetAudio(Slot.SfxHit);
        public static AudioClip SfxDeath => GetAudio(Slot.SfxDeath);
        public static AudioClip SfxCast => GetAudio(Slot.SfxCast);
        public static GameObject UiBarFrame => GetPrefab(Slot.UiBarFrame);
        public static GameObject InvectorCharacter => GetPrefab(Slot.InvectorCharacter);
        public static GameObject InvectorController => GetPrefab(Slot.InvectorController);
        public static GameObject InvectorCamera => GetPrefab(Slot.InvectorCamera);
        public static Texture2D PathDirtTexture => GetTexture(Slot.TexturePath);
        public static Texture2D GrassGroundTexture => GetTexture(Slot.TextureGrass);
        public static Material Skybox => GetMaterial(Slot.SkyboxMaterial);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            Initialize();
            LogReport();
        }

        public static void Initialize(bool force = false)
        {
            if (_initialized && !force) return;
            _initialized = true;
            _worldPoolsBuilt = false;

            Cache.Clear();
            ResolvedPaths.Clear();

            foreach (Slot slot in Enum.GetValues(typeof(Slot)))
            {
                if (!TryResolveSlot(slot, out var asset, out var path))
                    continue;
                Cache[slot] = asset;
                if (!string.IsNullOrEmpty(path))
                    ResolvedPaths[slot] = path;
            }

            BuildWorldPools();
        }

        public static GameObject PickWorldTree(System.Random rng)
        {
            EnsureInitialized();
            return PickFromPool(WorldTreePool, TreeLarge ?? TreeSmall, rng);
        }

        public static GameObject PickWorldBush(System.Random rng)
        {
            EnsureInitialized();
            return PickFromPool(WorldBushPool, Bush, rng);
        }

        public static GameObject PickWorldGrass(System.Random rng)
        {
            EnsureInitialized();
            var fallback = Grass ?? Bush;
            return PickFromPool(WorldGrassPool, fallback, rng);
        }

        public static int WorldGrassPoolCount
        {
            get { EnsureInitialized(); return WorldGrassPool.Count; }
        }

        public static GameObject PickWorldRock(System.Random rng)
        {
            EnsureInitialized();
            return PickFromPool(WorldRockPool, Rock, rng);
        }

        public static GameObject PickWorldRuin(System.Random rng)
        {
            EnsureInitialized();
            return PickFromPool(WorldRuinPool, Ruin, rng);
        }

        static GameObject PickFromPool(List<GameObject> pool, GameObject fallback, System.Random rng)
        {
            if (pool != null && pool.Count > 0)
                return pool[rng.Next(0, pool.Count)];
            return fallback;
        }

        static void BuildWorldPools()
        {
            if (_worldPoolsBuilt) return;
            _worldPoolsBuilt = true;
            WorldTreePool.Clear();
            WorldBushPool.Clear();
            WorldGrassPool.Clear();
            WorldRockPool.Clear();
            WorldRuinPool.Clear();

            if (TreeLarge != null) WorldTreePool.Add(TreeLarge);
            if (TreeSmall != null && TreeSmall != TreeLarge) WorldTreePool.Add(TreeSmall);
            if (Grass != null) AddUnique(WorldGrassPool, Grass);

#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (ShouldExcludeWorldPath(path)) continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                string lower = path.ToLowerInvariant();
                string name = prefab.name.ToLowerInvariant();

                if (IsWorldRuinPath(lower, name)) { AddUnique(WorldRuinPool, prefab); continue; }
                if (IsWorldRockPath(lower, name)) { AddUnique(WorldRockPool, prefab); continue; }
                if (IsWorldGrassPath(lower, name)) { AddUnique(WorldGrassPool, prefab); continue; }
                if (IsWorldBushPath(lower, name)) { AddUnique(WorldBushPool, prefab); continue; }
                if (IsWorldTreePath(lower, name)) AddUnique(WorldTreePool, prefab);
            }
#endif
        }

        static bool IsWorldGrassPath(string lower, string name)
        {
            if (lower.Contains("fantasy forest"))
                return name.StartsWith("grass") || name.Contains("grass01") || name.Contains("grassmesh");
            if (lower.Contains("nature starter"))
                return name.Contains("grass");
            return name == "grass" || name.StartsWith("grass_") || name.StartsWith("grass01");
        }

        static void AddUnique(List<GameObject> list, GameObject prefab)
        {
            if (prefab == null) return;
            foreach (var existing in list)
                if (existing != null && existing.name == prefab.name) return;
            list.Add(prefab);
        }

        static bool ShouldExcludeWorldPath(string path)
        {
            string lower = path.ToLowerInvariant();
            if (lower.Contains("khs lava") || lower.Contains("lava tube")) return true;
            if (lower.Contains("/editor/") || lower.Contains("gizmos")) return true;
            if (lower.Contains("_game/prefabs/enemies") || lower.Contains("_game/prefabs/player")) return true;
            if (lower.Contains("jmo assets") || lower.Contains("cfxr") || lower.Contains("cartoon fx")) return true;
            return false;
        }

        static bool IsWorldTreePath(string lower, string name)
        {
            if (lower.Contains("nature starter") || lower.Contains("fantasy forest"))
                return name.Contains("tree") || name.Contains("pine") || name.Contains("fir");
            return (name.Contains("tree") || name.Contains("pine") || name.Contains("oak")) &&
                   !name.Contains("palm") && !lower.Contains("christmas");
        }

        static bool IsWorldBushPath(string lower, string name)
        {
            if (lower.Contains("nature starter"))
                return name.Contains("bush") || name.Contains("shrub") || name.Contains("fern") || name.Contains("plant");
            return name.Contains("bush") || name.Contains("shrub") || name.Contains("fern");
        }

        static bool IsWorldRockPath(string lower, string name)
        {
            if (lower.Contains("rock_pack") || lower.Contains("rock pack") || lower.Contains("rocks hd"))
                return name.Contains("rock") || name.Contains("stone") || name.Contains("boulder") || name.Contains("cliff");
            return (name.Contains("rock") || name.Contains("boulder")) && !name.Contains("rocket");
        }

        static bool IsWorldRuinPath(string lower, string name)
        {
            if (lower.Contains("rpg dungeon") || lower.Contains("dungeon pack"))
                return name.Contains("wall") || name.Contains("column") || name.Contains("pillar") ||
                       name.Contains("arch") || name.Contains("ruin") || name.Contains("floor") ||
                       name.Contains("stairs") || name.Contains("barrel");
            return false;
        }

        public static GameObject GetPrefab(Slot slot)
        {
            EnsureInitialized();
            return Cache.TryGetValue(slot, out var obj) ? obj as GameObject : null;
        }

        public static AudioClip GetAudio(Slot slot)
        {
            EnsureInitialized();
            return Cache.TryGetValue(slot, out var obj) ? obj as AudioClip : null;
        }

        public static Texture2D GetTexture(Slot slot)
        {
            EnsureInitialized();
            return Cache.TryGetValue(slot, out var obj) ? obj as Texture2D : null;
        }

        public static Material GetMaterial(Slot slot)
        {
            EnsureInitialized();
            return Cache.TryGetValue(slot, out var obj) ? obj as Material : null;
        }

        public static bool TryGetPath(Slot slot, out string path)
        {
            EnsureInitialized();
            return ResolvedPaths.TryGetValue(slot, out path);
        }

        public static void LogReport()
        {
            EnsureInitialized();

            var sb = new StringBuilder();
            GameAssetPacks.AppendPackReport(sb);
            sb.AppendLine("[GameAssetRegistry] === Slot Resolution ===");

            int found = 0;
            foreach (Slot slot in Enum.GetValues(typeof(Slot)))
            {
                bool hasAsset = Cache.ContainsKey(slot);
                if (hasAsset) found++;

                ResolvedPaths.TryGetValue(slot, out var path);
                string status = hasAsset ? "OK" : "MISSING";
                string pathInfo = string.IsNullOrEmpty(path) ? "(brak)" : path;
                sb.AppendLine($"  {slot,-14} [{status,-7}] {pathInfo}");
            }

            sb.AppendLine($"[GameAssetRegistry] Slots: {found}/{Enum.GetValues(typeof(Slot)).Length} found");
            sb.AppendLine($"[GameAssetRegistry] World pools: trees={WorldTreePool.Count}, bushes={WorldBushPool.Count}, grass={WorldGrassPool.Count}, rocks={WorldRockPool.Count}, ruins={WorldRuinPool.Count}");

            ReportSlotResolution(sb, Slot.DruidModel, "GanzSe FREE Modular Character lub Starter Assets PlayerArmature");
            ReportSlotResolution(sb, Slot.InvectorCharacter, "Invector Third Person Controller LITE (VBOT2.0_Custom / ThirdPersonController_LITE)");
            ReportSlotResolution(sb, Slot.GoblinModel, "Fantasy Goblin (Assets/Goblin/Prefab/skin1.prefab, goblin_stonesword)");
            ReportSlotResolution(sb, Slot.GoblinElite, "Fantasy Goblin (Assets/Goblin/Prefab/skin3.prefab)");
            ReportSlotResolution(sb, Slot.Grass, "Fantasy Forest Environment (grass01.prefab) lub Nature Starter Kit 2");
            ReportSlotResolution(sb, Slot.Bush, "Nature Starter Kit 2 (Bush.prefab)");
            ReportSlotResolution(sb, Slot.TexturePath, "Fantasy Forest dirt01.tga");
            ReportSlotResolution(sb, Slot.TextureGrass, "Fantasy Forest grass01.tga");
            ReportSlotResolution(sb, Slot.SkyboxMaterial, "Fantasy Forest skyMaterial.mat");

            Debug.Log(sb.ToString());
        }

        static void ReportSlotResolution(StringBuilder sb, Slot slot, string hint)
        {
            if (Cache.ContainsKey(slot) && ResolvedPaths.TryGetValue(slot, out var p))
                sb.AppendLine($"[GameAssetRegistry] FOUND {slot} → {p}");
            else
                sb.AppendLine($"[GameAssetRegistry] MISSING {slot} — placeholder fallback aktywny. Sugestia: zaimportuj {hint}.");
        }

        static void EnsureInitialized()
        {
            if (!_initialized) Initialize();
        }

        static bool TryResolveSlot(Slot slot, out UnityEngine.Object asset, out string path)
        {
            asset = null;
            path = null;

            if (IsAudioSlot(slot))
            {
                asset = LoadAudioSlot(slot, out path);
                return asset != null;
            }

            if (slot == Slot.TexturePath || slot == Slot.TextureGrass)
            {
                string[] paths = slot == Slot.TexturePath ? GameAssetPaths.TexturePath : GameAssetPaths.TextureGrass;
                string res = slot == Slot.TexturePath ? GameAssetPaths.ResTexturePath : GameAssetPaths.ResTextureGrass;
                asset = GameAssetLoader.LoadTexture(paths, res);
                if (asset != null)
                {
#if UNITY_EDITOR
                    path = FindFirstExistingPath(paths) ?? res;
#else
                    path = res;
#endif
                    return true;
                }
                return false;
            }

            if (slot == Slot.SkyboxMaterial)
            {
                foreach (var p in GameAssetPaths.SkyboxMaterial)
                {
                    var mat = GameAssetLoader.LoadMaterial(new[] { p });
                    if (mat == null) continue;
                    asset = mat;
                    path = p;
                    return true;
                }
                return false;
            }

            if (PrimaryPaths.TryGetValue(slot, out var primary) && TryLoadAtPath(primary, slot, out asset))
            {
                path = primary;
                return true;
            }

            if (TryLoadWorldPrimaryPaths(slot, out asset, out path))
                return true;

            if (TryLoadCharacterPrimaryPaths(slot, out asset, out path))
                return true;

            if (TryFindCharacterByScoredScan(slot, out asset, out path))
                return true;

            if (IsVfxSlot(slot) && TryFindVfxByScoredScan(slot, out asset, out path))
                return true;

            return TryFindByScan(slot, out asset, out path);
        }

        static bool TryLoadCharacterPrimaryPaths(Slot slot, out UnityEngine.Object asset, out string path)
        {
            asset = null;
            path = null;
            string[] candidates = null;
            string[] fbxFallback = null;
            switch (slot)
            {
                case Slot.DruidModel:
                    candidates = GameAssetPaths.DruidModel;
                    fbxFallback = GameAssetPaths.DruidModelFbx;
                    break;
                case Slot.GoblinModel:
                    candidates = GameAssetPaths.GoblinModel;
                    break;
                case Slot.GoblinElite:
                    candidates = GameAssetPaths.GoblinElite;
                    break;
                case Slot.InvectorCharacter:
                    candidates = GameAssetPaths.InvectorCharacter;
                    break;
            }

            if (candidates != null)
            {
                foreach (var p in candidates)
                {
                    if (TryLoadAtPath(p, slot, out asset))
                    {
                        path = p;
                        return true;
                    }
                }
            }

            if (fbxFallback == null) return false;
            foreach (var p in fbxFallback)
            {
                if (TryLoadFbxAsPrefab(p, out asset))
                {
                    path = p;
                    return true;
                }
            }
            return false;
        }

#if UNITY_EDITOR
        static bool TryFindCharacterByScoredScan(Slot slot, out UnityEngine.Object asset, out string path)
        {
            asset = null;
            path = null;
            if (!IsCharacterSlot(slot)) return false;

            int bestScore = 0;
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            foreach (var guid in guids)
            {
                string candidatePath = AssetDatabase.GUIDToAssetPath(guid);
                if (ShouldExcludeCharacterPath(candidatePath)) continue;

                int score = ScoreCharacterPath(slot, candidatePath);
                if (score <= bestScore) continue;

                var loaded = AssetDatabase.LoadAssetAtPath<GameObject>(candidatePath);
                if (loaded == null) continue;

                bestScore = score;
                asset = loaded;
                path = candidatePath;
            }

            if (asset != null) return true;

            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets" }))
            {
                string candidatePath = AssetDatabase.GUIDToAssetPath(guid);
                if (ShouldExcludeCharacterPath(candidatePath)) continue;
                if (!candidatePath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;

                int score = ScoreCharacterPath(slot, candidatePath);
                if (score <= bestScore) continue;
                if (!TryLoadFbxAsPrefab(candidatePath, out var fbxRoot)) continue;

                bestScore = score;
                asset = fbxRoot;
                path = candidatePath;
            }

            return asset != null;
        }

        static bool TryLoadFbxAsPrefab(string assetPath, out UnityEngine.Object asset)
        {
            asset = null;
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return false;
            asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            return asset != null;
        }

        static bool IsCharacterSlot(Slot slot)
        {
            return slot == Slot.DruidModel || slot == Slot.GoblinModel || slot == Slot.GoblinElite;
        }

        static bool ShouldExcludeCharacterPath(string candidatePath)
        {
            if (string.IsNullOrEmpty(candidatePath)) return true;
            string lower = candidatePath.ToLowerInvariant();
            if (lower.Contains("/editor/") || lower.Contains("gizmos")) return true;
            if (lower.Contains("jmo assets") || lower.Contains("cfxr") || lower.Contains("cartoon fx")) return true;
            if (lower.Contains("/ui/") || lower.Contains("/vfx/")) return true;
            if (lower.Contains("/parts/") || lower.Contains("non-skinned mesh parts")
                || lower.Contains("/modular parts/") || lower.Contains("_part")) return true;
            if (lower.Contains("face parts") || lower.Contains("armor parts")) return true;
            if (lower.Contains("hair_") || lower.Contains("helmet_") || lower.Contains("boots_")) return true;
            return false;
        }

        static int ScoreCharacterPath(Slot slot, string candidatePath)
        {
            string lower = candidatePath.ToLowerInvariant();
            string name = Path.GetFileNameWithoutExtension(candidatePath).ToLowerInvariant();

            switch (slot)
            {
                case Slot.DruidModel:
                    if (name.Contains("druidmodel") || name.Contains("druid_model")) return 100;
                    if (lower.Contains("_game/prefabs/characters")) return 95;
                    if (lower.Contains("ganzse") && lower.Contains("modular character") && name.Contains("update")) return 92;
                    if (lower.Contains("ganzse") && lower.Contains("modular character") && !lower.Contains("/parts/")) return 88;
                    if (lower.Contains("ganzse") && (name == "character" || name.Contains("ganzse"))) return 90;
                    if (name.Contains("ganzsemale") || name == "ganzse") return 88;
                    if (lower.Contains("ganzse") && lower.Contains("modular")) return 85;
                    if (lower.Contains("ganzse")) return 80;
                    if (name.Contains("playerarmature") || name == "playerarmature") return 78;
                    if (lower.Contains("starter assets") && lower.Contains("third") && name.Contains("armature")) return 75;
                    if (lower.Contains("starter assets") && name.Contains("armature")) return 70;
                    if (name == "character" && lower.Contains("modular")) return 65;
                    if (lower.Contains("modular character")) return 60;
                    if (name.Contains("player") && !name.Contains("input")) return 40;
                    return 0;

                case Slot.GoblinModel:
                    if (name == "skin1" && lower.Contains("assets/goblin")) return 100;
                    if (name.Contains("goblin_stonesword") && lower.Contains("assets/goblin")) return 98;
                    if (lower.Contains("assets/goblin/prefab") && name.StartsWith("skin")) return 92;
                    if (name.Contains("goblinmodel") || name.Contains("goblin_model")) return 90;
                    if (lower.Contains("_game/prefabs/enemies/goblinmodel")) return 88;
                    if (name.Contains("goblin_warrior") || name.Contains("goblin warrior")) return 85;
                    if (lower.Contains("3d stylized goblin") && name.Contains("goblin")) return 80;
                    if (lower.Contains("stylized goblin") && name.Contains("warrior")) return 78;
                    if (lower.Contains("fantasy goblin") && name.Contains("goblin") && !name.Contains("archer") && !name.Contains("shaman")) return 70;
                    return 0;

                case Slot.GoblinElite:
                    if (name == "skin3" && lower.Contains("assets/goblin")) return 100;
                    if (name == "skin2" && lower.Contains("assets/goblin")) return 95;
                    if (name.Contains("goblinelite") || name.Contains("goblin_elite")) return 90;
                    if (lower.Contains("_game/prefabs/enemies/goblinelite")) return 88;
                    if (name.Contains("goblin_shaman") || name.Contains("goblin shaman")) return 85;
                    if (lower.Contains("fantasy goblin") && (name.Contains("shaman") || name.Contains("elite"))) return 82;
                    if (lower.Contains("assets/goblin")) return 75;
                    return 0;

                default:
                    return 0;
            }
        }
#else
        static bool TryFindCharacterByScoredScan(Slot slot, out UnityEngine.Object asset, out string path)
        {
            asset = null;
            path = null;
            return false;
        }

        static bool TryLoadFbxAsPrefab(string assetPath, out UnityEngine.Object asset)
        {
            asset = null;
            return false;
        }
#endif

        static AudioClip LoadAudioSlot(Slot slot, out string path)
        {
            path = null;
            string[] paths;
            string res;
            switch (slot)
            {
                case Slot.SfxHit:
                    paths = GameAssetPaths.SfxHit;
                    res = GameAssetPaths.ResSfxHit;
                    break;
                case Slot.SfxDeath:
                    paths = GameAssetPaths.SfxDeath;
                    res = GameAssetPaths.ResSfxDeath;
                    break;
                case Slot.SfxCast:
                    paths = GameAssetPaths.SfxCast;
                    res = GameAssetPaths.ResSfxCast;
                    break;
                default:
                    return null;
            }

            var clip = GameAssetLoader.LoadAudio(paths, res);
            if (clip != null)
            {
#if UNITY_EDITOR
                path = FindFirstExistingPath(paths) ?? res;
#else
                path = res;
#endif
                return clip;
            }

#if UNITY_EDITOR
            if (TryFindAudioByFuzzyScan(slot, out clip, out path))
                return clip;
#endif
            return null;
        }

#if UNITY_EDITOR
        static bool TryFindAudioByFuzzyScan(Slot slot, out AudioClip clip, out string path)
        {
            clip = null;
            path = null;
            if (!IsAudioSlot(slot)) return false;

            string[] nameTokens;
            switch (slot)
            {
                case Slot.SfxHit: nameTokens = new[] { "hit", "sword", "weapon", "impact" }; break;
                case Slot.SfxDeath: nameTokens = new[] { "death", "die", "enemy_death" }; break;
                case Slot.SfxCast: nameTokens = new[] { "fireball", "spell", "cast", "magic", "fire" }; break;
                default: return false;
            }

            int bestScore = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" }))
            {
                string candidatePath = AssetDatabase.GUIDToAssetPath(guid);
                if (!GameAssetPacks.PathMatchesPackFolder(candidatePath, new[] { "basic rpg sounds", "rpg sounds" })
                    && !NormalizeForMatch(candidatePath).Contains("basicsounds")
                    && !NormalizeForMatch(candidatePath).Contains("rpgsounds"))
                    continue;

                string normName = NormalizeForMatch(Path.GetFileNameWithoutExtension(candidatePath));
                int score = 0;
                foreach (var t in nameTokens)
                {
                    if (normName.Contains(NormalizeForMatch(t))) score += 20;
                }
                if (score <= bestScore) continue;

                var loaded = AssetDatabase.LoadAssetAtPath<AudioClip>(candidatePath);
                if (loaded == null) continue;
                bestScore = score;
                clip = loaded;
                path = candidatePath;
            }
            return clip != null;
        }
#endif

#if UNITY_EDITOR
        static string FindFirstExistingPath(string[] paths)
        {
            if (paths == null) return null;
            foreach (var p in paths)
            {
                if (!string.IsNullOrEmpty(p) && File.Exists(p)) return p;
            }
            return null;
        }
#endif

        static bool TryLoadWorldPrimaryPaths(Slot slot, out UnityEngine.Object asset, out string path)
        {
            asset = null;
            path = null;
            string[] candidates = null;
            switch (slot)
            {
                case Slot.TreeLarge:
                case Slot.TreeSmall:
                    candidates = GameAssetPaths.WorldTrees;
                    break;
                case Slot.Bush:
                    candidates = GameAssetPaths.WorldBushes;
                    break;
                case Slot.Grass:
                    candidates = GameAssetPaths.WorldGrass;
                    break;
                case Slot.Rock:
                    candidates = GameAssetPaths.WorldRocks;
                    break;
                case Slot.Ruin:
                    candidates = GameAssetPaths.WorldRuins;
                    break;
            }

            if (candidates == null) return false;
            foreach (var p in candidates)
            {
                if (TryLoadAtPath(p, slot, out asset))
                {
                    path = p;
                    return true;
                }
            }
            return false;
        }

        static bool TryLoadAtPath(string assetPath, Slot slot, out UnityEngine.Object asset)
        {
            asset = null;
            if (string.IsNullOrEmpty(assetPath)) return false;

#if UNITY_EDITOR
            if (!File.Exists(assetPath)) return false;
            asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            return asset != null && IsValidForSlot(slot, asset);
#else
            return false;
#endif
        }

        static bool TryFindByScan(Slot slot, out UnityEngine.Object asset, out string path)
        {
            asset = null;
            path = null;

#if UNITY_EDITOR
            if (!FallbackTokens.TryGetValue(slot, out var tokens) || tokens.Length == 0)
                return false;

            int bestScore = 0;
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            foreach (var guid in guids)
            {
                string candidatePath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(candidatePath) || ShouldExcludePathForSlot(slot, candidatePath)) continue;

                string normPath = NormalizeForMatch(candidatePath);
                string normName = NormalizeForMatch(Path.GetFileNameWithoutExtension(candidatePath));
                int score = ScoreTokenMatch(normName, tokens) + ScoreTokenMatch(normPath, tokens) / 2;
                if (score <= 0 || score <= bestScore) continue;

                var loaded = AssetDatabase.LoadAssetAtPath<GameObject>(candidatePath);
                if (loaded == null) continue;

                bestScore = score;
                asset = loaded;
                path = candidatePath;
            }

            return asset != null;
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        static bool TryFindVfxByScoredScan(Slot slot, out UnityEngine.Object asset, out string path)
        {
            asset = null;
            path = null;
            if (!IsVfxSlot(slot)) return false;

            int bestScore = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                string candidatePath = AssetDatabase.GUIDToAssetPath(guid);
                if (ShouldExcludePathForSlot(slot, candidatePath)) continue;

                int score = ScoreVfxPath(slot, candidatePath);
                if (score <= bestScore) continue;

                var loaded = AssetDatabase.LoadAssetAtPath<GameObject>(candidatePath);
                if (loaded == null) continue;

                bestScore = score;
                asset = loaded;
                path = candidatePath;
            }
            return asset != null;
        }

        static int ScoreVfxPath(Slot slot, string candidatePath)
        {
            string lower = candidatePath.ToLowerInvariant();
            string name = Path.GetFileNameWithoutExtension(candidatePath).ToLowerInvariant();

            switch (slot)
            {
                case Slot.VfxFireball:
                    if (name.Contains("fireball") || name.Contains("vfxfireball")) return 90;
                    if (lower.Contains("cfxr") && lower.Contains("fire")) return 85;
                    if (lower.Contains("cartoon fx") && name.Contains("fire")) return 80;
                    if (lower.Contains("fantasy effects") && name.Contains("fire")) return 75;
                    return 0;
                case Slot.VfxHeal:
                    if (name.Contains("healing") || name.Contains("heal")) return 90;
                    if (lower.Contains("cfxr2") && lower.Contains("heal")) return 88;
                    if (lower.Contains("cfx") && name.Contains("magic")) return 75;
                    if (lower.Contains("fantasy effects") && name.Contains("heal")) return 70;
                    return 0;
                default:
                    return 0;
            }
        }
#endif

        static bool ShouldExcludePathForSlot(Slot slot, string candidatePath)
        {
            if (IsCharacterSlot(slot)) return ShouldExcludeCharacterPath(candidatePath);
            if (IsVfxSlot(slot))
            {
                string lower = candidatePath.ToLowerInvariant();
                if (lower.Contains("/editor/") || lower.Contains("gizmos")) return true;
                if (lower.Contains("/ui/")) return true;
                return false;
            }
            return ShouldExcludeWorldPath(candidatePath);
        }

        static bool IsVfxSlot(Slot slot) => slot == Slot.VfxFireball || slot == Slot.VfxHeal;

        static string NormalizeForMatch(string value) => GameAssetPacks.NormalizeToken(value);

        static int ScoreTokenMatch(string haystack, string[] tokens)
        {
            if (string.IsNullOrEmpty(haystack) || tokens == null) return 0;
            string normHay = NormalizeForMatch(haystack);
            int best = 0;
            foreach (var token in tokens)
            {
                if (string.IsNullOrEmpty(token)) continue;
                string normToken = NormalizeForMatch(token);
                if (normHay.Contains(normToken)) best = Math.Max(best, normToken.Length);
            }
            return best;
        }

        static bool MatchesAnyToken(string haystack, string[] tokens) => ScoreTokenMatch(haystack, tokens) > 0;

        static bool IsAudioSlot(Slot slot)
        {
            return slot == Slot.SfxHit || slot == Slot.SfxDeath || slot == Slot.SfxCast;
        }

        static bool IsValidForSlot(Slot slot, UnityEngine.Object asset)
        {
            if (asset == null) return false;
            if (IsAudioSlot(slot)) return asset is AudioClip;
            if (slot == Slot.TexturePath || slot == Slot.TextureGrass) return asset is Texture2D;
            if (slot == Slot.SkyboxMaterial) return asset is Material;
            return asset is GameObject;
        }
    }
}
