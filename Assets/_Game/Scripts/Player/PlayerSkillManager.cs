using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerAnimationManager animationManager;
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private Transform aimTransform;

    [Header("UI")]
    [SerializeField] private SkillCooldownUI healCooldownUI;
    [SerializeField] private SkillCooldownUI fireballCooldownUI;

    [Header("Heal Skill")]
    [SerializeField] private KeyCode healKey = KeyCode.Q;
    [SerializeField] private int healManaCost = 15;
    [SerializeField] private int healAmount = 25;
    [SerializeField] private float healCooldown = 5f;

    [Header("Fireball Skill")]
    [SerializeField] private KeyCode fireballKey = KeyCode.E;
    [SerializeField] private int fireballManaCost = 10;
    [SerializeField] private int fireballDamage = 15;
    [SerializeField] private float fireballCooldown = 2.5f;
    [SerializeField] private LayerMask enemyLayers;

    private float nextHealTime;
    private float nextFireballTime;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (animationManager == null)
            animationManager = GetComponent<PlayerAnimationManager>();

        if (aimTransform == null)
            aimTransform = transform;

        if (fireballSpawnPoint == null)
            fireballSpawnPoint = transform;
    }

    private void Update()
    {
        HandleHealInput();
        HandleFireballInput();
    }

    private void HandleHealInput()
    {
        if (!Input.GetKeyDown(healKey))
            return;

        TryUseHeal();
    }

    private void HandleFireballInput()
    {
        if (!Input.GetKeyDown(fireballKey))
            return;

        TryUseFireball();
    }

    private void TryUseHeal()
    {
        if (Time.time < nextHealTime)
        {
            Debug.Log("Heal jest jeszcze na cooldownie.");
            return;
        }

        if (playerStats == null)
            return;

        if (!playerStats.UseMana(healManaCost))
            return;

        playerStats.Heal(healAmount);

        if (animationManager != null)
            animationManager.PlayCast();

        nextHealTime = Time.time + healCooldown;

        if (healCooldownUI != null)
            healCooldownUI.StartCooldown(healCooldown);

        Debug.Log("Użyto Heal.");
    }

    private void TryUseFireball()
    {
        if (Time.time < nextFireballTime)
        {
            Debug.Log("Fireball jest jeszcze na cooldownie.");
            return;
        }

        if (playerStats == null)
            return;

        if (!playerStats.UseMana(fireballManaCost))
            return;

        SpawnFireball();

        if (animationManager != null)
            animationManager.PlayCast();

        nextFireballTime = Time.time + fireballCooldown;

        if (fireballCooldownUI != null)
            fireballCooldownUI.StartCooldown(fireballCooldown);

        Debug.Log("Użyto Fireball.");
    }

    private void SpawnFireball()
    {
        Vector3 spawnPosition = fireballSpawnPoint.position + Vector3.up * 1.2f + aimTransform.forward * 0.8f;
        Vector3 direction = aimTransform.forward;

        GameObject fireball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fireball.name = "Fireball";
        fireball.transform.position = spawnPosition;
        fireball.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

        Renderer renderer = fireball.GetComponent<Renderer>();

        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
                shader = Shader.Find("Standard");

            Material material = new Material(shader);
            material.color = new Color(1f, 0.35f, 0f);
            renderer.material = material;
        }

        Collider collider = fireball.GetComponent<Collider>();

        if (collider != null)
            collider.isTrigger = true;

        Rigidbody rb = fireball.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        FireballProjectile projectile = fireball.AddComponent<FireballProjectile>();
        projectile.Initialize(direction, fireballDamage, enemyLayers);
    }
}