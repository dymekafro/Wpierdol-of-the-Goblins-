using System.Collections;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    [Header("Equipment Slots")]
    public Transform rightHandSlot;
    public Transform leftHandSlot;

    private GameObject equippedWeaponObject;
    private GameObject equippedShieldObject;

    private ItemData equippedWeapon;
    private ItemData equippedShield;

    private Coroutine swordSwingCoroutine;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("EquipmentManager aktywny.");
    }

    public void EquipItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("Próba założenia pustego przedmiotu.");
            return;
        }

        Debug.Log("Próba założenia: " + item.itemName + " | Typ: " + item.itemType);

        if (item.itemType == ItemType.Weapon)
        {
            EquipWeapon(item);
        }
        else if (item.itemType == ItemType.Shield)
        {
            EquipShield(item);
        }
        else
        {
            Debug.Log("Ten typ przedmiotu nie ma jeszcze obsługi zakładania: " + item.itemType);
        }
    }

    private void EquipWeapon(ItemData weapon)
    {
        equippedWeapon = weapon;

        if (equippedWeaponObject != null)
        {
            Destroy(equippedWeaponObject);
        }

        if (rightHandSlot == null)
        {
            Debug.LogError("Brak przypisanego RightHandSlot w EquipmentManager.");
            return;
        }

        equippedWeaponObject = CreateDebugSword();

        equippedWeaponObject.transform.SetParent(rightHandSlot);
        equippedWeaponObject.transform.localPosition = Vector3.zero;
        equippedWeaponObject.transform.localRotation = Quaternion.Euler(0f, 0f, -35f);
        equippedWeaponObject.transform.localScale = Vector3.one;

        Debug.Log("Założono broń: " + weapon.itemName);

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.CalculateStats();
        }
    }

    private void EquipShield(ItemData shield)
    {
        equippedShield = shield;

        if (equippedShieldObject != null)
        {
            Destroy(equippedShieldObject);
        }

        if (leftHandSlot == null)
        {
            Debug.LogError("Brak przypisanego LeftHandSlot w EquipmentManager.");
            return;
        }

        equippedShieldObject = CreateDebugShield();

        equippedShieldObject.transform.SetParent(leftHandSlot);
        equippedShieldObject.transform.localPosition = Vector3.zero;
        equippedShieldObject.transform.localRotation = Quaternion.Euler(0f, 0f, -54f);
        equippedShieldObject.transform.localScale = Vector3.one;

        Debug.Log("Założono tarczę: " + shield.itemName);

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.CalculateStats();
        }
    }

    private GameObject CreateDebugSword()
    {
        GameObject swordRoot = new GameObject("DebugSword_Runtime");

        GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blade.name = "Blade";
        blade.transform.SetParent(swordRoot.transform);
        blade.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        blade.transform.localRotation = Quaternion.identity;
        blade.transform.localScale = new Vector3(0.12f, 0.9f, 0.08f);

        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handle.name = "Handle";
        handle.transform.SetParent(swordRoot.transform);
        handle.transform.localPosition = new Vector3(0f, -0.15f, 0f);
        handle.transform.localRotation = Quaternion.identity;
        handle.transform.localScale = new Vector3(0.18f, 0.3f, 0.12f);

        GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        guard.name = "Guard";
        guard.transform.SetParent(swordRoot.transform);
        guard.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        guard.transform.localRotation = Quaternion.identity;
        guard.transform.localScale = new Vector3(0.55f, 0.08f, 0.12f);

        Destroy(blade.GetComponent<BoxCollider>());
        Destroy(handle.GetComponent<BoxCollider>());
        Destroy(guard.GetComponent<BoxCollider>());

        return swordRoot;
    }

    private GameObject CreateDebugShield()
    {
        GameObject shieldRoot = new GameObject("DebugShield_Runtime");

        GameObject shieldBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shieldBody.name = "ShieldBody";
        shieldBody.transform.SetParent(shieldRoot.transform);
        shieldBody.transform.localPosition = Vector3.zero;
        shieldBody.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        shieldBody.transform.localScale = new Vector3(0.5f, 0.08f, 0.7f);

        Destroy(shieldBody.GetComponent<CapsuleCollider>());

        return shieldRoot;
    }

    public ItemData GetEquippedWeapon()
    {
        return equippedWeapon;
    }

    public ItemData GetEquippedShield()
    {
        return equippedShield;
    }
    public void PlaySwordSwing()
    {
        if (equippedWeaponObject == null)
        {
            return;
        }

        if (swordSwingCoroutine != null)
        {
            StopCoroutine(swordSwingCoroutine);
        }

        swordSwingCoroutine = StartCoroutine(SwordSwingRoutine());
    }

    private IEnumerator SwordSwingRoutine()
    {
        Quaternion startRotation = Quaternion.Euler(0f, 0f, -35f);
        Quaternion swingRotation = Quaternion.Euler(80f, 0f, -120f);

        float swingTime = 0.12f;
        float returnTime = 0.18f;

        float timer = 0f;

        while (timer < swingTime)
        {
            timer += Time.deltaTime;
            float t = timer / swingTime;

            equippedWeaponObject.transform.localRotation = Quaternion.Slerp(startRotation, swingRotation, t);

            yield return null;
        }

        timer = 0f;

        while (timer < returnTime)
        {
            timer += Time.deltaTime;
            float t = timer / returnTime;

            equippedWeaponObject.transform.localRotation = Quaternion.Slerp(swingRotation, startRotation, t);

            yield return null;
        }

        equippedWeaponObject.transform.localRotation = startRotation;
        swordSwingCoroutine = null;
    }
}