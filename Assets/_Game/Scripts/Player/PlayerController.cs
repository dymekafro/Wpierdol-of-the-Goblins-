using UnityEngine;
using UnityEngine.InputSystem;

namespace WPG.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public ThirdPersonCamera cameraRig;
        public float gravity = -20f;
        public float jumpSpeed = 7f;
        public float rotateSpeed = 12f;

        private CharacterController _cc;
        private PlayerStats _stats;
        private float _verticalVel;
        private Vector3 _moveInput;
        public Vector3 LastMoveDir { get; private set; }

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerStats>();
        }

        private void Update()
        {
            if (_stats != null && _stats.IsDead) return;
            if (cameraRig == null) return;

            ReadInput();
            Move();
        }

        private void ReadInput()
        {
            float x = 0f, z = 0f;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed) z += 1f;
                if (kb.sKey.isPressed) z -= 1f;
                if (kb.dKey.isPressed) x += 1f;
                if (kb.aKey.isPressed) x -= 1f;
            }

            Vector3 fwd = cameraRig.GetCameraForwardFlat();
            Vector3 right = new Vector3(fwd.z, 0f, -fwd.x);
            _moveInput = (fwd * z + right * x);
            if (_moveInput.sqrMagnitude > 1f) _moveInput.Normalize();
        }

        private void Move()
        {
            float speed = _stats != null && _stats.attributes != null ? _stats.attributes.MoveSpeed : 5f;
            Vector3 horiz = _moveInput * speed;

            if (_cc.isGrounded)
            {
                if (_verticalVel < 0f) _verticalVel = -1f;
                var kb = Keyboard.current;
                if (kb != null && kb.spaceKey.wasPressedThisFrame)
                {
                    _verticalVel = jumpSpeed;
                }
            }
            _verticalVel += gravity * Time.deltaTime;

            Vector3 motion = horiz + Vector3.up * _verticalVel;
            _cc.Move(motion * Time.deltaTime);

            if (horiz.sqrMagnitude > 0.01f)
            {
                LastMoveDir = horiz.normalized;
                Quaternion targetRot = Quaternion.LookRotation(horiz, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }
        }

        public Vector3 AimDirection()
        {
            if (cameraRig == null) return transform.forward;
            return cameraRig.GetCameraForwardFlat();
        }
    }
}
