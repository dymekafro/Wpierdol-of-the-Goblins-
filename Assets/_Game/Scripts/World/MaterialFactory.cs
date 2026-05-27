using System.Collections.Generic;
using UnityEngine;

namespace WPG.World
{
    // Tworzy URP Lit materiały runtime cache'owane po kolorze i flagi emission.
    public static class MaterialFactory
    {
        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();
        private static Shader _litShader;

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
