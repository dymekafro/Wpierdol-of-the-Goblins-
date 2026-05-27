using UnityEngine;
using WPG.Character;
using WPG.Core;
using WPG.World;

namespace WPG.Player
{
    public static class PlayerBuilder
    {
        public const string PlayerLayer = "Default";

        public static GameObject BuildDruid(Vector3 spawnPos, PlayerAttributes attrs, out PlayerStats stats, out PlayerController ctrl, out PlayerCombat combat)
        {
            var go = new GameObject("Player_Druid");
            go.tag = "Player";
            go.transform.position = spawnPos;

            // CharacterController
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.85f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.93f, 0f);
            cc.stepOffset = 0.4f;
            cc.slopeLimit = 50f;

            // Stats + Controller + Combat
            stats = go.AddComponent<PlayerStats>();
            stats.Init(attrs);
            ctrl = go.AddComponent<PlayerController>();
            combat = go.AddComponent<PlayerCombat>();

            // Visual: ciemnozielone szaty (capsule) + brązowy plecak (cube) + jasna głowa
            BuildVisual(go.transform, combat);

            return go;
        }

        private static void BuildVisual(Transform root, PlayerCombat combat)
        {
            Material robe = MaterialFactory.Get(new Color(0.18f, 0.29f, 0.18f));    // ciemna zieleń
            Material skin = MaterialFactory.Get(new Color(0.85f, 0.75f, 0.62f));
            Material hood = MaterialFactory.Get(new Color(0.12f, 0.22f, 0.12f));
            Material wood = MaterialFactory.Get(new Color(0.36f, 0.25f, 0.16f));
            Material crystal = MaterialFactory.Get(new Color(0.5f, 1f, 0.6f), 0.6f, new Color(0.4f, 1f, 0.55f), 3f);

            // Korpus (capsule)
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root, false);
            body.transform.localScale = new Vector3(0.85f, 0.8f, 0.85f);
            body.transform.localPosition = new Vector3(0f, 0.93f, 0f);
            SetMat(body, robe);
            DestroyCollider(body);

            // Głowa
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root, false);
            head.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            head.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            SetMat(head, skin);
            DestroyCollider(head);

            // Hood (drugi nieco większy sphere wokół głowy)
            var hoodGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hoodGO.name = "Hood";
            hoodGO.transform.SetParent(root, false);
            hoodGO.transform.localScale = new Vector3(0.42f, 0.34f, 0.42f);
            hoodGO.transform.localPosition = new Vector3(0f, 1.78f, -0.05f);
            SetMat(hoodGO, hood);
            DestroyCollider(hoodGO);

            // Mount na ręce (parent dla kostura)
            var handMount = new GameObject("HandMount").transform;
            handMount.SetParent(root, false);
            handMount.localPosition = new Vector3(0.45f, 1.15f, 0.2f);
            handMount.localRotation = Quaternion.Euler(-30f, 0f, 0f);

            // Kostur (cylinder)
            var staff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            staff.name = "Staff";
            staff.transform.SetParent(handMount, false);
            staff.transform.localScale = new Vector3(0.06f, 0.7f, 0.06f);
            staff.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            SetMat(staff, wood);
            DestroyCollider(staff);

            // Kryształ na koniec kostura
            var tipGO = new GameObject("StaffTip");
            tipGO.transform.SetParent(handMount, false);
            tipGO.transform.localPosition = new Vector3(0f, 0.95f, 0f);

            var crystalGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crystalGO.name = "Crystal";
            crystalGO.transform.SetParent(tipGO.transform, false);
            crystalGO.transform.localScale = Vector3.one * 0.18f;
            SetMat(crystalGO, crystal);
            DestroyCollider(crystalGO);

            // Mały light na krysztale
            var crystalLight = new GameObject("CrystalLight").AddComponent<Light>();
            crystalLight.transform.SetParent(tipGO.transform, false);
            crystalLight.color = new Color(0.55f, 1f, 0.7f);
            crystalLight.intensity = 1.2f;
            crystalLight.range = 6f;

            if (combat != null)
            {
                combat.staffTip = tipGO.transform;
                combat.handMount = handMount;
            }
        }

        private static void SetMat(GameObject go, Material m)
        {
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = m;
        }

        private static void DestroyCollider(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
        }
    }
}
