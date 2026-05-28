using System.Collections.Generic;
using UnityEngine;
using WPG.Core;

namespace WPG.World
{
    // Tworzy URP Lit materiały runtime cache'owane po kolorze / teksturze i flagi emission.
    public static class MaterialFactory
    {
        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();
        private static Shader _litShader;

        public const float DefaultGroundTile = 7f;
        public const float PathTile = 8f;
        public const float MeadowTile = 6f;

        // Ciemna zieleń lasu — bez ciepłego / pomarańczowego tintu.
        private static readonly Color GroundDark = new Color(0.10f, 0.26f, 0.08f);

        /// <summary>Ustawiane przez ForestAtmosphereSettings (live edit podłoża).</summary>
        public static Color? GroundColorOverride { get; set; }
        private static readonly Color GrassMoss = new Color(0.10f, 0.24f, 0.08f);
        private static readonly Color GrassMid = new Color(0.12f, 0.28f, 0.10f);
        private static readonly Color GrassTextureTint = new Color(0.55f, 0.82f, 0.50f);
        private static readonly Color PathColor = new Color(0.28f, 0.22f, 0.14f);

        private static Shader LitShader
        {
            get
            {
                if (_litShader == null)
                    _litShader = Shader.Find("Universal Render Pipeline/Lit");
                if (_litShader == null)
                    _litShader = Shader.Find("Standard");
                return _litShader;
            }
        }

        /// <summary>Główna płaszczyzna lasu — stały ciemnozielony (bez tekstury trawy = brak pomarańczy).</summary>
        public static Material GetForestFloor()
        {
            var color = GroundColorOverride ?? GroundDark;
            return Get(color, 0.12f);
        }

        /// <summary>Polany — zielony tint na grass01 lub stały kolor.</summary>
        public static Material GetGrassMeadow(bool alternate = false)
        {
            var tex = GameAssetRegistry.GrassGroundTexture;
            if (tex != null)
                return GetTextured(tex, GrassTextureTint, MeadowTile, 0.14f);
            return Get(alternate ? GrassMoss : GrassMid, 0.14f);
        }

        /// <summary>Ścieżki — tylko brązowa ziemia (dirt01).</summary>
        public static Material GetPathDirt()
        {
            var tex = GameAssetRegistry.PathDirtTexture;
            if (tex != null)
                return GetTextured(tex, Color.white, PathTile, 0.1f);
            return Get(PathColor, 0.1f);
        }

        public static Material GetTextured(Texture2D texture, Color tint, float tileScale, float smoothness = 0.2f)
        {
            if (texture == null) return Get(tint, smoothness);

            string key = $"tex_{texture.name}_{texture.width}x{texture.height}_{tint.r:F2}_{tint.g:F2}_{tint.b:F2}_{tileScale:F1}_{smoothness:F2}";
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var mat = new Material(LitShader);
            mat.name = "WPG_Tex_" + texture.name;

            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", texture);
                mat.SetTextureScale("_BaseMap", new Vector2(tileScale, tileScale));
            }

            if (mat.HasProperty("_MainTex"))
            {
                mat.SetTexture("_MainTex", texture);
                mat.SetTextureScale("_MainTex", new Vector2(tileScale, tileScale));
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            else mat.color = tint;

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            else if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

            Cache[key] = mat;
            return mat;
        }

        public static Material Get(Color color, float smoothness = 0.2f, Color? emission = null, float emissionIntensity = 0f)
        {
            string key = $"{color.r:F2}_{color.g:F2}_{color.b:F2}_{smoothness:F2}_" +
                         (emission.HasValue ? $"{emission.Value.r:F2}_{emission.Value.g:F2}_{emission.Value.b:F2}_{emissionIntensity:F2}" : "noemit");
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var mat = new Material(LitShader);
            mat.name = "WPG_" + key;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else mat.color = color;

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            else if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

            if (emission.HasValue && emissionIntensity > 0f)
            {
                mat.EnableKeyword("_EMISSION");
                var emisFinal = emission.Value * emissionIntensity;
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emisFinal);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            Cache[key] = mat;
            return mat;
        }

        public static Material UI(Color color)
        {
            return Get(color, 0.1f);
        }
    }
}
