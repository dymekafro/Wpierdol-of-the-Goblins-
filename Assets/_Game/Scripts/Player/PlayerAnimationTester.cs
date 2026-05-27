using UnityEngine;

public class PlayerAnimationTester : MonoBehaviour
{
    [SerializeField] private PlayerAnimationManager animationManager;

    private void Awake()
    {
        if (animationManager == null)
            animationManager = GetComponent<PlayerAnimationManager>();
    }

    private void Update()
    {
        if (animationManager == null)
        {
            Debug.LogWarning("PlayerAnimationTester: Animation Manager is missing.", this);
            return;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("TEST: F1 Idle", this);
            animationManager.PlayIdle();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("TEST: F2 Move", this);
            animationManager.PlayMove();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("TEST: F3 Attack", this);
            animationManager.PlayAttack();
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("TEST: F4 Cast", this);
            animationManager.PlayCast();
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("TEST: F5 Hit", this);
            animationManager.PlayHit();
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            Debug.Log("TEST: F6 Death", this);
            animationManager.PlayDeath();
        }
    }
}