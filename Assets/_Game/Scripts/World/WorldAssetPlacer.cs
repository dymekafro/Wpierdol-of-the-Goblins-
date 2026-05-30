using System;
using System.Collections.Generic;
using UnityEngine;
using WPG.Character;
using WPG.Core;
using WPG.Enemies;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WPG.World
{
    /// <summary>
    /// Stawia prefaby z AssetCatalog lub placeholdery (prymitywy).
    /// </summary>
    public class WorldAssetPlacer
    {
        private readonly System.Random _rng;
        private readonly Transform _parent;
        private readonly List<Vector3> _pathPoints;
        private readonly float _pathHalfWidth;

        /// <summary>Brown path mesh is 5.5 m wide; logical half-width matches spawn exclusion.</summary>
        public const float DefaultPathHalfWidth = 2.75f;

        /// <summary>Extra margin beyond path edge so grass tufts do not bleed onto dirt.</summary>
        public const float PathSurfaceExtraMargin = 0.5f;

        private static readonly Color TrunkColor = new Color(0.20f, 0.14f, 0.08f);
        private static readonly Color FoliageDark = new Color(0.08f, 0.20f, 0.08f);
        private static readonly Color FoliageMid = new Color(0.10f, 0.25f, 0.10f);
        private static readonly Color RockColor = new Color(0.25f, 0.25f, 0.27f);
        private static readonly Color BushColor = new Color(0.10f, 0.18f, 0.08f);
        private static readonly Color MushroomGlow = new Color(0.4f, 0.85f, 1f);

        private int _placeholderCount;
        public int PlaceholderCount => _placeholderCount;

        public WorldAssetPlacer(System.Random rng, Transform parent, List<Vector3> pathPoints, float pathHalfWidth = 2.8f)
        {
            _rng = rng;
            _parent = parent;
            _pathPoints = pathPoints ?? new List<Vector3>();
            _pathHalfWidth = pathHalfWidth;
        }

        public void FillForest(
            int treeCount, int rockCount, int bushCount, int mushroomCount,
            float mapRadius, Func<Vector3, float, bool> isBlocked)
        {
            var vegRoot = new GameObject("Vegetation");
            vegRoot.transform.SetParent(_parent, false);
            Transform veg = vegRoot.transform;

            PlaceScatter(veg, treeCount, mapRadius, isBlocked, 1.2f, (p, t) => PlaceTree(t, p));
            PlaceScatter(veg, rockCount, mapRadius, isBlocked, 1f, (p, t) => PlaceRock(t, p));
            PlaceScatter(veg, bushCount, mapRadius, isBlocked, 0.5f, (p, t) => PlaceBush(t, p));
            PlaceScatter(veg, mushroomCount, mapRadius, (pos, pad) => isBlocked(pos, pad) || NearBaseOnly(pos), 0.5f,
                (p, t) => PlaceMushroom(t, p));
        }

        public void DecorateCamp(Transform campRoot, float radius, int ruinCount = 4)
        {
            for (int i = 0; i < ruinCount; i++)
            {
                float a = (float)(_rng.NextDouble() * Mathf.PI * 2.0);
                float d = radius * (0.75f + (float)_rng.NextDouble() * 0.35f);
                Vector3 local = new Vector3(Mathf.Cos(a) * d, 0f, Mathf.Sin(a) * d);
                PlaceRuin(campRoot, local);
            }

            int extraBushes = 3 + _rng.Next(0, 4);
            for (int j = 0; j < extraBushes; j++)
            {
                float a = (float)(_rng.NextDouble() * Mathf.PI * 2.0);
                float d = radius * (0.4f + (float)_rng.NextDouble() * 0.5f);
                Vector3 local = new Vector3(Mathf.Cos(a) * d, 0f, Mathf.Sin(a) * d);
                PlaceBush(campRoot, campRoot.position + local);
            }
        }

        public void DecoratePowerSite(Transform siteRoot, bool stoneCircle)
        {
            int ruins = stoneCircle ? 6 : 3;
            float ring = stoneCircle ? 3.2f : 2.2f;
            for (int i = 0; i < ruins; i++)
            {
                float a = i * (Mathf.PI * 2f / ruins) + (float)(_rng.NextDouble() * 0.2 - 0.1);
                Vector3 local = new Vector3(Mathf.Cos(a) * ring, 0f, Mathf.Sin(a) * ring);
                PlaceRuin(siteRoot, local);
            }
        }

        public void DecorateDruidBase(Transform baseRoot)
        {
            for (int i = 0; i < 4; i++)
            {
                float a = i * (Mathf.PI * 0.5f) + (float)(_rng.NextDouble() * 0.3);
                float d = 7f + (float)_rng.NextDouble() * 3f;
                Vector3 pos = new Vector3(Mathf.Cos(a) * d, 0f, Mathf.Sin(a) * d);
                PlaceTree(baseRoot, baseRoot.position + pos);
            }
        }

        private void PlaceScatter(
            Transform vegParent,
            int count, float mapRadius, Func<Vector3, float, bool> isBlocked, float padding,
            Action<Vector3, Transform> spawn)
        {
            int placed = 0;
            int attempts = 0;
            int maxAttempts = count * 6;
            while (placed < count && attempts < maxAttempts)
            {
                attempts++;
                Vector3 p = RandomMapPoint(mapRadius);
                if (isBlocked(p, padding) || IsOnPath(p)) continue;
                spawn(p, vegParent);
                placed++;
            }
        }

        /// <summary>True when position lies on the brown path surface (not meadow beside it).</summary>
        public bool IsOnPath(Vector3 worldPos) => IsNearPath(worldPos, PathSurfaceExtraMargin);

        public bool IsNearPath(Vector3 worldPos, float extraRadius = 0f)
        {
            if (_pathPoints == null || _pathPoints.Count < 2) return false;
            float threshold = _pathHalfWidth + extraRadius;
            float thresholdSq = threshold * threshold;
            for (int i = 0; i < _pathPoints.Count - 1; i++)
            {
                Vector3 a = _pathPoints[i];
                Vector3 b = _pathPoints[i + 1];
                float distSq = SqrDistancePointToSegmentXZ(worldPos, a, b);
                if (distSq < thresholdSq) return true;
            }
            return false;
        }

        /// <summary>Total exclusion radius from path centerline (≈3.5 m with default half-width).</summary>
        public float PathSurfaceExclusionRadius => _pathHalfWidth + PathSurfaceExtraMargin;

        float PathMeadowMinOffset => _pathHalfWidth + PathSurfaceExtraMargin + 0.35f;

        private static float SqrDistancePointToSegmentXZ(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector2 p2 = new Vector2(p.x, p.z);
            Vector2 a2 = new Vector2(a.x, a.z);
            Vector2 b2 = new Vector2(b.x, b.z);
            Vector2 ab = b2 - a2;
            float lenSq = ab.sqrMagnitude;
            if (lenSq < 0.0001f) return (p2 - a2).sqrMagnitude;
            float t = Mathf.Clamp01(Vector2.Dot(p2 - a2, ab) / lenSq);
            Vector2 closest = a2 + ab * t;
            return (p2 - closest).sqrMagnitude;
        }

        private Vector3 RandomMapPoint(float mapRadius)
        {
            float a = (float)(_rng.NextDouble() * Mathf.PI * 2.0);
            float r = Mathf.Sqrt((float)_rng.NextDouble()) * mapRadius;
            return new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
        }

        private static bool NearBaseOnly(Vector3 p) => p.sqrMagnitude < 20f * 20f;

        public static string GetPlacementSummary(int placeholderCount) =>
            $"prefaby świata + placeholdery={placeholderCount}";

        /// <summary>Osadza punkt świata na pofalowanym terenie (+ opcjonalny offset Y).</summary>
        static Vector3 GroundPos(Vector3 worldPos, float yOffset = 0f)
        {
            worldPos.y = WorldGround.GetGroundHeight(worldPos.x, worldPos.z) + yOffset;
            return worldPos;
        }

        public void PlaceTree(Transform parentT, Vector3 pos)
        {
            pos = GroundPos(pos);
            var prefab = PickTreePrefab();
            if (TryInstantiatePrefab(prefab, parentT, pos, "Tree")) return;
            _placeholderCount++;
            SpawnPlaceholderTree(parentT, pos);
        }

        public void PlaceRock(Transform parentT, Vector3 pos)
        {
            pos = GroundPos(pos);
            var prefab = GameAssetRegistry.PickWorldRock(_rng) ?? AssetCatalog.Rock;
            if (TryInstantiatePrefab(prefab, parentT, pos, "Rock")) return;
            _placeholderCount++;
            SpawnPlaceholderRock(parentT, pos);
        }

        public void PlaceBush(Transform parentT, Vector3 pos)
        {
            pos = GroundPos(pos);
            var prefab = GameAssetRegistry.PickWorldBush(_rng) ?? AssetCatalog.Bush;
            if (TryInstantiatePrefab(prefab, parentT, pos, "Bush")) return;
            _placeholderCount++;
            SpawnPlaceholderBush(parentT, pos);
        }

        public bool PlaceGrass(Transform parentT, Vector3 pos)
        {
            if (IsOnPath(pos)) return false;

            var prefab = GameAssetRegistry.PickWorldGrass(_rng);
            if (prefab == null) return false;
            pos = GroundPos(pos);
            var inst = UnityEngine.Object.Instantiate(prefab, parentT);
            inst.name = prefab.name;
            inst.transform.position = pos;
            inst.transform.rotation = Quaternion.Euler(0f, (float)(_rng.NextDouble() * 360.0), 0f);
            float s = 0.7f + (float)_rng.NextDouble() * 0.6f;
            inst.transform.localScale = new Vector3(s, s * (0.85f + (float)_rng.NextDouble() * 0.3f), s);
            foreach (var col in inst.GetComponentsInChildren<Collider>()) UnityEngine.Object.Destroy(col);
            MaterialUpgrader.UpgradeHierarchy(inst);
            return true;
        }

        public int ScatterGrassNearPaths(Transform parentT, float bandWidth, float density, float mapRadius, Func<Vector3, float, bool> isBlocked)
        {
            if (_pathPoints == null || _pathPoints.Count < 2) return 0;
            int placed = 0;
            for (int i = 0; i < _pathPoints.Count - 1; i++)
            {
                Vector3 a = _pathPoints[i];
                Vector3 b = _pathPoints[i + 1];
                Vector3 ab = b - a;
                float len = ab.magnitude;
                if (len < 0.5f) continue;

                int tufts = Mathf.CeilToInt(len * density);
                Vector3 fwd = ab.normalized;
                Vector3 perp = new Vector3(-fwd.z, 0f, fwd.x);

                for (int k = 0; k < tufts; k++)
                {
                    float t = (float)_rng.NextDouble();
                    Vector3 pOnSeg = a + ab * t;
                    float side = (_rng.NextDouble() < 0.5 ? -1f : 1f);
                    float offset = PathMeadowMinOffset + (float)_rng.NextDouble() * bandWidth;
                    Vector3 pos = pOnSeg + perp * offset * side;
                    pos.y = 0f;
                    if (pos.sqrMagnitude > mapRadius * mapRadius) continue;
                    if (isBlocked != null && isBlocked(pos, 0.2f)) continue;
                    if (IsOnPath(pos)) continue;
                    if (PlaceGrass(parentT, pos)) placed++;
                }
            }
            return placed;
        }

        public int ScatterGrassInClearings(Transform parentT, IEnumerable<Vector3> clearingCenters, float clearingRadius, int perClearing, float mapRadius, Func<Vector3, float, bool> isBlocked)
        {
            int placed = 0;
            foreach (var center in clearingCenters)
            {
                for (int i = 0; i < perClearing; i++)
                {
                    float a = (float)(_rng.NextDouble() * Mathf.PI * 2.0);
                    float r = Mathf.Sqrt((float)_rng.NextDouble()) * clearingRadius;
                    Vector3 pos = center + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                    if (pos.sqrMagnitude > mapRadius * mapRadius) continue;
                    if (isBlocked != null && isBlocked(pos, 0.2f)) continue;
                    if (IsOnPath(pos)) continue;
                    if (PlaceGrass(parentT, pos)) placed++;
                }
            }
            return placed;
        }

        public void PlaceRuin(Transform parentT, Vector3 localOrWorldPos)
        {
            bool local = parentT != _parent;
            if (!local) localOrWorldPos = GroundPos(localOrWorldPos);
            var prefab = GameAssetRegistry.PickWorldRuin(_rng) ?? AssetCatalog.Ruin;
            if (prefab != null)
            {
                var inst = UnityEngine.Object.Instantiate(prefab, parentT);
                inst.name = prefab.name;
                if (local)
                {
                    inst.transform.localPosition = localOrWorldPos;
                    inst.transform.localRotation = Quaternion.Euler(0f, (float)(_rng.NextDouble() * 360.0), 0f);
                }
                else
                {
                    inst.transform.position = localOrWorldPos;
                    inst.transform.rotation = Quaternion.Euler(0f, (float)(_rng.NextDouble() * 360.0), 0f);
                }
                float scale = 0.85f + (float)_rng.NextDouble() * 0.35f;
                inst.transform.localScale *= scale;
                MaterialUpgrader.UpgradeHierarchy(inst);
                return;
            }

            _placeholderCount++;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Ruin_Placeholder";
            cube.transform.SetParent(parentT, false);
            if (local)
            {
                cube.transform.localPosition = localOrWorldPos + Vector3.up * 0.8f;
                cube.transform.localScale = new Vector3(1.2f, 1.6f, 0.9f);
            }
            else
            {
                cube.transform.position = localOrWorldPos + Vector3.up * 0.8f;
                cube.transform.localScale = new Vector3(1.2f, 1.6f, 0.9f);
            }
            cube.transform.rotation = Quaternion.Euler((float)_rng.NextDouble() * 8f, (float)(_rng.NextDouble() * 360.0), 0f);
            cube.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.32f, 0.30f, 0.28f));
        }

        public void PlaceMushroom(Transform parentT, Vector3 pos)
        {
            pos = GroundPos(pos);
            Color c = _rng.NextDouble() < 0.5 ? MushroomGlow : new Color(0.5f, 1f, 0.3f);
            SpawnMushroomLight(parentT, pos, c, 1.5f, 7f);
        }

        private GameObject PickTreePrefab()
        {
            var pooled = GameAssetRegistry.PickWorldTree(_rng);
            if (pooled != null) return pooled;
            if (_rng.NextDouble() < 0.35f)
                return AssetCatalog.TreeSmall != null ? AssetCatalog.TreeSmall : AssetCatalog.TreeLarge;
            return AssetCatalog.TreeLarge != null ? AssetCatalog.TreeLarge : AssetCatalog.TreeSmall;
        }

        private bool TryInstantiatePrefab(GameObject prefab, Transform parentT, Vector3 pos, string fallbackName)
        {
            if (prefab == null) return false;
            var inst = UnityEngine.Object.Instantiate(prefab, parentT);
            inst.name = prefab.name;
            inst.transform.position = pos;
            inst.transform.rotation = Quaternion.Euler(0f, (float)(_rng.NextDouble() * 360.0), 0f);
            float scale = 0.75f + (float)_rng.NextDouble() * 0.55f;
            inst.transform.localScale = Vector3.one * scale;
            MaterialUpgrader.UpgradeHierarchy(inst);
            return true;
        }

        private void SpawnPlaceholderTree(Transform parentT, Vector3 pos)
        {
            float height = 5f + (float)_rng.NextDouble() * 7f;
            float trunkR = 0.25f + (float)_rng.NextDouble() * 0.25f;
            float canopyR = 1.6f + (float)_rng.NextDouble() * 2.2f;

            var t = new GameObject("Tree_Placeholder");
            t.transform.SetParent(parentT, false);
            t.transform.position = pos;
            t.transform.rotation = Quaternion.Euler(0f, (float)(_rng.NextDouble() * 360.0), 0f);

            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(t.transform, false);
            trunk.transform.localScale = new Vector3(trunkR, height * 0.5f, trunkR);
            trunk.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(TrunkColor);

            int layers = 1 + _rng.Next(0, 3);
            Color foliage = _rng.NextDouble() < 0.5 ? FoliageDark : FoliageMid;
            for (int i = 0; i < layers; i++)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                c.transform.SetParent(t.transform, false);
                float yOff = height + i * canopyR * 0.6f;
                c.transform.localPosition = new Vector3((float)_rng.NextDouble() * 0.5f - 0.25f, yOff, (float)_rng.NextDouble() * 0.5f - 0.25f);
                c.transform.localScale = Vector3.one * canopyR * (1f - i * 0.15f);
                var col = c.GetComponent<Collider>(); if (col != null) UnityEngine.Object.Destroy(col);
                c.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(foliage);
            }
        }

        private void SpawnPlaceholderRock(Transform parentT, Vector3 pos)
        {
            var r = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            r.name = "Rock_Placeholder";
            r.transform.SetParent(parentT, false);
            r.transform.position = pos + Vector3.up * 0.3f;
            float s = 0.6f + (float)_rng.NextDouble() * 1.8f;
            r.transform.localScale = new Vector3(s, s * (0.5f + (float)_rng.NextDouble() * 0.4f), s);
            r.transform.rotation = Quaternion.Euler((float)_rng.NextDouble() * 30f, (float)(_rng.NextDouble() * 360.0), (float)_rng.NextDouble() * 30f);
            r.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(RockColor);
        }

        private void SpawnPlaceholderBush(Transform parentT, Vector3 pos)
        {
            var b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            b.name = "Bush_Placeholder";
            b.transform.SetParent(parentT, false);
            b.transform.position = pos + Vector3.up * 0.3f;
            float s = 0.6f + (float)_rng.NextDouble() * 0.8f;
            b.transform.localScale = new Vector3(s, s * 0.6f, s);
            var col = b.GetComponent<Collider>(); if (col != null) UnityEngine.Object.Destroy(col);
            b.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(BushColor);
        }

        private void SpawnMushroomLight(Transform parentT, Vector3 pos, Color color, float intensity, float range)
        {
            var m = new GameObject("MushroomLight");
            m.transform.SetParent(parentT, false);
            m.transform.position = pos;

            var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.transform.SetParent(m.transform, false);
            stem.transform.localScale = new Vector3(0.1f, 0.25f, 0.1f);
            stem.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            var stemCol = stem.GetComponent<Collider>(); if (stemCol != null) UnityEngine.Object.Destroy(stemCol);
            stem.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.85f, 0.8f, 0.7f));

            var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.transform.SetParent(m.transform, false);
            cap.transform.localScale = new Vector3(0.35f, 0.2f, 0.35f);
            cap.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            var capCol = cap.GetComponent<Collider>(); if (capCol != null) UnityEngine.Object.Destroy(capCol);
            cap.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(color, 0.5f, color, 3f);

            var light = new GameObject("Light").AddComponent<Light>();
            light.transform.SetParent(m.transform, false);
            light.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            light.color = color;
            light.range = range;
            light.intensity = intensity;
        }

        // --- Character & VFX (Agent C) — static API dla PlayerBuilder / goblinów / czarów ---

        public enum CharacterModelKind { Druid, GoblinMelee, GoblinArcher, GoblinElite }
        public enum VfxKind { FireballCast, FireballImpact, Heal, TotemDestroy }

        public struct CharacterAttachResult
        {
            public bool Success;
            public bool AnimatorOk;
            public string PrefabPath;
            public string ModelSource;
            public string AnimatorStatus;
            public float AppliedScale;
            public string FailureReason;
            public Transform ModelRoot;
            public Transform BodyPivot;
            public Transform StaffTip;
            public Transform HandMount;
            public Transform LeftArm;
            public Transform RightArm;
            public Transform LeftLeg;
            public Transform RightLeg;
        }

        public const float DefaultCharacterModelScale = 1f;
        public const float PlayerCharacterModelScale = 1.3f;
        /// <summary>Gobliny o ~30% większe niż poprzedni default.</summary>
        public const float GoblinCharacterModelScale = 1.3f;

        static readonly Dictionary<CharacterModelKind, string> ResolvedCharacterPaths = new();
        static readonly Dictionary<VfxKind, string> ResolvedVfxPaths = new();
        static bool _characterAssetsScanned;

        static readonly Dictionary<CharacterModelKind, string[]> CharacterCandidates = new()
        {
            [CharacterModelKind.Druid] = GameAssetPaths.DruidModel,
            [CharacterModelKind.GoblinMelee] = GameAssetPaths.GoblinModel,
            [CharacterModelKind.GoblinArcher] = GameAssetPaths.GoblinArcherModel,
            [CharacterModelKind.GoblinElite] = GameAssetPaths.GoblinElite,
        };

        static readonly Dictionary<VfxKind, string[]> VfxCandidates = new()
        {
            [VfxKind.FireballCast] = new[]
            {
                "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Fire/CFXR Fireball.prefab",
                "Assets/JMO Assets/Cartoon FX (legacy)/CFX Prefabs/Fire/CFX_Fireball.prefab",
                "Assets/Fantasy Effects Pack/Prefabs/Fireball.prefab",
                "Assets/_Game/Prefabs/VFX/VfxFireball.prefab",
            },
            [VfxKind.FireballImpact] = new[]
            {
                "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Explosions/CFXR Explosion Fire.prefab",
                "Assets/Fantasy Effects Pack/Prefabs/Fire explosion.prefab",
            },
            [VfxKind.Heal] = new[]
            {
                "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Magic/CFXR2 Healing.prefab",
                "Assets/JMO Assets/Cartoon FX (legacy)/CFX Prefabs/Magic/CFX_MagicStars.prefab",
                "Assets/_Game/Prefabs/VFX/VfxHeal.prefab",
            },
            [VfxKind.TotemDestroy] = new[]
            {
                "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Explosions/CFXR Explosion Smoke 2.prefab",
                "Assets/JMO Assets/Cartoon FX (legacy)/CFX Prefabs/Explosions/CFX_Explosion_Brown.prefab",
            },
        };

        static readonly Dictionary<CharacterModelKind, string[]> CharacterSearchTokens = new()
        {
            [CharacterModelKind.Druid] = new[] { "ganzse", "ganzse free modular", "modular character update", "modular character", "druidmodel", "druid", "playerarmature", "ganzsemale", "invector", "third person", "basic locomotion", "thirdpersoncontroller", "vbot" },
            [CharacterModelKind.GoblinMelee] = new[] { "skin1", "goblin_stonesword", "assets/goblin", "fantasy goblin", "goblin warrior", "goblin_warrior", "goblinmodel", "stylized goblin", "3d stylized goblin" },
            [CharacterModelKind.GoblinArcher] = new[] { "skin2", "goblin archer", "goblin_archer", "assets/goblin" },
            [CharacterModelKind.GoblinElite] = new[] { "skin3", "skin2", "fantasy goblin", "goblin elite", "goblin shaman", "goblinelite", "goblin_shaman", "assets/goblin" },
        };

        static readonly Dictionary<VfxKind, string[]> VfxSearchTokens = new()
        {
            [VfxKind.FireballCast] = new[] { "cfxr fireball", "cfx_fireball", "vfxfireball", "fireball" },
            [VfxKind.FireballImpact] = new[] { "cfxr explosion fire", "fire explosion" },
            [VfxKind.Heal] = new[] { "cfxr2 healing", "cfx_magic", "vfxheal", "healing" },
            [VfxKind.TotemDestroy] = new[] { "cfxr explosion smoke", "cfx_explosion_brown", "wood destroy" },
        };

        public static bool TryAttachCharacterModel(
            Transform root,
            CharacterModelKind kind,
            float targetHeight,
            out CharacterAttachResult result,
            float modelScale = DefaultCharacterModelScale,
            Color? materialTint = null)
        {
            GameAssetRegistry.Initialize();
            EnsureCharacterAssetsScanned();
            result = default;

            GameObject prefab = ResolveCharacterPrefab(kind, out var path);
            if (prefab == null)
            {
                result.FailureReason = DescribeCharacterMiss(kind);
                Debug.LogWarning($"[WorldAssetPlacer] Brak modelu {kind}: {result.FailureReason}");
                return false;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, root);
            instance.name = prefab.name + "_Model";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            StripInvectorGameplayComponents(instance);
            FitCharacterHeight(instance.transform, targetHeight);
            if (Mathf.Abs(modelScale - 1f) > 0.001f)
            {
                instance.transform.localScale *= modelScale;
                RegroundCharacterModel(instance.transform);
            }
            StripCharacterColliders(instance);
            MaterialUpgrader.UpgradeHierarchy(instance);

            if (kind == CharacterModelKind.Druid && IsGanzSeCharacterPath(path))
            {
                ConfigureGanzSeDruidLoadout(instance);
                ApplyDruidMaterialTint(instance, path);
            }
            else if (kind == CharacterModelKind.Druid)
            {
                ApplyDruidMaterialTint(instance, path);
            }
            else if (IsFantasyGoblinPath(path) && materialTint.HasValue)
            {
                ApplyGoblinMaterialTint(instance, materialTint.Value, kind == CharacterModelKind.GoblinElite ? 0.28f : 0.18f);
            }

            var animator = instance.GetComponentInChildren<Animator>();
            if (animator != null) animator.applyRootMotion = false;

            result.Success = true;
            result.PrefabPath = path;
            result.ModelSource = DescribeModelSource(path);
            result.AppliedScale = instance.transform.localScale.x;
            result.ModelRoot = instance.transform;
            WireGoblinRigBones(kind, instance.transform, ref result);
            if (kind == CharacterModelKind.GoblinMelee || kind == CharacterModelKind.GoblinArcher || kind == CharacterModelKind.GoblinElite)
            {
                GoblinAnimSetup.EnsureAnimator(instance.transform);
                if (animator == null) animator = instance.GetComponentInChildren<Animator>();
            }
            result.HandMount = result.HandMount
                ?? FindCharacterBone(instance.transform, "Hand_R", "RightHand", "hand_r", "mixamorig:RightHand", "VBOT_:RightHand")
                ?? FindHandMountFallback(instance.transform);
            result.StaffTip = FindCharacterBone(instance.transform, "StaffTip", "Staff_Tip", "WeaponTip", "RightHand");
            if (result.StaffTip == null && result.HandMount != null)
            {
                var tip = new GameObject("StaffTip_Auto").transform;
                tip.SetParent(result.HandMount, false);
                tip.localPosition = new Vector3(0f, 0.15f, 0.35f);
                result.StaffTip = tip;
            }

            WireAnimatorToDriver(root, animator, path);
            result.AnimatorOk = animator != null && animator.runtimeAnimatorController != null;
            result.AnimatorStatus = DescribeAnimatorStatus(animator);
            Debug.Log($"[WorldAssetPlacer] Attached {kind}: {result.ModelSource} | {path} | scale={result.AppliedScale:F2} | {result.AnimatorStatus}");
            return true;
        }

        static bool IsFantasyGoblinPath(string prefabPath)
        {
            if (string.IsNullOrEmpty(prefabPath)) return false;
            string lower = prefabPath.ToLowerInvariant();
            return lower.Contains("assets/goblin") || lower.Contains("fantasy goblin");
        }

        static void ApplyGoblinMaterialTint(GameObject instance, Color tint, float mix)
        {
            foreach (var r in instance.GetComponentsInChildren<Renderer>())
            {
                if (r == null) continue;
                var mat = r.material;
                if (mat == null) continue;
                var baseCol = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
                var blended = Color.Lerp(baseCol, tint, mix);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", blended);
                else mat.color = blended;
            }
        }

        static void WireGoblinRigBones(CharacterModelKind kind, Transform modelRoot, ref CharacterAttachResult result)
        {
            if (kind != CharacterModelKind.GoblinMelee && kind != CharacterModelKind.GoblinArcher && kind != CharacterModelKind.GoblinElite)
                return;

            result.BodyPivot = FindCharacterBone(modelRoot, "spine_02", "spine_01", "root", "hips", "Hips");
            result.LeftArm = FindCharacterBone(modelRoot, "upperarm_l", "LeftArm", "arm_l");
            result.RightArm = FindCharacterBone(modelRoot, "upperarm_r", "RightArm", "arm_r");
            result.LeftLeg = FindCharacterBone(modelRoot, "thigh_l", "LeftLeg", "leg_l");
            result.RightLeg = FindCharacterBone(modelRoot, "thigh_r", "RightLeg", "leg_r");
            result.HandMount = FindCharacterBone(modelRoot, "hand_r", "Hand_R", "RightHand");
        }

        static string DescribeModelSource(string prefabPath)
        {
            if (string.IsNullOrEmpty(prefabPath)) return "Unknown";
            string lower = prefabPath.ToLowerInvariant();
            if (lower.Contains("ganzse")) return "GanzSe";
            if (lower.Contains("assets/goblin") || lower.Contains("fantasy goblin")) return "FantasyGoblin";
            if (lower.Contains("invector") || lower.Contains("vbot") || lower.Contains("thirdpersoncontroller")) return "Invector";
            if (lower.Contains("starter assets") && lower.Contains("armature")) return "StarterAssets";
            return System.IO.Path.GetFileNameWithoutExtension(prefabPath);
        }

        static string DescribeAnimatorStatus(Animator animator)
        {
            if (animator == null) return "brak Animatora — proceduralna animacja fallback";
            if (animator.runtimeAnimatorController == null) return "brak controllera — proceduralna animacja fallback";
            string ctrl = animator.runtimeAnimatorController.name;
            if (ctrl.Contains("BasicLocomotion") || ctrl.Contains("Invector"))
                return $"OK (Invector locomotion: InputMagnitude)";
            return $"OK ({ctrl})";
        }

        static void StripInvectorGameplayComponents(GameObject instance)
        {
            foreach (var behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null) continue;
                string typeName = behaviour.GetType().FullName ?? behaviour.GetType().Name;
                if (typeName.StartsWith("Invector.") || typeName.StartsWith("vThirdPerson"))
                    UnityEngine.Object.Destroy(behaviour);
            }
            foreach (var cc in instance.GetComponentsInChildren<CharacterController>(true))
                UnityEngine.Object.Destroy(cc);
            foreach (var rb in instance.GetComponentsInChildren<Rigidbody>(true))
                UnityEngine.Object.Destroy(rb);
        }

        static void RegroundCharacterModel(Transform modelRoot)
        {
            var renderers = modelRoot.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            float parentWorldY = modelRoot.parent != null ? modelRoot.parent.position.y : 0f;
            float delta = bounds.min.y - parentWorldY;
            modelRoot.localPosition -= new Vector3(0f, delta, 0f);
        }

        static void WireAnimatorToDriver(Transform root, Animator animator, string prefabPath)
        {
            if (animator == null) return;
            animator.applyRootMotion = false;

            var driver = root.GetComponent<CharacterAnimDriver>();
            if (driver == null) return;
            driver.animator = animator;
            if (animator.runtimeAnimatorController != null
                && (animator.runtimeAnimatorController.name.Contains("BasicLocomotion")
                    || animator.runtimeAnimatorController.name.Contains("Invector")))
            {
                driver.ConfigureForInvectorLocomotion();
            }

            var body = FindDeepChildBone(animator.transform, "spine_02")
                       ?? FindDeepChildBone(animator.transform, "Base Character Root");
            driver.bodyPivot = body != null ? body : animator.transform;
        }

        static bool IsGanzSeCharacterPath(string prefabPath)
        {
            if (string.IsNullOrEmpty(prefabPath)) return false;
            string lower = prefabPath.ToLowerInvariant();
            return lower.Contains("ganzse");
        }

        /// <summary>
        /// Jedna część pancerza na kategorię — zielony (Color 1) + brązowy pas (Color 2), kaptur zamiast hełmu.
        /// </summary>
        static void ConfigureGanzSeDruidLoadout(GameObject instance)
        {
            var armorRoot = instance.transform.Find("ARMOR PARTS");
            if (armorRoot == null)
            {
                foreach (var t in instance.GetComponentsInChildren<Transform>())
                {
                    if (t.name != "ARMOR PARTS") continue;
                    armorRoot = t;
                    break;
                }
            }
            if (armorRoot == null) return;

            var druidPieces = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "HEADS", "Head Armor Type 2 Color 1" },
                { "CHESTS", "Chest Armor Type 2 Color 1" },
                { "ARMS", "Arm Armor Type 1 Color 1" },
                { "LEGS", "Legs Armor Type 1 Color 1" },
                { "FEET", "Feet Armor Type 1 Color 1" },
                { "BELTS", "Belt Armor Type 1 Color 2" },
            };

            foreach (Transform category in armorRoot)
            {
                druidPieces.TryGetValue(category.name, out var preferredName);
                Transform pick = null;
                if (!string.IsNullOrEmpty(preferredName))
                    pick = category.Find(preferredName);

                if (pick == null)
                {
                    foreach (Transform child in category)
                    {
                        if (!child.name.Contains("Color 1", StringComparison.OrdinalIgnoreCase)) continue;
                        pick = child;
                        break;
                    }
                }

                foreach (Transform child in category)
                    child.gameObject.SetActive(child == pick);
            }

            var faceRoot = instance.transform.Find("FACE DETAILS PARTS");
            if (faceRoot != null) faceRoot.gameObject.SetActive(true);
        }

        static string DescribeCharacterMiss(CharacterModelKind kind)
        {
            switch (kind)
            {
                case CharacterModelKind.Druid:
                    if (GameAssetRegistry.TryGetPath(GameAssetRegistry.Slot.DruidModel, out _))
                        return "registry zwrócił null mimo ścieżki";
                    return "brak prefabu GanzSe / Starter Assets PlayerArmature w Assets/ — zaimportuj paczkę postaci";
                case CharacterModelKind.GoblinMelee:
                    return "brak Fantasy Goblin (Assets/Goblin/Prefab/skin1.prefab) w Assets/";
                case CharacterModelKind.GoblinArcher:
                    return "brak Fantasy Goblin (Assets/Goblin/Prefab/skin2.prefab) w Assets/";
                case CharacterModelKind.GoblinElite:
                    return "brak Fantasy Goblin (Assets/Goblin/Prefab/skin3.prefab) w Assets/";
                default:
                    return "nieznany typ postaci";
            }
        }

        static void ApplyDruidMaterialTint(GameObject instance, string prefabPath)
        {
            if (string.IsNullOrEmpty(prefabPath)) return;
            string lower = prefabPath.ToLowerInvariant();
            bool ganzSe = lower.Contains("ganzse");
            bool starterArmature = lower.Contains("starter assets") && lower.Contains("armature");
            if (!ganzSe && !starterArmature) return;

            var robe = new Color(0.20f, 0.36f, 0.18f);
            var trim = new Color(0.38f, 0.28f, 0.14f);
            float robeMix = ganzSe ? 0.35f : 0.55f;

            foreach (var r in instance.GetComponentsInChildren<Renderer>())
            {
                if (r == null) continue;
                var mat = r.material;
                if (mat == null) continue;

                string piece = r.gameObject.name.ToLowerInvariant();
                Color target = robe;
                if (piece.Contains("belt") || piece.Contains("feet") || piece.Contains("boot"))
                    target = trim;

                var baseCol = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
                var tinted = Color.Lerp(baseCol, target, robeMix);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tinted);
                else mat.color = tinted;
                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", trim * 0.12f);
            }
        }

        static Transform FindHandMountFallback(Transform root)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>())
            {
                string n = t.name.ToLowerInvariant();
                if (n.Contains("hand") && (n.Contains("right") || n.Contains("_r") || n.EndsWith(" r")))
                    return t;
            }

            var baseRoot = FindDeepChildBone(root, "Base Character Root");
            if (baseRoot == null) return null;

            var mount = new GameObject("HandMount_Auto").transform;
            mount.SetParent(baseRoot, false);
            mount.localPosition = new Vector3(0.42f, 1.05f, 0.12f);
            return mount;
        }

        public static GameObject TrySpawnVfx(VfxKind kind, Vector3 position, Quaternion rotation, float lifetime = 3f, Transform parent = null)
        {
            EnsureCharacterAssetsScanned();
            var prefab = ResolveVfxPrefab(kind);
            if (prefab == null) return null;
            var go = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            if (lifetime > 0f) UnityEngine.Object.Destroy(go, lifetime);
            return go;
        }

        static void EnsureCharacterAssetsScanned()
        {
            if (_characterAssetsScanned) return;
            _characterAssetsScanned = true;
#if UNITY_EDITOR
            foreach (CharacterModelKind k in Enum.GetValues(typeof(CharacterModelKind)))
                ResolvedCharacterPaths[k] = ResolveAssetPath(CharacterCandidates[k], CharacterSearchTokens[k]);
            foreach (VfxKind k in Enum.GetValues(typeof(VfxKind)))
                ResolvedVfxPaths[k] = ResolveAssetPath(VfxCandidates[k], VfxSearchTokens[k]);
#endif
        }

        static GameObject ResolveCharacterPrefab(CharacterModelKind kind, out string path)
        {
            path = null;
            GameObject prefab;
            if (kind == CharacterModelKind.Druid && GameAssetRegistry.DruidModel != null)
            {
                path = GetRegistryPath(GameAssetRegistry.Slot.DruidModel);
                return GameAssetRegistry.DruidModel;
            }
            if (kind == CharacterModelKind.GoblinMelee && GameAssetRegistry.GoblinModel != null)
            {
                path = GetRegistryPath(GameAssetRegistry.Slot.GoblinModel);
                return GameAssetRegistry.GoblinModel;
            }
            if (kind == CharacterModelKind.GoblinElite && GameAssetRegistry.GoblinElite != null)
            {
                path = GetRegistryPath(GameAssetRegistry.Slot.GoblinElite);
                return GameAssetRegistry.GoblinElite;
            }
            if (kind == CharacterModelKind.GoblinArcher)
            {
                foreach (var c in GameAssetPaths.GoblinArcherModel)
                {
                    if (TryLoadPrefabAtPath(c, out prefab))
                    {
                        path = c;
                        return prefab;
                    }
                }
            }

            EnsureCharacterAssetsScanned();
            if (!TryLoadPrefabAtPath(ResolvedCharacterPaths.TryGetValue(kind, out var p) ? p : null, out prefab))
            {
                if (!CharacterCandidates.TryGetValue(kind, out var list)) return null;
                foreach (var c in list)
                {
                    if (TryLoadPrefabAtPath(c, out prefab)) { path = c; return prefab; }
                }
                return null;
            }
            path = p;
            return prefab;
        }

        static GameObject ResolveVfxPrefab(VfxKind kind)
        {
            if (kind == VfxKind.FireballCast && GameAssetRegistry.VfxFireball != null) return GameAssetRegistry.VfxFireball;
            if (kind == VfxKind.Heal && GameAssetRegistry.VfxHeal != null) return GameAssetRegistry.VfxHeal;

            EnsureCharacterAssetsScanned();
            if (TryLoadPrefabAtPath(ResolvedVfxPaths.TryGetValue(kind, out var p) ? p : null, out var prefab)) return prefab;
            if (!VfxCandidates.TryGetValue(kind, out var list)) return null;
            foreach (var c in list)
                if (TryLoadPrefabAtPath(c, out prefab)) return prefab;
            return null;
        }

        static string GetRegistryPath(GameAssetRegistry.Slot slot)
        {
            return GameAssetRegistry.TryGetPath(slot, out var path) ? path : null;
        }

