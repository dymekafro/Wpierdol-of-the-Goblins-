using UnityEngine;
using UnityEngine.UI;
using WPG.Core;

namespace WPG.UI
{
    // Pełny ekran ustawień: sterowanie, audio, grafika. Stosuje zmiany w locie.
    public class SettingsMenuUI : MonoBehaviour
    {
        public bool IsOpen { get; private set; }

        private Canvas _canvas;
        private GameObject _root;

        // Sterowanie
        private Slider _sensSlider;
        private Text _sensValue;
        private Toggle _invertYToggle;

        // Audio
        private Slider _masterSlider, _musicSlider, _sfxSlider;
        private Text _masterValue, _musicValue, _sfxValue;

        // Grafika
        private OptionCycler _qualityCycler;
        private Toggle _fullscreenToggle;
        private Toggle _vsyncToggle;
        private OptionCycler _fpsCycler;
        private Toggle _showFpsToggle;

        private static readonly string[] QualityOptions = { "Niska", "Średnia", "Wysoka" };
        private static readonly string[] FpsOptionLabels = { "30 FPS", "60 FPS", "120 FPS", "144 FPS", "Bez limitu" };

        // Style
        private static readonly Color BgPanel = new Color(0.04f, 0.05f, 0.04f, 0.92f);
        private static readonly Color HeaderGreen = new Color(0.55f, 0.85f, 0.45f);
        private static readonly Color LabelColor = new Color(0.92f, 0.95f, 0.85f);
        private static readonly Color ValueColor = new Color(0.85f, 0.95f, 0.6f);
        private static readonly Color ColumnBg = new Color(0.07f, 0.1f, 0.07f, 0.85f);

        public static SettingsMenuUI EnsureExists(Transform parent = null)
        {
            var existing = FindAnyObjectByType<SettingsMenuUI>();
            if (existing != null) return existing;

            var go = new GameObject("[SettingsMenuUI]");
            if (parent != null) go.transform.SetParent(parent, false);
            return go.AddComponent<SettingsMenuUI>();
        }

        private void Awake()
        {
            SettingsManager.EnsureExists();
            UIFactory.EnsureEventSystem();

            _canvas = UIFactory.CreateScreenCanvas("Canvas_Settings", 20);
            _canvas.transform.SetParent(transform, false);

            Build();
            _root.SetActive(false);
        }

        private void Build()
        {
            _root = UIFactory.CreatePanel(_canvas.transform,
                BgPanel,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                "SettingsRoot").gameObject;

            // Tytuł
            var titleHolder = UIFactory.CreatePanel(_root.transform, new Color(0, 0, 0, 0),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0, -110), new Vector2(0, -30),
                "TitleHolder");
            var title = UIFactory.CreateText(titleHolder, "USTAWIENIA", 64, HeaderGreen, TextAnchor.MiddleCenter, "Title");
            title.fontStyle = FontStyle.Bold;

            // Lewa kolumna (Sterowanie + Audio)
            var leftCol = UIFactory.CreatePanel(_root.transform, ColumnBg,
                new Vector2(0.05f, 0.16f), new Vector2(0.49f, 0.86f),
                Vector2.zero, Vector2.zero,
                "LeftColumn");

            BuildControlSection(leftCol);
            BuildAudioSection(leftCol);

            // Prawa kolumna (Grafika)
            var rightCol = UIFactory.CreatePanel(_root.transform, ColumnBg,
                new Vector2(0.51f, 0.16f), new Vector2(0.95f, 0.86f),
                Vector2.zero, Vector2.zero,
                "RightColumn");

            BuildGraphicsSection(rightCol);

            // Dolny pasek przycisków
            BuildFooter();
        }

        // === SEKCJE ===

