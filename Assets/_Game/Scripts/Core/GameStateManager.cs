using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Gameplay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.Gameplay:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
                break;

            case GameState.Inventory:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
                break;

            case GameState.Dialogue:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 1f;
                break;
        }

        Debug.Log("GameState zmieniony na: " + newState);
    }

    public bool IsGameplay()
    {
        return CurrentState == GameState.Gameplay;
    }

    public bool IsInventory()
    {
        return CurrentState == GameState.Inventory;
    }

    public bool IsPaused()
    {
        return CurrentState == GameState.Paused;
    }

    public bool IsDialogue()
    {
        return CurrentState == GameState.Dialogue;
    }
}