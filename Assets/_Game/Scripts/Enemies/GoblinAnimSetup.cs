using UnityEngine;
using WPG.Core;
using WPG.Character;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WPG.Enemies
{
    /// <summary>
    /// Dodaje Animator + avatar + controller do Fantasy Goblin (paczka nie ma własnych animacji).
    /// </summary>
    public static class GoblinAnimSetup
    {
        static RuntimeAnimatorController _cachedController;

        public static void EnsureAnimator(Transform modelRoot)
        {
            if (modelRoot == null) return;

            var animator = modelRoot.GetComponent<Animator>();
            if (animator == null)
                animator = modelRoot.gameObject.AddComponent<Animator>();

            if (animator.avatar == null)
            {
                var avatar = FindAvatar(modelRoot);
                if (avatar != null)
                    animator.avatar = avatar;
            }

            if (animator.runtimeAnimatorController == null)
            {
                var ctrl = LoadLocomotionController();
                if (ctrl != null)
                    animator.runtimeAnimatorController = ctrl;
            }

            animator.applyRootMotion = false;
            // AlwaysAnimate (jak u gracza) — inaczej animacja może zamarznąć poza ekranem
            // i nie aktualizować transformów kości.
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            // Diagnostyka: bez controllera / valid avatara Mecanim nie zadziała i
            // CharacterAnimDriver przejdzie na procedural fallback.
            if (animator.runtimeAnimatorController == null)
                Debug.LogWarning($"[GoblinAnimSetup] Brak controllera na '{modelRoot.name}' — procedural fallback (rig bones).");
            if (animator.avatar == null || !animator.avatar.isValid)
                Debug.LogWarning($"[GoblinAnimSetup] Brak/invalid avatar na '{modelRoot.name}' — procedural locomotion (rig bones).");
        }

        public static void WireDriver(Transform enemyRoot, Transform modelRoot, CharacterAnimDriver driver)
        {
            if (driver == null || modelRoot == null) return;

            var animator = modelRoot.GetComponent<Animator>();
            if (animator != null)
            {
                driver.animator = animator;
                if (animator.runtimeAnimatorController != null
                    && (animator.runtimeAnimatorController.name.Contains("BasicLocomotion")
                        || animator.runtimeAnimatorController.name.Contains("Invector")))
                {
                    driver.ConfigureForInvectorLocomotion();
                }
            }

            driver.bodyPivot = FindBone(modelRoot, "spine_02", "spine_01", "root", "hips", "Hips") ?? modelRoot;
            driver.leftArm = FindBone(modelRoot, "upperarm_l", "LeftArm", "arm_l");
            driver.rightArm = FindBone(modelRoot, "upperarm_r", "RightArm", "arm_r");
            driver.leftLeg = FindBone(modelRoot, "thigh_l", "LeftLeg", "leg_l");
            driver.rightLeg = FindBone(modelRoot, "thigh_r", "RightLeg", "leg_r");
            driver.handMount = FindBone(modelRoot, "hand_r", "Hand_R", "RightHand") ?? driver.rightArm;
            driver.headPivot = FindBone(modelRoot, "head", "Head", "neck_01");
            driver.RefreshBaseTransforms();
        }

        static Transform FindBone(Transform root, params string[] names)
        {
            foreach (var n in names)
            {
                var t = FindDeepChild(root, n);
                if (t != null) return t;
            }
            return null;
        }

        static Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) return child;
                var found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
        }

        static Avatar FindAvatar(Transform modelRoot)
        {
#if UNITY_EDITOR
            foreach (var r in modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (r.sharedMesh == null) continue;
                var meshPath = AssetDatabase.GetAssetPath(r.sharedMesh);
                if (string.IsNullOrEmpty(meshPath)) continue;
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(meshPath))
                {
                    if (asset is Avatar av) return av;
                }
            }

            foreach (var fbxPath in new[]
                     {
                         "Assets/Goblin/Base Mesh/skin1.fbx",
                         "Assets/Goblin/Base Mesh/skin2.fbx",
                         "Assets/Goblin/Base Mesh/skin3.fbx",
                     })
            {
                if (!System.IO.File.Exists(fbxPath)) continue;
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
                {
                    if (asset is Avatar av) return av;
                }
            }
#endif
            // Runtime/build fallback — avatar wyeksportowany do Resources (opcjonalny).
            return Resources.Load<Avatar>(GameAssetPaths.ResGoblinAvatar);
        }

        static RuntimeAnimatorController LoadLocomotionController()
        {
            if (_cachedController != null) return _cachedController;
#if UNITY_EDITOR
            _cachedController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                GameAssetPaths.InvectorLocomotionController);
#endif
            // Runtime/build fallback — controller skopiowany do Resources/Anim/.
            if (_cachedController == null)
                _cachedController = Resources.Load<RuntimeAnimatorController>(GameAssetPaths.ResLocomotionController);
            return _cachedController;
        }
    }
}
