using Invector.vCharacterController;
using UnityEngine;
using WPG.Character;
using WPG.Core;

namespace WPG.Player
{
    /// <summary>
    /// Most pomiędzy systemem WPG (PlayerStats / PlayerCombat / SettingsManager)
    /// a Invector Third Person Controller LITE.
    ///
    /// Odpowiedzialności:
    /// - Po śmierci wyłącza vThirdPersonInput, żeby gracz nie chodził w trupiej animacji.
    /// - Po respawnie ponownie włącza input.
    /// - Synchronizuje czułość myszy z SettingsManager (xMouseSensitivity / yMouseSensitivity).
    /// - Trzyma referencje do animator-a, żeby CharacterAnimDriver mógł triggerować Attack/Cast.
    /// </summary>
    public class InvectorPlayerAdapter : MonoBehaviour
    {
        public vThirdPersonController controller;
        public vThirdPersonInput input;
        public vThirdPersonCamera tpCamera;
        public Animator animator;
        public CharacterAnimDriver animDriver;

        public float baseMouseSensitivity = 3f;

        private PlayerStats _stats;
        private bool? _lastDead;
        private float _lastSensitivityApplied = -1f;

        private void Awake()
        {
            if (controller == null) controller = GetComponent<vThirdPersonController>();
            if (input == null) input = GetComponent<vThirdPersonInput>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animDriver == null) animDriver = GetComponent<CharacterAnimDriver>();
            _stats = GetComponent<PlayerStats>();
        }

        private void OnEnable()
        {
            ApplyMouseSensitivity(force: true);
        }

        private void Update()
        {
            HandleDeathState();
            ApplyMouseSensitivity(force: false);
        }

        private void HandleDeathState()
        {
            if (_stats == null) return;
            bool dead = _stats.IsDead;
            if (_lastDead.HasValue && _lastDead.Value == dead) return;
            _lastDead = dead;

            if (input != null) input.enabled = !dead;
            if (animDriver != null) animDriver.SetDead(dead);

            if (dead)
            {
                // wyzeruj poziome velocity, żeby trup nie sunął
                var rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    var v = rb.linearVelocity;
                    rb.linearVelocity = new Vector3(0f, v.y, 0f);
                }
            }
        }

        private void ApplyMouseSensitivity(bool force)
        {
            if (tpCamera == null) return;
            float mul = SettingsManager.Instance != null ? SettingsManager.Instance.MouseSensitivity : 1f;
            float target = baseMouseSensitivity * mul;
            if (!force && Mathf.Abs(target - _lastSensitivityApplied) < 0.001f) return;
            _lastSensitivityApplied = target;
            tpCamera.xMouseSensitivity = target;
            tpCamera.yMouseSensitivity = target;
        }
    }
}
