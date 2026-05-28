using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WPG.Core;

namespace WPG.UI
{
    public static class UIFactory
    {
        private static Font _font;
        private static Sprite _defaultSprite;

        public static Font GetFont()
        {
            if (_font != null) return _font;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
            {
                // Fallback - na wypadek innej buildu Unity
                _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            return _font;
        }

        public static Sprite GetDefaultSprite()
        {
            if (_defaultSprite != null) return _defaultSprite;
            _defaultSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            return _defaultSprite;
        }

        public static Image CreateImage(Transform parent, Sprite sprite, Color tint, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, string name = "Image", Image.Type type = Image.Type.Simple)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            var img = go.AddComponent<Image>();
            img.sprite = sprite != null ? sprite : GetDefaultSprite();
            img.type = type;
            img.color = tint;
            img.raycastTarget = false;
            return img;
        }

        public static Canvas CreateScreenCanvas(string name, int sortOrder = 0)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            var existing = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
            if (existing != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            // Nowy Input System
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        public static RectTransform CreatePanel(Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string name = "Panel")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            var img = go.AddComponent<Image>();
            img.color = color;
            return rt;
        }

        public static Text CreateText(Transform parent, string text, int fontSize, Color color, TextAnchor anchor = TextAnchor.MiddleCenter, string name = "Text")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.text = text;
            t.color = color;
            t.font = GetFont();
            t.fontSize = fontSize;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Button CreateButton(Transform parent, string text, Color baseColor, Color hoverColor, Action onClick,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int fontSize = 36)
        {
            var go = new GameObject("Button_" + text);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var img = go.AddComponent<Image>();
            img.color = baseColor;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = baseColor * 0.7f;
            colors.selectedColor = hoverColor;
            colors.fadeDuration = 0.1f;
            colors.colorMultiplier = 1f;
            btn.colors = colors;
            btn.targetGraphic = img;

            if (onClick != null)
            {
                btn.onClick.AddListener(() =>
                {
                    GameAudioManager.EnsureExists()?.PlayUIClick();
                    onClick();
                });
            }

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            var label = labelGO.AddComponent<Text>();
            label.text = text;
            label.font = GetFont();
            label.fontSize = fontSize;
            label.color = new Color(0.95f, 0.96f, 0.9f);
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;

            return btn;
        }

        // ---- Settings widgets ----

        // Slider: tło + fill + handle. Zwraca komponent Slider.
        public static Slider CreateSlider(Transform parent, float min, float max, float value,
            Action<float> onChange,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            string name = "Slider")
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var slider = root.AddComponent<Slider>();

            // Background
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(root.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0f, 0.35f);
            bgRT.anchorMax = new Vector2(1f, 0.65f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.12f, 0.08f, 0.95f);

            // Fill Area
            var fillAreaGO = new GameObject("Fill Area");
            fillAreaGO.transform.SetParent(root.transform, false);
            var fillAreaRT = fillAreaGO.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = new Vector2(0f, 0.35f);
            fillAreaRT.anchorMax = new Vector2(1f, 0.65f);
            fillAreaRT.offsetMin = new Vector2(8f, 0f);
            fillAreaRT.offsetMax = new Vector2(-8f, 0f);

            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            var fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0f, 0f);
            fillRT.anchorMax = new Vector2(1f, 1f);
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = new Color(0.35f, 0.7f, 0.4f, 1f);

            // Handle Slide Area
            var handleAreaGO = new GameObject("Handle Slide Area");
            handleAreaGO.transform.SetParent(root.transform, false);
            var handleAreaRT = handleAreaGO.AddComponent<RectTransform>();
            handleAreaRT.anchorMin = new Vector2(0f, 0f);
            handleAreaRT.anchorMax = new Vector2(1f, 1f);
            handleAreaRT.offsetMin = new Vector2(10f, 0f);
            handleAreaRT.offsetMax = new Vector2(-10f, 0f);

            var handleGO = new GameObject("Handle");
            handleGO.transform.SetParent(handleAreaGO.transform, false);
            var handleRT = handleGO.AddComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(20f, 0f);
            handleRT.anchorMin = new Vector2(0f, 0f);
            handleRT.anchorMax = new Vector2(0f, 1f);
            var handleImg = handleGO.AddComponent<Image>();
            handleImg.color = new Color(0.85f, 0.95f, 0.6f, 1f);

            slider.fillRect = fillRT;
            slider.handleRect = handleRT;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = false;
            slider.value = Mathf.Clamp(value, min, max);

            if (onChange != null)
            {
                slider.onValueChanged.AddListener(v => onChange(v));
            }

