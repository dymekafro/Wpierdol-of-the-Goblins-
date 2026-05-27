using UnityEngine;

public enum PlayerVisualId
{
    Warrior,
    Rogue,
    Mage,
    Peasant
}

public class PlayerVisualManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visualRoot;

    [Header("Visual Prefabs")]
    [SerializeField] private GameObject warriorPrefab;
    [SerializeField] private GameObject roguePrefab;
    [SerializeField] private GameObject magePrefab;
    [SerializeField] private GameObject peasantPrefab;

    [Header("Settings")]
    [SerializeField] private PlayerVisualId startingVisual = PlayerVisualId.Warrior;
    [SerializeField] private bool createPlaceholderIfMissing = true;

    private GameObject currentVisualInstance;
    private PlayerVisualId currentVisualId;

    public PlayerVisualId CurrentVisualId => currentVisualId;
    public GameObject CurrentVisualInstance => currentVisualInstance;

    private void Awake()
    {
        if (visualRoot == null)
        {
            GameObject root = new GameObject("VisualRoot");
            root.transform.SetParent(transform);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            visualRoot = root.transform;
        }

        SetVisual(startingVisual);
    }

    public void SetVisual(PlayerVisualId visualId)
    {
        ClearCurrentVisual();

        currentVisualId = visualId;

        GameObject prefab = GetPrefabById(visualId);

        if (prefab != null)
        {
            currentVisualInstance = Instantiate(prefab, visualRoot);
            currentVisualInstance.transform.localPosition = Vector3.zero;
            currentVisualInstance.transform.localRotation = Quaternion.identity;
            currentVisualInstance.transform.localScale = Vector3.one;
        }
        else if (createPlaceholderIfMissing)
        {
            currentVisualInstance = CreatePlaceholderVisual(visualId);
        }
        else
        {
            Debug.LogWarning($"No visual prefab assigned for {visualId}.", this);
        }
    }

    public void RotateVisualTowards(Vector3 worldDirection)
    {
        worldDirection.y = 0f;

        if (worldDirection.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(worldDirection.normalized);
        visualRoot.rotation = targetRotation;
    }

    public void SetVisualRotation(Quaternion rotation)
    {
        visualRoot.rotation = rotation;
    }

    public void ResetVisualLocalTransform()
    {
        if (visualRoot == null)
            return;

        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;
    }

    private GameObject GetPrefabById(PlayerVisualId visualId)
    {
        return visualId switch
        {
            PlayerVisualId.Warrior => warriorPrefab,
            PlayerVisualId.Rogue => roguePrefab,
            PlayerVisualId.Mage => magePrefab,
            PlayerVisualId.Peasant => peasantPrefab,
            _ => null
        };
    }

    private void ClearCurrentVisual()
    {
        if (currentVisualInstance != null)
        {
            Destroy(currentVisualInstance);
            currentVisualInstance = null;
        }
    }

    private GameObject CreatePlaceholderVisual(PlayerVisualId visualId)
    {
        GameObject root = new GameObject($"{visualId}_Placeholder");
        root.transform.SetParent(visualRoot);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        Color color = GetPlaceholderColor(visualId);

        CreateBody(root.transform, color);
        CreateHead(root.transform, color);
        CreateWeaponOrMarker(root.transform, visualId);

        return root;
    }

    private void CreateBody(Transform parent, Color color)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(parent);
        body.transform.localPosition = new Vector3(0f, 1f, 0f);
        body.transform.localRotation = Quaternion.identity;
        body.transform.localScale = new Vector3(0.7f, 1f, 0.7f);

        ApplyColor(body, color);
    }

    private void CreateHead(Transform parent, Color color)
    {
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(parent);
        head.transform.localPosition = new Vector3(0f, 2.1f, 0f);
        head.transform.localRotation = Quaternion.identity;
        head.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);

        ApplyColor(head, color * 1.15f);
    }

    private void CreateWeaponOrMarker(Transform parent, PlayerVisualId visualId)
    {
        switch (visualId)
        {
            case PlayerVisualId.Warrior:
                CreateSword(parent);
                break;

            case PlayerVisualId.Rogue:
                CreateDagger(parent);
                break;

            case PlayerVisualId.Mage:
                CreateStaff(parent);
                break;

            case PlayerVisualId.Peasant:
                CreateBackpack(parent);
                break;
        }
    }

    private void CreateSword(Transform parent)
    {
        GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sword.name = "Sword";
        sword.transform.SetParent(parent);
        sword.transform.localPosition = new Vector3(0.55f, 1.15f, 0.35f);
        sword.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
        sword.transform.localScale = new Vector3(0.12f, 1.1f, 0.12f);

        ApplyColor(sword, Color.gray);
    }

    private void CreateDagger(Transform parent)
    {
        GameObject dagger = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dagger.name = "Dagger";
        dagger.transform.SetParent(parent);
        dagger.transform.localPosition = new Vector3(0.5f, 1f, 0.35f);
        dagger.transform.localRotation = Quaternion.Euler(0f, 0f, -35f);
        dagger.transform.localScale = new Vector3(0.1f, 0.55f, 0.1f);

        ApplyColor(dagger, Color.gray);
    }

    private void CreateStaff(Transform parent)
    {
        GameObject staff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        staff.name = "Staff";
        staff.transform.SetParent(parent);
        staff.transform.localPosition = new Vector3(0.6f, 1.15f, 0.25f);
        staff.transform.localRotation = Quaternion.Euler(0f, 0f, 12f);
        staff.transform.localScale = new Vector3(0.06f, 0.85f, 0.06f);

        ApplyColor(staff, new Color(0.45f, 0.25f, 0.1f));

        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "MagicOrb";
        orb.transform.SetParent(parent);
        orb.transform.localPosition = new Vector3(0.78f, 1.95f, 0.25f);
        orb.transform.localRotation = Quaternion.identity;
        orb.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);

        ApplyColor(orb, Color.cyan);
    }

    private void CreateBackpack(Transform parent)
    {
        GameObject backpack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backpack.name = "Backpack";
        backpack.transform.SetParent(parent);
        backpack.transform.localPosition = new Vector3(0f, 1.15f, -0.45f);
        backpack.transform.localRotation = Quaternion.identity;
        backpack.transform.localScale = new Vector3(0.55f, 0.75f, 0.2f);

        ApplyColor(backpack, new Color(0.35f, 0.2f, 0.1f));
    }

    private Color GetPlaceholderColor(PlayerVisualId visualId)
    {
        return visualId switch
        {
            PlayerVisualId.Warrior => new Color(0.65f, 0.15f, 0.15f),
            PlayerVisualId.Rogue => new Color(0.15f, 0.45f, 0.2f),
            PlayerVisualId.Mage => new Color(0.2f, 0.25f, 0.7f),
            PlayerVisualId.Peasant => new Color(0.55f, 0.42f, 0.25f),
            _ => Color.white
        };
    }

    private void ApplyColor(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();

        if (renderer == null)
            return;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        Material material = new Material(shader);
        material.color = color;

        renderer.material = material;
    }
}