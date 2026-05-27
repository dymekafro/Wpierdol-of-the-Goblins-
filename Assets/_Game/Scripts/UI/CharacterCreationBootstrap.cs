using UnityEngine;
using UnityEngine.UI;
using WPG.Character;
using WPG.Core;

namespace WPG.UI
{
    public class CharacterCreationBootstrap : MonoBehaviour
    {
        private PlayerAttributes _draft;

        private Text _strText, _dexText, _manaText, _intText, _endText, _chaText;
        private Text _pointsText, _previewText;

        private void Awake()
        {
            GameManager.EnsureExists();
            _draft = PlayerAttributes.CreateDruidBase();
        }

        private void Start()
        {
            var camGO = new GameObject("MainCamera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.07f, 0.05f);
            cam.fieldOfView = 60f;
            camGO.AddComponent<AudioListener>();

            UIFactory.EnsureEventSystem();
            var canvas = UIFactory.CreateScreenCanvas("Canvas_Creation");

            var bg = UIFactory.CreatePanel(canvas.transform, new Color(0.04f, 0.07f, 0.05f, 1f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "Background");

            var title = UIFactory.CreatePanel(canvas.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 0.88f), new Vector2(1f, 0.98f), Vector2.zero, Vector2.zero, "Title");
            UIFactory.CreateText(title, "Stworzenie Druida", 72, new Color(0.55f, 0.85f, 0.45f), TextAnchor.MiddleCenter).fontStyle = FontStyle.Bold;

            var sub = UIFactory.CreatePanel(canvas.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 0.82f), new Vector2(1f, 0.87f), Vector2.zero, Vector2.zero, "Sub");
            UIFactory.CreateText(sub, "Ostatni Strażnik Lasu - magia Ognia, Życia i Kształtu", 26,
                new Color(0.7f, 0.8f, 0.55f), TextAnchor.MiddleCenter);

            // Lewy panel - karta postaci
            var leftPanel = UIFactory.CreatePanel(canvas.transform, new Color(0.06f, 0.10f, 0.07f, 0.95f),
                new Vector2(0.06f, 0.15f), new Vector2(0.36f, 0.78f),
                Vector2.zero, Vector2.zero, "DruidCard");
            UIFactory.CreatePanel(leftPanel, new Color(0.2f, 0.4f, 0.22f, 1f),
                new Vector2(0f, 0.94f), new Vector2(1f, 1f),
                Vector2.zero, Vector2.zero, "CardHeader");
            UIFactory.CreateText(leftPanel.Find("CardHeader"), "DRUID", 38, new Color(0.95f, 0.96f, 0.9f), TextAnchor.MiddleCenter);

            var cardBody = UIFactory.CreatePanel(leftPanel, new Color(0f, 0f, 0f, 0f),
                new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.92f),
                Vector2.zero, Vector2.zero, "CardBody");
            UIFactory.CreateText(cardBody,
                "Ostatni z mistrzów dawnej puszczy. " +
                "Słabszy w walce wręcz, lecz silny w czarach Ognia i Życia. " +
                "Bonusy startowe: MANA +3, INT +2, END +1, STR -1.",
                24, new Color(0.85f, 0.88f, 0.7f), TextAnchor.UpperLeft);

            // Środkowy panel - atrybuty
            var midPanel = UIFactory.CreatePanel(canvas.transform, new Color(0.06f, 0.10f, 0.07f, 0.95f),
                new Vector2(0.38f, 0.15f), new Vector2(0.66f, 0.78f),
                Vector2.zero, Vector2.zero, "AttrPanel");
            UIFactory.CreatePanel(midPanel, new Color(0.2f, 0.4f, 0.22f, 1f),
                new Vector2(0f, 0.94f), new Vector2(1f, 1f),
                Vector2.zero, Vector2.zero, "AttrHeader");
            UIFactory.CreateText(midPanel.Find("AttrHeader"), "Atrybuty", 32, new Color(0.95f, 0.96f, 0.9f), TextAnchor.MiddleCenter);

