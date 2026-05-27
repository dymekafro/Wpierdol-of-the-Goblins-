using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 9f;
    [SerializeField] private float lifetime = 3f;

    [Header("Combat")]
    [SerializeField] private int damage = 15;
    [SerializeField] private LayerMask enemyLayers;

    private Vector3 direction;
    private bool initialized;

    public void Initialize(Vector3 moveDirection, int fireballDamage, LayerMask targetLayers)
    {
        direction = moveDirection.normalized;
        damage = fireballDamage;
        enemyLayers = targetLayers;
        initialized = true;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!initialized)
            return;

        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyLayers) == 0)
            return;

        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy == null)
            enemy = other.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            Vector3 hitDirection = other.transform.position - transform.position;
            hitDirection.y = 0f;

            enemy.TakeDamage(damage, hitDirection);
        }

        Destroy(gameObject);
    }
}