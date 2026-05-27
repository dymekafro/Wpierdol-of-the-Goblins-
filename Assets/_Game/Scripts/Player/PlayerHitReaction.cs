using System.Collections;
using UnityEngine;

public class PlayerHitReaction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerVisualManager visualManager;

    [Header("Flash")]
    [SerializeField] private Color flashColor = Color.yellow;
    [SerializeField] private float flashDuration = 0.12f;

    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (visualManager == null)
            visualManager = GetComponent<PlayerVisualManager>();
    }

    public void PlayHitReaction()
    {
        PlayFlash();
    }

    private void PlayFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        Renderer[] renderers = GetCurrentVisualRenderers();

        if (renderers == null || renderers.Length == 0)
            yield break;

        Color[] originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || renderers[i].material == null)
                continue;

            originalColors[i] = renderers[i].material.color;
            renderers[i].material.color = flashColor;
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || renderers[i].material == null)
                continue;

            renderers[i].material.color = originalColors[i];
        }

        flashCoroutine = null;
    }

    private Renderer[] GetCurrentVisualRenderers()
    {
        if (visualManager == null)
            return GetComponentsInChildren<Renderer>();

        if (visualManager.CurrentVisualInstance == null)
            return GetComponentsInChildren<Renderer>();

        return visualManager.CurrentVisualInstance.GetComponentsInChildren<Renderer>();
    }
}