            var attrBody = UIFactory.CreatePanel(midPanel, new Color(0f, 0f, 0f, 0f),
                new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.92f),
                Vector2.zero, Vector2.zero, "AttrBody");

            float y = 0f;
            float step = 70f;
            _strText = MakeAttrRow(attrBody, "STR (siła)", ref y, step, () => Bump("str", +1), () => Bump("str", -1));
            _dexText = MakeAttrRow(attrBody, "DEX (zręczność)", ref y, step, () => Bump("dex", +1), () => Bump("dex", -1));
            _manaText = MakeAttrRow(attrBody, "MANA", ref y, step, () => Bump("mana", +1), () => Bump("mana", -1));
            _intText = MakeAttrRow(attrBody, "INT (intelekt)", ref y, step, () => Bump("int", +1), () => Bump("int", -1));
            _endText = MakeAttrRow(attrBody, "END (wytrzymałość)", ref y, step, () => Bump("end", +1), () => Bump("end", -1));
            _chaText = MakeAttrRow(attrBody, "CHA (charyzma)", ref y, step, () => Bump("cha", +1), () => Bump("cha", -1));

            // Prawy panel - podgląd statów
            var rightPanel = UIFactory.CreatePanel(canvas.transform, new Color(0.06f, 0.10f, 0.07f, 0.95f),
                new Vector2(0.68f, 0.15f), new Vector2(0.94f, 0.78f),
                Vector2.zero, Vector2.zero, "Preview");
            UIFactory.CreatePanel(rightPanel, new Color(0.2f, 0.4f, 0.22f, 1f),
                new Vector2(0f, 0.94f), new Vector2(1f, 1f),
                Vector2.zero, Vector2.zero, "PreviewHeader");
            UIFactory.CreateText(rightPanel.Find("PreviewHeader"), "Podgląd", 32, new Color(0.95f, 0.96f, 0.9f), TextAnchor.MiddleCenter);

