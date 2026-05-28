using UnityEngine;
using WPG.Core;
using WPG.World;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WPG.Enemies
{
    public class GoblinStormtrooper : GoblinBase
    {
        public float attackRange = 1.9f;

        protected override WorldAssetPlacer.CharacterModelKind? AssetModelKind =>
            WorldAssetPlacer.CharacterModelKind.GoblinMelee;

        protected override void OnFantasyGoblinAttached(WorldAssetPlacer.CharacterAttachResult attach)
        {
            TryAttachMeleeWeapon(attach.HandMount);
        }

        static void TryAttachMeleeWeapon(Transform handMount)
        {
            if (handMount == null) return;
#if UNITY_EDITOR
            var weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameAssetPaths.FantasyGoblinMeleeWeaponPrefab);
            if (weaponPrefab == null) return;
            var weapon = Object.Instantiate(weaponPrefab, handMount);
            weapon.name = "MeleeWeapon";
            weapon.transform.localPosition = new Vector3(0.05f, 0f, 0.08f);
            weapon.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            weapon.transform.localScale = Vector3.one * 0.85f;
            MaterialUpgrader.UpgradeHierarchy(weapon);
            foreach (var col in weapon.GetComponentsInChildren<Collider>()) Object.Destroy(col);
#endif
        }

        protected override void Awake()
        {
            base.Awake();
            displayName = "Goblin Szturmowiec";
            maxHealth = 30;
            damage = 5;
            moveSpeed = 3.6f;
            baseColor = new Color(0.45f, 0.55f, 0.25f);
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
                    if (dist < attackRange) State = EnemyState.Attack;
                    else MoveTowardsXZ(target.position);
                    break;
                case EnemyState.Attack:
                    FacePlayer();
                    if (dist > attackRange * 1.1f) { State = EnemyState.Chase; break; }
                    if (Time.time >= NextAttackAt)
                    {
                        NextAttackAt = Time.time + attackCooldown;
                        DealDamage();
                    }
                    break;
                case EnemyState.Return:
                    if (Vector3.Distance(transform.position, homePosition) < 0.5f) State = EnemyState.Idle;
                    else MoveTowardsXZ(homePosition);
                    if (dist < detectRange * 0.8f) State = EnemyState.Chase;
                    break;
            }
        }

        private void DealDamage()
        {
            int dmg = buffedByTotem ? Mathf.RoundToInt(damage * 1.4f) : damage;
            NotifyAttackAnim();
            var dmgr = target != null ? target.GetComponentInParent<IDamageReceiver>() : null;
            if (dmgr != null) dmgr.ReceiveDamage(dmg, target.position);

            transform.position += transform.forward * 0.3f;
        }
    }
}