        private void BuildControlSection(RectTransform col)
        {
            AddHeader(col, "STEROWANIE", topOffset: -25f);

            // Row 1: Czułość myszy
            var row1 = AddRow(col, topOffset: -95f);
            AddLabel(row1, "Czułość myszy");
            _sensSlider = UIFactory.CreateSlider(row1,
                SettingsData.MinSensitivity, SettingsData.MaxSensitivity, 1.0f,
                v => OnSensitivityChanged(v),
                new Vector2(0.42f, 0f), new Vector2(0.84f, 1f),
                new Vector2(0, 8), new Vector2(0, -8), "SensSlider");
            _sensValue = AddValueText(row1, new Vector2(0.85f, 0f), new Vector2(1f, 1f), "1.0");

            // Row 2: Invert Y
            var row2 = AddRow(col, topOffset: -155f);
            AddLabel(row2, "Invert Y");
            _invertYToggle = UIFactory.CreateToggle(row2, false, v => OnInvertYChanged(v),
                new Vector2(0.42f, 0f), new Vector2(0.42f, 1f),
                new Vector2(0, 8), new Vector2(36, -8), "InvertYToggle");
        }

        private void BuildAudioSection(RectTransform col)
        {
            AddHeader(col, "AUDIO", topOffset: -240f);

            // Master
            var rowM = AddRow(col, topOffset: -310f);
            AddLabel(rowM, "Głośność główna");
            _masterSlider = UIFactory.CreateSlider(rowM, 0f, 1f, 0.8f,
                v => OnVolumeChanged(VolumeChannel.Master, v),
                new Vector2(0.42f, 0f), new Vector2(0.84f, 1f),
                new Vector2(0, 8), new Vector2(0, -8), "MasterSlider");
            _masterValue = AddValueText(rowM, new Vector2(0.85f, 0f), new Vector2(1f, 1f), "80%");

            // Music
            var rowMu = AddRow(col, topOffset: -370f);
            AddLabel(rowMu, "Muzyka");
            _musicSlider = UIFactory.CreateSlider(rowMu, 0f, 1f, 0.7f,
                v => OnVolumeChanged(VolumeChannel.Music, v),
                new Vector2(0.42f, 0f), new Vector2(0.84f, 1f),
                new Vector2(0, 8), new Vector2(0, -8), "MusicSlider");
            _musicValue = AddValueText(rowMu, new Vector2(0.85f, 0f), new Vector2(1f, 1f), "70%");

            // SFX
            var rowS = AddRow(col, topOffset: -430f);
            AddLabel(rowS, "Efekty (SFX)");
            _sfxSlider = UIFactory.CreateSlider(rowS, 0f, 1f, 1f,
                v => OnVolumeChanged(VolumeChannel.Sfx, v),
                new Vector2(0.42f, 0f), new Vector2(0.84f, 1f),
                new Vector2(0, 8), new Vector2(0, -8), "SfxSlider");
            _sfxValue = AddValueText(rowS, new Vector2(0.85f, 0f), new Vector2(1f, 1f), "100%");
        }

        private void BuildGraphicsSection(RectTransform col)
        {
            AddHeader(col, "GRAFIKA", topOffset: -25f);

            // Jakość
            var rowQ = AddRow(col, topOffset: -95f);
            AddLabel(rowQ, "Jakość grafiki");
            _qualityCycler = UIFactory.CreateOptionCycler(rowQ, QualityOptions, 2,
                idx => OnQualityChanged(idx),
                new Vector2(0.42f, 0f), new Vector2(1f, 1f),
                new Vector2(0, 8), new Vector2(-20, -8), "QualityCycler");

            // Fullscreen
            var rowF = AddRow(col, topOffset: -155f);
            AddLabel(rowF, "Pełny ekran");
            _fullscreenToggle = UIFactory.CreateToggle(rowF, true, v => OnFullscreenChanged(v),
                new Vector2(0.42f, 0f), new Vector2(0.42f, 1f),
                new Vector2(0, 8), new Vector2(36, -8), "FullscreenToggle");

            // V-Sync
            var rowV = AddRow(col, topOffset: -215f);
            AddLabel(rowV, "V-Sync");
            _vsyncToggle = UIFactory.CreateToggle(rowV, true, v => OnVsyncChanged(v),
                new Vector2(0.42f, 0f), new Vector2(0.42f, 1f),
                new Vector2(0, 8), new Vector2(36, -8), "VsyncToggle");

            // FPS cap
            var rowFps = AddRow(col, topOffset: -275f);
            AddLabel(rowFps, "Limit FPS");
            _fpsCycler = UIFactory.CreateOptionCycler(rowFps, FpsOptionLabels, 1,
                idx => OnFpsChanged(idx),
                new Vector2(0.42f, 0f), new Vector2(1f, 1f),
                new Vector2(0, 8), new Vector2(-20, -8), "FpsCycler");

            // Show FPS
            var rowShow = AddRow(col, topOffset: -335f);
            AddLabel(rowShow, "Pokaż FPS");
            _showFpsToggle = UIFactory.CreateToggle(rowShow, false, v => OnShowFpsChanged(v),
                new Vector2(0.42f, 0f), new Vector2(0.42f, 1f),
                new Vector2(0, 8), new Vector2(36, -8), "ShowFpsToggle");
        }

