using System;
using UnityEngine;
using UnityEngine.Rendering;
using WPG.Core;

namespace WPG.World
{
    /// <summary>
    /// Konwertuje materiały Built-in / Legacy / Invector custom → URP Lit (naprawia magenta #FF00FF).
    /// Liście / cutout / transparent: URP Lit + Alpha Clipping (_AlphaClip, _Cutoff).
    /// </summary>
    public static class MaterialUpgrader
    {
        public const string UrpLitShaderName = "Universal Render Pipeline/Lit";
        public const string FantasyForestMaterialsFolder =
            "Assets/Fantasy Forest Environment Free Sample/Materials";

        /// <summary>Domyślny folder paczki Invector LITE w tym projekcie.</summary>
        public const string InvectorMaterialsFolder = GameAssetPaths.InvectorFolder;

        private const string UrpUnlit = "Universal Render Pipeline/Unlit";
        private const string UrpSky = "Universal Render Pipeline/Skybox/Lit";
        private const int AlphaTestQueue = (int)RenderQueue.AlphaTest;

        private static Shader _urpLitShader;
        private static Shader _urpUnlitShader;

        public static bool IsInvectorAssetPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return false;

            var lower = assetPath.Replace('\\', '/').ToLowerInvariant();
            foreach (var token in GameAssetPaths.InvectorFolderTokens)
            {
                if (lower.Contains(token))
                    return true;
            }

            return lower.Contains("/invector");
        }

        public static bool IsInvectorMaterial(Material mat)
        {
            if (mat == null) return false;
            return IsInvectorAssetPath(GetMaterialAssetPath(mat))
                   || IsInvectorCharacterMaterial(mat);
        }

        public static bool IsInvectorCharacterMaterial(Material mat)
        {
            if (mat == null) return false;

            string name = mat.name.ToLowerInvariant();
            return name.Contains("body") || name.Contains("joint") || name.Contains("rubber")
                   || name.Contains("member") || name.Contains("plastic") || name.Contains("metal")
                   || name.Contains("cloth") || name.Contains("skin") || name.Contains("vbot")
                   || name.Contains("character");
        }

        public static bool IsInvectorBrokenShader(Shader shader)
        {
            if (shader == null) return true;

            string shaderName = shader.name;
            if (shaderName == "Hidden/InternalErrorShader") return true;
            if (shaderName.StartsWith("Invector/", StringComparison.OrdinalIgnoreCase)) return true;
            if (shaderName.Contains("Invector", StringComparison.OrdinalIgnoreCase)) return true;

            return shaderName.StartsWith("Standard", StringComparison.Ordinal)
                   || shaderName.Contains("Legacy Shaders/", StringComparison.Ordinal)
                   || shaderName.StartsWith("Mobile/", StringComparison.Ordinal)
                   || shaderName.StartsWith("Particles/", StringComparison.Ordinal)
                   || (shaderName.StartsWith("Skybox/", StringComparison.Ordinal)
                       && !shaderName.StartsWith("Universal Render Pipeline/", StringComparison.Ordinal));
        }

        public static bool NeedsUpgrade(Material mat)
        {
            if (mat == null || mat.shader == null) return false;

            string shaderName = mat.shader.name;
            if (shaderName.StartsWith("Universal Render Pipeline/"))
            {
                if (IsFoliageMaterial(mat) && !HasAlphaClipEnabled(mat))
                    return true;
                if (IsInvectorMaterial(mat) && NeedsInvectorPropertyMigration(mat))
                    return true;
                return false;
            }

            return shaderName == "Hidden/InternalErrorShader"
                   || !mat.shader.isSupported
                   || IsInvectorBrokenShader(mat.shader)
                   || shaderName.Contains("Standard")
                   || shaderName.Contains("Legacy")
                   || shaderName.StartsWith("Nature/")
                   || shaderName.StartsWith("Fantasy Forest/")
                   || shaderName.Contains("Built-in")
                   || IsCutoutLegacyShader(mat.shader);
        }

