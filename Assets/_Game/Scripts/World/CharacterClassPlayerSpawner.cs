using UnityEngine;

public class CharacterClassPlayerSpawner : MonoBehaviour
{
    [Header("Existing Scene Player")]
    [SerializeField] private string existingPlayerName = "Player_Invector";
    [SerializeField] private bool destroyExistingPlayer = true;

    [Header("Player Class Prefabs")]
    [SerializeField] private GameObject warriorPrefab;
    [SerializeField] private GameObject dwarfPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector3 fallbackSpawnPosition = Vector3.zero;
    [SerializeField] private Vector3 fallbackSpawnRotation = Vector3.zero;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private void Start()
    {
        SpawnSelectedPlayer();
    }

    private void SpawnSelectedPlayer()
    {
        CharacterCreationData characterData = CharacterCreationSession.CurrentCharacter;

        if (characterData == null)
        {
            Log("Brak danych z kreatora. Zostawiam istniejącego Player_Invector.");
            return;
        }

        GameObject prefabToSpawn = GetPrefabForCharacter(characterData);

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("[CharacterClassPlayerSpawner] Brak prefabu dla: " + characterData.selectedClass);
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

        GameObject spawnedPlayer = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        spawnedPlayer.name = prefabToSpawn.name;
        spawnedPlayer.tag = "Player";

        Log("Utworzono gracza: " + spawnedPlayer.name);
    }

    private GameObject GetPrefabForCharacter(CharacterCreationData characterData)
    {
        if (characterData.selectedClass == CharacterClassType.Dwarf)
            return dwarfPrefab != null ? dwarfPrefab : warriorPrefab;

        if (characterData.selectedClass == CharacterClassType.Warrior)
            return warriorPrefab;

        return warriorPrefab;
    }

    private void Log(string message)
    {
        if (!logDebug)
            return;

        Debug.Log("[CharacterClassPlayerSpawner] " + message);
    }
}