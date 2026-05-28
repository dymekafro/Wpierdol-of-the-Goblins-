using Invector.vCharacterController;
using UnityEngine;
using WPG.Character;
using WPG.Core;
using WPG.Enemies;
using WPG.Player;
using WPG.UI;

namespace WPG.World
{
    public class WorldBootstrap : MonoBehaviour
    {
        // Pełna integracja Invector Third Person Controller LITE — gdy true (default),
        // gracz spawnuje się z prefabu ThirdPersonController_LITE i używa vThirdPersonCamera.
        // Fallback: stary CharacterController-based PlayerBuilder z WPG ThirdPersonCamera.
        public bool useInvectorLocomotion = true;

        private WorldGenerator _generator;
        private GameObject _player;
        private PlayerStats _stats;
        private PlayerHUD _hud;
        private PauseMenu _pause;
        private Camera _mainCamera;
        private DruidBase _base;

        // Wybrany kamerowy adapter (Invector LUB WPG legacy).
        private vThirdPersonCamera _invectorCamera;
        private ThirdPersonCamera _wpgCamera;

        private void Awake()
        {
            GameManager.EnsureExists();
            SettingsManager.EnsureExists();
            GameAudioManager.EnsureExists();
        }

        private void Start()
        {
            UIFactory.EnsureEventSystem();
            ForestAtmosphereSettings.EnsureExists();

            // 1. World gen
            var worldRoot = new GameObject("WorldRoot");
            _generator = new WorldGenerator { parent = worldRoot.transform, seed = 13579 };
            _generator.Generate();
            _base = _generator.DruidBase;

            // 2. Spawn gracza
            var gm = GameManager.Instance;
            PlayerAttributes attrs = gm != null ? gm.attributes : PlayerAttributes.CreateDruidBase();

            Vector3 spawn = _generator.SpawnPoint;
            int? hp = null;
            int? mana = null;
            if (gm != null && gm.isContinuing && gm.pendingLoadData != null)
            {
                var data = gm.pendingLoadData;
                if (data.hasPosition) spawn = data.PlayerPosition;
                hp = data.currentHealth;
                mana = data.currentMana;
            }

            PlayerCombat combat = null;
            bool builtWithInvector = false;

            if (useInvectorLocomotion)
            {
                var playerScale = InvectorPlayerBuilder.InvectorPlayerScale;
                _player = InvectorPlayerBuilder.TryBuild(spawn, attrs, out _stats, out combat, out var adapter, playerScale);
                builtWithInvector = _player != null;
                if (builtWithInvector)
                {
                    BuildInvectorCamera(playerScale);
                    if (adapter != null) adapter.tpCamera = _invectorCamera;
                }
            }

            if (!builtWithInvector)
            {
                BuildWpgCamera();
                _player = PlayerBuilder.BuildDruid(spawn, attrs, out _stats, out var ctrl, out combat);
                ctrl.cameraRig = _wpgCamera;
                _wpgCamera.ApplyCharacterScale(PlayerBuilder.ModelScale);
                _wpgCamera.target = _player.transform;
                _wpgCamera.transform.position = _player.transform.position - _player.transform.forward * _wpgCamera.distance + Vector3.up * _wpgCamera.height;
                // InteractionDetector dodawany w starej ścieżce ręcznie (Invector dodaje go w buildzie).
                if (_player.GetComponent<InteractionDetector>() == null)
                    _player.AddComponent<InteractionDetector>();
            }

            if (hp.HasValue) _stats.Init(attrs, hp, mana);

            // 3. HUD
            var hudGO = new GameObject("HUD");
            _hud = hudGO.AddComponent<PlayerHUD>();
            _hud.Bind(_stats, combat);

            // 4. Pause menu
            var pauseGO = new GameObject("PauseMenu");
            _pause = pauseGO.AddComponent<PauseMenu>();

            // 5. Eventy
            _stats.OnDied += OnPlayerDied;
            PlayerHUD.OnRespawnRequested += OnRespawn;

            // 6. Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // 7. Zone wave
            if (gm != null && gm.isContinuing && gm.pendingLoadData != null && !string.IsNullOrEmpty(gm.pendingLoadData.lastZoneName))
            {
                WorldZone.RaiseExternal(gm.pendingLoadData.lastZoneName);
            }
            else
            {
                WorldZone.RaiseExternal("Sady Ostatniego Strażnika");
            }

            Debug.Log($"[WorldBootstrap] Player driver: {(builtWithInvector ? "Invector" : "WPG legacy")} | camps={_generator.Camps.Count} | power sites={_generator.PowerSites.Count} | spawn={spawn}");
        }

        private void BuildInvectorCamera(float characterScale)
        {
            // 1) Spróbuj prefab vThirdPersonCamera_LITE (ma już skonfigurowane offsetowy/clip itd.).
            GameObject camGO = null;
            var camPrefab = GameAssetRegistry.InvectorCamera;
            if (camPrefab != null)
            {
                camGO = Object.Instantiate(camPrefab);
                camGO.name = "MainCamera";
            }
            else
            {
                camGO = new GameObject("MainCamera");
                camGO.AddComponent<Camera>();
                camGO.AddComponent<vThirdPersonCamera>();
                Debug.LogWarning("[WorldBootstrap] Brak prefabu vThirdPersonCamera_LITE — utworzyłem podstawową kamerę manualnie.");
            }

            camGO.tag = "MainCamera";
            _mainCamera = camGO.GetComponent<Camera>();
            if (_mainCamera == null) _mainCamera = camGO.AddComponent<Camera>();
            _mainCamera.clearFlags = CameraClearFlags.Skybox;
            _mainCamera.backgroundColor = GoldenHourLighting.FogSepia;
            _mainCamera.fieldOfView = 65f;
            _mainCamera.farClipPlane = 250f;
            if (camGO.GetComponent<AudioListener>() == null) camGO.AddComponent<AudioListener>();

            _invectorCamera = camGO.GetComponent<vThirdPersonCamera>();
            if (_invectorCamera == null) _invectorCamera = camGO.AddComponent<vThirdPersonCamera>();

            // vThirdPersonInput sam podpina kamerę przez FindFirstObjectByType<vThirdPersonCamera>(),
            // ale możemy też explicit ustawić target.
            if (_player != null)
            {
                _invectorCamera.SetMainTarget(_player.transform);
            }

            InvectorCameraScale.Apply(_invectorCamera, characterScale);
        }

        private void BuildWpgCamera()
        {
            var camGO = new GameObject("MainCamera");
            camGO.tag = "MainCamera";
            _mainCamera = camGO.AddComponent<Camera>();
            _mainCamera.clearFlags = CameraClearFlags.Skybox;
            _mainCamera.backgroundColor = GoldenHourLighting.FogSepia;
            _mainCamera.fieldOfView = 65f;
            _mainCamera.farClipPlane = 250f;
            camGO.AddComponent<AudioListener>();
            _wpgCamera = camGO.AddComponent<ThirdPersonCamera>();
        }

        private void OnDestroy()
        {
            if (_stats != null) _stats.OnDied -= OnPlayerDied;
            PlayerHUD.OnRespawnRequested -= OnRespawn;
        }

        private void OnPlayerDied()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnRespawn()
        {
            if (_stats == null || _base == null) return;
            _stats.ReviveAt(_base.spawnPoint);
            if (_hud != null) _hud.HideDeathScreen();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
