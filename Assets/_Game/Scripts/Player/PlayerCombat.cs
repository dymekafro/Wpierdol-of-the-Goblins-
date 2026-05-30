using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using WPG.Character;
using WPG.Core;
using WPG.World;
using Invector.vCharacterController;

namespace WPG.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Fireball Casting")]
        [SerializeField] private float fireballReleaseDelay = 1.30f;
        [SerializeField] private float fireballMovementLockDuration = 1.7f;

        [Header("Fireball Charge Visual")]
        [SerializeField] private float fireballChargeStartScale = 0.01f;
        [SerializeField] private float fireballChargeEndScale = 0.3f;

        // Pozycja ładowania względem postaci, NIE względem dłoni.
        [SerializeField] private float fireballChargeForwardOffset = 0.75f;
        [SerializeField] private float fireballChargeUpOffset = 1.5f;

        [SerializeField] private float fireballChargeLightStartIntensity = 2f;
        [SerializeField] private float fireballChargeLightEndIntensity = 6f;
        [SerializeField] private float fireballChargeLightRange = 6f;

        [Header("Melee Attack")]
        [SerializeField] private float meleeDamageDelay = 0.5f;

        [Header("Heal Casting")]
        [SerializeField] private float healMovementLockDuration = 1.25f;

        [Header("Animation Triggers")]
        [SerializeField] private string attackTriggerName = "Attack";
        [SerializeField] private string fireballCastTriggerName = "Cast";
        [SerializeField] private string healTriggerName = "Heal";

        private bool _isCastingFireball;
        private Vector3 _fireballCastDirection;

        private GameObject _fireballChargeVisual;
        private Light _fireballChargeLight;
        private Material _fireballChargeMaterial;

        private Animator _animator;
        private AnimatorParameterMirror _animatorMirror;

        public Transform staffTip;
        public Transform handMount;
        public CharacterAnimDriver animDriver;

        public float meleeRange = 2.2f;
        public float meleeArc = 110f;
        public float meleeCooldown = 0.6f;

        public float fireballCooldown = 1.2f;
        public int fireballManaCost = 15;

        public float healCooldown = 4f;
        public int healManaCost = 25;
        public int healAmount = 35;

        private PlayerStats _stats;
        private PlayerController _ctrl;
        private WPG.World.InteractionDetector _interaction;

        private float _meleeReadyAt;
        private float _fireballReadyAt;
        private float _healReadyAt;

        public float MeleeCooldownNorm => Mathf.Clamp01(1f - (_meleeReadyAt - Time.time) / Mathf.Max(0.01f, meleeCooldown));
        public float FireballCooldownNorm => Mathf.Clamp01(1f - (_fireballReadyAt - Time.time) / Mathf.Max(0.01f, fireballCooldown));
        public float HealCooldownNorm => Mathf.Clamp01(1f - (_healReadyAt - Time.time) / Mathf.Max(0.01f, healCooldown));

        // Animacja swingu fallback, gdy nie ma prawdziwego animatora.
        private float _swingT = -1f;
        private const float SwingDuration = 0.35f;
        private Quaternion _swingFromLocal;
        private Quaternion _swingToLocal;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();

            _ctrl = GetComponent<PlayerController>();

            if (_ctrl == null)
                _ctrl = GetComponentInParent<PlayerController>();

            if (_ctrl == null)
                _ctrl = GetComponentInChildren<PlayerController>();

            if (animDriver == null)
                animDriver = GetComponent<CharacterAnimDriver>();
        }

        private void Start()
        {
            _interaction = GetComponent<WPG.World.InteractionDetector>();

            if (_interaction == null)
                _interaction = GetComponentInParent<WPG.World.InteractionDetector>();

            if (_interaction == null)
                _interaction = GetComponentInChildren<WPG.World.InteractionDetector>();

            _animator = GetComponentInParent<Animator>();

            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            _animatorMirror = GetComponentInParent<AnimatorParameterMirror>();

            if (_animatorMirror == null)
                _animatorMirror = GetComponentInChildren<AnimatorParameterMirror>();
        }

        private void Update()
        {
            if (_stats != null && _stats.IsDead)
                return;

            var mouse = Mouse.current;
            var kb = Keyboard.current;

            if (!_isCastingFireball)
            {
                if (mouse != null && mouse.leftButton.wasPressedThisFrame && Time.time >= _meleeReadyAt)
                    DoMelee();

                // E - fireball, ale jeśli przy interaktywnym obiekcie, oddajemy E interakcji.
                bool eConsumedByInteraction = _interaction != null && _interaction.HasReadyInteractable;

                if (kb != null && kb.eKey.wasPressedThisFrame && !eConsumedByInteraction && Time.time >= _fireballReadyAt)
                    DoFireball();

                if (kb != null && kb.qKey.wasPressedThisFrame && Time.time >= _healReadyAt)
                    DoHeal();
            }

            TickSwing();
        }

        private void OnDisable()
        {
            _isCastingFireball = false;

            DestroyFireballChargeVisual();

            vThirdPersonController.ClearExternalMovementLock();

            StopPhysicsVelocity();
        }

        private void DoMelee()
        {
            _meleeReadyAt = Time.time + meleeCooldown;

            TriggerAnimation(attackTriggerName);

            if (animDriver != null)
                animDriver.TriggerAttack();

            if (animDriver == null || !animDriver.HasRealAnimator)
                StartSwing();

            StartCoroutine(DealMeleeDamageDelayed());
        }

        private IEnumerator DealMeleeDamageDelayed()
        {
            if (meleeDamageDelay > 0f)
                yield return new WaitForSeconds(meleeDamageDelay);

            if (_stats != null && _stats.IsDead)
                yield break;

            DealMeleeDamage();
        }

        private void DealMeleeDamage()
        {
            GameAudioManager.EnsureExists()?.PlayHit(transform.position);

            int dmg = _stats != null && _stats.attributes != null
                ? _stats.attributes.MeleeDamage
                : 8;

            Vector3 origin = transform.position + Vector3.up * 1f;
            Vector3 forward = transform.forward;
            float halfArc = meleeArc * 0.5f;

            Collider[] hits = Physics.OverlapSphere(origin, meleeRange);

            foreach (var c in hits)
            {
                if (c.gameObject == gameObject || c.transform.IsChildOf(transform))
                    continue;

                Vector3 to = c.transform.position - origin;
                to.y = 0f;

                if (to.sqrMagnitude < 0.001f)
                    continue;

                float ang = Vector3.Angle(forward, to.normalized);

                if (ang > halfArc)
                    continue;

                var dmgr = c.GetComponentInParent<IDamageReceiver>();

                if (dmgr != null)
                    dmgr.ReceiveDamage(dmg, c.transform.position);
            }
        }

        private void DoFireball()
        {
            if (_isCastingFireball)
                return;

            if (_stats == null)
                return;

            if (!_stats.TrySpendMana(fireballManaCost))
                return;

            _isCastingFireball = true;
            _fireballReadyAt = Time.time + fireballCooldown;

            StartCoroutine(CastFireballDelayed());
        }

        private IEnumerator CastFireballDelayed()
        {
            BeginFireballCastLock();

            TriggerAnimation(fireballCastTriggerName);

            if (animDriver != null)
                animDriver.TriggerCast();

            GameAudioManager.EnsureExists()?.PlayFireballCast(transform.position);

            yield return StartCoroutine(ChargeFireballVisual(fireballReleaseDelay));

            if (_stats != null && _stats.IsDead)
            {
                _isCastingFireball = false;
                DestroyFireballChargeVisual();
                EndFireballCastLock();
                yield break;
            }

            DestroyFireballChargeVisual();
            SpawnFireball();

            float remainingLockTime = Mathf.Max(0f, fireballMovementLockDuration - fireballReleaseDelay);

            if (remainingLockTime > 0f)
                yield return new WaitForSeconds(remainingLockTime);

            _isCastingFireball = false;
            EndFireballCastLock();
        }

        private IEnumerator ChargeFireballVisual(float duration)
        {
            CreateFireballChargeVisual();
            UpdateFireballChargeVisual(0f);

            if (duration <= 0f)
                yield break;

            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / duration);
                UpdateFireballChargeVisual(t);

                yield return null;
            }

            UpdateFireballChargeVisual(1f);
        }

        private void CreateFireballChargeVisual()
        {
            DestroyFireballChargeVisual();

            _fireballChargeVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _fireballChargeVisual.name = "FireballChargeVisual";

            Collider col = _fireballChargeVisual.GetComponent<Collider>();

            if (col != null)
                Destroy(col);

            Renderer renderer = _fireballChargeVisual.GetComponent<Renderer>();

            if (renderer != null)
                ApplyFireballChargeMaterial(renderer);

            GameObject lightObj = new GameObject("FireballChargeLight");
            lightObj.transform.SetParent(_fireballChargeVisual.transform, false);
            lightObj.transform.localPosition = Vector3.zero;

            _fireballChargeLight = lightObj.AddComponent<Light>();
            _fireballChargeLight.type = LightType.Point;
            _fireballChargeLight.color = new Color(1f, 0.55f, 0.08f);
            _fireballChargeLight.range = fireballChargeLightRange;
            _fireballChargeLight.intensity = fireballChargeLightStartIntensity;
        }

        private void ApplyFireballChargeMaterial(Renderer renderer)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Unlit");

            if (shader == null)
                shader = Shader.Find("Unlit/Color");

            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            if (shader == null)
                shader = Shader.Find("Standard");

            _fireballChargeMaterial = new Material(shader);

            Color baseColor = new Color(1f, 0.48f, 0.04f, 1f);
            Color emissionColor = new Color(1f, 0.32f, 0.02f, 1f) * 3.5f;

            if (_fireballChargeMaterial.HasProperty("_BaseColor"))
                _fireballChargeMaterial.SetColor("_BaseColor", baseColor);

            if (_fireballChargeMaterial.HasProperty("_Color"))
                _fireballChargeMaterial.SetColor("_Color", baseColor);

            if (_fireballChargeMaterial.HasProperty("_EmissionColor"))
            {
                _fireballChargeMaterial.EnableKeyword("_EMISSION");
                _fireballChargeMaterial.SetColor("_EmissionColor", emissionColor);
            }

            renderer.material = _fireballChargeMaterial;
        }

        private void UpdateFireballChargeVisual(float t)
        {
            if (_fireballChargeVisual == null)
                return;

            Vector3 spawnPos = GetFireballSpawnPosition(_fireballCastDirection);
            _fireballChargeVisual.transform.position = spawnPos;

            if (_fireballCastDirection.sqrMagnitude > 0.001f)
                _fireballChargeVisual.transform.rotation = Quaternion.LookRotation(_fireballCastDirection, Vector3.up);

            float easedT = Mathf.SmoothStep(0f, 1f, t);
            float scale = Mathf.Lerp(fireballChargeStartScale, fireballChargeEndScale, easedT);

            _fireballChargeVisual.transform.localScale = Vector3.one * scale;

            if (_fireballChargeLight != null)
            {
                _fireballChargeLight.intensity = Mathf.Lerp(
                    fireballChargeLightStartIntensity,
                    fireballChargeLightEndIntensity,
                    easedT
                );

                _fireballChargeLight.range = fireballChargeLightRange;
            }
        }

        private void DestroyFireballChargeVisual()
        {
            if (_fireballChargeVisual != null)
                Destroy(_fireballChargeVisual);

            if (_fireballChargeMaterial != null)
                Destroy(_fireballChargeMaterial);

            _fireballChargeVisual = null;
            _fireballChargeLight = null;
            _fireballChargeMaterial = null;
        }

        private Vector3 GetFireballSpawnPosition(Vector3 dir)
        {
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.001f)
                dir = transform.forward;

            dir.y = 0f;

            if (dir.sqrMagnitude < 0.001f)
                dir = Vector3.forward;
            else
                dir.Normalize();

            // Kula ładowania i późniejszy pocisk pojawiają się przed postacią,
            // a nie przy dłoni/staffTip.
            Vector3 basePos = transform.position;

            Vector3 spawnPos =
                basePos +
                Vector3.up * fireballChargeUpOffset +
                dir * fireballChargeForwardOffset;

            return spawnPos;
        }

        private void SpawnFireball()
        {
            int dmg = _stats != null && _stats.attributes != null
                ? _stats.attributes.SpellPower * 4
                : 25;

            Vector3 dir = _fireballCastDirection;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.001f)
                dir = transform.forward;

            dir.y = 0f;

            if (dir.sqrMagnitude < 0.001f)
                dir = Vector3.forward;
            else
                dir.Normalize();

            Vector3 spawnPos = GetFireballSpawnPosition(dir);

            WorldAssetPlacer.TrySpawnVfx(
                WorldAssetPlacer.VfxKind.FireballCast,
                spawnPos,
                Quaternion.LookRotation(dir),
                2f
            );

            var go = new GameObject("Fireball");
            go.transform.position = spawnPos;
            go.transform.localScale = Vector3.one * 0.5f;

            var mesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mesh.transform.SetParent(go.transform, false);
            mesh.transform.localScale = Vector3.one * 0.6f;

            var col = mesh.GetComponent<Collider>();

            if (col != null)
                Destroy(col);

            var mr = mesh.GetComponent<MeshRenderer>();

            if (mr != null)
            {
                mr.sharedMaterial = MaterialFactory.Get(
                    new Color(1f, 0.55f, 0.1f),
                    0.4f,
                    new Color(1f, 0.4f, 0.05f),
                    4f
                );
            }

            var light = new GameObject("Light").AddComponent<Light>();
            light.transform.SetParent(go.transform, false);
            light.color = new Color(1f, 0.55f, 0.1f);
            light.range = 6f;
            light.intensity = 4f;

            var proj = go.AddComponent<FireballProjectile>();
            proj.Fire(dir, dmg, gameObject);
        }

        private void DoHeal()
        {
            if (_stats == null)
                return;

            if (!_stats.TrySpendMana(healManaCost))
                return;

            _healReadyAt = Time.time + healCooldown;

            TriggerAnimation(healTriggerName);

            // Blokada ruchu na czas animacji Heal.
            // Kamera zostaje aktywna.
            vThirdPersonController.LockMovementForSeconds(healMovementLockDuration);

            // Zostawiamy TriggerCast jako fallback dla CharacterAnimDriver,
            // ale główny Animator dostaje osobny trigger Heal.
            if (animDriver != null)
                animDriver.TriggerCast();

            _stats.Heal(healAmount);

            Vector3 healPos = transform.position + Vector3.up * 1f;

            if (WorldAssetPlacer.TrySpawnVfx(WorldAssetPlacer.VfxKind.Heal, healPos, Quaternion.identity, 2.5f) == null)
            {
                var go = new GameObject("HealFX");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = Vector3.up * 1f;

                var light = go.AddComponent<Light>();
                light.color = new Color(0.4f, 1f, 0.5f);
                light.range = 4f;
                light.intensity = 5f;

                Destroy(go, 0.6f);
            }
        }

        private void BeginFireballCastLock()
        {
            _fireballCastDirection = transform.forward;
            _fireballCastDirection.y = 0f;

            if (_fireballCastDirection.sqrMagnitude < 0.001f)
                _fireballCastDirection = Vector3.forward;
            else
                _fireballCastDirection.Normalize();

            // Blokada ruchu na czas castowania fireballa.
            // Kamera zostaje aktywna.
            vThirdPersonController.LockMovementForSeconds(fireballMovementLockDuration);

            StopPhysicsVelocity();
        }

        private void EndFireballCastLock()
        {
            StopPhysicsVelocity();
        }

        private void TriggerAnimation(string triggerName)
        {
            if (string.IsNullOrWhiteSpace(triggerName))
                return;

            if (_animatorMirror != null)
            {
                _animatorMirror.MirrorTrigger(triggerName);
                return;
            }

            if (_animator != null)
                _animator.SetTrigger(triggerName);
        }

        private void StopPhysicsVelocity()
        {
            var rb = GetComponent<Rigidbody>();

            if (rb == null)
                rb = GetComponentInParent<Rigidbody>();

            if (rb == null)
                rb = GetComponentInChildren<Rigidbody>();

            if (rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector3.zero;
#else
                rb.velocity = Vector3.zero;
#endif
                rb.angularVelocity = Vector3.zero;
            }
        }

        private void StartSwing()
        {
            if (handMount == null)
                return;

            _swingT = 0f;
            _swingFromLocal = Quaternion.Euler(-30f, 0f, 0f);
            _swingToLocal = Quaternion.Euler(90f, 0f, 0f);
        }

        // Zostawione dla kompatybilności.
        private Vector3 ResolveAimDirection()
        {
            if (_ctrl != null)
                return _ctrl.AimDirection();

            Camera cam = Camera.main;

            if (cam != null)
            {
                Vector3 fwd = cam.transform.forward;
                fwd.y = 0f;

                if (fwd.sqrMagnitude > 0.001f)
                    return fwd.normalized;
            }

            return transform.forward;
        }

        private void TickSwing()
        {
            if (handMount == null || _swingT < 0f)
                return;

            _swingT += Time.deltaTime / SwingDuration;

            if (_swingT >= 1f)
            {
                _swingT = -1f;
                handMount.localRotation = Quaternion.Euler(-30f, 0f, 0f);
                return;
            }

            // Half forward then back.
            float t = _swingT < 0.5f
                ? _swingT * 2f
                : (1f - _swingT) * 2f;

            handMount.localRotation = Quaternion.Slerp(_swingFromLocal, _swingToLocal, t);
        }
    }
}