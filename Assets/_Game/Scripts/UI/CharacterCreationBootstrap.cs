using UnityEngine;

/// <summary>
/// Główny punkt startowy sceny CharacterCreation.
/// Jeżeli w projekcie masz już CharacterCreationBootstrap.cs, scal tę klasę z istniejącą:
/// zostaw jeden bootstrap i dopilnuj, żeby tworzył lub aktywował CharacterCreationUI.
/// </summary>
public class CharacterCreationBootstrap : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameplaySceneName = "World";

    [Header("UI")]
    [SerializeField] private CharacterCreationUI characterCreationUI;
    [SerializeField] private bool buildUiIfMissing = true;

    private void Awake()
    {
        if (characterCreationUI == null)
            characterCreationUI = FindAnyObjectByType<CharacterCreationUI>();

        if (characterCreationUI == null && buildUiIfMissing)
        {
            GameObject uiObject = new GameObject("CharacterCreationUI");
            characterCreationUI = uiObject.AddComponent<CharacterCreationUI>();
        }

        if (characterCreationUI == null)
        {
            Debug.LogError("[CharacterCreationBootstrap] Nie znaleziono CharacterCreationUI.");
            return;
        }

        characterCreationUI.Initialize(mainMenuSceneName, gameplaySceneName);
    }
}
