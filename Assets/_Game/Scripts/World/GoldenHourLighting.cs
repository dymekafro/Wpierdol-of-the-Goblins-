using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace WPG.World
{
    /// <summary>
    /// Magiczny Las — gęsta mgła zielono-złota, las zielony, horyzont = mgła (bez niebieskiego / pomarańczowej zupy).
    /// </summary>
    public static class GoldenHourLighting
    {
        private const float GoldenHourStart = 17f;
        private const float GoldenHourEnd = 19f;

        // Domyślne wartości (używane gdy brak ForestAtmosphereSettings w scenie).
        public static readonly Color DefaultFogColor = new(0.42f, 0.45f, 0.38f);
        public const float DefaultFogDensity = 0.032f;
        public static readonly Color DefaultAmbientSky = new(0.35f, 0.38f, 0.32f);
        public static readonly Color DefaultAmbientEquator = new(0.28f, 0.35f, 0.25f);
        public static readonly Color DefaultAmbientGround = new(0.10f, 0.22f, 0.08f);
        public static readonly Color DefaultSunColor = new(0.95f, 0.88f, 0.70f);
        public const float DefaultSunIntensity = 0.85f;
        public static readonly Vector3 DefaultSunRotation = new(50f, -30f, 0f);
        public static readonly Color DefaultGroundColor = new(0.10f, 0.26f, 0.08f);

        public static Color FogSepia =>
            ForestAtmosphereSettings.Instance != null
                ? ForestAtmosphereSettings.Instance.fogColor
                : DefaultFogColor;

        private static readonly Color SkyZenithWarm = new(0.40f, 0.43f, 0.36f);
        private static readonly Color SkyHorizonWarm = new(0.42f, 0.45f, 0.38f); // horyzont = FogSepia

        public static bool Apply(Transform parent)
        {
            ApplyAtmosphere();
            ApplySkybox();

            if (TryApplyCelestialCycles())
            {
                ApplyAtmosphere();
                ApplySkybox();
                return true;
            }

            ApplyManualGoldenHour(parent);
            return false;
        }

        /// <summary>Mgła i ambient — zielono-szary las, minimalny pomarańczowy bounce.</summary>
        public static void ApplyAtmosphere()
        {
            var settings = ForestAtmosphereSettings.Instance
                           ?? UnityEngine.Object.FindAnyObjectByType<ForestAtmosphereSettings>();
            if (settings != null)
            {
                settings.ApplyAtmosphere();
                return;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = DefaultAmbientSky;
            RenderSettings.ambientEquatorColor = DefaultAmbientEquator;
            RenderSettings.ambientGroundColor = DefaultAmbientGround;
            RenderSettings.ambientIntensity = 1.0f;
            RenderSettings.reflectionIntensity = 0.5f;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = DefaultFogDensity;
            RenderSettings.fogColor = DefaultFogColor;
        }

        /// <summary>Proceduralne niebo — wyłącznie ciepłe odcienie (horyzont = mgła).</summary>
        public static void ApplySkybox()
        {
            var sky = CreateWarmHazeSky();
            if (sky == null)
            {
                RenderSettings.skybox = null;
                Debug.LogWarning("[GoldenHour] Brak Skybox/Procedural — niebo wypełni mgła.");
                return;
            }

            RenderSettings.skybox = sky;
            DynamicGI.UpdateEnvironment();
            Debug.Log($"[GoldenHour] Skybox: {sky.name} (shader={sky.shader.name})");
        }

        private static Material CreateWarmHazeSky()
        {
            var shader = FindProceduralSkyShader();
            if (shader == null) return null;

            var sky = new Material(shader);
            sky.name = "WPG_MagicForestHazeSky";

            if (sky.HasProperty("_SkyTint"))
                sky.SetColor("_SkyTint", SkyZenithWarm);
            if (sky.HasProperty("_GroundColor"))
                sky.SetColor("_GroundColor", SkyHorizonWarm);
            if (sky.HasProperty("_AtmosphereThickness"))
                sky.SetFloat("_AtmosphereThickness", 2.35f);
            if (sky.HasProperty("_SunSize"))
                sky.SetFloat("_SunSize", 0.04f);
            if (sky.HasProperty("_SunSizeConvergence"))
                sky.SetFloat("_SunSizeConvergence", 5f);
            if (sky.HasProperty("_Exposure"))
                sky.SetFloat("_Exposure", 0.92f);

            return sky;
        }

        private static Shader FindProceduralSkyShader()
        {
            return Shader.Find("Universal Render Pipeline/Skybox/Procedural")
                   ?? Shader.Find("Skybox/Procedural");
        }

        private static bool TryApplyCelestialCycles()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = asm.GetName().Name;
                if (name == null || !name.Contains("Celestial")) continue;

                Type managerType = asm.GetType("CelestialCycles.CelestialCyclesManager") ??
                                   asm.GetType("CelestialCyclesManager") ??
                                   FindTypeByNameFragment(asm, "Celestial");
                if (managerType == null) continue;

                var instance = FindSingleton(managerType);
                if (instance == null)
                {
                    var existing = UnityEngine.Object.FindAnyObjectByType(managerType);
                    if (existing != null) instance = existing;
                }

                if (instance == null) continue;

                SetTimeOfDay(managerType, instance, (GoldenHourStart + GoldenHourEnd) * 0.5f);
                Debug.Log("[GoldenHour] Celestial Cycles — ustawiono złotą godzinę (~18:00).");
                return true;
            }

            return false;
        }

        private static Type FindTypeByNameFragment(Assembly asm, string fragment)
        {
            foreach (var t in asm.GetTypes())
            {
                if (t.Name.Contains(fragment) && typeof(MonoBehaviour).IsAssignableFrom(t))
                    return t;
            }
            return null;
        }

        private static object FindSingleton(Type type)
        {
            var prop = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (prop != null) return prop.GetValue(null);
            var field = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
            return field?.GetValue(null);
        }

        private static void SetTimeOfDay(Type type, object instance, float hour)
        {
            string[] names = { "timeOfDay", "TimeOfDay", "currentTime", "CurrentTime", "time", "Time" };
            foreach (var n in names)
            {
                var prop = type.GetProperty(n, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite && prop.PropertyType == typeof(float))
                {
                    prop.SetValue(instance, hour);
                    return;
                }
                var field = type.GetField(n, BindingFlags.Public | BindingFlags.Instance);
                if (field != null && field.FieldType == typeof(float))
                {
                    field.SetValue(instance, hour);
                    return;
                }
            }

            var method = type.GetMethod("SetTimeOfDay", BindingFlags.Public | BindingFlags.Instance);
            method?.Invoke(instance, new object[] { hour });
        }

        public static void ApplyManualGoldenHour(Transform parent)
        {
            var settings = ForestAtmosphereSettings.Instance
                           ?? UnityEngine.Object.FindAnyObjectByType<ForestAtmosphereSettings>();

            var sunColor = settings != null ? settings.sunColor : DefaultSunColor;
            var sunIntensity = settings != null ? settings.sunIntensity : DefaultSunIntensity;
            var sunEuler = settings != null ? settings.sunRotation : DefaultSunRotation;

            var sun = new GameObject("GoldenHourSun");
            sun.transform.SetParent(parent, false);
            sun.transform.rotation = Quaternion.Euler(sunEuler);
            var l = sun.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = sunColor;
            l.intensity = sunIntensity;
            l.shadows = LightShadows.Soft;
            l.shadowStrength = 0.55f;
            RenderSettings.sun = l;

            settings?.ApplyAtmosphere();

            var fill = new GameObject("GoldenHourFill");
            fill.transform.SetParent(parent, false);
            fill.transform.rotation = Quaternion.Euler(28f, 140f, 0f);
            var fillL = fill.AddComponent<Light>();
            fillL.type = LightType.Directional;
            fillL.color = new Color(0.42f, 0.46f, 0.36f);
            fillL.intensity = 0.18f;
            fillL.shadows = LightShadows.None;

            Debug.Log("[GoldenHour] Ręczne ciepłe światło + mgła (brak Celestial Cycles).");
        }
    }
}
