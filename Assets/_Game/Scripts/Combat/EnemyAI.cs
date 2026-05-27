using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float detectionRange = 8f;
    public float attackRange = 1.7f;
    public float moveSpeed = 2f;

    [Header("Attack")]
    public int attackDamage = 5;
    public float attackCooldown = 1.5f;

    private float lastAttackTime;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void Update()
    {
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsGameplay())
        {
            return;
        }

        if (player == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            MoveTowardsPlayer();
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            return;
        }

        direction.Normalize();

        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void AttackPlayer()
    {
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.TakeDamage(attackDamage);
            Debug.Log("Przeciwnik zaatakował gracza za " + attackDamage + " obrażeń.");
        }
    }
}