using UnityEngine;
using WPG.Character;
using WPG.Core;
using WPG.World;

namespace WPG.Player
{
    public static class PlayerBuilder
    {
        public const string PlayerLayer = "Default";
        public const float BaseCharacterHeight = 1.85f;
        public const float ModelScale = WorldAssetPlacer.PlayerCharacterModelScale;

        public static float ScaledCharacterHeight => BaseCharacterHeight * ModelScale;

        public static GameObject BuildDruid(Vector3 spawnPos, PlayerAttributes attrs, out PlayerStats stats, out PlayerController ctrl, out PlayerCombat combat)
        {
            var go = new GameObject("Player_Druid");
            go.tag = "Player";
            go.transform.position = spawnPos;

            var cc = go.AddComponent<CharacterController>();
            cc.height = ScaledCharacterHeight;
            cc.radius = 0.4f * ModelScale;
            cc.center = new Vector3(0f, ScaledCharacterHeight * 0.5f, 0f);
            cc.stepOffset = 0.4f * ModelScale;
            cc.slopeLimit = 50f;

            stats = go.AddComponent<PlayerStats>();
            stats.Init(attrs);
            ctrl = go.AddComponent<PlayerController>();
            combat = go.AddComponent<PlayerCombat>();

            var driver = go.AddComponent<CharacterAnimDriver>();
            BuildVisual(go.transform, combat, driver);
            ctrl.animDriver = driver;
            combat.animDriver = driver;

            return go;
        }

        private static void BuildVisual(Transform root, PlayerCombat combat, CharacterAnimDriver driver)
        {
            GameAssetRegistry.Initialize();

            if (WorldAssetPlacer.TryAttachCharacterModel(root, WorldAssetPlacer.CharacterModelKind.Druid, BaseCharacterHeight, out var attach, ModelScale))
            {
                if (combat != null)
                {
                    if (attach.StaffTip != null) combat.staffTip = attach.StaffTip;
                    if (attach.HandMount != null) combat.handMount = attach.HandMount;
                }
                if (driver != null)
                {
                    driver.handMount = attach.HandMount;
                    driver.bodyPivot = attach.ModelRoot;
                }
                Debug.Log($"[PlayerBuilder] Attached {attach.ModelSource}: {attach.PrefabPath} | scale={attach.AppliedScale:F2} | Animator: {attach.AnimatorStatus}");
                return;
            }

            Debug.LogWarning($"[PlayerBuilder] FALLBACK placeholder — reason: {attach.FailureReason}");
            BuildPlaceholderVisual(root, combat, driver);
        }