            return slider;
        }

        // Toggle ze stylizowanym checkbox-em po prawej (label dodajesz osobno).
        public static Toggle CreateToggle(Transform parent, bool isOn, Action<bool> onChange,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            string name = "Toggle")
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var toggle = root.AddComponent<Toggle>();

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(root.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0f, 0f);
            bgRT.anchorMax = new Vector2(1f, 1f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.12f, 0.08f, 0.95f);

            var checkGO = new GameObject("Checkmark");
            checkGO.transform.SetParent(bgGO.transform, false);
            var checkRT = checkGO.AddComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(0f, 0f);
            checkRT.anchorMax = new Vector2(1f, 1f);
            checkRT.offsetMin = new Vector2(6f, 6f);
            checkRT.offsetMax = new Vector2(-6f, -6f);
            var checkImg = checkGO.AddComponent<Image>();
            checkImg.color = new Color(0.55f, 0.85f, 0.45f, 1f);

            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;
            toggle.isOn = isOn;

            if (onChange != null)
            {
                toggle.onValueChanged.AddListener(v => onChange(v));
            }

            return toggle;
        }

        // Cycler opcji: < label > z dwoma przyciskami nawigacji.
        public static OptionCycler CreateOptionCycler(Transform parent, string[] options, int currentIndex,
            Action<int> onChange,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            string name = "OptionCycler")
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.12f, 0.08f, 0.95f);
            bg.raycastTarget = false;

            var cycler = root.AddComponent<OptionCycler>();

            var leftBtn = CreateButton(root.transform, "<",
                new Color(0.18f, 0.32f, 0.2f, 1f), new Color(0.3f, 0.55f, 0.32f, 1f),
                () => cycler.Prev(),
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(0f, 0f), new Vector2(50f, 0f), 28);

            var rightBtn = CreateButton(root.transform, ">",
                new Color(0.18f, 0.32f, 0.2f, 1f), new Color(0.3f, 0.55f, 0.32f, 1f),
                () => cycler.Next(),
                new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(-50f, 0f), new Vector2(0f, 0f), 28);

            var labelHolder = CreatePanel(root.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(55f, 0f), new Vector2(-55f, 0f), "LabelHolder");
            var label = CreateText(labelHolder, "", 24, new Color(0.92f, 0.95f, 0.8f), TextAnchor.MiddleCenter, "Value");

            cycler.Init(options, currentIndex, label, onChange);

            return cycler;
        }

        // Horizontal bar (HP/Mana) - returns a fill RectTransform pivoted at left.
        public static RectTransform CreateBar(Transform parent, Color bg, Color fill,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string name = "Bar")
        {
            return CreateBar(parent, bg, fill, null, null, anchorMin, anchorMax, offsetMin, offsetMax, name);
        }

        // Bar z opcjonalnymi sprite'ami Fantasy Free GUI (fallback: kolory).
        public static RectTransform CreateBar(Transform parent, Color bg, Color fill,
            Sprite bgSprite, Sprite fillSprite,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string name = "Bar")
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            var bgImg = root.AddComponent<Image>();
            bgImg.sprite = bgSprite != null ? bgSprite : GetDefaultSprite();
            bgImg.type = bgSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            bgImg.color = bgSprite != null ? Color.white : bg;
            bgImg.raycastTarget = false;

            var fillRoot = new GameObject("Fill");
            fillRoot.transform.SetParent(root.transform, false);
            var fillRT = fillRoot.AddComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0f, 0f);
            fillRT.anchorMax = new Vector2(1f, 1f);
            fillRT.pivot = new Vector2(0f, 0.5f);
            fillRT.offsetMin = new Vector2(3f, 3f);
            fillRT.offsetMax = new Vector2(-3f, -3f);
            var fillImg = fillRoot.AddComponent<Image>();
            fillImg.sprite = fillSprite != null ? fillSprite : GetDefaultSprite();
            fillImg.type = fillSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            fillImg.color = fillSprite != null ? Color.white : fill;
            fillImg.raycastTarget = false;

            return fillRT;
        }

        public static Button CreateFantasyButton(Transform parent, string text, Action onClick,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            Color fallbackBase, Color fallbackHover, int fontSize = 36)
        {
            var btnSprite = GameAssetLoader.LoadSprite(GameAssetPaths.GuiButton, GameAssetPaths.ResUiButton);
            if (btnSprite == null)
            {
                return CreateButton(parent, text, fallbackBase, fallbackHover, onClick,
                    anchorMin, anchorMax, offsetMin, offsetMax, fontSize);
            }

            var go = new GameObject("Button_" + text);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var img = go.AddComponent<Image>();
            img.sprite = btnSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 0.92f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.7f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            btn.targetGraphic = img;

            if (onClick != null)
            {
                btn.onClick.AddListener(() =>
                {
                    GameAudioManager.EnsureExists()?.PlayUIClick();
                    onClick();
                });
            }

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            var label = labelGO.AddComponent<Text>();
            label.text = text;
            label.font = GetFont();
            label.fontSize = fontSize;
            label.color = new Color(0.95f, 0.96f, 0.9f);
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;

            return btn;
        }
    }

    // Pomocniczy komponent: przełącza listę opcji w lewo/prawo i odświeża etykietę.
    public class OptionCycler : MonoBehaviour
    {
        private string[] _options;
        private int _index;
        private Text _label;
        private Action<int> _onChange;
        private bool _initialized;

        public int Index => _index;

        public void Init(string[] options, int currentIndex, Text label, Action<int> onChange)
        {
            _options = options ?? new string[0];
            _index = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, _options.Length - 1));
            _label = label;
            _onChange = onChange;
            _initialized = true;
            Refresh();
        }

        public void SetIndex(int index, bool notify = true)
        {
            if (!_initialized || _options.Length == 0) return;
            _index = Mathf.Clamp(index, 0, _options.Length - 1);
            Refresh();
            if (notify) _onChange?.Invoke(_index);
        }

        public void Next()
        {
            if (!_initialized || _options.Length == 0) return;
            _index = (_index + 1) % _options.Length;
            Refresh();
            _onChange?.Invoke(_index);
        }

        public void Prev()
        {
            if (!_initialized || _options.Length == 0) return;
            _index = (_index - 1 + _options.Length) % _options.Length;
            Refresh();
            _onChange?.Invoke(_index);
        }

        private void Refresh()
        {
            if (_label != null && _options != null && _options.Length > 0)
                _label.text = _options[_index];
        }
    }
}
