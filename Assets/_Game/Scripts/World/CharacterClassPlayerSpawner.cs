using System;
using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;
using WPG.Character;
using WPG.Core;
using WPG.Items;
using WPG.Player;
using WPG.UI;

/// <summary>
/// Podmienia gracza w scenie świata na prefab odpowiadający klasie wybranej w kreatorze.
/// Po podmianie inicjalizuje statystyki z kreatora oraz przepina HUD, ekwipunek,
/// UI bazy i kamerę Invectora na nowo utworzoną postać.
///
/// Mapowanie klasa -> prefab:
/// 1) lista <see cref="classPrefabs"/> (preferowana, ustawiana w Inspectorze),
/// 2) pola legacy <see cref="warriorPrefab"/> / <see cref="dwarfPrefab"/> (kompatybilność wsteczna),
/// 3) pierwszy dostępny prefab jako fallback.
/// </summary>
public class CharacterClassPlayerSpawner : MonoBehaviour
{
    [Serializable]
    public class ClassPrefabEntry
    {
        public CharacterClassType classType;
        public GameObject prefab;
    }

    [Header("Existing Scene Player")]
    [SerializeField] private string existingPlayerName = "Player_Invector";
    [SerializeField] private bool destroyExistingPlayer = true;

    [Header("Class -> Prefab (preferowane)")]
    [Tooltip("Mapowanie klasy postaci na prefab gracza. Sprawdzane jako pierwsze.")]
    [SerializeField] private List<ClassPrefabEntry> classPrefabs = new List<ClassPrefabEntry>();

    [Header("Legacy Prefab Fields (fallback)")]
    [SerializeField] private GameObject warriorPrefab;
    [SerializeField] private GameObject dwarfPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector3 fallbackSpawnPosition = Vector3.zero;
    [SerializeField] private Vector3 fallbackSpawnRotation = Vector3.zero;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    /// <summary>
    /// Klasy bez własnego modelu współdzielą model klasy pokrewnej. Gdy w mapie/legacy
    /// nie ma jawnego wpisu, próbujemy prefabu aliasu (czysto runtime, bez ładowania assetów).
    /// </summary>
    private static readonly Dictionary<CharacterClassType, CharacterClassType> ClassAliases =
        new Dictionary<CharacterClassType, CharacterClassType>
        {
            { CharacterClassType.Archer, CharacterClassType.Ranger },
            { CharacterClassType.Wanderer, CharacterClassType.Ranger },
            { CharacterClassType.Rogue, CharacterClassType.Knight },
            { CharacterClassType.Mage, CharacterClassType.Druid },
        };

    private void Start()
    {
        SpawnSelectedPlayer();
    }

    private void SpawnSelectedPlayer()
    {
        CharacterCreationData characterData = CharacterCreationSession.CurrentCharacter;

        if (characterData == null)
        {
            Log("Brak danych z kreatora. Zostawiam istniejącego " + existingPlayerName + ".");
            return;
        }

        GameObject prefabToSpawn = GetPrefabForCharacter(characterData);

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("[CharacterClassPlayerSpawner] Brak prefabu dla klasy: " + characterData.selectedClass);
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

        if (CanUsePlayerTag())
            spawnedPlayer.tag = "Player";

        InitializeSpawnedPlayer(spawnedPlayer, characterData);

        Log("Utworzono gracza: " + spawnedPlayer.name + " (klasa: " + characterData.selectedClass + ")");
    }

    private GameObject GetPrefabForCharacter(CharacterCreationData characterData)
    {
        CharacterClassType selected = characterData.selectedClass;

        // 1) Jawne mapowanie z listy (dla wybranej klasy).
        GameObject direct = ResolveFromList(selected);
        if (direct != null)
            return direct;

        // 2) Pola legacy.
        if (selected == CharacterClassType.Dwarf && dwarfPrefab != null)
            return dwarfPrefab;

        if (selected == CharacterClassType.Warrior && warriorPrefab != null)
            return warriorPrefab;

        // 3) Alias klasy pokrewnej (np. Łucznik→Łowca, Mag→Druid) — jeśli wybrana klasa
        //    nie ma własnego prefabu, użyj modelu klasy spokrewnionej z listy/legacy.
        if (ClassAliases.TryGetValue(selected, out CharacterClassType alias))
        {
            GameObject aliased = ResolveFromList(alias);
            if (aliased != null)
                return aliased;

            if (alias == CharacterClassType.Dwarf && dwarfPrefab != null)
                return dwarfPrefab;
            if (alias == CharacterClassType.Warrior && warriorPrefab != null)
                return warriorPrefab;
        }

        // 4) Fallback: pierwszy dostępny prefab z listy.
        if (classPrefabs != null)
        {
            foreach (ClassPrefabEntry entry in classPrefabs)
            {
                if (entry != null && entry.prefab != null)
                    return entry.prefab;
            }
        }

        // 5) Fallback legacy.
        if (warriorPrefab != null)
            return warriorPrefab;

        return dwarfPrefab;
    }

    private GameObject ResolveFromList(CharacterClassType type)
    {
        if (classPrefabs == null)
            return null;

        foreach (ClassPrefabEntry entry in classPrefabs)
        {
            if (entry != null && entry.prefab != null && entry.classType == type)
                return entry.prefab;
        }

        return null;
    }

    /// <summary>
    /// Po podmianie postaci: ustaw statystyki z kreatora i przepnij HUD/ekwipunek/UI/kamerę.
    /// Wszystko defensywnie — brak komponentu skutkuje pominięciem, nie wyjątkiem.
    /// </summary>
    private void InitializeSpawnedPlayer(GameObject player, CharacterCreationData characterData)
    {
        PlayerAttributes attrs = PlayerAttributes.FromCreatedCharacter(characterData);

        GameManager gm = GameManager.Instance;
        if (gm != null)
            gm.attributes = attrs;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
            stats.Init(attrs);

        PlayerCombat combat = player.GetComponent<PlayerCombat>();

        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory == null)
            inventory = player.AddComponent<Inventory>();

        if (stats != null)
        {
            inventory.Bind(stats, combat);
            if (inventory.IsEmpty())
                inventory.GiveStarterItems();
        }

        BaseUIManager baseUI = player.GetComponent<BaseUIManager>();
        if (baseUI == null)
            baseUI = player.AddComponent<BaseUIManager>();
        if (stats != null)
            baseUI.Initialize(inventory, stats, combat);

        PlayerHUD hud = FindAnyObjectByType<PlayerHUD>();
        if (hud != null && stats != null)
            hud.Bind(stats, combat);

        vThirdPersonCamera invectorCamera = FindAnyObjectByType<vThirdPersonCamera>();
        if (invectorCamera != null)
            invectorCamera.SetMainTarget(player.transform);

        Log("Statystyki zastosowane: " + characterData.characterName +
            " | STR " + attrs.strength + " DEX " + attrs.dexterity +
            " INT " + attrs.intelligence + " END " + attrs.endurance +
            " | HP " + attrs.MaxHealth);
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

        Debug.Log("[CharacterClassPlayerSpawner] " + message);
    }
}
