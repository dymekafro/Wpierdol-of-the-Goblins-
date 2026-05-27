using UnityEngine;
using UnityEngine.UI;
using WPG.Core;
using WPG.Player;
using WPG.World;

namespace WPG.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        public PlayerStats stats;
        public PlayerCombat combat;

        private Canvas _canvas;
        private RectTransform _hpFill;
        private RectTransform _manaFill;
        private Text _hpText;
        private Text _manaText;
        private Text _zoneText;
        private Text _promptText;
        private Text _saveToast;
        private float _saveToastUntil;

        // Cooldown squares
        private Image _meleeIcon, _fireIcon, _healIcon;

        // Death screen
        private GameObject _deathRoot;
        private Text _deathText;

        public static event System.Action OnRespawnRequested;

        private void Awake()
        {
            UIFactory.EnsureEventSystem();
            _canvas = UIFactory.CreateScreenCanvas("Canvas_HUD", 5);
            BuildHUD();

            WorldZone.OnZoneEntered += OnZoneEntered;
            InteractionDetector.OnPromptChanged += OnPromptChanged;
            DruidBase.OnGameSaved += OnGameSaved;
        }

        private void OnDestroy()
        {
            WorldZone.OnZoneEntered -= OnZoneEntered;
            InteractionDetector.OnPromptChanged -= OnPromptChanged;
            DruidBase.OnGameSaved -= OnGameSaved;
            if (stats != null)
            {
                stats.OnHealthChanged -= OnHpChanged;
                stats.OnManaChanged -= OnManaChanged;
                stats.OnDied -= OnPlayerDied;
            }
        }

        public void Bind(PlayerStats s, PlayerCombat c)
        {
            stats = s;
            combat = c;
            if (s != null)
            {
                s.OnHealthChanged += OnHpChanged;
                s.OnManaChanged += OnManaChanged;
                s.OnDied += OnPlayerDied;
                OnHpChanged(s.currentHealth, s.attributes.MaxHealth);
                OnManaChanged(Mathf.RoundToInt(s.currentMana), s.attributes.MaxMana);
            }
        }

        private void Update()
        {
            if (combat != null)
            {
                if (_meleeIcon != null) _meleeIcon.fillAmount = combat.MeleeCooldownNorm;
                if (_fireIcon != null) _fireIcon.fillAmount = combat.FireballCooldownNorm;
                if (_healIcon != null) _healIcon.fillAmount = combat.HealCooldownNorm;
            }

            if (_saveToast != null)
            {
                if (Time.time < _saveToastUntil)
                {
                    Color c = _saveToast.color;
                    c.a = Mathf.Clamp01(_saveToastUntil - Time.time);
                    _saveToast.color = c;
                }
                else
                {
                    Color c = _saveToast.color;
                    c.a = 0f;
                    _saveToast.color = c;
                }
            }

            if (_deathRoot != null && _deathRoot.activeSelf)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb != null && kb.rKey.wasPressedThisFrame)
                {
                    _deathRoot.SetActive(false);
                    OnRespawnRequested?.Invoke();
                }
            }
        }

        private void BuildHUD()
        {
            // HP bar - lewy górny
            _hpFill = UIFactory.CreateBar(_canvas.transform,
                new Color(0.1f, 0.05f, 0.05f, 0.85f),
                new Color(0.9f, 0.2f, 0.2f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(30, -65), new Vector2(430, -25),
                "HP_Bar");
            var hpLabel = UIFactory.CreatePanel(_canvas.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(30, -90), new Vector2(430, -65),
                "HP_Label");
            _hpText = UIFactory.CreateText(hpLabel, "HP 100/100", 22, new Color(0.95f, 0.85f, 0.85f), TextAnchor.MiddleLeft);

            // Mana bar
            _manaFill = UIFactory.CreateBar(_canvas.transform,
                new Color(0.05f, 0.05f, 0.12f, 0.85f),
                new Color(0.25f, 0.55f, 1f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(30, -140), new Vector2(430, -100),
                "Mana_Bar");
            var manaLabel = UIFactory.CreatePanel(_canvas.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(30, -165), new Vector2(430, -140),
                "Mana_Label");
            _manaText = UIFactory.CreateText(manaLabel, "MANA 50/50", 22, new Color(0.8f, 0.85f, 1f), TextAnchor.MiddleLeft);

            // Zone name - górna część ekranu
            var zoneHolder = UIFactory.CreatePanel(_canvas.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0, -50), new Vector2(0, -10),
                "ZoneHolder");
            _zoneText = UIFactory.CreateText(zoneHolder, "Sady Ostatniego Strażnika", 32,
                new Color(0.85f, 0.95f, 0.7f), TextAnchor.UpperCenter, "ZoneText");
            _zoneText.fontStyle = FontStyle.Bold;

            // Prompt interakcji
            var promptHolder = UIFactory.CreatePanel(_canvas.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(0, -120), new Vector2(0, -50),
                "PromptHolder");
            _promptText = UIFactory.CreateText(promptHolder, "", 28, new Color(1f, 0.95f, 0.7f), TextAnchor.MiddleCenter, "Prompt");

            // Save toast
            var toastHolder = UIFactory.CreatePanel(_canvas.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-360, -60), new Vector2(-30, -20),
                "ToastHolder");
            _saveToast = UIFactory.CreateText(toastHolder, "Zapisano grę", 26, new Color(0.5f, 1f, 0.6f, 0f), TextAnchor.MiddleRight, "SaveToast");

            // Cooldown icons - prawy dół
            float baseX = -260f;
            _meleeIcon = MakeCooldownIcon("Atak (LPM)", baseX, new Color(0.7f, 0.4f, 0.2f));
            baseX += 110f;
            _fireIcon = MakeCooldownIcon("Ognisty Cios (E)", baseX, new Color(1f, 0.5f, 0.1f));
            baseX += 110f;
            _healIcon = MakeCooldownIcon("Leczenie (Q)", baseX, new Color(0.3f, 1f, 0.5f));

            BuildDeathScreen();
        }

        private Image MakeCooldownIcon(string label, float xOffset, Color color)
        {
            var holder = UIFactory.CreatePanel(_canvas.transform,
                new Color(0.05f, 0.05f, 0.05f, 0.9f),
                new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(xOffset, 30), new Vector2(xOffset + 100, 130),
                "Cool_" + label);

            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(holder, false);
            var fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0f, 0f);
            fillRT.anchorMax = new Vector2(1f, 1f);
            fillRT.offsetMin = new Vector2(4, 4);
            fillRT.offsetMax = new Vector2(-4, -4);
            var img = fillGO.AddComponent<Image>();
            img.color = color;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Radial360;
            img.fillOrigin = (int)Image.Origin360.Top;
            img.fillAmount = 1f;
            img.raycastTarget = false;

            UIFactory.CreateText(holder, label, 16, new Color(0.95f, 0.95f, 0.95f), TextAnchor.LowerCenter, "Label");
            return img;
        }

        private void BuildDeathScreen()
        {
            _deathRoot = UIFactory.CreatePanel(_canvas.transform,
                new Color(0.05f, 0.0f, 0.0f, 0.85f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "DeathScreen").gameObject;

            UIFactory.CreateText(_deathRoot.transform, "POLEGŁEŚ", 120,
                new Color(1f, 0.25f, 0.2f), TextAnchor.MiddleCenter, "DeathTitle");

            var sub = UIFactory.CreatePanel(_deathRoot.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 0.3f), new Vector2(1f, 0.45f),
                Vector2.zero, Vector2.zero, "DeathSub");
            _deathText = UIFactory.CreateText(sub, "Las pochłonął cię swoim mrokiem.\nWciśnij R aby odrodzić się w bazie.",
                36, new Color(0.9f, 0.8f, 0.7f), TextAnchor.MiddleCenter, "DeathText");

            _deathRoot.SetActive(false);
        }

        private void OnHpChanged(int current, int max)
        {
            if (_hpFill != null) _hpFill.localScale = new Vector3(Mathf.Clamp01((float)current / Mathf.Max(1, max)), 1f, 1f);
            if (_hpText != null) _hpText.text = $"HP   {current} / {max}";
        }

        private void OnManaChanged(int current, int max)
        {
            if (_manaFill != null) _manaFill.localScale = new Vector3(Mathf.Clamp01((float)current / Mathf.Max(1, max)), 1f, 1f);
            if (_manaText != null) _manaText.text = $"MANA {current} / {max}";
        }

        private void OnPlayerDied()
        {
            if (_deathRoot != null) _deathRoot.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void HideDeathScreen()
        {
            if (_deathRoot != null) _deathRoot.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnZoneEntered(string name)
        {
            if (_zoneText != null) _zoneText.text = name;
        }

        private void OnPromptChanged(string p)
        {
            if (_promptText != null) _promptText.text = p ?? "";
        }

        private void OnGameSaved()
        {
            if (_saveToast == null) return;
            Color c = _saveToast.color;
            c.a = 1f;
            _saveToast.color = c;
            _saveToastUntil = Time.time + 2f;
        }
    }
}
