using System;

namespace WPG.Core
{
    // Persistowane ustawienia gry. Trzymane w wpg_settings.json.
    [Serializable]
    public class SettingsData
    {
        public int settingsVersion = 1;

        // Sterowanie
        public float mouseSensitivity = 1.0f; // 0.1 - 5.0
        public bool invertY = false;

        // Audio (0..1)
        public float masterVolume = 0.8f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 1.0f;

        // Grafika
        public bool fullscreen = true;
        public bool vSync = true;
        public int targetFPS = 60;   // 30 / 60 / 120 / 144 / 0(=unlimited)
        public int qualityLevel = 2; // 0 Low, 1 Medium, 2 High

        // HUD
        public bool showFPS = false;

        // I18n (na razie placeholder)
        public string language = "PL";

        public const float MinSensitivity = 0.1f;
        public const float MaxSensitivity = 5.0f;

        public static readonly int[] FpsOptions = new[] { 30, 60, 120, 144, 0 };

        public SettingsData Clone()
        {
            return (SettingsData)MemberwiseClone();
        }

        public void CopyFrom(SettingsData other)
        {
            if (other == null) return;
            settingsVersion = other.settingsVersion;
            mouseSensitivity = other.mouseSensitivity;
            invertY = other.invertY;
            masterVolume = other.masterVolume;
            musicVolume = other.musicVolume;
            sfxVolume = other.sfxVolume;
            fullscreen = other.fullscreen;
            vSync = other.vSync;
            targetFPS = other.targetFPS;
            qualityLevel = other.qualityLevel;
            showFPS = other.showFPS;
            language = other.language;
        }
    }
}
