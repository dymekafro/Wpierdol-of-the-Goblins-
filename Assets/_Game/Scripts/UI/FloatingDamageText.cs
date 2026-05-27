using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshPro textMesh;

    [Header("Motion")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float floatSpeed = 1.2f;
    [SerializeField] private float horizontalDrift = 0.25f;

    [Header("Scale")]
    [SerializeField] private float startScale = 1f;
    [SerializeField] private float endScale = 1.25f;

    private float timer;
    private Color startColor;
    private Vector3 driftDirection;

    private Camera mainCamera;

    private void Awake()
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        mainCamera = Camera.main;

        if (textMesh != null)
            startColor = textMesh.color;

        driftDirection = new Vector3(
            Random.Range(-horizontalDrift, horizontalDrift),
            0f,
            Random.Range(-horizontalDrift, horizontalDrift)
        );
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float normalizedTime = timer / lifetime;
        normalizedTime = Mathf.Clamp01(normalizedTime);

        MoveText();
        ScaleText(normalizedTime);
        FadeText(normalizedTime);
        FaceCamera();

        if (timer >= lifetime)
            Destroy(gameObject);
    }

    public void Setup(int damage)
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        if (textMesh != null)
            textMesh.text = damage.ToString();
    }

    private void MoveText()
    {
        Vector3 movement = Vector3.up * floatSpeed;
        movement += driftDirection;

        transform.position += movement * Time.deltaTime;
    }

    private void ScaleText(float normalizedTime)
    {
        float scale = Mathf.Lerp(startScale, endScale, normalizedTime);
        transform.localScale = Vector3.one * scale;
    }

    private void FadeText(float normalizedTime)
    {
        if (textMesh == null)
            return;

        Color color = startColor;
        color.a = Mathf.Lerp(1f, 0f, normalizedTime);
        textMesh.color = color;
    }

    private void FaceCamera()
    {
        if (mainCamera == null)
            return;

        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }
}