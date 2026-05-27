using UnityEngine;

public enum PlayerAnimationState
{
    Idle,
    Move,
    Attack,
    Cast,
    Hit,
    Death
}

public class PlayerAnimationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerVisualManager visualManager;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;

    [Header("Placeholder Animation Settings")]
    [SerializeField] private float moveBobSpeed = 10f;
    [SerializeField] private float moveBobAmount = 0.25f;
    [SerializeField] private float idleBobAmount = 0.06f;
    [SerializeField] private float attackTiltAmount = 35f;
    [SerializeField] private float hitShakeAmount = 0.15f;
    [SerializeField] private float castPulseAmount = 0.15f;

    private PlayerAnimationState currentState = PlayerAnimationState.Idle;

    private Transform currentVisualTransform;
    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private Vector3 baseLocalScale;

    private float stateTimer;
    private bool initialized;

    public PlayerAnimationState CurrentState => currentState;

    private void Awake()
    {
        if (visualManager == null)
            visualManager = GetComponent<PlayerVisualManager>();
    }

    private void Start()
    {
        CacheCurrentVisual();
        PlayIdle();
    }

    private void Update()
    {
        if (visualManager == null)
            return;

        if (visualManager.CurrentVisualInstance == null)
            return;

        if (currentVisualTransform == null || currentVisualTransform.gameObject != visualManager.CurrentVisualInstance)
            CacheCurrentVisual();

        stateTimer += Time.deltaTime;

        UpdatePlaceholderAnimation();
    }

    public void PlayIdle()
    {
        SetState(PlayerAnimationState.Idle);
    }

    public void PlayMove()
    {
        SetState(PlayerAnimationState.Move);
    }

    public void PlayAttack()
    {
        SetState(PlayerAnimationState.Attack);
    }

    public void PlayCast()
    {
        SetState(PlayerAnimationState.Cast);
    }

    public void PlayHit()
    {
        SetState(PlayerAnimationState.Hit);
    }

    public void PlayDeath()
    {
        SetState(PlayerAnimationState.Death);
    }

    public void SetMovementState(bool isMoving)
    {
        if (currentState == PlayerAnimationState.Attack)
            return;

        if (currentState == PlayerAnimationState.Cast)
            return;

        if (currentState == PlayerAnimationState.Hit)
            return;

        if (currentState == PlayerAnimationState.Death)
            return;

        if (isMoving)
            PlayMove();
        else
            PlayIdle();
    }

    private void SetState(PlayerAnimationState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        stateTimer = 0f;

        ResetVisualTransform();

        if (logStateChanges)
            Debug.Log($"Player animation state: {newState}", this);
    }

    private void CacheCurrentVisual()
    {
        if (visualManager == null)
            return;

        if (visualManager.CurrentVisualInstance == null)
            return;

        currentVisualTransform = visualManager.CurrentVisualInstance.transform;
        baseLocalPosition = currentVisualTransform.localPosition;
        baseLocalRotation = currentVisualTransform.localRotation;
        baseLocalScale = currentVisualTransform.localScale;

        initialized = true;
    }

    private void UpdatePlaceholderAnimation()
    {
        if (!initialized || currentVisualTransform == null)
            return;

        switch (currentState)
        {
            case PlayerAnimationState.Idle:
                UpdateIdleAnimation();
                break;

            case PlayerAnimationState.Move:
                UpdateMoveAnimation();
                break;

            case PlayerAnimationState.Attack:
                UpdateAttackAnimation();
                break;

            case PlayerAnimationState.Cast:
                UpdateCastAnimation();
                break;

            case PlayerAnimationState.Hit:
                UpdateHitAnimation();
                break;

            case PlayerAnimationState.Death:
                UpdateDeathAnimation();
                break;
        }
    }

    private void UpdateIdleAnimation()
    {
        float bob = Mathf.Sin(Time.time * 2f) * idleBobAmount;

        currentVisualTransform.localPosition = baseLocalPosition + new Vector3(0f, bob, 0f);
        currentVisualTransform.localRotation = baseLocalRotation;
        currentVisualTransform.localScale = baseLocalScale;
    }

    private void UpdateMoveAnimation()
    {
        float bob = Mathf.Abs(Mathf.Sin(Time.time * moveBobSpeed)) * moveBobAmount;
        float tilt = Mathf.Sin(Time.time * moveBobSpeed) * 6f;

        currentVisualTransform.localPosition = baseLocalPosition + new Vector3(0f, bob, 0f);
        currentVisualTransform.localRotation = baseLocalRotation * Quaternion.Euler(tilt, 0f, 0f);
        currentVisualTransform.localScale = baseLocalScale;
    }

    private void UpdateAttackAnimation()
    {
        float duration = 0.35f;
        float progress = stateTimer / duration;
        float swing = Mathf.Sin(progress * Mathf.PI) * attackTiltAmount;

        currentVisualTransform.localPosition = baseLocalPosition;
        currentVisualTransform.localRotation = baseLocalRotation * Quaternion.Euler(swing, 0f, 0f);
        currentVisualTransform.localScale = baseLocalScale;

        if (stateTimer >= duration)
            PlayIdle();
    }

    private void UpdateCastAnimation()
    {
        float duration = 0.7f;
        float scalePulse = 1f + Mathf.Sin(Time.time * 12f) * castPulseAmount;

        currentVisualTransform.localPosition = baseLocalPosition;
        currentVisualTransform.localRotation = baseLocalRotation * Quaternion.Euler(0f, stateTimer * 180f, 0f);
        currentVisualTransform.localScale = baseLocalScale * scalePulse;

        if (stateTimer >= duration)
        {
            currentVisualTransform.localScale = baseLocalScale;
            PlayIdle();
        }
    }

    private void UpdateHitAnimation()
    {
        float duration = 0.25f;
        float shake = Mathf.Sin(Time.time * 60f) * hitShakeAmount;

        currentVisualTransform.localPosition = baseLocalPosition + new Vector3(shake, 0f, 0f);
        currentVisualTransform.localRotation = baseLocalRotation;
        currentVisualTransform.localScale = baseLocalScale;

        if (stateTimer >= duration)
            PlayIdle();
    }

    private void UpdateDeathAnimation()
    {
        currentVisualTransform.localPosition = baseLocalPosition;
        currentVisualTransform.localScale = baseLocalScale;

        currentVisualTransform.localRotation = Quaternion.Lerp(
            currentVisualTransform.localRotation,
            baseLocalRotation * Quaternion.Euler(90f, 0f, 0f),
            Time.deltaTime * 5f
        );
    }

    private void ResetVisualTransform()
    {
        if (currentVisualTransform == null)
            return;

        currentVisualTransform.localPosition = baseLocalPosition;
        currentVisualTransform.localRotation = baseLocalRotation;
        currentVisualTransform.localScale = baseLocalScale;
    }
}