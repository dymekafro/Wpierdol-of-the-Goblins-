using UnityEngine;
using UnityEngine.InputSystem;
using WPG.Core;

namespace WPG.Player
{
    // Klasyczna kamera 3rd person za plecami z mouse look (yaw/pitch).
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 5.5f;
        public float height = 2.2f;
        // Bazowa czułość; mnożona przez SettingsManager.MouseSensitivity (1.0 = neutralne).
        public float mouseSensitivity = 180f;
        public float pitchMin = -25f;
        public float pitchMax = 60f;
        public float followSmooth = 12f;

        public float Yaw { get; private set; }
        public float Pitch { get; private set; } = 15f;

        private void LateUpdate()
        {
            if (target == null) return;

            if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                float sensMul = 1f;
                bool invertY = false;
                if (SettingsManager.Instance != null)
                {
                    sensMul = SettingsManager.Instance.MouseSensitivity;
                    invertY = SettingsManager.Instance.InvertY;
                }
                Yaw += delta.x * mouseSensitivity * sensMul * Time.unscaledDeltaTime * 0.05f;
                float pitchDelta = delta.y * mouseSensitivity * sensMul * Time.unscaledDeltaTime * 0.05f;
                Pitch += (invertY ? pitchDelta : -pitchDelta);
                Pitch = Mathf.Clamp(Pitch, pitchMin, pitchMax);
            }

            Quaternion rotation = Quaternion.Euler(Pitch, Yaw, 0f);
            Vector3 desiredPos = target.position + Vector3.up * height - rotation * Vector3.forward * distance;

            // Raycast omijający kolizje gracza
            RaycastHit[] hits = Physics.RaycastAll(target.position + Vector3.up * height,
                (desiredPos - (target.position + Vector3.up * height)).normalized,
                Vector3.Distance(target.position + Vector3.up * height, desiredPos));
            float closestDist = float.MaxValue;
            Vector3 closestPoint = desiredPos;
            Vector3 closestNormal = Vector3.zero;
            bool blocked = false;
            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (hit.collider.isTrigger) continue;
                // pomiń gracza i jego dzieci
                if (hit.collider.transform == target || hit.collider.transform.IsChildOf(target)) continue;
                if (hit.distance < closestDist)
                {
                    closestDist = hit.distance;
                    closestPoint = hit.point;
                    closestNormal = hit.normal;
                    blocked = true;
                }
            }
            if (blocked) desiredPos = closestPoint + closestNormal * 0.2f;

            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * followSmooth);
            transform.rotation = rotation;
        }

        public Vector3 GetCameraForwardFlat()
        {
            Vector3 fwd = transform.forward;
            fwd.y = 0f;
            return fwd.sqrMagnitude < 0.001f ? Vector3.forward : fwd.normalized;
        }
    }
}
