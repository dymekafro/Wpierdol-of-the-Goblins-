using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterCreationUI : MonoBehaviour
{
    private string mainMenuSceneName = "MainMenu";
    private string gameplaySceneName = "World";

    private Font uiFont;
    private Canvas canvas;

    private InputField nameInput;
    private Text validationText;
    private Text classDetailsText;
    private Text summaryText;
    private Text previewText;
    private Text remainingPointsText;
    private Text combatAttributesText;
    private Button startButton;

    private readonly Dictionary<CharacterStatType, Text> statValueLabels = new Dictionary<CharacterStatType, Text>();
    private readonly Dictionary<CharacterStatType, Button> statMinusButtons = new Dictionary<CharacterStatType, Button>();
    private readonly Dictionary<CharacterClassType, Button> classButtons = new Dictionary<CharacterClassType, Button>();

    private CharacterClassDefinition selectedClass;
    private CharacterStatsData allocatedStats = CharacterStatsData.Zero();
    private CharacterAppearanceData appearance = new CharacterAppearanceData();

    private Text bodyValueText;
    private Text faceValueText;
    private Text hairValueText;
    private Text facialHairValueText;
    private Text specialMarkValueText;
    private Text outfitValueText;

    public void Initialize(string mainMenuScene, string gameplayScene)
    {
        mainMenuSceneName = string.IsNullOrWhiteSpace(mainMenuScene) ? "MainMenu" : mainMenuScene;
        gameplaySceneName = string.IsNullOrWhiteSpace(gameplayScene) ? "World" : gameplayScene;

        LoadUIFont();
        BuildUI();
        SelectClass(CharacterClassType.Warrior);
        RefreshAll();
    }

    private void LoadUIFont()
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (uiFont == null)
            Debug.LogWarning("[CharacterCreationUI] Nie znaleziono LegacyRuntime.ttf. UI użyje domyślnego fontu Unity.");
    }

    private void BuildUI()
    {
        canvas = CreateCanvas("Character Creation Canvas");

        GameObject background = CreateUIObject("Background", canvas.transform);
        Stretch(background.GetComponent<RectTransform>());
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.05f, 0.055f, 0.065f, 1f);

        GameObject root = CreateUIObject("Root", canvas.transform);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = new Vector2(12f, 12f);
        rootRect.offsetMax = new Vector2(-12f, -12f);

        Text title = CreateText("Title", root.transform, "TWORZENIE POSTACI", 34, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.offsetMin = new Vector2(0f, -44f);
        titleRect.offsetMax = new Vector2(0f, 0f);

        GameObject columns = CreateUIObject("Columns", root.transform);
        RectTransform columnsRect = columns.GetComponent<RectTransform>();
        columnsRect.anchorMin = new Vector2(0f, 0.25f);
        columnsRect.anchorMax = new Vector2(1f, 0.94f);
        columnsRect.offsetMin = new Vector2(0f, 0f);
        columnsRect.offsetMax = new Vector2(0f, -4f);

        GameObject bottom = CreateHorizontalPanel("Bottom_Summary_And_Actions", root.transform, 0f);
        RectTransform bottomRect = bottom.GetComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0f, 0f);
        bottomRect.anchorMax = new Vector2(1f, 0.235f);
        bottomRect.offsetMin = new Vector2(0f, 0f);
        bottomRect.offsetMax = new Vector2(0f, -2f);

        Transform left = CreateAnchoredVerticalPanel("Left_ClassSelection", columns.transform, 0f, 0.445f, 0f, 0f).transform;
        Transform middle = CreateAnchoredVerticalPanel("Middle_Preview", columns.transform, 0.455f, 0.675f, 0f, 0f).transform;
        Transform right = CreateAnchoredVerticalPanel("Right_Stats", columns.transform, 0.685f, 1f, 0f, 0f).transform;

        BuildClassPanel(left);
        BuildPreviewPanel(middle);
        BuildStatsPanel(right);
        BuildBottomPanel(bottom.transform);
    }

    private void BuildClassPanel(Transform parent)
    {
        CreateHeader(parent, "KLASA");

        foreach (CharacterClassDefinition definition in CharacterClassDatabase.All)
        {
            CharacterClassType classType = definition.classType;
            Button button = CreateButton(parent, definition.displayName, () => SelectClass(classType), 42f, 18);
            classButtons[classType] = button;
        }

        classDetailsText = CreateText("ClassDetails", parent, "", 18, FontStyle.Normal, TextAnchor.UpperLeft, Color.white);
        classDetailsText.horizontalOverflow = HorizontalWrapMode.Wrap;
        classDetailsText.verticalOverflow = VerticalWrapMode.Truncate;
        classDetailsText.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
    }

    private void BuildPreviewPanel(Transform parent)
    {
        CreateHeader(parent, "PODGLĄD");

        previewText = CreateText("PreviewText", parent, "", 18, FontStyle.Normal, TextAnchor.UpperLeft, Color.white);
        previewText.horizontalOverflow = HorizontalWrapMode.Wrap;
        previewText.verticalOverflow = VerticalWrapMode.Overflow;
        previewText.gameObject.AddComponent<LayoutElement>().preferredHeight = 135f;

        CreateHeader(parent, "WYGLĄD");

        bodyValueText = CreateEnumSelector<BodyType>(parent, "Sylwetka", appearance.bodyType, value =>
        {
            appearance.bodyType = value;
            RefreshAll();
        });

        faceValueText = CreateEnumSelector<FaceVariant>(parent, "Twarz", appearance.faceVariant, value =>
        {
            appearance.faceVariant = value;
            RefreshAll();
        });

        hairValueText = CreateEnumSelector<HairVariant>(parent, "Fryzura", appearance.hairVariant, value =>
        {
            appearance.hairVariant = value;
            RefreshAll();
        });

        facialHairValueText = CreateEnumSelector<FacialHairVariant>(parent, "Zarost", appearance.facialHairVariant, value =>
        {
            appearance.facialHairVariant = value;
            RefreshAll();
        });

        specialMarkValueText = CreateEnumSelector<SpecialMarkVariant>(parent, "Znak szczególny", appearance.specialMarkVariant, value =>
        {
            appearance.specialMarkVariant = value;
            RefreshAll();
        });

        outfitValueText = CreateEnumSelector<OutfitVariant>(parent, "Strój", appearance.outfitVariant, value =>
        {
            appearance.outfitVariant = value;
            RefreshAll();
        });

        CreateButton(parent, "LOSUJ WYGLĄD", RandomizeAppearance, 44f, 18);
    }

    private void BuildStatsPanel(Transform parent)
    {
        CreateHeader(parent, "STATYSTYKI");

        remainingPointsText = CreateText("RemainingPoints", parent, "", 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.91f, 0.35f, 1f));
        remainingPointsText.gameObject.AddComponent<LayoutElement>().preferredHeight = 42f;

        CreateStatRow(parent, CharacterStatType.Strength, "Siła");
        CreateStatRow(parent, CharacterStatType.Dexterity, "Zręczność");
        CreateStatRow(parent, CharacterStatType.Intelligence, "Inteligencja");
        CreateStatRow(parent, CharacterStatType.Endurance, "Wytrzymałość");
        CreateStatRow(parent, CharacterStatType.Perception, "Percepcja");
        CreateStatRow(parent, CharacterStatType.Charisma, "Charyzma");

        CreateHeader(parent, "PARAMETRY BOJOWE");

        combatAttributesText = CreateText("CombatAttributesText", parent, "", 15, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.86f, 0.90f, 1f, 1f));
        combatAttributesText.horizontalOverflow = HorizontalWrapMode.Wrap;
        combatAttributesText.verticalOverflow = VerticalWrapMode.Overflow;
        combatAttributesText.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
    }

    private void BuildBottomPanel(Transform parent)
    {
        Transform left = CreateVerticalPanel("Bottom_Left", parent, 0.34f).transform;
        Transform right = CreateVerticalPanel("Bottom_Right", parent, 0.66f).transform;

        CreateHeader(left, "IMIĘ BOHATERA");

        nameInput = CreateInput(left, "Wpisz nazwę postaci...");
        nameInput.characterLimit = CharacterCreationRules.MaxCharacterNameLength;
        nameInput.onValueChanged.AddListener(_ => RefreshAll());

        validationText = CreateText("ValidationText", left, "", 18, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.52f, 0.40f, 1f));
        validationText.horizontalOverflow = HorizontalWrapMode.Wrap;
        validationText.verticalOverflow = VerticalWrapMode.Overflow;
        validationText.gameObject.AddComponent<LayoutElement>().preferredHeight = 62f;

        GameObject actions = CreateUIObject("Actions", left);
        HorizontalLayoutGroup actionsLayout = actions.AddComponent<HorizontalLayoutGroup>();
        actionsLayout.spacing = 8f;
        actionsLayout.childControlHeight = true;
        actionsLayout.childControlWidth = true;
        actionsLayout.childForceExpandHeight = false;
        actionsLayout.childForceExpandWidth = true;
        actions.AddComponent<LayoutElement>().preferredHeight = 54f;

        CreateButton(actions.transform, "WSTECZ", BackToMenu, 50f, 18);
        startButton = CreateButton(actions.transform, "ROZPOCZNIJ GRĘ", StartGame, 50f, 18);

        CreateHeader(right, "PODSUMOWANIE");

        summaryText = CreateText("SummaryText", right, "", 16, FontStyle.Normal, TextAnchor.UpperLeft, Color.white);
        summaryText.horizontalOverflow = HorizontalWrapMode.Wrap;
        summaryText.verticalOverflow = VerticalWrapMode.Truncate;
        summaryText.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
    }

    private void SelectClass(CharacterClassType classType)
    {
        selectedClass = CharacterClassDatabase.Get(classType);
        allocatedStats = CharacterStatsData.Zero();
        RefreshAll();
    }

    private void AddStat(CharacterStatType statType)
    {
        if (GetRemainingPoints() <= 0)
            return;

        allocatedStats.AddValue(statType, 1);
        RefreshAll();
    }

    private void RemoveStat(CharacterStatType statType)
    {
        if (allocatedStats.GetValue(statType) <= 0)
            return;

        allocatedStats.AddValue(statType, -1);
        RefreshAll();
    }

    private int GetRemainingPoints()
    {
        return CharacterCreationRules.AllocatableStatPoints - allocatedStats.Sum();
    }

    private CharacterCreationData BuildData()
    {
        return CharacterCreationData.FromState(
            nameInput != null ? nameInput.text : string.Empty,
            selectedClass,
            allocatedStats,
            GetRemainingPoints(),
            appearance
        );
    }

    private void RefreshAll()
    {
        RefreshClassDetails();
        RefreshStats();
        RefreshCombatAttributes();
        RefreshAppearanceSelectors();
        RefreshSummaryAndValidation();
        RefreshPreview();
    }

    private void RefreshClassDetails()
    {
        if (classDetailsText == null)
            return;

        if (selectedClass == null)
        {
            classDetailsText.text = "Wybierz klasę postaci.";
            return;
        }

        classDetailsText.text =
            $"{selectedClass.displayName}\n\n" +
            $"{selectedClass.description}\n\n" +
            $"Styl gry:\n{selectedClass.playStyle}\n\n" +
            $"Ekwipunek:\n{string.Join(", ", selectedClass.startingItems)}\n\n" +
            $"Umiejętności:\n{string.Join(", ", selectedClass.startingSkills)}";

        foreach (KeyValuePair<CharacterClassType, Button> pair in classButtons)
        {
            Image image = pair.Value.GetComponent<Image>();
            if (image == null)
                continue;

            bool active = selectedClass != null && pair.Key == selectedClass.classType;
            image.color = active ? new Color(0.42f, 0.56f, 0.95f, 1f) : new Color(0.88f, 0.88f, 0.88f, 1f);
        }
    }

    private void RefreshStats()
    {
        CharacterStatsData baseStats = selectedClass != null
            ? selectedClass.baseStats
            : CharacterStatsData.Zero();

        CharacterStatsData finalStats = CharacterStatsData.Add(baseStats, allocatedStats);

        foreach (CharacterStatType statType in Enum.GetValues(typeof(CharacterStatType)))
        {
            if (statValueLabels.TryGetValue(statType, out Text label))
                label.text = $"{finalStats.GetValue(statType)}    (+{allocatedStats.GetValue(statType)})";

            if (statMinusButtons.TryGetValue(statType, out Button minusButton))
                minusButton.interactable = allocatedStats.GetValue(statType) > 0;
        }

        if (remainingPointsText != null)
            remainingPointsText.text = $"Wolne punkty: {GetRemainingPoints()}/{CharacterCreationRules.AllocatableStatPoints}";
    }

    private void RefreshCombatAttributes()
    {
        if (combatAttributesText == null)
            return;

        CharacterStatsData baseStats = selectedClass != null
            ? selectedClass.baseStats
            : CharacterStatsData.Zero();

        CharacterStatsData finalStats = CharacterStatsData.Add(baseStats, allocatedStats);

        int attack = 10 + finalStats.strength * 2;
        int carryWeight = 30 + finalStats.strength * 5;

        int movementSpeed = 100 + finalStats.dexterity * 3;
        int dodgeChance = 3 + finalStats.dexterity;

        int magicPower = 5 + finalStats.intelligence * 2;
        int mana = 20 + finalStats.intelligence * 5;

        int health = 80 + finalStats.endurance * 10;
        int stamina = 50 + finalStats.endurance * 4;

        int criticalChance = 2 + finalStats.perception;
        int detectionRange = 8 + finalStats.perception * 2;

        int tradeBonus = finalStats.charisma * 2;
        int dialogueBonus = finalStats.charisma;

        combatAttributesText.text =
            $"Atak: {attack}  (+{allocatedStats.strength * 2})\n" +
            $"Udźwig: {carryWeight}  (+{allocatedStats.strength * 5})\n" +
            $"Szybkość ruchu: {movementSpeed}%  (+{allocatedStats.dexterity * 3}%)\n" +
            $"Unik: {dodgeChance}%  (+{allocatedStats.dexterity}%)\n" +
            $"Moc magiczna: {magicPower}  (+{allocatedStats.intelligence * 2})\n" +
            $"Mana: {mana}  (+{allocatedStats.intelligence * 5})\n" +
            $"Zdrowie: {health}  (+{allocatedStats.endurance * 10})\n" +
            $"Kondycja: {stamina}  (+{allocatedStats.endurance * 4})\n" +
            $"Szansa kryt.: {criticalChance}%  (+{allocatedStats.perception}%)\n" +
            $"Wykrywanie: {detectionRange} m  (+{allocatedStats.perception * 2} m)\n" +
            $"Handel: {tradeBonus}%  (+{allocatedStats.charisma * 2}%)\n" +
            $"Dialogi: {dialogueBonus}  (+{allocatedStats.charisma})";
    }

    private void RefreshAppearanceSelectors()
    {
        if (bodyValueText != null) bodyValueText.text = appearance.bodyType.ToString();
        if (faceValueText != null) faceValueText.text = appearance.faceVariant.ToString();
        if (hairValueText != null) hairValueText.text = appearance.hairVariant.ToString();
        if (facialHairValueText != null) facialHairValueText.text = appearance.facialHairVariant.ToString();
        if (specialMarkValueText != null) specialMarkValueText.text = appearance.specialMarkVariant.ToString();
        if (outfitValueText != null) outfitValueText.text = appearance.outfitVariant.ToString();
    }

    private void RefreshSummaryAndValidation()
    {
        CharacterCreationData data = BuildData();
        bool valid = data.IsValid(out string validationMessage);

        if (validationText != null)
        {
            validationText.text = valid ? "Postać gotowa do rozpoczęcia gry." : validationMessage;
            validationText.color = valid ? new Color(0.45f, 1f, 0.55f, 1f) : new Color(1f, 0.52f, 0.40f, 1f);
        }

        if (startButton != null)
            startButton.interactable = true;

        if (summaryText != null)
            summaryText.text = BuildSummary(data);
    }

    private void RefreshPreview()
    {
        if (previewText == null)
            return;

        CharacterCreationData data = BuildData();
        string className = selectedClass != null ? selectedClass.displayName : "Nie wybrano";

        previewText.text =
            "PODGLĄD POSTACI\n\n" +
            $"Nazwa: {(string.IsNullOrWhiteSpace(data.characterName) ? "-" : data.characterName)}\n" +
            $"Klasa: {className}\n\n" +
            "Wygląd wybierasz poniżej za pomocą strzałek.";
    }

    private string BuildSummary(CharacterCreationData data)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"Nazwa: {(string.IsNullOrWhiteSpace(data.characterName) ? "-" : data.characterName)}");
        builder.AppendLine($"Klasa: {(selectedClass != null ? selectedClass.displayName : "-")}");
        builder.AppendLine($"Wolne punkty: {data.remainingStatPoints}");
        builder.AppendLine($"Statystyki: Siła {data.finalStats.strength}, Zręczność {data.finalStats.dexterity}, Inteligencja {data.finalStats.intelligence}, Wytrzymałość {data.finalStats.endurance}, Percepcja {data.finalStats.perception}, Charyzma {data.finalStats.charisma}");
        builder.AppendLine($"Ekwipunek: {(data.startingItems != null && data.startingItems.Count > 0 ? string.Join(", ", data.startingItems) : "-")}");
        builder.AppendLine($"Umiejętności: {(data.startingSkills != null && data.startingSkills.Count > 0 ? string.Join(", ", data.startingSkills) : "-")}");

        return builder.ToString();
    }

    private void RandomizeAppearance()
    {
        appearance.Randomize();
        RefreshAll();
    }

    private void BackToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void StartGame()
    {
        CharacterCreationData data = BuildData();

        if (!data.IsValid(out string validationMessage))
        {
            if (validationText != null)
            {
                validationText.text = validationMessage;
                validationText.color = new Color(1f, 0.52f, 0.40f, 1f);
            }

            Debug.LogWarning("[CharacterCreationUI] Nie można rozpocząć gry: " + validationMessage);
            return;
        }

        CharacterCreationSession.SetCurrentCharacter(data);
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void CreateStatRow(Transform parent, CharacterStatType statType, string label)
    {
        GameObject row = CreateUIObject(label + "Row", parent);

        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.padding = new RectOffset(0, 0, 2, 2);
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        row.AddComponent<LayoutElement>().preferredHeight = 48f;

        Text nameLabel = CreateText(label + "Label", row.transform, label, 18, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        nameLabel.gameObject.AddComponent<LayoutElement>().preferredWidth = 155f;

        Text valueLabel = CreateText(label + "Value", row.transform, "0", 18, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.91f, 0.35f, 1f));
        valueLabel.gameObject.AddComponent<LayoutElement>().preferredWidth = 115f;
        statValueLabels[statType] = valueLabel;

        Button minus = CreateButton(row.transform, "-", () => RemoveStat(statType), 42f, 22);
        SetLayoutWidth(minus.gameObject, 48f);
        statMinusButtons[statType] = minus;

        Button plus = CreateButton(row.transform, "+", () => AddStat(statType), 42f, 22);
        SetLayoutWidth(plus.gameObject, 48f);
    }

    private Text CreateEnumSelector<T>(Transform parent, string label, T currentValue, Action<T> onChanged) where T : Enum
    {
        Text labelText = CreateText(label + "Label", parent, label, 15, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.82f, 0.86f, 0.95f, 1f));
        labelText.gameObject.AddComponent<LayoutElement>().preferredHeight = 20f;

        GameObject row = CreateUIObject(label + "SelectorRow", parent);

        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 6f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        row.AddComponent<LayoutElement>().preferredHeight = 40f;

        Text valueText = null;

        Button leftButton = CreateButton(row.transform, "<", () =>
        {
            T next = StepEnum(currentValueGetter: () => ParseEnum<T>(valueText.text), direction: -1);
            onChanged?.Invoke(next);
        }, 38f, 20);
        SetLayoutSize(leftButton.gameObject, 44f, 38f, 0f);

        GameObject valueBox = CreateUIObject(label + "ValueBox", row.transform);

        Image valueImage = valueBox.AddComponent<Image>();
        valueImage.color = new Color(0.95f, 0.95f, 0.95f, 1f);

        SetLayoutSize(valueBox, 150f, 38f, 1f);

        valueText = CreateText("Value", valueBox.transform, currentValue.ToString(), 17, FontStyle.Bold, TextAnchor.MiddleCenter, Color.black);
        valueText.horizontalOverflow = HorizontalWrapMode.Overflow;
        valueText.verticalOverflow = VerticalWrapMode.Truncate;
        Stretch(valueText.GetComponent<RectTransform>());

        Button rightButton = CreateButton(row.transform, ">", () =>
        {
            T next = StepEnum(currentValueGetter: () => ParseEnum<T>(valueText.text), direction: 1);
            onChanged?.Invoke(next);
        }, 38f, 20);
        SetLayoutSize(rightButton.gameObject, 44f, 38f, 0f);

        return valueText;
    }

    private void SetLayoutWidth(GameObject target, float width)
    {
        SetLayoutSize(target, width, -1f, 0f);
    }

    private void SetLayoutSize(GameObject target, float width, float height, float flexibleWidth)
    {
        LayoutElement layoutElement = target.GetComponent<LayoutElement>();

        if (layoutElement == null)
            layoutElement = target.AddComponent<LayoutElement>();

        if (width >= 0f)
        {
            layoutElement.minWidth = width;
            layoutElement.preferredWidth = width;
        }

        if (height >= 0f)
        {
            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;
        }

        layoutElement.flexibleWidth = flexibleWidth;
    }

    private T StepEnum<T>(Func<T> currentValueGetter, int direction) where T : Enum
    {
        Array values = Enum.GetValues(typeof(T));
        T currentValue = currentValueGetter();
        int index = Array.IndexOf(values, currentValue);

        if (index < 0)
            index = 0;

        int nextIndex = index + direction;

        if (nextIndex < 0)
            nextIndex = values.Length - 1;

        if (nextIndex >= values.Length)
            nextIndex = 0;

        return (T)values.GetValue(nextIndex);
    }

    private T ParseEnum<T>(string value) where T : Enum
    {
        if (Enum.TryParse(typeof(T), value, out object parsed))
            return (T)parsed;

        Array values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(0);
    }

    private Canvas CreateCanvas(string objectName)
    {
        GameObject canvasObject = new GameObject(objectName);

        Canvas newCanvas = canvasObject.AddComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return newCanvas;
    }

    private void CreateHeader(Transform parent, string text)
    {
        Text header = CreateText(text + "Header", parent, text, 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.91f, 0.35f, 1f));
        header.gameObject.AddComponent<LayoutElement>().preferredHeight = 38f;
    }

    private GameObject CreateAnchoredVerticalPanel(string objectName, Transform parent, float anchorMinX, float anchorMaxX, float leftOffset, float rightOffset)
    {
        GameObject panel = CreateUIObject(objectName, parent);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(anchorMinX, 0f);
        rect.anchorMax = new Vector2(anchorMaxX, 1f);
        rect.offsetMin = new Vector2(leftOffset, 0f);
        rect.offsetMax = new Vector2(-rightOffset, 0f);

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.12f, 0.13f, 0.16f, 0.96f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 12, 12);
        layout.spacing = 8f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        return panel;
    }

    private GameObject CreateVerticalPanel(string objectName, Transform parent, float widthMode)
    {
        GameObject panel = CreateUIObject(objectName, parent);

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.12f, 0.13f, 0.16f, 0.96f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 12, 12);
        layout.spacing = 8f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        LayoutElement layoutElement = panel.AddComponent<LayoutElement>();

        if (widthMode > 0f && widthMode <= 1f)
        {
            layoutElement.flexibleWidth = widthMode;
        }
        else if (widthMode > 1f)
        {
            layoutElement.minWidth = widthMode;
            layoutElement.preferredWidth = widthMode;
            layoutElement.flexibleWidth = 0f;
        }

        return panel;
    }

    private GameObject CreateHorizontalPanel(string objectName, Transform parent, float flexibleWidth)
    {
        GameObject panel = CreateUIObject(objectName, parent);

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.12f, 0.13f, 0.16f, 0.96f);

        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.spacing = 12f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = true;

        LayoutElement layoutElement = panel.AddComponent<LayoutElement>();
        if (flexibleWidth > 0f)
            layoutElement.flexibleWidth = flexibleWidth;

        return panel;
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private Text CreateText(string objectName, Transform parent, string value, int fontSize, FontStyle fontStyle, TextAnchor alignment, Color color)
    {
        GameObject obj = CreateUIObject(objectName, parent);

        Text text = obj.AddComponent<Text>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        if (uiFont != null)
            text.font = uiFont;

        return text;
    }

    private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick, float preferredHeight, int fontSize)
    {
        GameObject obj = CreateUIObject(label + "Button", parent);

        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.88f, 0.88f, 0.88f, 1f);

        Button button = obj.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        LayoutElement layoutElement = obj.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = preferredHeight;

        Text buttonText = CreateText("Text", obj.transform, label, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter, Color.black);
        Stretch(buttonText.GetComponent<RectTransform>());

        return button;
    }

    private InputField CreateInput(Transform parent, string placeholder)
    {
        GameObject obj = CreateUIObject("NameInput", parent);

        Image image = obj.AddComponent<Image>();
        image.color = Color.white;

        InputField input = obj.AddComponent<InputField>();
        obj.AddComponent<LayoutElement>().preferredHeight = 50f;

        Text text = CreateText("Text", obj.transform, "", 20, FontStyle.Normal, TextAnchor.MiddleLeft, Color.black);
        RectTransform textRect = text.GetComponent<RectTransform>();
        Stretch(textRect);
        textRect.offsetMin = new Vector2(10f, 4f);
        textRect.offsetMax = new Vector2(-10f, -4f);

        Text placeholderText = CreateText("Placeholder", obj.transform, placeholder, 20, FontStyle.Italic, TextAnchor.MiddleLeft, new Color(0.35f, 0.35f, 0.35f, 1f));
        RectTransform placeholderRect = placeholderText.GetComponent<RectTransform>();
        Stretch(placeholderRect);
        placeholderRect.offsetMin = new Vector2(10f, 4f);
        placeholderRect.offsetMax = new Vector2(-10f, -4f);

        input.textComponent = text;
        input.placeholder = placeholderText;

        return input;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
