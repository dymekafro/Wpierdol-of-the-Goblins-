using Invector.vCharacterController;
using UnityEngine;
using WPG.Character;
using WPG.Core;
using WPG.World;

namespace WPG.Player
{
    /// <summary>
    /// Buduje postać gracza opartą o Invector Third Person Controller LITE
    /// + dokleja warstwę WPG (PlayerStats, PlayerCombat, InteractionDetector,
    /// CharacterAnimDriver, InvectorPlayerAdapter).
    ///
    /// Strategia (HYBRID):
    /// - Root = instancja prefabu ThirdPersonController_LITE (CapsuleCollider + Rigidbody +
    ///   Animator + vThirdPersonController + vThirdPersonInput).
    /// - Tag "Player".
    /// - Skala = InvectorPlayerScale (= WorldAssetPlacer.PlayerCharacterModelScale, default 1.3).
    /// - PlayerStats / PlayerCombat / InteractionDetector / CharacterAnimDriver dodawane jako
    ///   komponenty na tym samym GameObject.
    /// - Stara CharacterController-based ścieżka (PlayerBuilder.BuildDruid) pozostaje jako
    ///   fallback gdy prefab Invector nie jest dostępny.
    /// </summary>
    public static class InvectorPlayerBuilder
    {
        /// <summary>Alias <see cref="WorldAssetPlacer.PlayerCharacterModelScale"/> — ten sam rozmiar co legacy GanzSe/placeholder.</summary>
        public const float InvectorPlayerScale = WorldAssetPlacer.PlayerCharacterModelScale;

        /// <summary>
        /// Zwraca null jeśli nie udało się załadować prefabu Invector (Editor-only AssetDatabase).
        /// </summary>
        public static GameObject TryBuild(
            Vector3 spawnPos,
            PlayerAttributes attrs,
            out PlayerStats stats,
            out PlayerCombat combat,
            out InvectorPlayerAdapter adapter,
            float scale = InvectorPlayerScale)
        {
            stats = null;
            combat = null;
            adapter = null;

            GameAssetRegistry.Initialize();
            var prefab = GameAssetRegistry.InvectorController;
            if (prefab == null)
            {
                Debug.LogWarning("[InvectorPlayerBuilder] Brak prefabu ThirdPersonController_LITE — używam fallbacku PlayerBuilder.BuildDruid.");
                return null;
            }

            var go = Object.Instantiate(prefab);
            go.name = "Player_Invector";
            go.tag = "Player";
            go.transform.position = spawnPos;
            if (Mathf.Abs(scale - 1f) > 0.001f)
            {
                go.transform.localScale = Vector3.one * scale;
            }

            // Napraw magenta (Built-in → URP Lit) na wszystkich rendererach V-Bota / demo mesh.
            MaterialUpgrader.UpgradeHierarchy(go);

            // Invector setup zwykle ma już te komponenty z prefabu, ale safety-check.
            var controller = go.GetComponent<vThirdPersonController>();
            var input = go.GetComponent<vThirdPersonInput>();
            var animator = go.GetComponentInChildren<Animator>();

            // Wyłącz root motion — animacje Invector mają in-place clipy + ruch idzie przez Rigidbody.
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            // PlayerStats jest wymagany przez PlayerCombat / InvectorPlayerAdapter / IDamageReceiver.
            stats = go.AddComponent<PlayerStats>();
            stats.Init(attrs ?? PlayerAttributes.CreateDruidBase());

            // CharacterAnimDriver — używamy go do triggerów Attack/Cast/Death.
            // Inicjalizacja po dodaniu Animator-a, by go znalazł.
            var driver = go.AddComponent<CharacterAnimDriver>();
            driver.animator = animator;
            driver.ConfigureForInvectorLocomotion();

            // PlayerCombat — używa ResolveAimDirection() (kamera fallback gdy nie ma PlayerController).
            combat = go.AddComponent<PlayerCombat>();
            combat.animDriver = driver;
            ResolveCombatAttachments(animator, combat, driver);

            // Interakcje (E/F na campach itd.).
            go.AddComponent<InteractionDetector>();

            // Adapter: martwy gracz / czułość myszy.
            adapter = go.AddComponent<InvectorPlayerAdapter>();
            adapter.controller = controller;
            adapter.input = input;
            adapter.animator = animator;
            adapter.animDriver = driver;

            Debug.Log($"[InvectorPlayerBuilder] Player_Invector zbudowany. scale={scale:F2}, animator={(animator != null ? animator.runtimeAnimatorController?.name : "brak")}");
            return go;
        }

        /// <summary>
        /// Stara się znaleźć prawą rękę i końcówkę (różdżki/staffu) dla VFX i animacji proceduralnej.
        /// V-Bot ma kości typu VBOT_:RightHand.
        /// </summary>
        private static void ResolveCombatAttachments(Animator animator, PlayerCombat combat, CharacterAnimDriver driver)
        {
            if (animator == null) return;

            Transform rightHand = null;
            if (animator.isHuman)
            {
                rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            }
            if (rightHand == null) rightHand = FindBoneByName(animator.transform, "RightHand", "Hand_R", "VBOT_:RightHand", "mixamorig:RightHand");

            if (rightHand != null)
            {
                combat.handMount = rightHand;
                driver.handMount = rightHand;

                var staffTip = new GameObject("StaffTip_Invector").transform;
                staffTip.SetParent(rightHand, false);
                staffTip.localPosition = new Vector3(0.05f, 0.15f, 0.25f);
                combat.staffTip = staffTip;
            }

            var hips = animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Hips) : null;
            if (hips != null) driver.bodyPivot = hips.parent != null ? hips.parent : hips;
            else driver.bodyPivot = animator.transform;
        }

        private static Transform FindBoneByName(Transform root, params string[] names)
        {
            foreach (var n in names)
            {
                var t = FindBoneRecursive(root, n);
                if (t != null) return t;
            }
            return null;
        }

        private static Transform FindBoneRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (string.Equals(child.name, name, System.StringComparison.OrdinalIgnoreCase)) return child;
                var found = FindBoneRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
