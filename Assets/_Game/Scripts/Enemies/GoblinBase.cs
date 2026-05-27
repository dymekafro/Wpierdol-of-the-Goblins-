using System;
using UnityEngine;
using WPG.Core;
using WPG.World;

namespace WPG.Enemies
{
    public enum GoblinKind { Stormtrooper, Archer, Shaman }
    public enum EnemyState { Idle, Detect, Chase, Attack, Flee, Return, Dead }

    public abstract class GoblinBase : MonoBehaviour, IDamageReceiver
    {
        public string displayName = "Goblin";
        public int maxHealth = 30;
        public int damage = 5;
        public float moveSpeed = 3.2f;
        public float detectRange = 14f;
        public float loseRange = 22f;
        public float attackCooldown = 1.2f;
        public Color baseColor = new Color(0.45f, 0.55f, 0.25f);
        public float scale = 0.85f;

        public Transform target;            // gracz
        public GoblinCamp camp;             // obóz źródłowy
        public Vector3 homePosition;        // pozycja powrotu
        public bool buffedByTotem;          // zwiększa damage/HP od totemu

        protected int CurrentHealth;
        protected float NextAttackAt;
        protected EnemyState State = EnemyState.Idle;
        protected Renderer[] Renderers;
        protected Material EyeMat;
        protected GameObject HealthBarRoot;
        protected Transform HealthBarFill;

        public bool IsDead => State == EnemyState.Dead;

        public event Action<GoblinBase> OnDeath;

        protected virtual void Awake()
        {
            homePosition = transform.position;
        }

        protected virtual void Start()
        {
            CurrentHealth = maxHealth;
            BuildVisual();
            BuildHealthBar();
        }

        protected virtual void Update()
        {
            if (IsDead) return;
            if (target == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) target = p.transform;
            }
            UpdateAI();
            UpdateHealthBar();
        }

        protected abstract void UpdateAI();

