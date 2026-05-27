using UnityEngine;
using UnityEngine.InputSystem;
using WPG.Core;
using WPG.World;

namespace WPG.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        public Transform staffTip;
        public Transform handMount;
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

        // Animacja swingu
        private float _swingT = -1f;
        private const float SwingDuration = 0.35f;
        private Quaternion _swingFromLocal;
        private Quaternion _swingToLocal;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _ctrl = GetComponent<PlayerController>();
        }

        private void Start()
        {
            _interaction = GetComponent<WPG.World.InteractionDetector>();
        }

        private void Update()
        {
            if (_stats != null && _stats.IsDead) return;

            var mouse = Mouse.current;
            var kb = Keyboard.current;

            if (mouse != null && mouse.leftButton.wasPressedThisFrame && Time.time >= _meleeReadyAt)
                DoMelee();

            // E - fireball, ale jeśli przy interaktywnym obiekcie, oddajemy E interakcji.
            bool eConsumedByInteraction = _interaction != null && _interaction.HasReadyInteractable;
            if (kb != null && kb.eKey.wasPressedThisFrame && !eConsumedByInteraction && Time.time >= _fireballReadyAt)
                DoFireball();
            if (kb != null && kb.qKey.wasPressedThisFrame && Time.time >= _healReadyAt)
                DoHeal();

            TickSwing();
        }

        private void DoMelee()
        {
            _meleeReadyAt = Time.time + meleeCooldown;
            StartSwing();

            int dmg = _stats != null && _stats.attributes != null ? _stats.attributes.MeleeDamage : 8;
            Vector3 origin = transform.position + Vector3.up * 1f;
            Vector3 forward = transform.forward;
            float halfArc = meleeArc * 0.5f;

            Collider[] hits = Physics.OverlapSphere(origin, meleeRange);
            foreach (var c in hits)
            {
                if (c.gameObject == gameObject || c.transform.IsChildOf(transform)) continue;
                Vector3 to = (c.transform.position - origin); to.y = 0f;
                if (to.sqrMagnitude < 0.001f) continue;
                float ang = Vector3.Angle(forward, to.normalized);
                if (ang > halfArc) continue;
                var dmgr = c.GetComponentInParent<IDamageReceiver>();
                if (dmgr != null) dmgr.ReceiveDamage(dmg, c.transform.position);
            }
        }

        private void DoFireball()
        {
            if (_stats == null) return;
            if (!_stats.TrySpendMana(fireballManaCost)) return;
            _fireballReadyAt = Time.time + fireballCooldown;

            int dmg = _stats.attributes != null ? _stats.attributes.SpellPower * 4 : 25;
            Vector3 dir = _ctrl != null ? _ctrl.AimDirection() : transform.forward;
            Vector3 spawnPos = (staffTip != null ? staffTip.position : transform.position + Vector3.up * 1.2f) + dir * 0.4f;

            var go = new GameObject("Fireball");
            go.transform.position = spawnPos;
            go.transform.localScale = Vector3.one * 0.5f;

            var mesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mesh.transform.SetParent(go.transform, false);
            mesh.transform.localScale = Vector3.one * 0.6f;
            var col = mesh.GetComponent<Collider>();
            if (col != null) Destroy(col);
            var mr = mesh.GetComponent<MeshRenderer>();
            if (mr != null)
                mr.sharedMaterial = MaterialFactory.Get(new Color(1f, 0.55f, 0.1f), 0.4f, new Color(1f, 0.4f, 0.05f), 4f);

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
            if (_stats == null) return;
            if (!_stats.TrySpendMana(healManaCost)) return;
            _healReadyAt = Time.time + healCooldown;
            _stats.Heal(healAmount);

            // VFX placeholder: zielony błysk
            var go = new GameObject("HealFX");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.up * 1f;
            var light = go.AddComponent<Light>();
            light.color = new Color(0.4f, 1f, 0.5f);
            light.range = 4f;
            light.intensity = 5f;
            Destroy(go, 0.6f);
        }

        private void StartSwing()
        {
            if (handMount == null) return;
            _swingT = 0f;
            _swingFromLocal = Quaternion.Euler(-30f, 0f, 0f);
            _swingToLocal = Quaternion.Euler(90f, 0f, 0f);
        }

        private void TickSwing()
        {
            if (handMount == null || _swingT < 0f) return;
            _swingT += Time.deltaTime / SwingDuration;
            if (_swingT >= 1f)
            {
                _swingT = -1f;
                handMount.localRotation = Quaternion.Euler(-30f, 0f, 0f);
                return;
            }
            // Half forward then back
            float t = _swingT < 0.5f ? _swingT * 2f : (1f - _swingT) * 2f;
            handMount.localRotation = Quaternion.Slerp(_swingFromLocal, _swingToLocal, t);
        }
    }
}