        private void BuildFooter()
        {
            var footer = UIFactory.CreatePanel(_root.transform, new Color(0, 0, 0, 0),
                new Vector2(0f, 0f), new Vector2(1f, 0.14f),
                Vector2.zero, Vector2.zero, "Footer");

            UIFactory.CreateButton(footer, "Zastosuj",
                new Color(0.15f, 0.4f, 0.2f, 0.95f), new Color(0.25f, 0.65f, 0.3f, 1f),
                () => ApplyAndSave(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-580, -40), new Vector2(-200, 40), 30);

            UIFactory.CreateButton(footer, "Przywróć domyślne",
                new Color(0.35f, 0.3f, 0.15f, 0.95f), new Color(0.55f, 0.5f, 0.25f, 1f),
                () => ResetDefaults(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-190, -40), new Vector2(190, 40), 30);

            UIFactory.CreateButton(footer, "Wróć",
                new Color(0.3f, 0.3f, 0.35f, 0.95f), new Color(0.45f, 0.45f, 0.5f, 1f),
                () => Close(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(200, -40), new Vector2(580, 40), 30);
        }

        // === LAYOUT HELPERS ===

        private RectTransform AddHeader(RectTransform col, string text, float topOffset)
        {
            var go = new GameObject("Header_" + text);
            go.transform.SetParent(col, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(20f, topOffset - 50f);
            rt.offsetMax = new Vector2(-20f, topOffset);
            var t = UIFactory.CreateText(rt, text, 28, HeaderGreen, TextAnchor.MiddleLeft, "HeaderText");
            t.fontStyle = FontStyle.Bold;
            return rt;
        }

        private RectTransform AddRow(RectTransform col, float topOffset)
        {
            var go = new GameObject("Row");
            go.transform.SetParent(col, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(20f, topOffset - 50f);
            rt.offsetMax = new Vector2(-20f, topOffset);
            return rt;
        }

        private void AddLabel(RectTransform row, string text)
        {
            var labelHolder = UIFactory.CreatePanel(row, new Color(0, 0, 0, 0),
                new Vector2(0f, 0f), new Vector2(0.4f, 1f),
                Vector2.zero, Vector2.zero, "Label");
            UIFactory.CreateText(labelHolder, text, 22, LabelColor, TextAnchor.MiddleLeft, "LabelText");
        }

        private Text AddValueText(RectTransform row, Vector2 aMin, Vector2 aMax, string initial)
        {
            var holder = UIFactory.CreatePanel(row, new Color(0, 0, 0, 0),
                aMin, aMax, Vector2.zero, Vector2.zero, "Value");
            return UIFactory.CreateText(holder, initial, 22, ValueColor, TextAnchor.MiddleCenter, "ValueText");
        }

        // === OPEN / CLOSE ===

        public void Open()
        {
            if (_root == null) return;
            RefreshFromSettings();
            _root.SetActive(true);
            IsOpen = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Close()
        {
            if (_root == null) return;
            // Zapisz wybór użytkownika (real-time apply już się stało)
            if (SettingsManager.Instance != null) SettingsManager.Instance.Save();
            _root.SetActive(false);
            IsOpen = false;
        }

        public void Toggle()
        {
            if (IsOpen) Close(); else Open();
        }

        // === HANDLERS (real-time apply) ===

        private void OnSensitivityChanged(float v)
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.UpdateAndApply(s => s.mouseSensitivity = v, save: false);
            if (_sensValue != null) _sensValue.text = v.ToString("0.0");
        }

        private void OnInvertYChanged(bool v)
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.UpdateAndApply(s => s.invertY = v, save: false);
        }

        private enum VolumeChannel { Master, Music, Sfx }

        private void OnVolumeChanged(VolumeChannel ch, float v)
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.UpdateAndApply(s =>
            {
                switch (ch)
                {
                    case VolumeChannel.Master: s.masterVolume = v; break;
                    case VolumeChannel.Music: s.musicVolume = v; break;
                    case VolumeChannel.Sfx: s.sfxVolume = v; break;
                }
            }, save: false);
            int pct = Mathf.RoundToInt(v * 100f);
            switch (ch)
            {
                case VolumeChannel.Master: if (_masterValue != null) _masterValue.text = pct + "%"; break;
                case VolumeChannel.Music: if (_musicValue != null) _musicValue.text = pct + "%"; break;
                case VolumeChannel.Sfx: if (_sfxValue != null) _sfxValue.text = pct + "%"; break;
            }
        }

        private void OnQualityChanged(int idx)
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.UpdateAndApply(s => s.qualityLevel = Mathf.Clamp(idx, 0, 2), save: false);
        }