#if UNITY_EDITOR
        static string ResolveAssetPath(string[] candidates, string[] searchTokens)
        {
            foreach (var c in candidates)
                if (!string.IsNullOrEmpty(c) && AssetDatabase.LoadAssetAtPath<GameObject>(c) != null) return c;

            int bestScore = 0;
            string bestPath = null;
            foreach (var token in searchTokens)
            {
                string normToken = GameAssetPacks.NormalizeToken(token);
                foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;
                    if (path.ToLowerInvariant().Contains("/parts/")
                        || path.ToLowerInvariant().Contains("non-skinned mesh parts")
                        || path.ToLowerInvariant().Contains("/modular parts/")) continue;

                    string normPath = GameAssetPacks.NormalizeToken(path);
                    string normName = GameAssetPacks.NormalizeToken(System.IO.Path.GetFileNameWithoutExtension(path));
                    int score = 0;
                    if (normPath.Contains(normToken)) score += 25;
                    if (normName.Contains(normToken)) score += 20;
                    if (score <= bestScore) continue;
                    if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;

                    bestScore = score;
                    bestPath = path;
                }
            }
            return bestPath;
        }

        static bool TryLoadPrefabAtPath(string assetPath, out GameObject prefab)
        {
            prefab = null;
            if (string.IsNullOrEmpty(assetPath)) return false;
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            return prefab != null;
        }
#else
        static bool TryLoadPrefabAtPath(string assetPath, out GameObject prefab) { prefab = null; return false; }
#endif

        static void FitCharacterHeight(Transform modelRoot, float targetHeight)
        {
            var renderers = modelRoot.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            if (bounds.size.y < 0.01f) return;
            modelRoot.localScale = Vector3.one * (targetHeight / bounds.size.y);
            bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            modelRoot.localPosition = new Vector3(0f, -(bounds.min.y - modelRoot.position.y) * modelRoot.localScale.y, 0f);
        }

        static void StripCharacterColliders(GameObject go)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>()) UnityEngine.Object.Destroy(c);
        }

        static Transform FindCharacterBone(Transform root, params string[] names)
        {
            foreach (var n in names) { var t = FindDeepChildBone(root, n); if (t != null) return t; }
            return null;
        }

        static Transform FindDeepChildBone(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Equals(name, StringComparison.OrdinalIgnoreCase)) return child;
                var found = FindDeepChildBone(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
