using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AttributeAllocationManager : MonoBehaviour
{
    [Header("Texts")]
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI dexterityText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI intelligenceText;
    public TextMeshProUGUI enduranceText;
    public TextMeshProUGUI charismaText;
    public TextMeshProUGUI availablePointsText;

    private PlayerAttributes attributes;

    private void Start()
    {
        if (GameData.Instance == null)
        {
            Debug.LogError("Brak GameData. Uruchom grę od sceny MainMenu.");
            return;
        }

        attributes = GameData.Instance.playerAttributes;
        RefreshUI();
    }

    public void AddStrength()
    {
        if (!CanSpendPoint()) return;
        attributes.strength++;
        attributes.availablePoints--;
        RefreshUI();
    }

    public void AddDexterity()
    {
        if (!CanSpendPoint()) return;
        attributes.dexterity++;
        attributes.availablePoints--;
        RefreshUI();
    }

    public void AddMana()
    {
        if (!CanSpendPoint()) return;
        attributes.mana++;
        attributes.availablePoints--;
        RefreshUI();
    }

    public void AddIntelligence()
    {
        if (!CanSpendPoint()) return;
        attributes.intelligence++;
        attributes.availablePoints--;
        RefreshUI();
    }

    public void AddEndurance()
    {
        if (!CanSpendPoint()) return;
        attributes.endurance++;
        attributes.availablePoints--;
        RefreshUI();
    }

    public void AddCharisma()
    {
        if (!CanSpendPoint()) return;
        attributes.charisma++;
        attributes.availablePoints--;
        RefreshUI();
    }

    public void ConfirmAttributes()
    {
        SceneManager.LoadScene("World");
    }

    private bool CanSpendPoint()
    {
        return attributes != null && attributes.availablePoints > 0;
    }

    private void RefreshUI()
    {
        strengthText.text = "Siła: " + attributes.strength;
        dexterityText.text = "Zręczność: " + attributes.dexterity;
        manaText.text = "Mana: " + attributes.mana;
        intelligenceText.text = "Inteligencja: " + attributes.intelligence;
        enduranceText.text = "Wytrwałość: " + attributes.endurance;
        charismaText.text = "Charyzma: " + attributes.charisma;
        availablePointsText.text = "Punkty: " + attributes.availablePoints;
    }
}