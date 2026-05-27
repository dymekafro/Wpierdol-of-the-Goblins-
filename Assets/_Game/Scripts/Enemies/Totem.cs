using System;
using UnityEngine;
using WPG.Core;
using WPG.World;

namespace WPG.Enemies
{
    public class Totem : MonoBehaviour, IDamageReceiver
    {
        public int maxHealth = 80;
        public float buffRadius = 18f;
        public GoblinCamp camp;

        private int _hp;
        private bool _dead;
        private Transform _barFill;
        private GameObject _barRoot;
        private Light _topLight;
        private float _nextBuffCheck;

        public event Action OnDestroyed;

        public bool IsDead => _dead;

        private void Awake()
        {
            _hp = maxHealth;
        }

        private void Start()
        {
            BuildVisual();
        }

        private void Update()
        {
            if (_dead) return;
            UpdateBar();
            if (Time.time >= _nextBuffCheck)
            {
                _nextBuffCheck = Time.time + 0.5f;
                ApplyBuffs();
            }
        }

        private void ApplyBuffs()
        {
            if (camp == null) return;
            foreach (var g in camp.Goblins)
            {
                if (g == null || g.IsDead) continue;
                float d = Vector3.Distance(g.transform.position, transform.position);
                g.ApplyTotemBuff(d <= buffRadius);
            }
        }

        public void ReceiveDamage(int amount, Vector3 hitPoint)
        {
            if (_dead) return;
            _hp -= amount;
            if (_hp <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            _dead = true;
            if (_topLight != null) _topLight.color = new Color(0.3f, 0.3f, 0.3f);
            if (_barRoot != null) _barRoot.SetActive(false);
            // Pochyl totem
            transform.rotation = Quaternion.Euler(15f, transform.eulerAngles.y, 5f);

            // usuń buffy
            if (camp != null)
            {
                foreach (var g in camp.Goblins)
                {
                    if (g != null) g.ApplyTotemBuff(false);
                }
            }

            OnDestroyed?.Invoke();
        }

        private void BuildVisual()
        {
            // Pień - cylinder
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(transform, false);
            trunk.transform.localScale = new Vector3(0.6f, 1.5f, 0.6f);
            trunk.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            var trunkCol = trunk.GetComponent<Collider>(); if (trunkCol != null) Destroy(trunkCol);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.18f, 0.12f, 0.08f));

            // Maska - cube
            var mask = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mask.transform.SetParent(transform, false);
            mask.transform.localScale = new Vector3(0.9f, 0.9f, 0.5f);
            mask.transform.localPosition = new Vector3(0f, 3.0f, 0f);
            var maskCol = mask.GetComponent<Collider>(); if (maskCol != null) Destroy(maskCol);
            mask.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.5f, 0.1f, 0.1f), 0.3f, new Color(0.6f, 0.05f, 0.05f), 0.6f);

            // Czaszka na górze - sphere
            var skull = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            skull.transform.SetParent(transform, false);
            skull.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            skull.transform.localPosition = new Vector3(0f, 3.8f, 0f);
            var skullCol = skull.GetComponent<Collider>(); if (skullCol != null) Destroy(skullCol);
            skull.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.85f, 0.78f, 0.6f), 0.2f, new Color(1f, 0.2f, 0.1f), 0.3f);

            // Czerwone światło
            var lgo = new GameObject("TotemLight");
            lgo.transform.SetParent(transform, false);
            lgo.transform.localPosition = new Vector3(0f, 3.8f, 0f);
            _topLight = lgo.AddComponent<Light>();
            _topLight.color = new Color(1f, 0.25f, 0.15f);
            _topLight.range = 16f;
            _topLight.intensity = 4f;

            // Collider
            var cap = gameObject.AddComponent<CapsuleCollider>();
            cap.height = 4.2f;
            cap.radius = 0.6f;
            cap.center = new Vector3(0f, 2.1f, 0f);

            // Pasek HP
            _barRoot = new GameObject("TotemBar");
            _barRoot.transform.SetParent(transform, false);
            _barRoot.transform.localPosition = new Vector3(0f, 4.6f, 0f);

            var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.transform.SetParent(_barRoot.transform, false);
            bg.transform.localScale = new Vector3(2f, 0.2f, 0.08f);
            var bgCol = bg.GetComponent<Collider>(); if (bgCol != null) Destroy(bgCol);
            bg.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.05f, 0.05f, 0.05f));

            var pivot = new GameObject("Pivot");
            pivot.transform.SetParent(_barRoot.transform, false);
            pivot.transform.localPosition = new Vector3(-1f, 0f, -0.05f);
            var fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fill.transform.SetParent(pivot.transform, false);
            fill.transform.localPosition = new Vector3(1f, 0f, 0f);
            fill.transform.localScale = new Vector3(2f, 0.2f, 0.08f);
            var fillCol = fill.GetComponent<Collider>(); if (fillCol != null) Destroy(fillCol);
            fill.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.9f, 0.2f, 0.2f), 0.3f, new Color(1f, 0.25f, 0.2f), 0.8f);
            _barFill = pivot.transform;
        }

        private void UpdateBar()
        {
            if (_barFill == null || _barRoot == null) return;
            float f = Mathf.Clamp01((float)_hp / Mathf.Max(1, maxHealth));
            _barFill.localScale = new Vector3(f, 1f, 1f);
            if (Camera.main != null) _barRoot.transform.forward = Camera.main.transform.forward;
        }

        public void DimLight()
        {
            if (_topLight != null) _topLight.color = new Color(0.4f, 0.4f, 0.5f);
        }
    }
}
