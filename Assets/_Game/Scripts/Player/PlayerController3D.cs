using UnityEngine;

public class PlayerController3D : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 150f;
    public float gravity = -9.81f;
    public float jumpheight = 1.5f;

    private CharacterController controller;
    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        DebugPlayerData();
    }

    private void Update()
    {
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsGameplay())
        {
            return;
        }

        MovePlayer();
        RotatePlayer();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void MovePlayer()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * moveSpeed * Time.deltaTime);

        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Wciśnięto Space. isGrounded = " + isGrounded);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpheight * -2f * gravity);
            Debug.Log("Skok wykonany. velocity.y = " + velocity.y);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    private void RotatePlayer()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
    }

    private void DebugPlayerData()
    {
        if (GameData.Instance == null)
        {
            Debug.LogWarning("Brak GameData. Uruchom grę od sceny MainMenu.");
            return;
        }

        PlayerAttributes attr = GameData.Instance.playerAttributes;

        Debug.Log("Wybrana postać: " + GameData.Instance.selectedCharacter);

        Debug.Log(
            "Atrybuty: " +
            "Siła " + attr.strength +
            ", Zręczność " + attr.dexterity +
            ", Mana " + attr.mana +
            ", Inteligencja " + attr.intelligence +
            ", Wytrwałość " + attr.endurance +
            ", Charyzma " + attr.charisma
        );
    }
}