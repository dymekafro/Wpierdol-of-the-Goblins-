using System;
using UnityEngine;
using WPG.Character;
using WPG.Core;
using WPG.World;
using WPG.UI;
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
        public float hopSoundInterval = 0.45f;
        public Color baseColor = new Color(0.45f, 0.55f, 0.25f);
        public float scale = 0.85f;

        public Transform target;
        public GoblinCamp camp;
        public Vector3 homePosition;
        public bool buffedByTotem;

        protected int CurrentHealth;
        protected float NextAttackAt;
        protected EnemyState State = EnemyState.Idle;
        protected Renderer[] Renderers;
        protected Material EyeMat;
        protected GameObject HealthBarRoot;
        protected Transform HealthBarFill;
        protected bool VisualFromAsset;
        protected CharacterAnimDriver AnimDriver;

        float _currentMoveSpeed;

        public bool IsDead => State == EnemyState.Dead;

        public event Action<GoblinBase> OnDeath;

        protected virtual void Awake()
        {
            homePosition = transform.position;
        }

        protected virtual void Start()
        {
            GameAssetRegistry.Initialize();
            CurrentHealth = maxHealth;
            BuildVisual();
            BuildHealthBar();
        }

        protected virtual void Update()
        {
            if (IsDead) return;
            if (target == null && Time.time >= _nextTargetSearchAt)
            {
                _nextTargetSearchAt = Time.time + 0.5f;
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) target = p.transform;
            }

            _movedThisFrame = false;
            UpdateAI();
            SnapToGround();
            UpdateHealthBar();

            if (!_movedThisFrame)
                _currentMoveSpeed = Mathf.MoveTowards(_currentMoveSpeed, 0f, moveSpeed * Time.deltaTime * 6f);

            // Podskakiwanie goblina podczas ruchu — cykliczny dźwięk odbicia.
            if (_movedThisFrame && Time.time >= _nextHopAt)
            {
                _nextHopAt = Time.time + hopSoundInterval;
                GameAudioManager.EnsureExists()?.PlayGoblinHop(transform.position);
            }

            if (AnimDriver != null)
            {
                AnimDriver.SetSpeed(_currentMoveSpeed, moveSpeed);
                AnimDriver.SetGrounded(true);
            }
        }

        protected abstract void UpdateAI();

        // Gobliny poruszają się tylko w płaszczyźnie XZ (MoveTowardsXZ), więc co klatkę
        // dociskamy je do pofalowanego terenu. Na płaskim świecie (brak konfiguracji) = Y 0.
        void SnapToGround()
        {
            Vector3 p = transform.position;
            p.y = WorldGround.GetGroundHeight(p.x, p.z);
            transform.position = p;
        }

        protected void NotifyAttackAnim()
        {
            if (AnimDriver != null) AnimDriver.TriggerAttack();
        }

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

            _currentMoveSpeed = moveSpeed * speedMult;
            _movedThisFrame = true;
        }

        public void ReceiveDamage(int amount, Vector3 hitPoint)
        {
            if (IsDead) return;
            if (amount <= 0) return;

            const float criticalChance = 0.08f;

            bool isCritical = UnityEngine.Random.value <= criticalChance;
            int finalDamage = isCritical ? amount * 2 : amount;

            Color damageColor = isCritical
                ? new Color(1f, 0.9f, 0.05f, 1f)      // krytyk - żółty
                : new Color(1f, 0.22f, 0.12f, 1f);    // zwykłe obrażenia - czerwony

            CurrentHealth -= finalDamage;

            DamageNumber.Show(
                finalDamage,
                transform.position + Vector3.up * 2.2f,
                damageColor,
                isCritical
            );

            GameAudioManager.EnsureExists()?.PlayHit(hitPoint);
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
            GameAudioManager.EnsureExists()?.PlayDeath(transform.position);
            OnDeath?.Invoke(this);
            if (HealthBarRoot != null) HealthBarRoot.SetActive(false);
            if (AnimDriver != null) AnimDriver.SetDead(true);
            transform.rotation = Quaternion.Euler(80f, transform.eulerAngles.y, 0f);
            Destroy(gameObject, 4f);
        }

        protected void FlashHit()
        {
            if (Renderers == null || Renderers.Length == 0) return;
            CancelInvoke(nameof(ResetEmission));
            SetEmission(Color.red * 2f);
            Invoke(nameof(ResetEmission), 0.08f);
        }

        private void ResetEmission()
        {
            SetEmission(Color.black);
        }

        // MaterialPropertyBlock zamiast r.material — bez klonowania materiałów (mniej GC, lepszy batching).
        private void SetEmission(Color color)
        {
            if (Renderers == null) return;
            _flashBlock ??= new MaterialPropertyBlock();
            foreach (var r in Renderers)
            {
                if (r == null) continue;
                var mat = r.sharedMaterial;
                if (mat == null || !mat.HasProperty(EmissionColorId)) continue;
                mat.EnableKeyword("_EMISSION");
                r.GetPropertyBlock(_flashBlock);
                _flashBlock.SetColor(EmissionColorId, color);
                r.SetPropertyBlock(_flashBlock);
            }
        }

        protected virtual WorldAssetPlacer.CharacterModelKind? AssetModelKind => null;

        /// <summary>Wysokość modelu względem gracza (~0.8–1.0).</summary>
        protected virtual float ModelHeightMultiplier => 1.65f;

        protected virtual float ModelScaleMultiplier => WorldAssetPlacer.GoblinCharacterModelScale;

        bool _movedThisFrame;
        float _nextHopAt;
        float _nextTargetSearchAt;
        MaterialPropertyBlock _flashBlock;
        static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        protected virtual void BuildVisual()
        {
            EnsureAnimDriver();
            var kind = AssetModelKind;
            WorldAssetPlacer.CharacterAttachResult attach = default;
            float targetHeight = scale * ModelHeightMultiplier;

            if (TryWireEmbeddedGoblinModel(kind, targetHeight, out attach))
            {
                VisualFromAsset = true;
                Renderers = GetComponentsInChildren<Renderer>();
                OnFantasyGoblinAttached(attach);
                var cap = gameObject.AddComponent<CapsuleCollider>();
                cap.height = targetHeight;
                cap.radius = scale * 0.4f * WorldAssetPlacer.GoblinCharacterModelScale;
                cap.center = new Vector3(0f, targetHeight * 0.5f, 0f);
                return;
            }

            if (kind.HasValue && WorldAssetPlacer.TryAttachCharacterModel(
                    transform, kind.Value, targetHeight, out attach, ModelScaleMultiplier, baseColor))
            {
                VisualFromAsset = true;
                Renderers = GetComponentsInChildren<Renderer>();
                Debug.Log($"[GoblinBase] Attached Fantasy Goblin: {attach.PrefabPath}");
                if (!attach.AnimatorOk)
                    Debug.Log($"[GoblinBase] Animator: brak controllera — proceduralna animacja (rig bones)");
                if (AnimDriver != null && attach.ModelRoot != null)
                    GoblinAnimSetup.WireDriver(transform, attach.ModelRoot, AnimDriver);
                else if (AnimDriver != null)
                {
                    if (attach.BodyPivot != null) AnimDriver.bodyPivot = attach.BodyPivot;
                    else if (attach.ModelRoot != null) AnimDriver.bodyPivot = attach.ModelRoot;
                    AnimDriver.handMount = attach.HandMount;
                    AnimDriver.leftArm = attach.LeftArm;
                    AnimDriver.rightArm = attach.RightArm;
                    AnimDriver.leftLeg = attach.LeftLeg;
                    AnimDriver.rightLeg = attach.RightLeg;
                    AnimDriver.RefreshBaseTransforms();
                }
                OnFantasyGoblinAttached(attach);
                var cap = gameObject.AddComponent<CapsuleCollider>();
                cap.height = targetHeight;
                cap.radius = scale * 0.4f * WorldAssetPlacer.GoblinCharacterModelScale;
                cap.center = new Vector3(0f, targetHeight * 0.5f, 0f);
                return;
            }

            string missReason = kind.HasValue ? attach.FailureReason : "brak AssetModelKind";
            Debug.LogWarning($"[GoblinBase] FALLBACK placeholder — reason: {missReason}");
            BuildPlaceholderVisual();
        }

        bool TryWireEmbeddedGoblinModel(WorldAssetPlacer.CharacterModelKind? kind, float targetHeight, out WorldAssetPlacer.CharacterAttachResult attach)
        {
            attach = default;
            if (!kind.HasValue) return false;

            var skinned = GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (skinned == null) return false;

            var modelRoot = FindGoblinModelRoot(skinned.transform);
            if (modelRoot == null) return false;

            MaterialUpgrader.UpgradeHierarchy(modelRoot.gameObject);
            GoblinAnimSetup.EnsureAnimator(modelRoot);
            GoblinAnimSetup.WireDriver(transform, modelRoot, AnimDriver);

            float currentHeight = ComputeRendererHeight(modelRoot);
            if (currentHeight > 0.01f)
            {
                float scaleFactor = (targetHeight / currentHeight) * ModelScaleMultiplier;
                modelRoot.localScale *= scaleFactor;
            }
            else
            {
                modelRoot.localScale *= ModelScaleMultiplier;
            }

            attach.Success = true;
            attach.ModelRoot = modelRoot;
            attach.PrefabPath = "embedded/WorldRoot";
            attach.ModelSource = "EmbeddedGoblin";
            var animator = modelRoot.GetComponent<Animator>();
            attach.AnimatorOk = animator != null && animator.runtimeAnimatorController != null;
            attach.AnimatorStatus = attach.AnimatorOk ? "OK (embedded + Invector)" : "procedural rig";
            return true;
        }

        static Transform FindGoblinModelRoot(Transform skinnedTransform)
        {
            foreach (var t in skinnedTransform.GetComponentsInParent<Transform>(true))
            {
                if (t.name.Equals("root", StringComparison.OrdinalIgnoreCase) && t.parent != null)
                    return t.parent;
            }

            var walk = skinnedTransform;
            while (walk.parent != null)
            {
                if (walk.parent.GetComponent<GoblinBase>() != null)
                    return walk;
                walk = walk.parent;
            }

            return skinnedTransform.parent != null ? skinnedTransform.parent : skinnedTransform;
        }

        static float ComputeRendererHeight(Transform modelRoot)
        {
            var renderers = modelRoot.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return 0f;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds.size.y;
        }

        /// <summary>Hook po podpięciu Fantasy Goblin (broń, dodatkowy tint itd.).</summary>
        protected virtual void OnFantasyGoblinAttached(WorldAssetPlacer.CharacterAttachResult attach) { }

        protected void ApplyHierarchyMaterialTint(Color tint, float mix = 0.35f)
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                if (r == null) continue;
                var mat = r.material;
                if (mat == null) continue;
                var baseCol = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
                var blended = Color.Lerp(baseCol, tint, mix);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", blended);
                else mat.color = blended;
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", tint * 0.25f);
                }
            }
        }

        void EnsureAnimDriver()
        {
            if (AnimDriver == null) AnimDriver = GetComponent<CharacterAnimDriver>();
            if (AnimDriver == null) AnimDriver = gameObject.AddComponent<CharacterAnimDriver>();
        }

        protected void BuildPlaceholderVisual()
        {
            EnsureAnimDriver();
            float s = scale * WorldAssetPlacer.GoblinCharacterModelScale;

            var pivot = new GameObject("BodyPivot").transform;
            pivot.SetParent(transform, false);
            pivot.localPosition = Vector3.zero;

            Color torsoColor = baseColor;
            Color skinColor = baseColor * 0.85f; skinColor.a = 1f;
            Color limbColor = baseColor * 0.7f; limbColor.a = 1f;
            Color loinclothColor = new Color(0.18f, 0.13f, 0.08f);
            var torsoMat = MaterialFactory.Get(torsoColor);
            var skinMat = MaterialFactory.Get(skinColor);
            var limbMat = MaterialFactory.Get(limbColor);
            var loinMat = MaterialFactory.Get(loinclothColor);

            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(pivot, false);
            torso.transform.localScale = new Vector3(s * 0.78f, s * 0.5f, s * 0.55f);
            torso.transform.localPosition = new Vector3(0f, s * 1.05f, 0f);
            DestroyCol(torso);
            torso.GetComponent<MeshRenderer>().sharedMaterial = torsoMat;

            var hips = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hips.name = "Hips";
            hips.transform.SetParent(pivot, false);
            hips.transform.localScale = new Vector3(s * 0.7f, s * 0.3f, s * 0.55f);
            hips.transform.localPosition = new Vector3(0f, s * 0.75f, 0f);
            DestroyCol(hips);
            hips.GetComponent<MeshRenderer>().sharedMaterial = loinMat;

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(pivot, false);
            head.transform.localScale = new Vector3(s * 0.45f, s * 0.45f, s * 0.5f);
            head.transform.localPosition = new Vector3(0f, s * 1.6f, s * 0.02f);
            DestroyCol(head);
            head.GetComponent<MeshRenderer>().sharedMaterial = skinMat;

            var earL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            earL.name = "Ear_L";
            earL.transform.SetParent(head.transform, false);
            earL.transform.localScale = new Vector3(0.45f, 0.18f, 0.18f);
            earL.transform.localPosition = new Vector3(-0.55f, 0.15f, -0.1f);
            earL.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
            DestroyCol(earL);
            earL.GetComponent<MeshRenderer>().sharedMaterial = skinMat;

            var earR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            earR.name = "Ear_R";
            earR.transform.SetParent(head.transform, false);
            earR.transform.localScale = new Vector3(0.45f, 0.18f, 0.18f);
            earR.transform.localPosition = new Vector3(0.55f, 0.15f, -0.1f);
            earR.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
            DestroyCol(earR);
            earR.GetComponent<MeshRenderer>().sharedMaterial = skinMat;

            EyeMat = MaterialFactory.Get(new Color(1f, 0.1f, 0.1f), 0.3f, new Color(1f, 0.1f, 0.1f), 3f);
            BuildEye(s, EyeMat, new Vector3(-0.08f, s * 1.62f, s * 0.22f));
            BuildEye(s, EyeMat, new Vector3(0.08f, s * 1.62f, s * 0.22f));

            var leftArm = BuildLimb(pivot, "LeftArm", new Vector3(-s * 0.45f, s * 1.25f, 0f),
                new Vector3(s * 0.18f, s * 0.30f, s * 0.18f), new Vector3(0f, -s * 0.30f, 0f), limbMat);
            var rightArm = BuildLimb(pivot, "RightArm", new Vector3(s * 0.45f, s * 1.25f, 0f),
                new Vector3(s * 0.18f, s * 0.30f, s * 0.18f), new Vector3(0f, -s * 0.30f, 0f), limbMat);
            var leftLeg = BuildLimb(pivot, "LeftLeg", new Vector3(-s * 0.18f, s * 0.55f, 0f),
                new Vector3(s * 0.20f, s * 0.32f, s * 0.20f), new Vector3(0f, -s * 0.32f, 0f), limbMat);
            var rightLeg = BuildLimb(pivot, "RightLeg", new Vector3(s * 0.18f, s * 0.55f, 0f),
                new Vector3(s * 0.20f, s * 0.32f, s * 0.20f), new Vector3(0f, -s * 0.32f, 0f), limbMat);

            var handMount = new GameObject("HandMount").transform;
            handMount.SetParent(rightArm, false);
            handMount.localPosition = new Vector3(0f, -s * 0.55f, s * 0.1f);

            var cap = gameObject.AddComponent<CapsuleCollider>();
            cap.height = s * 1.8f;
            cap.radius = s * 0.4f;
            cap.center = new Vector3(0f, s * 0.9f, 0f);

            Renderers = GetComponentsInChildren<Renderer>();

            if (AnimDriver != null)
            {
                AnimDriver.bodyPivot = pivot;
                AnimDriver.headPivot = head.transform;
                AnimDriver.handMount = handMount;
                AnimDriver.leftArm = leftArm;
                AnimDriver.rightArm = rightArm;
                AnimDriver.leftLeg = leftLeg;
                AnimDriver.rightLeg = rightLeg;
            }
        }

        Transform BuildLimb(Transform parent, string name, Vector3 jointPos, Vector3 limbScale, Vector3 limbOffset, Material mat)
        {
            var p = new GameObject(name).transform;
            p.SetParent(parent, false);
            p.localPosition = jointPos;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = name + "_Mesh";
            visual.transform.SetParent(p, false);
            visual.transform.localScale = limbScale;
            visual.transform.localPosition = limbOffset;
            DestroyCol(visual);
            var mr = visual.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
            return p;
        }

        void DestroyCol(GameObject go)
        {
            var c = go.GetComponent<Collider>();
            if (c != null) Destroy(c);
        }

        protected void BuildEye(float s, Material mat, Vector3 localPos)
        {
            var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(transform, false);
            eye.transform.localScale = Vector3.one * s * 0.08f;
            eye.transform.localPosition = localPos;
            DestroyCol(eye);
            var mr = eye.GetComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
        }

        protected void BuildHealthBar()
        {
            float barHeight = scale * WorldAssetPlacer.GoblinCharacterModelScale * 2.2f;
            var root = new GameObject("HealthBar");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, barHeight, 0f);

            var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.transform.SetParent(root.transform, false);
            bg.transform.localScale = new Vector3(1.2f, 0.12f, 0.05f);
            DestroyCol(bg);
            bg.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.05f, 0.05f, 0.05f));

            var fillPivot = new GameObject("FillPivot");
            fillPivot.transform.SetParent(root.transform, false);
            fillPivot.transform.localPosition = new Vector3(-0.6f, 0f, -0.03f);
            var fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fill.transform.SetParent(fillPivot.transform, false);
            fill.transform.localPosition = new Vector3(0.6f, 0f, 0f);
            fill.transform.localScale = new Vector3(1.2f, 0.12f, 0.05f);
            DestroyCol(fill);
            fill.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.9f, 0.15f, 0.15f), 0.3f, new Color(1f, 0.2f, 0.15f), 0.6f);
            HealthBarFill = fillPivot.transform;
            HealthBarRoot = root;
        }

        protected void UpdateHealthBar()
        {
            if (HealthBarRoot == null || HealthBarFill == null) return;
            float frac = Mathf.Clamp01((float)CurrentHealth / Mathf.Max(1, maxHealth));
            HealthBarFill.localScale = new Vector3(frac, 1f, 1f);

            var cam = CameraCache.Main;
            if (cam != null)
            {
                HealthBarRoot.transform.forward = cam.transform.forward;
            }
        }

        public void ApplyTotemBuff(bool on)
        {
            if (buffedByTotem == on) return;
            buffedByTotem = on;
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