        protected void FacePlayer()
        {
            if (target == null) return;
            Vector3 to = target.position - transform.position; to.y = 0f;
            if (to.sqrMagnitude < 0.001f) return;
            Quaternion rot = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 8f);
        }

        protected void MoveTowardsXZ(Vector3 destination, float speedMult = 1f)
        {
            Vector3 to = destination - transform.position;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist < 0.05f) return;
            Vector3 step = to.normalized * (moveSpeed * speedMult) * Time.deltaTime;
            // proste obejście przeszkód: jeśli wykryjemy collider tuż przed, lekko skręcamy
            if (Physics.SphereCast(transform.position + Vector3.up * 0.6f, 0.35f, to.normalized, out RaycastHit hit, 0.7f))
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
                {
                    Vector3 left = Quaternion.Euler(0, -45, 0) * to.normalized;
                    step = left * (moveSpeed * speedMult) * Time.deltaTime;
                }
            }
            transform.position += step;
            Vector3 dir = step; dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 6f);
            }
        }

        public void ReceiveDamage(int amount, Vector3 hitPoint)
        {
            if (IsDead) return;
            CurrentHealth -= amount;
            // szybki flash via emission
            FlashHit();
            if (CurrentHealth <= 0)
            {
                Die();
            }
            else if (State == EnemyState.Idle && target != null)
            {
                State = EnemyState.Chase;
            }
        }

        protected virtual void Die()
        {
            State = EnemyState.Dead;
            OnDeath?.Invoke(this);
            if (HealthBarRoot != null) HealthBarRoot.SetActive(false);
            // Padnij na ziemię
            transform.rotation = Quaternion.Euler(80f, transform.eulerAngles.y, 0f);
            // Zniknij po chwili
            Destroy(gameObject, 4f);
        }

        protected void FlashHit()
        {
            if (Renderers == null || Renderers.Length == 0) return;
            CancelInvoke(nameof(ResetEmission));
            foreach (var r in Renderers)
            {
                if (r.material.HasProperty("_EmissionColor"))
                {
                    r.material.EnableKeyword("_EMISSION");
                    r.material.SetColor("_EmissionColor", Color.red * 2f);
                }
            }
            Invoke(nameof(ResetEmission), 0.08f);
        }

        private void ResetEmission()
        {
            if (Renderers == null) return;
            foreach (var r in Renderers)
            {
                if (r.material.HasProperty("_EmissionColor"))
                    r.material.SetColor("_EmissionColor", Color.black);
            }
        }

        protected virtual void BuildVisual()
        {
            // Korpus
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform, false);
            body.transform.localScale = new Vector3(scale * 0.9f, scale * 0.85f, scale * 0.9f);
            body.transform.localPosition = new Vector3(0f, scale * 0.85f, 0f);
            var bodyCol = body.GetComponent<Collider>(); if (bodyCol != null) Destroy(bodyCol);
            var bodyMR = body.GetComponent<MeshRenderer>();
            bodyMR.sharedMaterial = MaterialFactory.Get(baseColor);

            // Głowa
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(transform, false);
            head.transform.localScale = new Vector3(scale * 0.42f, scale * 0.42f, scale * 0.42f);
            head.transform.localPosition = new Vector3(0f, scale * 1.55f, 0f);
            var headCol = head.GetComponent<Collider>(); if (headCol != null) Destroy(headCol);
            var headMR = head.GetComponent<MeshRenderer>();
            Color headCol2 = baseColor * 0.85f; headCol2.a = 1f;
            headMR.sharedMaterial = MaterialFactory.Get(headCol2);

            // Oczy świecące czerwone
            EyeMat = MaterialFactory.Get(new Color(1f, 0.1f, 0.1f), 0.3f, new Color(1f, 0.1f, 0.1f), 3f);
            BuildEye(scale, EyeMat, new Vector3(-0.08f, scale * 1.6f, scale * 0.16f));
            BuildEye(scale, EyeMat, new Vector3(0.08f, scale * 1.6f, scale * 0.16f));

            // Collider właściwy
            var cap = gameObject.AddComponent<CapsuleCollider>();
            cap.height = scale * 1.8f;
            cap.radius = scale * 0.4f;
            cap.center = new Vector3(0f, scale * 0.9f, 0f);

            Renderers = GetComponentsInChildren<Renderer>();
        }

        protected void BuildEye(float s, Material mat, Vector3 localPos)
        {
            var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(transform, false);
            eye.transform.localScale = Vector3.one * s * 0.08f;
            eye.transform.localPosition = localPos;
            var col = eye.GetComponent<Collider>(); if (col != null) Destroy(col);
            var mr = eye.GetComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
        }

        protected void BuildHealthBar()
        {
            var root = new GameObject("HealthBar");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, scale * 2.2f, 0f);

            // Tło
            var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.transform.SetParent(root.transform, false);
            bg.transform.localScale = new Vector3(1.2f, 0.12f, 0.05f);
            var bgCol = bg.GetComponent<Collider>(); if (bgCol != null) Destroy(bgCol);
            var bgMR = bg.GetComponent<MeshRenderer>();
            bgMR.sharedMaterial = MaterialFactory.Get(new Color(0.05f, 0.05f, 0.05f));

            // Wypełnienie - pivot lewy
            var fillPivot = new GameObject("FillPivot");
            fillPivot.transform.SetParent(root.transform, false);
            fillPivot.transform.localPosition = new Vector3(-0.6f, 0f, -0.03f);
            var fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fill.transform.SetParent(fillPivot.transform, false);
            fill.transform.localPosition = new Vector3(0.6f, 0f, 0f);
            fill.transform.localScale = new Vector3(1.2f, 0.12f, 0.05f);
            var fillCol = fill.GetComponent<Collider>(); if (fillCol != null) Destroy(fillCol);
            var fillMR = fill.GetComponent<MeshRenderer>();
            fillMR.sharedMaterial = MaterialFactory.Get(new Color(0.9f, 0.15f, 0.15f), 0.3f, new Color(1f, 0.2f, 0.15f), 0.6f);
            HealthBarFill = fillPivot.transform;
            HealthBarRoot = root;
        }

        protected void UpdateHealthBar()
        {
            if (HealthBarRoot == null || HealthBarFill == null) return;
            float frac = Mathf.Clamp01((float)CurrentHealth / Mathf.Max(1, maxHealth));
            HealthBarFill.localScale = new Vector3(frac, 1f, 1f);

            // Bilbordowanie
            if (Camera.main != null)
            {
                HealthBarRoot.transform.forward = Camera.main.transform.forward;
            }
        }

        public void ApplyTotemBuff(bool on)
        {
            if (buffedByTotem == on) return;
            buffedByTotem = on;
            // Wizualnie: lekka aura w postaci silniejszego emission
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                if (r.material.HasProperty("_EmissionColor"))
                {
                    r.material.EnableKeyword("_EMISSION");
                    r.material.SetColor("_EmissionColor", on ? new Color(0.3f, 0.05f, 0.0f) : Color.black);
                }
            }
        }
    }
}
