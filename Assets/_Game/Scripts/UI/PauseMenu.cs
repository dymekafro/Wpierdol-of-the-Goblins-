using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using WPG.Core;

namespace WPG.UI
{
    public class PauseMenu : MonoBehaviour
    {
        private GameObject _root;
        private Canvas _canvas;
        private SettingsMenuUI _settingsMenu;
        public bool IsOpen { get; private set; }

        private void Awake()
        {
            SettingsManager.EnsureExists();
            UIFactory.EnsureEventSystem();
            _canvas = UIFactory.CreateScreenCanvas("Canvas_Pause", 10);
            Build();
            _root.SetActive(false);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                // ESC w panelu ustawień zamyka tylko ustawienia (nie wraca do gry).
                if (_settingsMenu != null && _settingsMenu.IsOpen)
                {
                    _settingsMenu.Close();
                    return;
                }
                Toggle();
            }
        }

        private void Build()
        {
            _root = UIFactory.CreatePanel(_canvas.transform,
                new Color(0f, 0f, 0f, 0.78f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                "PauseRoot").gameObject;

            UIFactory.CreateText(_root.transform, "Pauza", 96, new Color(0.55f, 0.85f, 0.45f), TextAnchor.UpperCenter, "Title");

            UIFactory.CreateButton(_root.transform, "Wróć do gry",
                new Color(0.15f, 0.4f, 0.2f, 0.95f), new Color(0.25f, 0.65f, 0.3f, 1f),
                () => Close(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-220, 160), new Vector2(220, 240), 32);

            UIFactory.CreateButton(_root.transform, "Ustawienia",
                new Color(0.2f, 0.3f, 0.35f, 0.95f), new Color(0.3f, 0.45f, 0.5f, 1f),
                () => OpenSettings(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-220, 60), new Vector2(220, 140), 32);

            UIFactory.CreateButton(_root.transform, "Menu główne",
                new Color(0.3f, 0.3f, 0.35f, 0.95f), new Color(0.45f, 0.45f, 0.5f, 1f),
                () => { Time.timeScale = 1f; GameManager.Instance.GoToMainMenu(); },
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-220, -40), new Vector2(220, 40), 32);

            UIFactory.CreateButton(_root.transform, "Wyjście",
                new Color(0.4f, 0.15f, 0.15f, 0.95f), new Color(0.6f, 0.25f, 0.25f, 1f),
                () => GameManager.Instance.QuitGame(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-220, -140), new Vector2(220, -60), 32);
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

        public void Toggle()
        {
            if (IsOpen) Close(); else Open();
        }

        public void Open()
        {
            IsOpen = true;
            _root.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Close()
        {
            if (_settingsMenu != null && _settingsMenu.IsOpen) _settingsMenu.Close();
            IsOpen = false;
            _root.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
