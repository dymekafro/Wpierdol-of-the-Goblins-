using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WPG.Core;

namespace WPG.UI
{
    public sealed class DamageNumber : MonoBehaviour
    {
        private const float Lifetime = 0.85f;
        private const float StartScaleNormal = 1.05f;
        private const float StartScaleCritical = 1.35f;
        private const float EndScale = 0.65f;

        private static Canvas _worldCanvas;
        private static readonly Queue<DamageNumber> Pool = new Queue<DamageNumber>();

        private RectTransform _rectTransform;
        private Text _text;
        private CanvasGroup _canvasGroup;
        private Camera _camera;

        private Vector3 _worldPosition;
        private Vector3 _worldVelocity;
        private float _time;
        private float _startScale;

        public static void Show(int damage, Vector3 worldPosition)
        {
            Show(damage, worldPosition, new Color(1f, 0.22f, 0.12f, 1f), false);
        }

        public static void Show(int damage, Vector3 worldPosition, Color color, bool isCritical)
        {
            if (damage <= 0)
                return;

            EnsureCanvas();

            DamageNumber number = GetFromPool();
            number.Activate(damage, worldPosition, color, isCritical);
        }

        // Prosty pool — recykling obiektów UI zamiast Instantiate/Destroy przy każdym trafieniu.
        private static DamageNumber GetFromPool()
        {
            while (Pool.Count > 0)
            {
                DamageNumber pooled = Pool.Dequeue();
                if (pooled != null)
                    return pooled;
            }

            GameObject go = new GameObject("DamageNumber");
            go.transform.SetParent(_worldCanvas.transform, false);

            DamageNumber number = go.AddComponent<DamageNumber>();
            number.BuildComponents();
            return number;
        }

        private static void EnsureCanvas()
        {
            if (_worldCanvas != null)
                return;

            GameObject canvasObject = new GameObject("DamageNumbersCanvas");
            DontDestroyOnLoad(canvasObject);

            _worldCanvas = canvasObject.AddComponent<Canvas>();
            _worldCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _worldCanvas.sortingOrder = 200;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
        }

        // Jednorazowe utworzenie komponentów (AddComponent można wołać tylko raz na obiekt).
        private void BuildComponents()
        {
            _rectTransform = gameObject.AddComponent<RectTransform>();
            _rectTransform.sizeDelta = new Vector2(150f, 60f);

            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _text = gameObject.AddComponent<Text>();
            _text.alignment = TextAnchor.MiddleCenter;
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _text.fontStyle = FontStyle.Bold;
            _text.raycastTarget = false;
        }

        private void Activate(int damage, Vector3 worldPosition, Color color, bool isCritical)
        {
            _camera = CameraCache.Main;
            _worldPosition = worldPosition;
            _time = 0f;

            gameObject.name = isCritical ? "CriticalDamageNumber" : "DamageNumber";

            _text.text = damage.ToString();
            _text.fontSize = isCritical ? 34 : 28;
            _text.color = color;

            _startScale = isCritical ? StartScaleCritical : StartScaleNormal;

            Vector3 cameraRight = _camera != null ? _camera.transform.right : Vector3.right;

            float sideDirection = Random.value < 0.5f ? -1f : 1f;
            float sideSpeed = Random.Range(0.75f, 1.65f);
            float upSpeed = Random.Range(0.65f, 1.15f);

            _worldVelocity = cameraRight * sideDirection * sideSpeed + Vector3.up * upSpeed;

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            UpdateVisual();
        }

        private void Release()
        {
            gameObject.SetActive(false);
            Pool.Enqueue(this);
        }

        private void Update()
        {
            _time += Time.deltaTime;

            if (_time >= Lifetime)
            {
                Release();
                return;
            }

            // Lekki spadek prędkości w czasie, żeby numer wyglądał mniej liniowo.
            _worldPosition += _worldVelocity * Time.deltaTime;
            _worldVelocity *= 0.965f;

            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_camera == null)
                _camera = CameraCache.Main;

            if (_camera == null)
                return;

            Vector3 screenPosition = _camera.WorldToScreenPoint(_worldPosition);

            if (screenPosition.z < 0f)
            {
                _canvasGroup.alpha = 0f;
                return;
            }

            _rectTransform.position = screenPosition;

            float t = Mathf.Clamp01(_time / Lifetime);

            _canvasGroup.alpha = 1f - t;

            float scale = Mathf.Lerp(_startScale, EndScale, t);
            _rectTransform.localScale = Vector3.one * scale;
        }
    }
}