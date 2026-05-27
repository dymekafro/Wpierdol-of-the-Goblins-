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
        private WorldGenerator _generator;
        private GameObject _player;
        private ThirdPersonCamera _cam;
        private PlayerStats _stats;
        private PlayerHUD _hud;
        private PauseMenu _pause;
        private Camera _mainCamera;
        private DruidBase _base;

        private void Awake()
        {
            GameManager.EnsureExists();
        }

        private void Start()
        {
            BuildCamera();
            UIFactory.EnsureEventSystem();

            // World gen
            var worldRoot = new GameObject("WorldRoot");
            _generator = new WorldGenerator { parent = worldRoot.transform, seed = 13579 };
            _generator.Generate();
            _base = _generator.DruidBase;

            // Spawn gracza
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

            _player = PlayerBuilder.BuildDruid(spawn, attrs, out _stats, out var ctrl, out var combat);

            // Camera follow + Controller binding
            ctrl.cameraRig = _cam;
            _cam.target = _player.transform;
            // ustaw kamerę za graczem
            _cam.transform.position = _player.transform.position - _player.transform.forward * 5f + Vector3.up * 2.5f;

            if (hp.HasValue) _stats.Init(attrs, hp, mana);

            // Interaction detector
            _player.AddComponent<InteractionDetector>();

            // HUD
            var hudGO = new GameObject("HUD");
            _hud = hudGO.AddComponent<PlayerHUD>();
            _hud.Bind(_stats, combat);

            // Pause menu
            var pauseGO = new GameObject("PauseMenu");
            _pause = pauseGO.AddComponent<PauseMenu>();

            // Eventy
            _stats.OnDied += OnPlayerDied;
            PlayerHUD.OnRespawnRequested += OnRespawn;

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // jeśli kontynuujemy, wczytaj zone name
            if (gm != null && gm.isContinuing && gm.pendingLoadData != null && !string.IsNullOrEmpty(gm.pendingLoadData.lastZoneName))
            {
                WorldZone.RaiseExternal(gm.pendingLoadData.lastZoneName);
            }
            else
            {
                WorldZone.RaiseExternal("Sady Ostatniego Strażnika");
            }

            // Diagnostyka
            Debug.Log($"[WorldBootstrap] Wygenerowano: {_generator.Camps.Count} obozów, " +
                      $"{_generator.PowerSites.Count} miejsc mocy. Spawn: {spawn}");
        }

        private void BuildCamera()
        {
            var camGO = new GameObject("MainCamera");
            camGO.tag = "MainCamera";
            _mainCamera = camGO.AddComponent<Camera>();
            _mainCamera.clearFlags = CameraClearFlags.SolidColor;
            _mainCamera.backgroundColor = new Color(0.04f, 0.06f, 0.08f);
            _mainCamera.fieldOfView = 65f;
            _mainCamera.farClipPlane = 250f;
            camGO.AddComponent<AudioListener>();
            _cam = camGO.AddComponent<ThirdPersonCamera>();
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
