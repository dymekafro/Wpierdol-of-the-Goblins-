using UnityEngine;
using UnityEngine.InputSystem;
using WPG.Character;
using WPG.Core;

namespace WPG.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public ThirdPersonCamera cameraRig;
        public CharacterAnimDriver animDriver;

        public float gravity = -20f;
        public float jumpSpeed = 7f;
        public float rotateSpeed = 12f;
        public float footstepInterval = 0.42f;

        private CharacterController _cc;
        private PlayerStats _stats;

        private float _verticalVel;
        private Vector3 _moveInput;
        private float _nextFootstepAt;

        public Vector3 LastMoveDir { get; private set; }
        public float CurrentSpeed { get; private set; }
        public bool IsMoving => CurrentSpeed > 0.1f;

        // Globalna blokada ruchu ustawiana przez PlayerCombat.
        // Blokuje wyłącznie WSAD i skok.
        // Kamera zostaje bez zmian.
        public static bool GlobalMovementLocked { get; set; }

        // Lokalna blokada dla konkretnego PlayerController.
        public bool MovementLocked { get; set; }

        private bool IsMovementInputLocked => MovementLocked || GlobalMovementLocked;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerStats>();

            if (animDriver == null)
                animDriver = GetComponent<CharacterAnimDriver>();
        }

        private void Update()
        {
            if (_stats != null && _stats.IsDead)
            {
                StopMovementImmediately();

                if (animDriver != null)
                {
                    animDriver.SetSpeed(0f);
                    animDriver.SetDead(true);
                }

                return;
            }

            if (animDriver != null)
                animDriver.SetDead(false);

            if (cameraRig == null)
                return;

            ReadInput();
            Move();
        }

        private void ReadInput()
        {
            // Podczas castowania ignorujemy tylko WSAD.
            // Kamery tutaj nie ruszamy.
            if (IsMovementInputLocked)
            {
                _moveInput = Vector3.zero;
                return;
            }

            float x = 0f;
            float z = 0f;

            var kb = Keyboard.current;

            if (kb != null)
            {
                if (kb.wKey.isPressed)
                    z += 1f;

                if (kb.sKey.isPressed)
                    z -= 1f;

                if (kb.dKey.isPressed)
                    x += 1f;

                if (kb.aKey.isPressed)
                    x -= 1f;
            }

            Vector3 fwd = cameraRig.GetCameraForwardFlat();
            Vector3 right = new Vector3(fwd.z, 0f, -fwd.x);

            _moveInput = fwd * z + right * x;

            if (_moveInput.sqrMagnitude > 1f)
                _moveInput.Normalize();
        }

        private void Move()
        {
            float speed = _stats != null && _stats.attributes != null
                ? _stats.attributes.MoveSpeed
                : 5f;

            Vector3 horiz = _moveInput * speed;

            // Dodatkowe zabezpieczenie.
            // Nawet jeśli _moveInput miał starą wartość z poprzedniej klatki,
            // podczas castowania ruch poziomy jest zerowany.
            if (IsMovementInputLocked)
                horiz = Vector3.zero;

            CurrentSpeed = horiz.magnitude;

            if (animDriver != null)
            {
                animDriver.SetGrounded(_cc.isGrounded);
                animDriver.SetSpeed(CurrentSpeed, speed);
            }

            if (_cc.isGrounded)
            {
                if (_verticalVel < 0f)
                    _verticalVel = -1f;

                var kb = Keyboard.current;

                // Skok też blokujemy podczas castowania.
                if (!IsMovementInputLocked && kb != null && kb.spaceKey.wasPressedThisFrame)
                {
                    _verticalVel = jumpSpeed;
                    GameAudioManager.EnsureExists()?.PlayJump(transform.position);
                }
            }

            _verticalVel += gravity * Time.deltaTime;

            Vector3 motion = horiz + Vector3.up * _verticalVel;
            _cc.Move(motion * Time.deltaTime);

            if (!IsMovementInputLocked && horiz.sqrMagnitude > 0.01f)
            {
                LastMoveDir = horiz.normalized;

                Quaternion targetRot = Quaternion.LookRotation(horiz, Vector3.up);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    rotateSpeed * Time.deltaTime
                );

                if (_cc.isGrounded && Time.time >= _nextFootstepAt)
                {
                    _nextFootstepAt = Time.time + footstepInterval;
                    GameAudioManager.EnsureExists()?.PlayFootstep(transform.position);
                }
            }
        }

        public void StopMovementImmediately()
        {
            _moveInput = Vector3.zero;
            CurrentSpeed = 0f;

            if (animDriver != null)
            {
                animDriver.SetSpeed(0f);

                if (_cc != null)
                    animDriver.SetGrounded(_cc.isGrounded);
            }
        }

        public Vector3 AimDirection()
        {
            if (cameraRig == null)
                return transform.forward;

            return cameraRig.GetCameraForwardFlat();
        }
    }
}