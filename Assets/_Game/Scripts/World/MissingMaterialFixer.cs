using UnityEngine;
using WPG.Core;

namespace WPG.World
{
    /// <summary>
    /// Naprawia renderery z brakującym materiałem (magenta / fileID: 0) w WorldRoot i podobnych obiektach.
    /// </summary>
    public static class MissingMaterialFixer
    {
        private static Material _goblinBody;
        private static Material _goblinParts;
        private static Material _fireGlow;
        private static Material _mushroomGlow;
        private static Material _mushroomStem;
        private static Material _smoke;

        public static int FixNullMaterials(GameObject root)
        {
            if (root == null) return 0;

            EnsureGoblinMaterials();
            EnsureFixupMaterials();
            int fixedCount = 0;

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (TryFixRenderer(renderer))
                    fixedCount++;
            }

            if (fixedCount > 0)
                Debug.Log($"[MissingMaterialFixer] Przypisano materiały do {fixedCount} rendererów w '{root.name}'.");

            return fixedCount;
        }

        private static bool TryFixRenderer(Renderer renderer)
        {
            if (renderer == null) return false;

            var shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0) return false;

            var result = new Material[shared.Length];
            var changed = false;

            for (int i = 0; i < shared.Length; i++)
            {
                var src = shared[i];
                if (!NeedsReplacement(src))
                {
                    result[i] = src;
                    continue;
                }

                result[i] = ResolveMaterial(renderer, i);
                changed = true;
            }

            if (changed)
                renderer.sharedMaterials = result;

            return changed;
        }

        private static bool NeedsReplacement(Material mat)
        {
            if (mat == null) return true;
            if (mat.shader == null) return true;
            if (mat.shader.name == "Hidden/InternalErrorShader") return true;
            return !mat.shader.isSupported;
        }

        private static Material ResolveMaterial(Renderer renderer, int slotIndex)
        {
            var go = renderer.gameObject;
            var nameLower = go.name.ToLowerInvariant();

            if (renderer is ParticleSystemRenderer || nameLower == "smoke")
                return _smoke != null
                    ? _smoke
                    : MaterialFactory.GetParticle(new Color(0.3f, 0.25f, 0.2f, 0.55f));

            if (IsUnderNamedAncestor(go, "MushroomLight"))
            {
                if (nameLower == "sphere")
                    return _mushroomGlow != null ? _mushroomGlow : GetMushroomGlowMaterial(go);
                if (nameLower == "cylinder")
                    return _mushroomStem != null ? _mushroomStem : MaterialFactory.Get(new Color(0.85f, 0.8f, 0.7f));
            }

            if (IsUnderNamedAncestor(go, "Campfire", "BaseFire", "Fire"))
            {
                if (nameLower == "sphere")
                    return _fireGlow != null
                        ? _fireGlow
                        : MaterialFactory.Get(new Color(1f, 0.55f, 0.2f), 0.3f, new Color(1f, 0.5f, 0.15f), 4f);
                if (nameLower == "cylinder")
                    return MaterialFactory.Get(new Color(0.16f, 0.1f, 0.05f));
            }

            if (IsUnderNamedAncestor(go, "PowerSite"))
            {
                if (nameLower == "sphere")
                    return MaterialFactory.Get(new Color(0.7f, 0.5f, 1f), 0.5f, new Color(0.7f, 0.5f, 1f), 3f);
                if (nameLower == "cube")
                    return MaterialFactory.Get(new Color(0.34f, 0.36f, 0.33f), 0.4f);
            }

            if (IsGoblinRenderer(renderer))
            {
                if (nameLower.Contains("parts") || slotIndex > 0)
                    return _goblinParts != null ? _goblinParts : MaterialFactory.Get(new Color(0.35f, 0.48f, 0.22f));
                return _goblinBody != null ? _goblinBody : MaterialFactory.Get(new Color(0.45f, 0.55f, 0.25f));
            }

            if (nameLower == "sphere")
                return MaterialFactory.Get(new Color(0.6f, 0.95f, 0.55f), 0.4f, new Color(0.5f, 1f, 0.7f), 5f);

            if (nameLower == "cylinder")
                return MaterialFactory.Get(new Color(0.25f, 0.16f, 0.10f));

            return MaterialFactory.Get(new Color(0.5f, 0.5f, 0.5f));
        }

        private static Material GetMushroomGlowMaterial(GameObject sphereGo)
        {
            var root = sphereGo.transform.parent;
            Color glow = new Color(0.5f, 1f, 0.3f);
            if (root != null)
            {
                var light = root.GetComponentInChildren<Light>();
                if (light != null)
                    glow = light.color;
            }

            return MaterialFactory.Get(glow, 0.5f, glow, 3f);
        }

        private static bool IsGoblinRenderer(Renderer renderer)
        {
            var go = renderer.gameObject;
            var nameLower = go.name.ToLowerInvariant();
            if (nameLower.Contains("goblin") || nameLower.Contains("gobs_"))
                return true;

            if (go.GetComponentInParent<WPG.Enemies.GoblinBase>() != null)
                return true;

            var parent = go.transform.parent;
            while (parent != null)
            {
                var parentName = parent.name.ToLowerInvariant();
                if (parentName.Contains("goblin") || parentName.Contains("goblin_storm"))
                    return true;
                parent = parent.parent;
            }

            return renderer is SkinnedMeshRenderer skinned
                   && skinned.sharedMesh != null
                   && skinned.sharedMesh.name.ToLowerInvariant().Contains("goblin");
        }

        private static bool IsUnderNamedAncestor(GameObject go, params string[] names)
        {
            var t = go.transform;
            while (t != null)
            {
                foreach (var name in names)
                {
                    if (t.name.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }

                t = t.parent;
            }

            return false;
        }

        private static void EnsureFixupMaterials()
        {
            if (_fireGlow == null)
                _fireGlow = LoadMaterialAsset(GameAssetPaths.WPGFireGlowMaterial);
            if (_mushroomGlow == null)
                _mushroomGlow = LoadMaterialAsset(GameAssetPaths.WPGMushroomGlowMaterial);
            if (_mushroomStem == null)
                _mushroomStem = LoadMaterialAsset(GameAssetPaths.WPGMushroomStemMaterial);
            if (_smoke == null)
                _smoke = LoadMaterialAsset(GameAssetPaths.WPGSmokeMaterial);
        }

        private static void EnsureGoblinMaterials()
        {
            if (_goblinBody != null && _goblinParts != null) return;

            _goblinBody = LoadMaterialAsset(GameAssetPaths.GoblinBodyMaterialSkin2)
                          ?? LoadMaterialAsset(GameAssetPaths.GoblinBodyMaterialSkin1);
            _goblinParts = LoadMaterialAsset(GameAssetPaths.GoblinPartsMaterialSkin2)
                           ?? LoadMaterialAsset(GameAssetPaths.GoblinPartsMaterialSkin1);

            if (_goblinBody == null || _goblinParts == null)
                TryExtractFromGoblinPrefab();
        }

        private static void TryExtractFromGoblinPrefab()
        {
            GameAssetRegistry.Initialize();
            var prefab = GameAssetRegistry.GoblinModel ?? GameAssetRegistry.GoblinElite;
            if (prefab == null) return;

            foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat == null) continue;
                    var lower = mat.name.ToLowerInvariant();
                    if (_goblinBody == null && lower.Contains("body"))
                        _goblinBody = mat;
                    if (_goblinParts == null && lower.Contains("parts"))
                        _goblinParts = mat;
                }
            }
        }

        private static Material LoadMaterialAsset(string path)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
#else
            return null;
#endif
        }
    }
}
