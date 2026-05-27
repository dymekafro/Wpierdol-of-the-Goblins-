using UnityEngine;

public class PlayerMotionAnimationBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAnimationManager animationManager;

    [Header("Detection")]
    [SerializeField] private float movementThreshold = 0.01f;
    [SerializeField] private bool ignoreVerticalMovement = true;

    private Vector3 lastPosition;

    private void Awake()
    {
        if (animationManager == null)
            animationManager = GetComponent<PlayerAnimationManager>();

        lastPosition = transform.position;
    }

    private void Update()
    {
        if (animationManager == null)
            return;

        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - lastPosition;

        if (ignoreVerticalMovement)
            delta.y = 0f;

        bool isMoving = delta.sqrMagnitude > movementThreshold * movementThreshold;

        animationManager.SetMovementState(isMoving);

        lastPosition = currentPosition;
    }
}