            var pvBody = UIFactory.CreatePanel(rightPanel, new Color(0f, 0f, 0f, 0f),
                new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.92f),
                Vector2.zero, Vector2.zero, "PvBody");
            _previewText = UIFactory.CreateText(pvBody, "", 24, new Color(0.85f, 0.88f, 0.7f), TextAnchor.UpperLeft, "PreviewText");

            // Punkty do rozdania
            var pointsPanel = UIFactory.CreatePanel(canvas.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 0.08f), new Vector2(1f, 0.14f), Vector2.zero, Vector2.zero, "Points");
            _pointsText = UIFactory.CreateText(pointsPanel, "", 32, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleCenter);

            // Przyciski
            UIFactory.CreateButton(canvas.transform, "Powrót",
                new Color(0.3f, 0.3f, 0.3f, 0.9f), new Color(0.45f, 0.45f, 0.45f, 1f),
                () => GameManager.Instance.GoToMainMenu(),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-440, 30), new Vector2(-40, 90), 30);

            UIFactory.CreateButton(canvas.transform, "Wejdź do lasu",
                new Color(0.15f, 0.4f, 0.2f, 0.95f), new Color(0.25f, 0.65f, 0.3f, 1f),
                Confirm,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(40, 30), new Vector2(440, 90), 32);

            RefreshUI();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private Text MakeAttrRow(Transform parent, string label, ref float yOffset, float step, System.Action plus, System.Action minus)
        {
            var rowGO = new GameObject("Row_" + label);
            rowGO.transform.SetParent(parent, false);
            var rowRT = rowGO.AddComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0f, 1f);
            rowRT.anchorMax = new Vector2(1f, 1f);
            rowRT.pivot = new Vector2(0.5f, 1f);
            rowRT.anchoredPosition = new Vector2(0f, -yOffset);
            rowRT.sizeDelta = new Vector2(0f, step - 8f);

            UIFactory.CreateText(rowRT, label, 26, new Color(0.85f, 0.88f, 0.7f), TextAnchor.MiddleLeft, "Label");

            UIFactory.CreateButton(rowRT, "-",
                new Color(0.5f, 0.2f, 0.2f, 0.9f), new Color(0.7f, 0.3f, 0.3f, 1f), minus,
                new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(-220, 5), new Vector2(-160, -5), 30);

            var valGO = new GameObject("Value");
            valGO.transform.SetParent(rowRT, false);
            var valRT = valGO.AddComponent<RectTransform>();
            valRT.anchorMin = new Vector2(1f, 0f);
            valRT.anchorMax = new Vector2(1f, 1f);
            valRT.pivot = new Vector2(1f, 0.5f);
            valRT.offsetMin = new Vector2(-150, 0);
            valRT.offsetMax = new Vector2(-70, 0);
            var val = valGO.AddComponent<Text>();
            val.font = UIFactory.GetFont();
            val.fontSize = 32;
            val.alignment = TextAnchor.MiddleCenter;
            val.color = new Color(1f, 0.95f, 0.7f);
            val.text = "5";
            val.raycastTarget = false;

            UIFactory.CreateButton(rowRT, "+",
                new Color(0.2f, 0.5f, 0.2f, 0.9f), new Color(0.3f, 0.7f, 0.3f, 1f), plus,
                new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(-60, 5), new Vector2(0, -5), 30);

            yOffset += step;
            return val;
        }

        private void Bump(string stat, int delta)
        {
            if (delta > 0 && _draft.unallocatedPoints <= 0) return;
            if (delta < 0)
            {
                // Można cofać do bazowych wartości druida
                var baseAttrs = PlayerAttributes.CreateDruidBase();
                int currentVal = stat switch
                {
                    "str" => _draft.strength,
                    "dex" => _draft.dexterity,
                    "mana" => _draft.mana,
                    "int" => _draft.intelligence,
                    "end" => _draft.endurance,
                    "cha" => _draft.charisma,
                    _ => 0
                };
                int baseVal = stat switch
                {
                    "str" => baseAttrs.strength,
                    "dex" => baseAttrs.dexterity,
                    "mana" => baseAttrs.mana,
                    "int" => baseAttrs.intelligence,
                    "end" => baseAttrs.endurance,
                    "cha" => baseAttrs.charisma,
                    _ => 0
                };
                if (currentVal <= baseVal) return;
            }

            switch (stat)
            {
                case "str": _draft.strength += delta; break;
                case "dex": _draft.dexterity += delta; break;
                case "mana": _draft.mana += delta; break;
                case "int": _draft.intelligence += delta; break;
                case "end": _draft.endurance += delta; break;
                case "cha": _draft.charisma += delta; break;
            }
            _draft.unallocatedPoints -= delta;
            RefreshUI();
        }

        private void RefreshUI()
        {
            _strText.text = _draft.strength.ToString();
            _dexText.text = _draft.dexterity.ToString();
            _manaText.text = _draft.mana.ToString();
            _intText.text = _draft.intelligence.ToString();
            _endText.text = _draft.endurance.ToString();
            _chaText.text = _draft.charisma.ToString();
            _pointsText.text = $"Punkty do rozdania: {_draft.unallocatedPoints}";
            _previewText.text =
                $"Maks. HP:      {_draft.MaxHealth}\n" +
                $"Maks. Mana:    {_draft.MaxMana}\n" +
                $"Regen many:    {_draft.ManaRegenPerSecond:F1} / s\n" +
                $"Atak wręcz:    {_draft.MeleeDamage}\n" +
                $"Moc czaru:     {_draft.SpellPower}\n" +
                $"Prędkość:      {_draft.MoveSpeed:F1}";
        }

        private void Confirm()
        {
            GameManager.Instance.attributes = _draft.Clone();
            GameManager.Instance.GoToWorldFromCreation();
        }
    }
}
