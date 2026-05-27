using UnityEngine;
using UnityEngine.UI;

namespace WPG.UI
{
    // Lekki licznik FPS w prawym górnym rogu. Steruje nim SettingsManager.SetVisible().
    public class FPSCounter : MonoBehaviour
    {
        private static FPSCounter _instance;

        private Canvas _canvas;
        private Text _text;

        private float _accum;
        private int _frames;
        private float _timeLeft;

        private const float UpdateInterval = 0.5f;

        public static void SetVisible(bool visible)
        {
            if (visible)
            {
                EnsureInstance();
                if (_instance != null && _instance._canvas != null)
                    _instance._canvas.enabled = true;
            }
            else
            {
                if (_instance != null && _instance._canvas != null)
                    _instance._canvas.enabled = false;
            }
        }

        private static FPSCounter EnsureInstance()
        {
            if (_instance != null) return _instance;
            var existing = FindAnyObjectByType<FPSCounter>();
            if (existing != null)
            {
                _instance = existing;
                return _instance;
            }
            var go = new GameObject("[FPSCounter]");
            _instance = go.AddComponent<FPSCounter>();
            DontDestroyOnLoad(go);
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Build();
            _timeLeft = UpdateInterval;
        }

        private void Build()
        {
            UIFactory.EnsureEventSystem();
            _canvas = UIFactory.CreateScreenCanvas("Canvas_FPS", 50);
            _canvas.transform.SetParent(transform, false);

            var holder = UIFactory.CreatePanel(_canvas.transform,
                new Color(0f, 0f, 0f, 0.45f),
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-180f, -50f), new Vector2(-20f, -10f),
                "FPSHolder");
            _text = UIFactory.CreateText(holder, "FPS --", 22,
                new Color(0.65f, 1f, 0.6f), TextAnchor.MiddleCenter, "FPSText");
            _text.fontStyle = FontStyle.Bold;
        }

        private void Update()
        {
            // Liczymy FPS niezależnie od Time.timeScale.
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f) return;
            _accum += 1f / dt;
            _frames++;
            _timeLeft -= dt;
            if (_timeLeft <= 0f)
            {
                float fps = _frames > 0 ? _accum / _frames : 0f;
                if (_text != null) _text.text = "FPS " + Mathf.RoundToInt(fps);
                _timeLeft = UpdateInterval;
                _accum = 0f;
                _frames = 0;
            }
        }
    }
}
