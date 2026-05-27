using UnityEngine;
using WPG.Core;

namespace WPG.Enemies
{
    public class GoblinStormtrooper : GoblinBase
    {
        public float attackRange = 1.9f;

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
            var dmgr = target != null ? target.GetComponentInParent<IDamageReceiver>() : null;
            if (dmgr != null) dmgr.ReceiveDamage(dmg, target.position);

            // Wizualny lunge - mały skok do przodu
            transform.position += transform.forward * 0.3f;
        }
    }
}