        public static bool IsFoliageMaterial(Material mat)
        {
            if (mat == null) return false;

            string name = mat.name.ToLowerInvariant();
            if (name.Contains("leaf") || name.Contains("leaves") || name.Contains("foliage")
                || name.Contains("branch") || name.Contains("branches"))
                return true;

            if (name.Contains("grass") && !name.Contains("dirt"))
                return true;

            if (name.Contains("tree") && (name.Contains("leaf") || name.Contains("branch")))
                return true;

            return mat.shader != null && IsCutoutLegacyShader(mat.shader);
        }

        public static bool IsCutoutLegacyShader(Shader shader)
        {
            if (shader == null) return false;

            string shaderName = shader.name;
            return shaderName.StartsWith("Nature/")
                   || shaderName.StartsWith("Fantasy Forest/")
                   || shaderName.Contains("Cutout")
                   || shaderName.Contains("Transparent")
                   || shaderName.Contains("Foliage")
                   || shaderName.Contains("Tree");
        }

        public static bool IsInvectorCutoutMaterial(Material mat)
        {
            if (mat == null) return false;

            if (IsFoliageMaterial(mat) || IsCutoutLegacyShader(mat.shader))
                return true;

            string name = mat.name.ToLowerInvariant();
            if (name.Contains("banner") || name.Contains("logo") || name.Contains("icon")
                || name.Contains("cutout") || name.Contains("alpha") || name.Contains("health"))
                return true;

            if (mat.HasProperty("_Mode") && mat.GetFloat("_Mode") >= 1f)
                return true;

            if (mat.HasProperty("_Surface") && mat.GetFloat("_Surface") >= 1f)
                return true;

            return mat.IsKeywordEnabled("_ALPHATEST_ON")
                   || mat.IsKeywordEnabled("_ALPHABLEND_ON")
                   || mat.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON");
        }

        public static bool UpgradeMaterial(Material mat)
        {
            if (mat == null || mat.shader == null) return false;

            bool foliage = IsFoliageMaterial(mat);
            bool invectorCutout = IsInvectorCutoutMaterial(mat);
            bool cutoutLegacy = IsCutoutLegacyShader(mat.shader);
            bool isSkybox = mat.shader.name.StartsWith("Skybox/", StringComparison.Ordinal);
            bool needsShaderSwap = !mat.shader.name.StartsWith("Universal Render Pipeline/")
                                   || mat.shader.name == "Hidden/InternalErrorShader"
                                   || !mat.shader.isSupported
                                   || IsInvectorBrokenShader(mat.shader);
            bool needsPropertyMigration = NeedsInvectorPropertyMigration(mat);

            if (!needsShaderSwap && !needsPropertyMigration
                && !(foliage && !HasAlphaClipEnabled(mat))
                && !(invectorCutout && !HasAlphaClipEnabled(mat)))
                return false;

            Color baseColor = ReadBaseColor(mat);
            Texture mainTex = ReadMainTexture(mat);
            Texture bumpMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
            Texture metallicMap = mat.HasProperty("_MetallicGlossMap") ? mat.GetTexture("_MetallicGlossMap") : null;

            float smoothness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness")
                : mat.HasProperty("_Smoothness") ? mat.GetFloat("_Smoothness") : 0.15f;

            float metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
            float cutoff = mat.HasProperty("_Cutoff") ? mat.GetFloat("_Cutoff") : 0.5f;

            bool alphaClip = foliage || cutoutLegacy || invectorCutout
                             || mat.IsKeywordEnabled("_ALPHATEST_ON")
                             || (mat.HasProperty("_Mode") && mat.GetFloat("_Mode") >= 1f)
                             || mat.shader.name.Contains("Cutout");

            Shader urpShader;
            if (isSkybox)
            {
                urpShader = UrpSkyShader ?? UrpUnlitShader ?? UrpLitShader;
            }
            else
            {
                urpShader = UrpLitShader ?? UrpUnlitShader;
            }

            if (urpShader == null) return false;

            if (needsShaderSwap)
                mat.shader = urpShader;

            if (alphaClip && baseColor.a < 0.01f)
                baseColor.a = 1f;

            ApplyBaseColor(mat, baseColor);
            ApplyMainTexture(mat, mainTex);
            if (bumpMap != null && mat.HasProperty("_BumpMap")) mat.SetTexture("_BumpMap", bumpMap);
            if (metallicMap != null && mat.HasProperty("_MetallicGlossMap")) mat.SetTexture("_MetallicGlossMap", metallicMap);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

            if (alphaClip)
                ApplyAlphaClipSettings(mat, cutoff, doubleSided: foliage || cutoutLegacy || invectorCutout);
            else if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 2f);

