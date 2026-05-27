using UnityEngine;
using WPG.Core;
using WPG.World;

namespace WPG.Player
{
    public class FireballProjectile : MonoBehaviour
    {
        public float speed = 18f;
        public float lifetime = 4f;
        public int damage = 30;
        public float impactRadius = 1.5f;
        public LayerMask hitMask = ~0;

        private Vector3 _direction;
        private float _born;
        private GameObject _owner;

        public void Fire(Vector3 dir, int dmg, GameObject owner)
        {
            _direction = dir.normalized;
            damage = dmg;
            _born = Time.time;
            _owner = owner;
            transform.forward = _direction;
        }

        private void Update()
        {
            transform.position += _direction * speed * Time.deltaTime;
            if (Time.time - _born > lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // Hit detection - sphere overlap each frame
            Collider[] hits = Physics.OverlapSphere(transform.position, 0.6f, hitMask, QueryTriggerInteraction.Ignore);
            foreach (var h in hits)
            {
                if (h.gameObject == _owner) continue;
                if (h.transform.IsChildOf(_owner.transform)) continue;

                var dmgr = h.GetComponentInParent<IDamageReceiver>();
                if (dmgr != null)
                {
                    Explode();
                    return;
                }

                // Hit środowiska
                if (!h.isTrigger)
                {
                    Explode();
                    return;
                }
            }
        }

        private void Explode()
        {
            Collider[] all = Physics.OverlapSphere(transform.position, impactRadius, hitMask, QueryTriggerInteraction.Ignore);
            foreach (var c in all)
            {
                if (c.gameObject == _owner) continue;
                var dmgr = c.GetComponentInParent<IDamageReceiver>();
                if (dmgr != null)
                {
                    dmgr.ReceiveDamage(damage, transform.position);
                }
            }

            Destroy(gameObject);
        }
    }
}
