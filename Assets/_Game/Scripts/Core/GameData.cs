using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    public CharacterType selectedCharacter;
    public PlayerAttributes playerAttributes = new PlayerAttributes();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectCharacter(CharacterType characterType)
    {
        selectedCharacter = characterType;

        playerAttributes = new PlayerAttributes();
        playerAttributes.ApplyCharacterBonus(characterType);
    }
}