            return true;
        }

        public static void ApplyAlphaClipSettings(Material mat, float cutoff = 0.5f, bool doubleSided = true)
        {
            if (mat == null) return;

            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 0f);
            if (mat.HasProperty("_AlphaClip")) mat.SetFloat("_AlphaClip", 1f);
            if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", cutoff);
            mat.EnableKeyword("_ALPHATEST_ON");
            if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", doubleSided ? 0f : 2f);
            mat.renderQueue = AlphaTestQueue;
        }

        public static void UpgradeHierarchy(GameObject root)
        {
            if (root == null) return;

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                UpgradeRenderer(renderer);
        }

        private static void UpgradeRenderer(Renderer renderer)
        {
            if (renderer == null) return;

            var shared = renderer.sharedMaterials;
            var result = new Material[shared.Length];
            var changed = false;

            for (int i = 0; i < shared.Length; i++)
            {
                var src = shared[i];
                if (src == null)
                {
                    result[i] = null;
                    continue;
                }

                if (!NeedsUpgrade(src))
                {
                    result[i] = src;
                    continue;
                }

                var inst = new Material(src);
                if (UpgradeMaterial(inst))
                {
                    result[i] = inst;
                    changed = true;
                }
                else
                {
                    result[i] = src;
                }
            }

            if (changed) renderer.materials = result;
        }

        private static bool HasAlphaClipEnabled(Material mat)
        {
            return mat.HasProperty("_AlphaClip") && mat.GetFloat("_AlphaClip") >= 0.5f
                   && mat.IsKeywordEnabled("_ALPHATEST_ON");
        }

        private static bool NeedsInvectorPropertyMigration(Material mat)
        {
            if (mat == null) return false;

            Texture mainTex = ReadMainTexture(mat);
            if (mainTex == null) return false;

            if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") == null)
                return true;

            if (mat.HasProperty("_BaseColor") && mat.HasProperty("_Color"))
            {
                var baseColor = mat.GetColor("_BaseColor");
                var legacyColor = mat.GetColor("_Color");
                if (baseColor == Color.white && legacyColor != Color.white)
                    return true;
            }

            return false;
        }

        private static Color ReadBaseColor(Material mat)
        {
            Color baseColor = Color.white;
            if (mat.HasProperty("_Color"))
                baseColor = mat.GetColor("_Color");
            else if (mat.HasProperty("_BaseColor"))
                baseColor = mat.GetColor("_BaseColor");

            if (mat.HasProperty("_BaseColor"))
            {
                var urpColor = mat.GetColor("_BaseColor");
                if (baseColor == Color.white && urpColor != Color.white)
                    baseColor = urpColor;
            }

            return baseColor;
        }

        private static Texture ReadMainTexture(Material mat)
        {
            Texture mainTex = null;
            if (mat.HasProperty("_MainTex")) mainTex = mat.GetTexture("_MainTex");
            if (mainTex == null && mat.HasProperty("_BaseMap")) mainTex = mat.GetTexture("_BaseMap");
            return mainTex;
        }

        private static void ApplyBaseColor(Material mat, Color baseColor)
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", baseColor);
        }

        private static void ApplyMainTexture(Material mat, Texture mainTex)
        {
            if (mainTex == null) return;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", mainTex);
        }

        private static string GetMaterialAssetPath(Material mat)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(mat);
#else
            return mat != null ? mat.name : string.Empty;
#endif
        }

        private static Shader UrpSkyShader
        {
            get
            {
                if (_urpSkyShader == null) _urpSkyShader = Shader.Find(UrpSky);
                return _urpSkyShader;
            }
        }

        private static Shader _urpSkyShader;

        private static Shader UrpLitShader
        {
            get
            {
                if (_urpLitShader == null) _urpLitShader = Shader.Find(UrpLitShaderName);
                return _urpLitShader;
            }
        }

        private static Shader UrpUnlitShader
        {
            get
            {
                if (_urpUnlitShader == null) _urpUnlitShader = Shader.Find(UrpUnlit);
                return _urpUnlitShader;
            }
        }
    }
}
