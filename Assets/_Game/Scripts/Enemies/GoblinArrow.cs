using UnityEngine;
using WPG.Core;

namespace WPG.Enemies
{
    public class GoblinArrow : MonoBehaviour
    {
        public float speed = 16f;
        public int damage = 8;
        public float lifetime = 4f;

        private Vector3 _dir;
        private float _born;
        private GameObject _owner;

        public void Fire(Vector3 dir, int dmg, GameObject owner)
        {
            _dir = dir.normalized;
            damage = dmg;
            _born = Time.time;
            _owner = owner;
            if (_dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(_dir, Vector3.up);
        }

        private void Update()
        {
            transform.position += _dir * speed * Time.deltaTime;
            if (Time.time - _born > lifetime)
            {
                Destroy(gameObject);
                return;
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, 0.25f);
            foreach (var c in hits)
            {
                if (_owner != null && (c.gameObject == _owner || c.transform.IsChildOf(_owner.transform))) continue;
                if (c.isTrigger) continue;

                var dmgr = c.GetComponentInParent<IDamageReceiver>();
                if (dmgr is GoblinBase || dmgr is Totem) continue; // nie strzelamy swoich

                if (dmgr != null)
                {
                    dmgr.ReceiveDamage(damage, transform.position);
                    Destroy(gameObject);
                    return;
                }

                // Ściana
                if (c.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
}
