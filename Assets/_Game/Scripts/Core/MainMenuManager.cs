using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("CharacterSelection");
    }

    public void LoadGame()
    {
        Debug.Log("Wczytaj grę: system zapisu dodamy później.");
    }

    public void OpenOptions()
    {
        Debug.Log("Opcje: panel opcji dodamy później.");
    }

    public void ExitGame()
    {
        Debug.Log("Wyjście z gry.");
        Application.Quit();
    }
}