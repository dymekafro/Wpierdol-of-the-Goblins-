using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Base Stats")]
    public int baseDamage = 3;
    public int baseArmor = 0;
    public int maxHealth = 100;
    public int currentHealth = 100;

    [Header("Mana")]
    public int maxMana = 50;
    public int currentMana = 50;

    [Header("Calculated Stats")]
    public int totalDamage;
    public int totalArmor;

    [Header("References")]
    [SerializeField] private PlayerAnimationManager animationManager;
    [SerializeField] private PlayerHitReaction hitReaction;

    [Header("Floating Damage Text")]
    [SerializeField] private FloatingDamageText floatingDamageTextPrefab;
    [SerializeField] private Transform floatingTextSpawnPoint;
    [SerializeField] private Vector3 floatingTextOffset = new Vector3(0f, 1.8f, 0f);

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnManaChanged;

    private bool isDead = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Wykryto drugi PlayerStats. Usuwam tylko komponent, nie całego Playera.");
            Destroy(this);
            return;
        }

        Instance = this;

        if (animationManager == null)
            animationManager = GetComponent<PlayerAnimationManager>();

        if (hitReaction == null)
            hitReaction = GetComponent<PlayerHitReaction>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;

        CalculateStats();

        NotifyHealthChanged();
        NotifyManaChanged();
    }

    public void CalculateStats()
    {
        totalDamage = baseDamage;
        totalArmor = baseArmor;

        if (EquipmentManager.Instance != null)
        {
            ItemData weapon = EquipmentManager.Instance.GetEquippedWeapon();
            ItemData shield = EquipmentManager.Instance.GetEquippedShield();

            if (weapon != null)
            {
                totalDamage += weapon.damageBonus;
            }

            if (shield != null)
            {
                totalArmor += shield.armorBonus;
            }
        }

        Debug.Log("Statystyki przeliczone. Obrażenia: " + totalDamage + ", Pancerz: " + totalArmor);
    }

    public void TakeDamage(int incomingDamage)
    {
        if (isDead)
            return;

        CalculateStats();

        int finalDamage = Mathf.Max(1, incomingDamage - totalArmor);

        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log("Gracz otrzymał obrażenia: " + finalDamage + ". HP: " + currentHealth + "/" + maxHealth);

        SpawnPlayerHitEffect();
        SpawnFloatingDamageText(finalDamage);

        if (hitReaction != null)
            hitReaction.PlayHitReaction();

        if (animationManager != null)
            animationManager.PlayHit();

        NotifyHealthChanged();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Gracz uleczony o: " + amount + ". HP: " + currentHealth + "/" + maxHealth);

        NotifyHealthChanged();
    }

    public bool UseMana(int amount)
    {
        if (amount <= 0)
            return true;

        if (currentMana < amount)
        {
            Debug.Log("Za mało many. Wymagane: " + amount + ", obecnie: " + currentMana);
            return false;
        }

        currentMana -= amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);

        Debug.Log("Zużyto manę: " + amount + ". Mana: " + currentMana + "/" + maxMana);

        NotifyManaChanged();

        return true;
    }

    public void RestoreMana(int amount)
    {
        if (amount <= 0)
            return;

        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);

        Debug.Log("Przywrócono manę: " + amount + ". Mana: " + currentMana + "/" + maxMana);

        NotifyManaChanged();
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void NotifyManaChanged()
    {
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    private void SpawnFloatingDamageText(int damage)
    {
        if (floatingDamageTextPrefab == null)
            return;

        Vector3 spawnPosition;

        if (floatingTextSpawnPoint != null)
            spawnPosition = floatingTextSpawnPoint.position;
        else
            spawnPosition = transform.position + floatingTextOffset;

        FloatingDamageText textInstance = Instantiate(
            floatingDamageTextPrefab,
            spawnPosition,
            Quaternion.identity
        );

        textInstance.Setup(damage);
    }

    private void SpawnPlayerHitEffect()
    {
        GameObject hitEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hitEffect.name = "PlayerHitEffect";

        hitEffect.transform.position = transform.position + Vector3.up * 1.2f;
        hitEffect.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);

        Renderer renderer = hitEffect.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material.color = Color.yellow;
        }

        Collider collider = hitEffect.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        Destroy(hitEffect, 0.25f);
    }

    private void Die()
    {
        isDead = true;

        if (animationManager != null)
            animationManager.PlayDeath();

        Debug.Log("Gracz zginął.");

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(GameState.Paused);
        }

        PlayerController3D controller = GetComponent<PlayerController3D>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        PlayerCombat combat = GetComponent<PlayerCombat>();

        if (combat != null)
        {
            combat.enabled = false;
        }
    }
}