        private static void BuildPlaceholderVisual(Transform root, PlayerCombat combat, CharacterAnimDriver driver)
        {
            Material robe = MaterialFactory.Get(new Color(0.18f, 0.29f, 0.18f));
            Material skin = MaterialFactory.Get(new Color(0.85f, 0.75f, 0.62f));
            Material hood = MaterialFactory.Get(new Color(0.12f, 0.22f, 0.12f));
            Material wood = MaterialFactory.Get(new Color(0.36f, 0.25f, 0.16f));
            Material leather = MaterialFactory.Get(new Color(0.22f, 0.16f, 0.10f));
            Material crystal = MaterialFactory.Get(new Color(0.5f, 1f, 0.6f), 0.6f, new Color(0.4f, 1f, 0.55f), 3f);

            var bodyPivot = new GameObject("BodyPivot").transform;
            bodyPivot.SetParent(root, false);
            bodyPivot.localPosition = Vector3.zero;

            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(bodyPivot, false);
            torso.transform.localScale = new Vector3(0.62f, 0.5f, 0.4f);
            torso.transform.localPosition = new Vector3(0f, 1.10f, 0f);
            SetMat(torso, robe);
            DestroyCollider(torso);

            var hips = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hips.name = "Hips";
            hips.transform.SetParent(bodyPivot, false);
            hips.transform.localScale = new Vector3(0.55f, 0.32f, 0.4f);
            hips.transform.localPosition = new Vector3(0f, 0.78f, 0f);
            SetMat(hips, robe);
            DestroyCollider(hips);

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(bodyPivot, false);
            head.transform.localScale = new Vector3(0.32f, 0.34f, 0.32f);
            head.transform.localPosition = new Vector3(0f, 1.66f, 0f);
            SetMat(head, skin);
            DestroyCollider(head);

            var hoodGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hoodGO.name = "Hood";
            hoodGO.transform.SetParent(head.transform, false);
            hoodGO.transform.localScale = new Vector3(1.25f, 1.05f, 1.25f);
            hoodGO.transform.localPosition = new Vector3(0f, 0.18f, -0.18f);
            SetMat(hoodGO, hood);
            DestroyCollider(hoodGO);

            var beard = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            beard.name = "Beard";
            beard.transform.SetParent(head.transform, false);
            beard.transform.localScale = new Vector3(0.85f, 0.55f, 0.7f);
            beard.transform.localPosition = new Vector3(0f, -0.55f, 0.25f);
            SetMat(beard, MaterialFactory.Get(new Color(0.78f, 0.72f, 0.65f)));
            DestroyCollider(beard);

            var leftArm = BuildLimb(bodyPivot, "LeftArm", new Vector3(-0.35f, 1.32f, 0f),
                new Vector3(0.16f, 0.34f, 0.16f), new Vector3(0f, -0.34f, 0f), robe);
            var rightArm = BuildLimb(bodyPivot, "RightArm", new Vector3(0.35f, 1.32f, 0f),
                new Vector3(0.16f, 0.34f, 0.16f), new Vector3(0f, -0.34f, 0f), robe);
            var leftLeg = BuildLimb(bodyPivot, "LeftLeg", new Vector3(-0.16f, 0.62f, 0f),
                new Vector3(0.18f, 0.34f, 0.18f), new Vector3(0f, -0.34f, 0f), leather);
            var rightLeg = BuildLimb(bodyPivot, "RightLeg", new Vector3(0.16f, 0.62f, 0f),
                new Vector3(0.18f, 0.34f, 0.18f), new Vector3(0f, -0.34f, 0f), leather);

            var handMount = new GameObject("HandMount").transform;
            handMount.SetParent(rightArm, false);
            handMount.localPosition = new Vector3(0f, -0.62f, 0.18f);
            handMount.localRotation = Quaternion.Euler(-30f, 0f, 0f);

            var staff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            staff.name = "Staff";
            staff.transform.SetParent(handMount, false);
            staff.transform.localScale = new Vector3(0.06f, 0.7f, 0.06f);
            staff.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            SetMat(staff, wood);
            DestroyCollider(staff);

            var tipGO = new GameObject("StaffTip");
            tipGO.transform.SetParent(handMount, false);
            tipGO.transform.localPosition = new Vector3(0f, 0.95f, 0f);

            var crystalGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crystalGO.name = "Crystal";
            crystalGO.transform.SetParent(tipGO.transform, false);
            crystalGO.transform.localScale = Vector3.one * 0.18f;
            SetMat(crystalGO, crystal);
            DestroyCollider(crystalGO);

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
            if (driver != null)
            {
                driver.bodyPivot = bodyPivot;
                driver.headPivot = head.transform;
                driver.handMount = handMount;
                driver.leftArm = leftArm;
                driver.rightArm = rightArm;
                driver.leftLeg = leftLeg;
                driver.rightLeg = rightLeg;
                driver.RefreshBaseTransforms();
            }
        }

        private static Transform BuildLimb(Transform parent, string name, Vector3 jointPos, Vector3 limbScale, Vector3 limbOffset, Material mat)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = jointPos;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = name + "_Mesh";
            visual.transform.SetParent(pivot, false);
            visual.transform.localScale = limbScale;
            visual.transform.localPosition = limbOffset;
            SetMat(visual, mat);
            DestroyCollider(visual);
            return pivot;
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
