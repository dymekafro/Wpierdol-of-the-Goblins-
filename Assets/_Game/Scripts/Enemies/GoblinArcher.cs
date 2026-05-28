using UnityEngine;
using WPG.Core;
using WPG.World;

namespace WPG.Enemies
{
    public class GoblinArcher : GoblinBase
    {
        protected override WorldAssetPlacer.CharacterModelKind? AssetModelKind =>
            WorldAssetPlacer.CharacterModelKind.GoblinArcher;

        public float kiteRange = 5.5f;
        public float idealRange = 9f;
        public float maxRange = 14f;
        public float shotCooldown = 1.8f;
        public int arrowDamage = 8;

        protected override void Awake()
        {
            base.Awake();
            displayName = "Goblin Łucznik";
            maxHealth = 20;
            damage = arrowDamage;
            moveSpeed = 3.0f;
            baseColor = new Color(0.35f, 0.5f, 0.3f);
            detectRange = 18f;
        }

        protected override void OnFantasyGoblinAttached(WorldAssetPlacer.CharacterAttachResult attach)
        {
            TryAttachBow(attach.HandMount ?? attach.ModelRoot);
        }

        void TryAttachBow(Transform parent)
        {
            if (parent == null) return;
            var bow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bow.name = "Bow_Prop";
            bow.transform.SetParent(parent, false);
            bow.transform.localScale = new Vector3(0.06f, 0.55f, 0.06f);
            bow.transform.localPosition = new Vector3(0.12f, 0f, 0.15f);
            bow.transform.localRotation = Quaternion.Euler(0f, 90f, 25f);
            var col = bow.GetComponent<Collider>();
            if (col != null) Destroy(col);
            bow.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Get(new Color(0.3f, 0.2f, 0.1f));
        }

        protected override void Start()
        {
            base.Start();

            if (VisualFromAsset) return;

            // Luk placeholder gdy brak modelu 3D
            var bow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bow.transform.SetParent(transform, false);
            bow.transform.localScale = new Vector3(0.08f, 0.7f, 0.08f);
            bow.transform.localPosition = new Vector3(scale * 0.4f, scale * 1.1f, 0f);
            var col = bow.GetComponent<Collider>(); if (col != null) Destroy(col);
            var mr = bow.GetComponent<MeshRenderer>();
            mr.sharedMaterial = MaterialFactory.Get(new Color(0.3f, 0.2f, 0.1f));
        }

        protected override void UpdateAI()
        {
            if (target == null)
            {
                State = EnemyState.Idle;
                return;
            }
            float dist = Vector3.Distance(transform.position, target.position);

            switch (State)
            {
                case EnemyState.Idle:
                    if (dist < detectRange) State = EnemyState.Chase;
                    break;
                case EnemyState.Chase:
                    if (dist > loseRange) { State = EnemyState.Return; break; }
                    if (dist < kiteRange) { State = EnemyState.Flee; break; }
                    if (dist < maxRange) State = EnemyState.Attack;
                    else MoveTowardsXZ(target.position);
                    break;
                case EnemyState.Attack:
                    if (dist < kiteRange) { State = EnemyState.Flee; break; }
                    if (dist > maxRange) { State = EnemyState.Chase; break; }
                    FacePlayer();
                    if (Time.time >= NextAttackAt)
                    {
                        NextAttackAt = Time.time + shotCooldown;
                        ShootArrow();
                    }
                    // Kork korekcji dystansu
                    if (dist > idealRange + 1.5f) MoveTowardsXZ(target.position, 0.5f);
                    break;
                case EnemyState.Flee:
                    FacePlayer();
                    Vector3 away = transform.position + (transform.position - target.position).normalized * 3f;
                    MoveTowardsXZ(away, 1.1f);
                    if (dist > idealRange) State = EnemyState.Attack;
                    if (Time.time >= NextAttackAt)
                    {
                        NextAttackAt = Time.time + shotCooldown * 1.3f;
                        ShootArrow();
                    }
                    break;
                case EnemyState.Return:
                    if (Vector3.Distance(transform.position, homePosition) < 0.5f) State = EnemyState.Idle;
                    else MoveTowardsXZ(homePosition);
                    if (dist < detectRange * 0.8f) State = EnemyState.Chase;
                    break;
            }
        }

        private void ShootArrow()
        {
            if (target == null) return;
            NotifyAttackAnim();
            Vector3 origin = transform.position + Vector3.up * (scale * 1.3f) + transform.forward * 0.4f;
            Vector3 dir = (target.position + Vector3.up * 1f - origin).normalized;

            int dmg = buffedByTotem ? Mathf.RoundToInt(arrowDamage * 1.5f) : arrowDamage;

            var go = new GameObject("GoblinArrow");
            go.transform.position = origin;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.transform.SetParent(go.transform, false);
            visual.transform.localScale = new Vector3(0.05f, 0.3f, 0.05f);
            visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var col = visual.GetComponent<Collider>(); if (col != null) Destroy(col);
            var mr = visual.GetComponent<MeshRenderer>();
            mr.sharedMaterial = MaterialFactory.Get(new Color(0.45f, 0.3f, 0.15f));

            var arrow = go.AddComponent<GoblinArrow>();
            arrow.Fire(dir, dmg, gameObject);
        }
    }
}
