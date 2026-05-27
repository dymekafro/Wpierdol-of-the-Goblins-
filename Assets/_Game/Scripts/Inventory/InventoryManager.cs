using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("UI")]
    public GameObject inventoryPanel;
    public TextMeshProUGUI inventoryItemsText;
    public TextMeshProUGUI inventoryStatsText;

    private List<ItemData> items = new List<ItemData>();
    private bool inventoryOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        RefreshInventoryUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Naciśnięto 1 - próba założenia pierwszego przedmiotu.");
            EquipItemByIndex(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Naciśnięto 2 - próba założenia drugiego przedmiotu.");
            EquipItemByIndex(1);
        }
    }

    public void AddItem(ItemData item)
    {
        items.Add(item);

        Debug.Log("Dodano do ekwipunku: " + item.itemName);
        Debug.Log("Liczba przedmiotów w ekwipunku: " + items.Count);

        RefreshInventoryUI();
    }

    public List<ItemData> GetItems()
    {
        return items;
    }

    public void EquipItemByIndex(int index)
    {
        Debug.Log("EquipItemByIndex uruchomione. Index: " + index);

        if (index < 0 || index >= items.Count)
        {
            Debug.Log("Nie ma przedmiotu pod numerem: " + (index + 1));
            return;
        }

        ItemData item = items[index];

        Debug.Log("Przedmiot znaleziony: " + item.itemName + " | Typ: " + item.itemType);

        if (EquipmentManager.Instance == null)
        {
            Debug.LogError("Brak EquipmentManager w scenie.");
            return;
        }

        Debug.Log("EquipmentManager znaleziony. Próba założenia przedmiotu.");

        EquipmentManager.Instance.EquipItem(item);

        RefreshInventoryUI();
    }

    private void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(inventoryOpen);
        }

        if (GameStateManager.Instance != null)
        {
            if (inventoryOpen)
            {
                GameStateManager.Instance.SetState(GameState.Inventory);
            }
            else
            {
                GameStateManager.Instance.SetState(GameState.Gameplay);
            }
        }

        RefreshInventoryUI();
    }

    private void RefreshInventoryUI()
    {
        RefreshStatsUI();

        if (inventoryItemsText == null) return;

        if (items.Count == 0)
        {
            inventoryItemsText.text = "Brak przedmiotów";
            return;
        }

        string text = "";

        for (int i = 0; i < items.Count; i++)
        {
            ItemData item = items[i];

            text += (i + 1) + ". " + item.itemName;

            if (item.itemType == ItemType.Weapon)
            {
                text += " | Broń | Obrażenia +" + item.damageBonus;
            }
            else if (item.itemType == ItemType.Armor)
            {
                text += " | Zbroja | Pancerz +" + item.armorBonus;
            }
            else if (item.itemType == ItemType.Shield)
            {
                text += " | Tarcza | Pancerz +" + item.armorBonus;
            }
            else
            {
                text += " | " + item.itemType;
            }

            text += "\n";
        }

        inventoryItemsText.text = text;
    }

    private void RefreshStatsUI()
    {
        if (inventoryStatsText == null) return;

        if (PlayerStats.Instance == null)
        {
            inventoryStatsText.text = "Statystyki:\nBrak PlayerStats";
            return;
        }

        PlayerStats.Instance.CalculateStats();

        inventoryStatsText.text =
            "Statystyki:\n" +
            "Obrażenia: " + PlayerStats.Instance.totalDamage + "\n" +
            "Pancerz: " + PlayerStats.Instance.totalArmor + "\n" +
            "Zdrowie: " + PlayerStats.Instance.currentHealth + "/" + PlayerStats.Instance.maxHealth;
    }
}