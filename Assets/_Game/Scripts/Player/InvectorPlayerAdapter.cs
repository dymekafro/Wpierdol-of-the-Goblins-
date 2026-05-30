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
    /// - Gra SFX kroków/skoku (Invector nie woła WPG PlayerController, więc audio musi iść stąd).
    /// </summary>
    public class InvectorPlayerAdapter : MonoBehaviour
    {
        public vThirdPersonController controller;
        public vThirdPersonInput input;
        public vThirdPersonCamera tpCamera;
        public Animator animator;
        public CharacterAnimDriver animDriver;

        public float baseMouseSensitivity = 3f;

        [Header("Footstep SFX")]
        public float footstepInterval = 0.45f;   // odstęp przy chodzie
        public float sprintFootstepInterval = 0.3f; // krótszy przy biegu

        [Header("Fall SFX (woosh przy długim spadaniu)")]
        // Minimalny czas w powietrzu, po którym uznajemy spadek za "dłuższy niż zwykły skok".
        public float fallWooshAirtime = 0.7f;
        // Woosh gra dopiero, gdy faktycznie spadamy (prędkość pionowa poniżej tego progu).
        public float fallWooshVelocityY = -3f;

        private PlayerStats _stats;
        private Rigidbody _rb;
        private bool? _lastDead;
        private float _lastSensitivityApplied = -1f;

        private float _nextFootstepAt;
        private bool _wasJumping;
        private float _airTime;
        private bool _fallWooshPlayed;

        private void Awake()
        {
            if (controller == null) controller = GetComponent<vThirdPersonController>();
            if (input == null) input = GetComponent<vThirdPersonInput>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animDriver == null) animDriver = GetComponent<CharacterAnimDriver>();
            _stats = GetComponent<PlayerStats>();
            _rb = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            ApplyMouseSensitivity(force: true);
        }

        private void Update()
        {
            HandleDeathState();
            ApplyMouseSensitivity(force: false);
            HandleLocomotionAudio();
        }

        // Invector porusza postacią po swojemu (vThirdPersonController), więc WPG PlayerController
        // — jedyne miejsce z PlayFootstep/PlayJump — nigdy się tu nie odpala. Pollujemy stan
        // kontrolera i sami gramy SFX kroków/skoku.
        private void HandleLocomotionAudio()
        {
            if (controller == null) return;
            if (_stats != null && _stats.IsDead) return;

            bool grounded = controller.isGrounded;

            // Skok: zbocze narastające isJumping (false -> true) -> krótki chrząk wysiłku ("hszyy").
            bool jumping = controller.isJumping;
            if (jumping && !_wasJumping)
                GameAudioManager.EnsureExists()?.PlayJump(transform.position);
            _wasJumping = jumping;

            // Długi spadek: gdy w powietrzu dłużej niż zwykły skok i faktycznie spadamy,
            // zagraj woosh (dawny dźwięk skoku) RAZ na jeden lot — nie spamuj.
            if (!grounded)
            {
                _airTime += Time.deltaTime;
                float vy = _rb != null ? _rb.linearVelocity.y : 0f;
                if (!_fallWooshPlayed && _airTime >= fallWooshAirtime && vy <= fallWooshVelocityY)
                {
                    _fallWooshPlayed = true;
                    GameAudioManager.EnsureExists()?.PlayFall(transform.position);
                }
            }
            else
            {
                _airTime = 0f;
                _fallWooshPlayed = false;
            }

            // Kroki: tylko gdy na ziemi i jest realny input ruchu.
            bool moving = controller.input.sqrMagnitude > 0.1f;
            if (grounded && !jumping && moving)
            {
                if (Time.time >= _nextFootstepAt)
                {
                    float interval = controller.isSprinting ? sprintFootstepInterval : footstepInterval;
                    _nextFootstepAt = Time.time + interval;
                    GameAudioManager.EnsureExists()?.PlayFootstep(transform.position);
                }
            }
            else
            {
                // Reset, żeby pierwszy krok po starcie ruchu zagrał od razu.
                _nextFootstepAt = 0f;
            }
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
