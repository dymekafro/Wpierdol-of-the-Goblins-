using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bezpieczny router dla przycisku „Nowa Gra”.
/// Możesz podpiąć go bezpośrednio w Inspectorze do Button.onClick,
/// albo wywołać LoadCharacterCreation() z istniejącego MainMenuBootstrap.cs.
/// </summary>
public class MainMenuNewGameRouter : MonoBehaviour
{
    [SerializeField] private string characterCreationSceneName = "CharacterCreation";

    public void LoadCharacterCreation()
    {
        if (string.IsNullOrWhiteSpace(characterCreationSceneName))
        {
            Debug.LogError("[MainMenuNewGameRouter] Brak nazwy sceny tworzenia postaci.");
            return;
        }

        SceneManager.LoadScene(characterCreationSceneName);
    }
}
