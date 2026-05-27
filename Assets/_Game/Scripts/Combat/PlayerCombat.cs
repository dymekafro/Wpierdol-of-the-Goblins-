using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAnimationManager animationManager;
    [SerializeField] private Transform attackPoint;

    [Header("Attack Settings")]
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 0.6f;

    [Header("Target Detection")]
    [SerializeField] private LayerMask enemyLayers;

    [Header("Debug")]
    [SerializeField] private bool showAttackRange = true;
    [SerializeField] private bool logAttacks = true;

    private float nextAttackTime;

    private void Awake()
    {
        if (animationManager == null)
            animationManager = GetComponent<PlayerAnimationManager>();

        if (attackPoint == null)
            attackPoint = transform;
    }

    private void Update()
    {
        HandleAttackInput();
    }

    private void HandleAttackInput()
    {
        if (!Input.GetKeyDown(attackKey))
            return;

        if (Time.time < nextAttackTime)
            return;

        PerformAttack();
    }

    private void PerformAttack()
    {
        nextAttackTime = Time.time + attackCooldown;

        if (animationManager != null)
            animationManager.PlayAttack();

        if (logAttacks)
            Debug.Log("Player attack performed.", this);

        Collider[] hitTargets = Physics.OverlapSphere(
            attackPoint.position,
            attackRange,
            enemyLayers
        );

        foreach (Collider target in hitTargets)
        {
            Vector3 hitDirection = target.transform.position - transform.position;
            hitDirection.y = 0f;

            Enemy enemy = target.GetComponent<Enemy>();

            if (enemy == null)
                enemy = target.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage, hitDirection);
            }
            else
            {
                target.SendMessage(
                    "TakeDamage",
                    attackDamage,
                    SendMessageOptions.DontRequireReceiver
                );
            }

            if (logAttacks)
                Debug.Log($"Hit target: {target.name} for {attackDamage} damage.", target);
        }
    }

    public void ForceAttack()
    {
        if (Time.time < nextAttackTime)
            return;

        PerformAttack();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showAttackRange)
            return;

        Transform point = attackPoint != null ? attackPoint : transform;

        Gizmos.DrawWireSphere(point.position, attackRange);
    }
}