        private void OnFullscreenChanged(bool v)
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.UpdateAndApply(s => s.fullscreen = v, save: false);
        }

        private void OnVsyncChanged(bool v)
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.UpdateAndApply(s => s.vSync = v, save: false);
        }

        private void OnFpsChanged(int idx)
        {
            if (SettingsManager.Instance == null) return;
            int fps = SettingsData.FpsOptions[Mathf.Clamp(idx, 0, SettingsData.FpsOptions.Length - 1)];
            SettingsManager.Instance.UpdateAndApply(s => s.targetFPS = fps, save: false);
        }

        private void OnShowFpsChanged(bool v)
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.UpdateAndApply(s => s.showFPS = v, save: false);
        }

        // === REFRESH / RESET ===

        public void RefreshFromSettings()
        {
            var mgr = SettingsManager.Instance;
            if (mgr == null) return;
            var s = mgr.Settings;

            if (_sensSlider != null) _sensSlider.SetValueWithoutNotify(s.mouseSensitivity);
            if (_sensValue != null) _sensValue.text = s.mouseSensitivity.ToString("0.0");

            if (_invertYToggle != null) _invertYToggle.SetIsOnWithoutNotify(s.invertY);

            if (_masterSlider != null) _masterSlider.SetValueWithoutNotify(s.masterVolume);
            if (_musicSlider != null) _musicSlider.SetValueWithoutNotify(s.musicVolume);
            if (_sfxSlider != null) _sfxSlider.SetValueWithoutNotify(s.sfxVolume);
            if (_masterValue != null) _masterValue.text = Mathf.RoundToInt(s.masterVolume * 100f) + "%";
            if (_musicValue != null) _musicValue.text = Mathf.RoundToInt(s.musicVolume * 100f) + "%";
            if (_sfxValue != null) _sfxValue.text = Mathf.RoundToInt(s.sfxVolume * 100f) + "%";

            if (_qualityCycler != null) _qualityCycler.SetIndex(Mathf.Clamp(s.qualityLevel, 0, QualityOptions.Length - 1), notify: false);
            if (_fullscreenToggle != null) _fullscreenToggle.SetIsOnWithoutNotify(s.fullscreen);
            if (_vsyncToggle != null) _vsyncToggle.SetIsOnWithoutNotify(s.vSync);

            int fpsIdx = 1;
            for (int i = 0; i < SettingsData.FpsOptions.Length; i++)
            {
                if (SettingsData.FpsOptions[i] == s.targetFPS) { fpsIdx = i; break; }
            }
            if (_fpsCycler != null) _fpsCycler.SetIndex(fpsIdx, notify: false);

            if (_showFpsToggle != null) _showFpsToggle.SetIsOnWithoutNotify(s.showFPS);
        }

        private void ApplyAndSave()
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.ApplySettings();
            SettingsManager.Instance.Save();
        }

        private void ResetDefaults()
        {
            if (SettingsManager.Instance == null) return;
            SettingsManager.Instance.ResetToDefaults();
            RefreshFromSettings();
        }
    }
}
