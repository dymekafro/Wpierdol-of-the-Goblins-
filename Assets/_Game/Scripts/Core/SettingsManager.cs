using System;
using System.IO;
using UnityEngine;

namespace WPG.Core
{
    // Singleton zarządzający ustawieniami gry: persystencja JSON + apply do silnika.
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        private const string FileName = "wpg_settings.json";

        public SettingsData Settings { get; private set; } = new SettingsData();

        // Wygodne aliasy dla często czytanych pól (kamera itp.)
        public float MouseSensitivity => Settings.mouseSensitivity;
        public bool InvertY => Settings.invertY;
        public bool ShowFPS => Settings.showFPS;

        public static event Action OnSettingsChanged;

        public static string SavePath
        {
            get
            {
                string baseDir = Application.persistentDataPath;
                if (string.IsNullOrEmpty(baseDir)) baseDir = ".";
                return Path.Combine(baseDir, FileName);
            }
        }

        public static SettingsManager EnsureExists()
        {
            if (Instance != null) return Instance;

            var existing = FindAnyObjectByType<SettingsManager>();
            if (existing != null)
            {
                Instance = existing;
                return Instance;
            }

            var go = new GameObject("[SettingsManager]");
            Instance = go.AddComponent<SettingsManager>();
            DontDestroyOnLoad(go);
            return Instance;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
            ApplySettings(notify: false);
        }

        // === API ===

        public void Load()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    Settings = new SettingsData();
                    return;
                }
                string json = File.ReadAllText(SavePath);
                var loaded = JsonUtility.FromJson<SettingsData>(json);
                if (loaded == null)
                {
                    Settings = new SettingsData();
                    return;
                }
                Settings = loaded;
                ClampAndNormalize();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SettingsManager] Błąd odczytu ustawień: {e.Message}");
                Settings = new SettingsData();
            }
        }

        public void Save()
        {
            try
            {
                ClampAndNormalize();
                string json = JsonUtility.ToJson(Settings, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SettingsManager] Błąd zapisu ustawień: {e.Message}");
            }
        }

        public void ResetToDefaults()
        {
            Settings = new SettingsData();
            ApplySettings();
            Save();
        }

        public void UpdateAndApply(Action<SettingsData> mutate, bool save = true)
        {
            if (mutate == null) return;
            mutate(Settings);
            ApplySettings();
            if (save) Save();
        }

        public void ApplySettings(bool notify = true)
        {
            ClampAndNormalize();

            // Quality
            int maxQuality = Mathf.Max(0, QualitySettings.names.Length - 1);
            int q = Mathf.Clamp(Settings.qualityLevel, 0, maxQuality);
            QualitySettings.SetQualityLevel(q, true);

            // Fullscreen
            if (Screen.fullScreen != Settings.fullscreen)
            {
                Screen.fullScreen = Settings.fullscreen;
            }

            // V-Sync
            QualitySettings.vSyncCount = Settings.vSync ? 1 : 0;

            // FPS cap (0 = unlimited)
            Application.targetFrameRate = Settings.targetFPS <= 0 ? -1 : Settings.targetFPS;

            // Master volume (music/sfx mogą być honorowane przez własne miksery audio gry; tu sterujemy globalem)
            AudioListener.volume = Mathf.Clamp01(Settings.masterVolume);

            // FPS overlay włącz/wyłącz
            WPG.UI.FPSCounter.SetVisible(Settings.showFPS);

            if (notify) OnSettingsChanged?.Invoke();
        }

        private void ClampAndNormalize()
        {
            if (Settings == null) Settings = new SettingsData();
            Settings.mouseSensitivity = Mathf.Clamp(Settings.mouseSensitivity, SettingsData.MinSensitivity, SettingsData.MaxSensitivity);
            Settings.masterVolume = Mathf.Clamp01(Settings.masterVolume);
            Settings.musicVolume = Mathf.Clamp01(Settings.musicVolume);
            Settings.sfxVolume = Mathf.Clamp01(Settings.sfxVolume);

            // Snap FPS do listy znanych wartości
            bool valid = false;
            foreach (var v in SettingsData.FpsOptions)
            {
                if (v == Settings.targetFPS) { valid = true; break; }
            }
            if (!valid) Settings.targetFPS = 60;

            if (Settings.qualityLevel < 0) Settings.qualityLevel = 0;
            if (Settings.qualityLevel > 2) Settings.qualityLevel = 2;

            if (string.IsNullOrEmpty(Settings.language)) Settings.language = "PL";
        }
    }
}
