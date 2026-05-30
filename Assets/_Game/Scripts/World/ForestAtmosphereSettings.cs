using UnityEngine;
using UnityEngine.Rendering;

namespace WPG.World
{
    /// <summary>
    /// Live-editable fog, ambient and sun. Add to scene as "Atmosphere" or let WorldBootstrap create it.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class ForestAtmosphereSettings : MonoBehaviour
    {
        public static ForestAtmosphereSettings Instance { get; private set; }

        [Header("Mgła / Fog")]
        public bool fogEnabled = true;
        public Color fogColor = GoldenHourLighting.DefaultFogColor;
        [Range(0f, 0.2f)]
        public float fogDensity = GoldenHourLighting.DefaultFogDensity;

        [Header("Ambient (Trilight)")]
        public Color ambientSkyColor = GoldenHourLighting.DefaultAmbientSky;
        public Color ambientEquatorColor = GoldenHourLighting.DefaultAmbientEquator;
        public Color ambientGroundColor = GoldenHourLighting.DefaultAmbientGround;

        [Header("Słońce / Sun")]
        public Color sunColor = GoldenHourLighting.DefaultSunColor;
        [Range(0f, 3f)]
        public float sunIntensity = GoldenHourLighting.DefaultSunIntensity;
        public Vector3 sunRotation = GoldenHourLighting.DefaultSunRotation;

        [Header("Podłoże / Ground")]
        public Color groundColor = GoldenHourLighting.DefaultGroundColor;

        // ~2x na sekundę w trybie gry — bez ciężkiej pracy co klatkę.
        private const float ApplyInterval = 0.5f;
        private float _nextApplyAt;

        private MeshRenderer _groundRenderer;
        private Light _sun;

        private void OnEnable()
        {
            Instance = this;
            ApplyAtmosphere();
        }

        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnValidate()
        {
            ApplyAtmosphere();
        }

        private void Update()
        {
            if (!Application.isPlaying)
                return;

            if (Time.unscaledTime < _nextApplyAt)
                return;

            _nextApplyAt = Time.unscaledTime + ApplyInterval;
            ApplyAtmosphere();
        }

        [ContextMenu("Apply Now")]
        public void ApplyAtmosphere()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.reflectionIntensity = 0.5f;

            RenderSettings.fog = fogEnabled;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogColor = fogColor;

            ApplySun();
            ApplyGroundTint();

            SyncMainCameraBackground();
        }

        private void ApplySun()
        {
            if (_sun == null)
                _sun = RenderSettings.sun;
            if (_sun == null)
                _sun = FindDirectionalLight();

            if (_sun == null)
                return;

            _sun.color = sunColor;
            _sun.intensity = sunIntensity;
            _sun.transform.rotation = Quaternion.Euler(sunRotation);
            RenderSettings.sun = _sun;
        }

        private static Light FindDirectionalLight()
        {
            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include);
            foreach (var light in lights)
            {
                if (light != null && light.type == LightType.Directional)
                    return light;
            }

            return null;
        }

        private void ApplyGroundTint()
        {
            MaterialFactory.GroundColorOverride = groundColor;

            if (_groundRenderer == null)
            {
                var groundPlane = GameObject.Find("GroundPlane");
                if (groundPlane != null)
                    _groundRenderer = groundPlane.GetComponent<MeshRenderer>();
            }

            if (_groundRenderer == null)
                return;

            var mat = _groundRenderer.sharedMaterial;
            if (mat == null)
                return;

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", groundColor);
            else
                mat.color = groundColor;
        }

        private static void SyncMainCameraBackground()
        {
            var cam = Camera.main;
            if (cam == null)
                return;

            var fog = Instance != null ? Instance.fogColor : GoldenHourLighting.DefaultFogColor;
            cam.backgroundColor = fog;
        }

        /// <summary>WorldBootstrap / editor: ensure one Atmosphere object exists.</summary>
        public static ForestAtmosphereSettings EnsureExists()
        {
            var existing = Object.FindAnyObjectByType<ForestAtmosphereSettings>();
            if (existing != null)
                return existing;

            var go = new GameObject("Atmosphere");
            return go.AddComponent<ForestAtmosphereSettings>();
        }
    }
}
