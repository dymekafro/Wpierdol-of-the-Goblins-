using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Health UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    [Header("Mana UI")]
    [SerializeField] private Slider manaSlider;
    [SerializeField] private TMP_Text manaText;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = PlayerStats.Instance;
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        if (playerStats == null)
            playerStats = PlayerStats.Instance;

        TrySubscribe();

        if (playerStats != null)
        {
            UpdateHealth(playerStats.currentHealth, playerStats.maxHealth);
            UpdateMana(playerStats.currentMana, playerStats.maxMana);
        }
    }

    private void OnDisable()
    {
        if (playerStats == null)
            return;

        playerStats.OnHealthChanged -= UpdateHealth;
        playerStats.OnManaChanged -= UpdateMana;
    }

    private void TrySubscribe()
    {
        if (playerStats == null)
            return;

        playerStats.OnHealthChanged -= UpdateHealth;
        playerStats.OnManaChanged -= UpdateMana;

        playerStats.OnHealthChanged += UpdateHealth;
        playerStats.OnManaChanged += UpdateMana;
    }

    private void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
            healthText.text = $"HP: {currentHealth} / {maxHealth}";
    }

    private void UpdateMana(int currentMana, int maxMana)
    {
        if (manaSlider != null)
        {
            manaSlider.maxValue = maxMana;
            manaSlider.value = currentMana;
        }

        if (manaText != null)
            manaText.text = $"Mana: {currentMana} / {maxMana}";
    }
}