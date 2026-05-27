using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TMP_Text cooldownText;

    private float cooldownDuration;
    private float cooldownTimer;
    private bool isCoolingDown;

    private void Awake()
    {
        SetReady();
    }

    private void Update()
    {
        if (!isCoolingDown)
            return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            SetReady();
            return;
        }

        float fillAmount = cooldownTimer / cooldownDuration;

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = fillAmount;

        if (cooldownText != null)
            cooldownText.text = Mathf.CeilToInt(cooldownTimer).ToString();
    }

    public void StartCooldown(float duration)
    {
        cooldownDuration = duration;
        cooldownTimer = duration;
        isCoolingDown = true;

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 1f;

        if (cooldownText != null)
            cooldownText.text = Mathf.CeilToInt(duration).ToString();
    }

    private void SetReady()
    {
        isCoolingDown = false;
        cooldownTimer = 0f;

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f;

        if (cooldownText != null)
            cooldownText.text = "";
    }
}