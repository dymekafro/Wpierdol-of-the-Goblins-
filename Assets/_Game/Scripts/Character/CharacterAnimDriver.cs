using UnityEngine;

namespace WPG.Character
{
    /// <summary>
    /// Sterownik animacji postaci.
    /// - Jeśli na modelu jest Animator z runtime controllerem (np. GanzSe, 3D Stylized Goblin, Fantasy Goblin) →
    ///   ustawia parametry Speed / triggers Attack / Cast / Death.
    /// - W przeciwnym razie animuje proceduralnie placeholder transforms (body bob, leg swing, hand swing).
    /// API: SetSpeed(), TriggerAttack(), TriggerCast(), SetDead(true).
    /// </summary>
    public class CharacterAnimDriver : MonoBehaviour
    {
        public Animator animator;

        // Procedural rig (opcjonalne — gdy któreś jest null, ten kanał jest pomijany).
        public Transform bodyPivot;
        public Transform headPivot;
        public Transform handMount;
        public Transform leftArm;
        public Transform rightArm;
        public Transform leftLeg;
        public Transform rightLeg;

        // Animator parameters (stringi — niektóre paczki mogą używać innych nazw, można podmienić).
        public string speedParam = "Speed";
        public string attackTrigger = "Attack";
        public string castTrigger = "Cast";
        public string deathBool = "Death";
        public string isMovingParam = "IsMoving";

        // Invector Basic Locomotion (InputMagnitude / InputVertical zamiast Speed).
        public string invectorInputMagnitude = "InputMagnitude";
        public string invectorInputVertical = "InputVertical";
        public string invectorInputHorizontal = "InputHorizontal";
        public string invectorIsGrounded = "IsGrounded";

        public float idleBobAmplitude = 0.025f;
        public float walkBobAmplitude = 0.07f;
        public float walkLegSwingDeg = 28f;
        public float walkArmSwingDeg = 22f;
        public float swingDuration = 0.35f;
        public float castDuration = 0.6f;

        bool _hasAnimator;
        bool _usesInvectorLocomotion;
        bool _grounded = true;
        bool _dead;
        float _speedNorm;
        float _swingT = -1f;
        float _castT = -1f;
        float _phase;

        Vector3 _bodyBasePos;
        Quaternion _headBaseRot;
        Quaternion _handBaseRot;
        Quaternion _leftArmBase, _rightArmBase, _leftLegBase, _rightLegBase;
        bool _capturedBody, _capturedHead, _capturedHand, _capturedLA, _capturedRA, _capturedLL, _capturedRL;

        public bool HasRealAnimator => _hasAnimator;

        void OnEnable()
        {
            ResolveAnimator();
        }

        void Start()
        {
            ResolveAnimator();
            CaptureBaseTransforms();
        }

        public void RefreshBaseTransforms()
        {
            CaptureBaseTransforms();
            ResolveAnimator();
        }

        void CaptureBaseTransforms()
        {
            if (!_capturedBody && bodyPivot != null) { _bodyBasePos = bodyPivot.localPosition; _capturedBody = true; }
            if (!_capturedHead && headPivot != null) { _headBaseRot = headPivot.localRotation; _capturedHead = true; }
            if (!_capturedHand && handMount != null) { _handBaseRot = handMount.localRotation; _capturedHand = true; }
            if (!_capturedLA && leftArm != null) { _leftArmBase = leftArm.localRotation; _capturedLA = true; }
            if (!_capturedRA && rightArm != null) { _rightArmBase = rightArm.localRotation; _capturedRA = true; }
            if (!_capturedLL && leftLeg != null) { _leftLegBase = leftLeg.localRotation; _capturedLL = true; }
            if (!_capturedRL && rightLeg != null) { _rightLegBase = rightLeg.localRotation; _capturedRL = true; }
        }

        void ResolveAnimator()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            _hasAnimator = animator != null && animator.runtimeAnimatorController != null;
            if (_hasAnimator && !_usesInvectorLocomotion)
                _usesInvectorLocomotion = HasParam(invectorInputMagnitude, AnimatorControllerParameterType.Float);
        }

        public void ConfigureForInvectorLocomotion()
        {
            _usesInvectorLocomotion = true;
            ResolveAnimator();
        }

        public void SetGrounded(bool grounded)
        {
            _grounded = grounded;
            if (!_hasAnimator || string.IsNullOrEmpty(invectorIsGrounded)) return;
            if (HasParam(invectorIsGrounded, AnimatorControllerParameterType.Bool))
                animator.SetBool(invectorIsGrounded, grounded);
        }

        public void SetSpeed(float metersPerSecond, float maxSpeed = 5f)
        {
            float norm = maxSpeed > 0.01f ? Mathf.Clamp01(metersPerSecond / maxSpeed) : 0f;
            _speedNorm = norm;

            if (_hasAnimator)
            {
                if (_usesInvectorLocomotion || HasParam(invectorInputMagnitude, AnimatorControllerParameterType.Float))
                {
                    _usesInvectorLocomotion = true;
                    if (!string.IsNullOrEmpty(invectorInputMagnitude))
                        animator.SetFloat(invectorInputMagnitude, norm);
                    if (!string.IsNullOrEmpty(invectorInputVertical))
                        animator.SetFloat(invectorInputVertical, norm);
                    if (!string.IsNullOrEmpty(invectorInputHorizontal))
                        animator.SetFloat(invectorInputHorizontal, 0f);
                    if (!string.IsNullOrEmpty(invectorIsGrounded) && HasParam(invectorIsGrounded, AnimatorControllerParameterType.Bool))
                        animator.SetBool(invectorIsGrounded, _grounded);
                }
                else
                {
                    if (!string.IsNullOrEmpty(speedParam) && HasParam(speedParam, AnimatorControllerParameterType.Float))
                        animator.SetFloat(speedParam, metersPerSecond);
                    if (!string.IsNullOrEmpty(isMovingParam) && HasParam(isMovingParam, AnimatorControllerParameterType.Bool))
                        animator.SetBool(isMovingParam, metersPerSecond > 0.05f);
                }
            }
        }

