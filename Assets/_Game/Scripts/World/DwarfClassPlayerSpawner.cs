using UnityEngine;

public class DwarfClassPlayerSpawner : MonoBehaviour
{
    [Header("Existing Scene Player")]
    [SerializeField] private string existingPlayerName = "Player_Invector";
    [SerializeField] private bool destroyExistingPlayer = true;

    [Header("Dwarf Prefab")]
    [SerializeField] private GameObject dwarfPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector3 fallbackSpawnPosition = Vector3.zero;
    [SerializeField] private Vector3 fallbackSpawnRotation = Vector3.zero;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private void Start()
    {
        SpawnDwarfIfSelected();
    }

    private void SpawnDwarfIfSelected()
    {
        CharacterCreationData characterData = CharacterCreationSession.CurrentCharacter;

        if (characterData == null)
        {
            Log("Brak danych z kreatora. Zostawiam Player_Invector.");
            return;
        }

        if (characterData.selectedClass != CharacterClassType.Dwarf)
        {
            Log("Wybrana klasa to " + characterData.selectedClass + ". Zostawiam Player_Invector.");
            return;
        }

        if (dwarfPrefab == null)
        {
            Debug.LogWarning("[DwarfClassPlayerSpawner] Wybrano Krasnoluda, ale pole Dwarf Prefab jest puste.");
            return;
        }

        GameObject existingPlayer = GameObject.Find(existingPlayerName);

        Vector3 spawnPosition = fallbackSpawnPosition;
        Quaternion spawnRotation = Quaternion.Euler(fallbackSpawnRotation);

        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
            spawnRotation = spawnPoint.rotation;
        }
        else if (existingPlayer != null)
        {
            spawnPosition = existingPlayer.transform.position;
            spawnRotation = existingPlayer.transform.rotation;
        }

        if (existingPlayer != null && destroyExistingPlayer)
            Destroy(existingPlayer);

        GameObject dwarfPlayer = Instantiate(dwarfPrefab, spawnPosition, spawnRotation);
        dwarfPlayer.name = "Player_Dwarf_" + dwarfPrefab.name;

        if (CanUsePlayerTag())
            dwarfPlayer.tag = "Player";

        ApplyCreatedCharacterData(dwarfPlayer, characterData);

        Log("Utworzono krasnoluda: " + dwarfPlayer.name);
    }

    private void ApplyCreatedCharacterData(GameObject player, CharacterCreationData characterData)
    {
        // Tu później podepniemy realne statystyki do komponentów gracza,
        // gdy ustalimy strukturę prefabu Meshtint.
        Log("Dane postaci: " +
            characterData.characterName + " / " +
            characterData.selectedClass + " / " +
            characterData.bodyType);
    }

    private bool CanUsePlayerTag()
    {
        try
        {
            GameObject test = new GameObject("TagTest");
            test.tag = "Player";
            Destroy(test);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void Log(string message)
    {
        if (!logDebug)
            return;

        Debug.Log("[DwarfClassPlayerSpawner] " + message);
    }
}
