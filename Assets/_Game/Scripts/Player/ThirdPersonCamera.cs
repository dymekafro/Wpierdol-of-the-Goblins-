using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    [Header("Camera Settings")]
    public float distance = 5f;
    public float pivotHeight = 1.5f;
    public float mouseSensitivity = 150f;
    public float minPitch = -25f;
    public float maxPitch = 60f;
    public float smoothSpeed = 12f;

    private float pitch = 15f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsGameplay())
        {
            UpdateCameraPosition(false);
            return;
        }

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        UpdateCameraPosition(true);
    }

    private void UpdateCameraPosition(bool smooth)
    {
        Vector3 pivotPoint = target.position + Vector3.up * pivotHeight;

        Quaternion cameraRotation = Quaternion.Euler(pitch, target.eulerAngles.y, 0f);

        Vector3 desiredPosition = pivotPoint - cameraRotation * Vector3.forward * distance;

        if (smooth)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.position = desiredPosition;
        }

        transform.rotation = cameraRotation;
    }
}