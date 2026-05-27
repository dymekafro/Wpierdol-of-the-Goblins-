using TMPro;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public TextMeshProUGUI interactionPromptText;

    private bool playerInRange = false;
    private bool isOpened = false;

    private ItemData swordInside;
    private ItemData shieldInside;

    private void Start()
    {
        swordInside = new ItemData(
            "Zardzewiały Miecz",
            ItemType.Weapon,
            5,
            0,
            3
        );

        shieldInside = new ItemData(
            "Drewniana Tarcza",
            ItemType.Shield,
            0,
            3,
            1
        );

        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsGameplay())
        {
            return;
        }

        if (playerInRange && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("Brak InventoryManager w scenie World.");
            return;
        }

        InventoryManager.Instance.AddItem(swordInside);
        InventoryManager.Instance.AddItem(shieldInside);

        isOpened = true;

        if (interactionPromptText != null)
        {
            interactionPromptText.text = "Skrzynka pusta";
            interactionPromptText.gameObject.SetActive(true);
        }

        Debug.Log("Otworzono skrzynkę.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (!isOpened && interactionPromptText != null)
        {
            interactionPromptText.text = "Naciśnij E, aby otworzyć";
            interactionPromptText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }
}