        public void TriggerAttack()
        {
            if (_dead) return;
            if (_hasAnimator && !string.IsNullOrEmpty(attackTrigger) && HasParam(attackTrigger, AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger(attackTrigger);
                return;
            }
            _swingT = 0f;
        }

        public void TriggerCast()
        {
            if (_dead) return;
            if (_hasAnimator && !string.IsNullOrEmpty(castTrigger) && HasParam(castTrigger, AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger(castTrigger);
                return;
            }
            _castT = 0f;
        }

        public void SetDead(bool dead)
        {
            _dead = dead;
            if (_hasAnimator && !string.IsNullOrEmpty(deathBool) && HasParam(deathBool, AnimatorControllerParameterType.Bool))
                animator.SetBool(deathBool, dead);
        }

        bool HasParam(string name, AnimatorControllerParameterType type)
        {
            if (animator == null || animator.parameters == null) return false;
            foreach (var p in animator.parameters)
                if (p.name == name && p.type == type) return true;
            return false;
        }

        void Update()
        {
            CaptureBaseTransforms();

            if (_dead)
            {
                ResetPlaceholderPose();
                return;
            }

            if (_hasAnimator)
            {
                return;
            }

            ProceduralUpdate(Time.deltaTime);
        }

        void ProceduralUpdate(float dt)
        {
            float walkSpeed = _speedNorm;
            _phase += dt * Mathf.Lerp(2.5f, 7.5f, walkSpeed);

            float bobAmp = Mathf.Lerp(idleBobAmplitude, walkBobAmplitude, walkSpeed);
            if (bodyPivot != null && _capturedBody)
            {
                float bob = Mathf.Sin(_phase * 2f) * bobAmp;
                bodyPivot.localPosition = _bodyBasePos + new Vector3(0f, bob, 0f);
            }

            if (headPivot != null && _capturedHead)
            {
                float headWag = Mathf.Sin(_phase * 1.2f) * 3f * (0.4f + walkSpeed * 0.6f);
                headPivot.localRotation = _headBaseRot * Quaternion.Euler(0f, headWag, 0f);
            }

            float legSwing = Mathf.Sin(_phase) * walkLegSwingDeg * walkSpeed;
            float armSwing = Mathf.Sin(_phase) * walkArmSwingDeg * walkSpeed;

            if (leftLeg != null && _capturedLL) leftLeg.localRotation = _leftLegBase * Quaternion.Euler(legSwing, 0f, 0f);
            if (rightLeg != null && _capturedRL) rightLeg.localRotation = _rightLegBase * Quaternion.Euler(-legSwing, 0f, 0f);
            if (leftArm != null && _capturedLA) leftArm.localRotation = _leftArmBase * Quaternion.Euler(-armSwing, 0f, 0f);
            if (rightArm != null && _capturedRA) rightArm.localRotation = _rightArmBase * Quaternion.Euler(armSwing, 0f, 0f);

            if (handMount != null && _capturedHand)
            {
                if (_swingT >= 0f)
                {
                    _swingT += dt / Mathf.Max(0.05f, swingDuration);
                    if (_swingT >= 1f)
                    {
                        _swingT = -1f;
                        handMount.localRotation = _handBaseRot;
                    }
                    else
                    {
                        float t = _swingT < 0.5f ? _swingT * 2f : (1f - _swingT) * 2f;
                        Quaternion swingTo = _handBaseRot * Quaternion.Euler(120f, 0f, 0f);
                        handMount.localRotation = Quaternion.Slerp(_handBaseRot, swingTo, t);
                    }
                }
                else if (_castT >= 0f)
                {
                    _castT += dt / Mathf.Max(0.05f, castDuration);
                    if (_castT >= 1f)
                    {
                        _castT = -1f;
                        handMount.localRotation = _handBaseRot;
                    }
                    else
                    {
                        float t = _castT < 0.4f ? _castT / 0.4f : (1f - _castT) / 0.6f;
                        Quaternion castUp = _handBaseRot * Quaternion.Euler(-70f, 0f, -10f);
                        handMount.localRotation = Quaternion.Slerp(_handBaseRot, castUp, t);
                    }
                }
                else if (!(_capturedRA && rightArm != null))
                {
                    handMount.localRotation = _handBaseRot * Quaternion.Euler(armSwing * 0.5f, 0f, 0f);
                }
            }
        }

        void ResetPlaceholderPose()
        {
            if (_hasAnimator) return;
            if (bodyPivot != null && _capturedBody) bodyPivot.localPosition = _bodyBasePos;
            if (headPivot != null && _capturedHead) headPivot.localRotation = _headBaseRot;
            if (handMount != null && _capturedHand) handMount.localRotation = _handBaseRot;
        }
    }
}
