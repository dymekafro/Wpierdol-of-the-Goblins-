using System.Collections;
using UnityEngine;

public class EnemyHitReaction : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.12f;

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 0.45f;
    [SerializeField] private float knockbackDuration = 0.12f;
    [SerializeField] private bool lockYPosition = true;

    [Header("References")]
    [SerializeField] private Renderer[] renderers;

    private Color[] originalColors;
    private Coroutine flashCoroutine;
    private Coroutine knockbackCoroutine;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        CacheOriginalColors();
    }

    public void PlayHitReaction(Vector3 hitDirection)
    {
        PlayFlash();
        PlayKnockback(hitDirection);
    }

    public void PlayFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    public void PlayKnockback(Vector3 hitDirection)
    {
        if (knockbackCoroutine != null)
            StopCoroutine(knockbackCoroutine);

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(hitDirection));
    }

    private void CacheOriginalColors()
    {
        if (renderers == null)
            return;

        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || renderers[i].material == null)
                continue;

            originalColors[i] = renderers[i].material.color;
        }
    }

    private IEnumerator FlashRoutine()
    {
        SetRenderersColor(flashColor);

        yield return new WaitForSeconds(flashDuration);

        RestoreOriginalColors();

        flashCoroutine = null;
    }

    private IEnumerator KnockbackRoutine(Vector3 hitDirection)
    {
        hitDirection.y = 0f;

        if (hitDirection.sqrMagnitude <= 0.001f)
            hitDirection = -transform.forward;

        hitDirection.Normalize();

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + hitDirection * knockbackDistance;

        if (lockYPosition)
            targetPosition.y = startPosition.y;

        float timer = 0f;

        while (timer < knockbackDuration)
        {
            timer += Time.deltaTime;

            float t = timer / knockbackDuration;
            t = Mathf.Clamp01(t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        transform.position = targetPosition;
        knockbackCoroutine = null;
    }

    private void SetRenderersColor(Color color)
    {
        if (renderers == null)
            return;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || renderer.material == null)
                continue;

            renderer.material.color = color;
        }
    }

    private void RestoreOriginalColors()
    {
        if (renderers == null || originalColors == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || renderers[i].material == null)
                continue;

            renderers[i].material.color = originalColors[i];
        }
    }
}