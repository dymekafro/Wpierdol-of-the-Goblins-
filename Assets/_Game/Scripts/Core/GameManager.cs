using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WPG.Character;
using WPG.Items;

namespace WPG.Core
{
    // Singleton trzymający stan między scenami.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public CharacterType characterType = CharacterType.Druid;
        public PlayerAttributes attributes = PlayerAttributes.CreateDruidBase();
        public SaveData pendingLoadData;     // ustawiamy przy Kontynuuj, używamy w World
        public bool isContinuing;            // true jeśli właśnie wczytaliśmy save
        public GameState currentState = GameState.MainMenu;

        // Stany obozów (campId -> state). Wypełniane przez WorldGenerator i GoblinCamp.
        public readonly Dictionary<string, CampState> campStates = new Dictionary<string, CampState>();
        public readonly HashSet<string> visitedPowerSites = new HashSet<string>();

        public static GameManager EnsureExists()
        {
            if (Instance != null) return Instance;

            var existing = FindAnyObjectByType<GameManager>();
            if (existing != null)
            {
                Instance = existing;
                return Instance;
            }

            var go = new GameObject("[GameManager]");
            Instance = go.AddComponent<GameManager>();
            DontDestroyOnLoad(go);
            return Instance;
        }

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

        public void StartNewGame()
        {
            isContinuing = false;
            pendingLoadData = null;
            attributes = PlayerAttributes.CreateDruidBase();
            campStates.Clear();
            visitedPowerSites.Clear();
            currentState = GameState.CharacterCreation;
            SceneManager.LoadScene(SceneNames.CharacterCreation);
        }

        public void ContinueGame()
        {
            SaveData data = SaveSystem.Load();
            if (data == null)
            {
                Debug.LogWarning("[GameManager] Brak save — startuję nową grę.");
                StartNewGame();
                return;
            }
            isContinuing = true;
            pendingLoadData = data;
            attributes = data.attributes != null ? data.attributes.Clone() : PlayerAttributes.CreateDruidBase();

            campStates.Clear();
            if (data.camps != null)
            {
                foreach (var entry in data.camps)
                {
                    if (!string.IsNullOrEmpty(entry.campId))
                        campStates[entry.campId] = entry.state;
                }
            }

            visitedPowerSites.Clear();
            if (data.visitedPowerSites != null)
            {
                foreach (var id in data.visitedPowerSites)
                    if (!string.IsNullOrEmpty(id)) visitedPowerSites.Add(id);
            }

            currentState = GameState.Playing;
            SceneManager.LoadScene(SceneNames.World);
        }

        public void GoToWorldFromCreation()
        {
            isContinuing = false;
            currentState = GameState.Playing;
            SceneManager.LoadScene(SceneNames.World);
        }

        public void GoToMainMenu()
        {
            currentState = GameState.MainMenu;
            SceneManager.LoadScene(SceneNames.MainMenu);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public CampState GetCampState(string campId, CampState defaultState = CampState.Active)
        {
            if (string.IsNullOrEmpty(campId)) return defaultState;
            return campStates.TryGetValue(campId, out var st) ? st : defaultState;
        }

        public void SetCampState(string campId, CampState state)
        {
            if (string.IsNullOrEmpty(campId)) return;
            campStates[campId] = state;
        }

        public SaveData BuildSaveData(Vector3 playerPos, int currentHp, int currentMana, string zoneName)
        {
            var data = new SaveData
            {
                attributes = attributes.Clone(),
                currentHealth = currentHp,
                currentMana = currentMana,
                lastZoneName = zoneName,
                characterType = characterType.ToString()
            };
            data.PlayerPosition = playerPos;
            foreach (var kv in campStates)
                data.camps.Add(new CampSaveEntry { campId = kv.Key, state = kv.Value });
            foreach (var id in visitedPowerSites)
                data.visitedPowerSites.Add(id);

            var inv = Object.FindAnyObjectByType<Inventory>();
            if (inv != null)
                data.inventory = inv.ToSaveList();

            return data;
        }
    }
}
