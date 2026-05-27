using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public string enemyName = "Goblin";
    public int maxHealth = 20;
    public int currentHealth = 20;

    [Header("UI")]
    public Slider healthBar;

    [Header("Floating Damage Text")]
    [SerializeField] private FloatingDamageText floatingDamageTextPrefab;
    [SerializeField] private Transform floatingTextSpawnPoint;
    [SerializeField] private Vector3 floatingTextOffset = new Vector3(0f, 1.8f, 0f);

    [Header("Hit Reaction")]
    [SerializeField] private EnemyHitReaction hitReaction;

    [Header("Loot")]
    public bool dropsLoot = true;

    private void Awake()
    {
        if (hitReaction == null)
            hitReaction = GetComponent<EnemyHitReaction>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector3.zero);
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(enemyName + " otrzymał obrażenia: " + damage + ". HP: " + currentHealth + "/" + maxHealth);

        SpawnHitEffect();
        SpawnFloatingDamageText(damage);
        UpdateHealthBar();

        if (hitReaction != null)
            hitReaction.PlayHitReaction(hitDirection);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null) return;

        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
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

    private void SpawnHitEffect()
    {
        GameObject hitEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hitEffect.name = "HitEffect";

        hitEffect.transform.position = transform.position + Vector3.up * 1.2f;
        hitEffect.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

        Renderer renderer = hitEffect.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material.color = Color.red;
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
        Debug.Log(enemyName + " został pokonany.");

        if (dropsLoot && InventoryManager.Instance != null)
        {
            ItemData loot = new ItemData(
                "Gobliński Ząb",
                ItemType.Material,
                0,
                0,
                0
            );

            InventoryManager.Instance.AddItem(loot);
            Debug.Log(enemyName + " upuścił loot: " + loot.itemName);
        }

        Destroy(gameObject);
    }
}