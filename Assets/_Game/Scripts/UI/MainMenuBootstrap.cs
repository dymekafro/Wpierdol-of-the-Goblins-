using UnityEngine;
using UnityEngine.UI;
using WPG.Core;

namespace WPG.UI
{
    // Entry point sceny MainMenu - buduje wszystko proceduralnie.
    public class MainMenuBootstrap : MonoBehaviour
    {
        private SettingsMenuUI _settingsMenu;

        private void Awake()
        {
            GameManager.EnsureExists();
            SettingsManager.EnsureExists();
            GameAudioManager.EnsureExists();
            GameAssetLoader.LogAssetScanOnce();
        }

        private void Start()
        {
            // Camera + tło sceny
            var camGO = new GameObject("MainCamera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.08f, 0.05f);
            cam.fieldOfView = 60f;
            camGO.AddComponent<AudioListener>();

            UIFactory.EnsureEventSystem();

            var canvas = UIFactory.CreateScreenCanvas("Canvas_MainMenu");

            // Tło — Fantasy Free GUI menu BG lub gradient kolorystyczny
            var menuBgSprite = GameAssetLoader.LoadSprite(GameAssetPaths.GuiMenuBackground, GameAssetPaths.ResUiMenuBg);
            var panelSprite = GameAssetLoader.LoadSprite(GameAssetPaths.GuiPanel, GameAssetPaths.ResUiPanel);

            RectTransform bg;
            if (menuBgSprite != null)
            {
                var bgImg = UIFactory.CreateImage(canvas.transform, menuBgSprite, Color.white,
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "Background", Image.Type.Simple);
                bg = bgImg.rectTransform;
            }
            else
            {
                bg = UIFactory.CreatePanel(canvas.transform,
                    new Color(0.04f, 0.07f, 0.05f, 1f),
                    Vector2.zero, Vector2.one,
                    Vector2.zero, Vector2.zero,
                    "Background");
            }

            // Ramka tytułu (opcjonalna)
            if (panelSprite != null)
            {
                UIFactory.CreateImage(canvas.transform, panelSprite, new Color(1f, 1f, 1f, 0.35f),
                    new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.94f),
                    Vector2.zero, Vector2.zero, "TitleFrame", Image.Type.Sliced);
            }

            // Wewnętrzny gradient nakładki - dolny ciemniejszy
            var dark = UIFactory.CreatePanel(bg,
                new Color(0f, 0f, 0f, 0.4f),
                new Vector2(0f, 0f), new Vector2(1f, 0.5f),
                Vector2.zero, Vector2.zero,
                "DarkGradient");

            // Tytuł
            var titleHolder = UIFactory.CreatePanel(canvas.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0.1f, 0.75f), new Vector2(0.9f, 0.92f),
                Vector2.zero, Vector2.zero,
                "TitleHolder");
            var title = UIFactory.CreateText(titleHolder, "Wpierdol of the Goblins", 108,
                new Color(0.55f, 0.85f, 0.45f), TextAnchor.MiddleCenter, "Title");
            title.fontStyle = FontStyle.Bold;

            var subHolder = UIFactory.CreatePanel(canvas.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.74f),
                Vector2.zero, Vector2.zero,
                "Subtitle");
            UIFactory.CreateText(subHolder, "Magiczny Ciemny Las - opowieść druida", 36,
                new Color(0.7f, 0.8f, 0.55f), TextAnchor.MiddleCenter, "Subtitle");

            // Przyciski
            Color btnColor = new Color(0.15f, 0.4f, 0.2f, 0.92f);
            Color hoverColor = new Color(0.25f, 0.6f, 0.3f, 1f);
            Color btnDisabled = new Color(0.1f, 0.15f, 0.1f, 0.6f);

            UIFactory.CreateFantasyButton(canvas.transform, "Nowa Gra",
                () => GameManager.Instance.StartNewGame(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-220, 120), new Vector2(220, 200),
                btnColor, hoverColor, 40);

            bool hasSave = SaveSystem.HasSave();
            var continueBtn = UIFactory.CreateFantasyButton(canvas.transform, "Kontynuuj",
                () =>
                {
                    if (SaveSystem.HasSave()) GameManager.Instance.ContinueGame();
                },
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-220, 20), new Vector2(220, 100),
                hasSave ? btnColor : btnDisabled, hasSave ? hoverColor : btnDisabled, 40);
            continueBtn.interactable = hasSave;

            UIFactory.CreateFantasyButton(canvas.transform, "Ustawienia",
                () => OpenSettings(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-220, -80), new Vector2(220, 0),
                new Color(0.2f, 0.3f, 0.35f, 0.92f), new Color(0.3f, 0.45f, 0.5f, 1f), 40);

            UIFactory.CreateFantasyButton(canvas.transform, "Wyjście",
                () => GameManager.Instance.QuitGame(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-220, -180), new Vector2(220, -100),
                new Color(0.4f, 0.15f, 0.15f, 0.92f), new Color(0.6f, 0.25f, 0.25f, 1f), 40);

            // Footer
            var footerHolder = UIFactory.CreatePanel(canvas.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 0f), new Vector2(1f, 0.05f),
                Vector2.zero, Vector2.zero, "Footer");
            UIFactory.CreateText(footerHolder, "WSAD ruch  |  Spacja skok  |  LPM atak  |  E ognisty cios / interakcja  |  Q leczenie  |  ESC pauza", 22,
                new Color(0.55f, 0.65f, 0.55f), TextAnchor.MiddleCenter, "Hint");

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OpenSettings()
        {
            if (_settingsMenu == null)
            {
                var go = new GameObject("SettingsMenu");
                _settingsMenu = go.AddComponent<SettingsMenuUI>();
            }
            _settingsMenu.Open();
        }
    }
}
