using UnityEngine;

namespace WPG.Player
{
    public sealed class AnimatorParameterMirror : MonoBehaviour
    {
        [SerializeField] private Animator sourceAnimator;
        [SerializeField] private Animator targetAnimator;

        [Header("Optional combat trigger names")]
        [SerializeField]
        private string[] mirroredTriggers =
        {
            "Attack",
            "Melee",
            "Cast",
            "Fireball",
            "Heal",
            "Hit",
            "Death"
        };

        private void LateUpdate()
        {
            if (sourceAnimator == null || targetAnimator == null)
                return;

            AnimatorControllerParameter[] parameters = sourceAnimator.parameters;

            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];

                if (!HasParameter(targetAnimator, parameter.nameHash))
                    continue;

                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Float:
                        targetAnimator.SetFloat(parameter.nameHash, sourceAnimator.GetFloat(parameter.nameHash));
                        break;

                    case AnimatorControllerParameterType.Int:
                        targetAnimator.SetInteger(parameter.nameHash, sourceAnimator.GetInteger(parameter.nameHash));
                        break;

                    case AnimatorControllerParameterType.Bool:
                        targetAnimator.SetBool(parameter.nameHash, sourceAnimator.GetBool(parameter.nameHash));
                        break;

                    case AnimatorControllerParameterType.Trigger:
                        // Unity nie pozwala bezpiecznie odczytać, czy trigger był aktywowany.
                        // Triggery combatowe obsłużymy osobną metodą MirrorTrigger().
                        break;
                }
            }
        }

        public void MirrorTrigger(string triggerName)
        {
            if (string.IsNullOrWhiteSpace(triggerName))
                return;

            if (targetAnimator == null)
                return;

            int triggerHash = Animator.StringToHash(triggerName);

            if (!HasParameter(targetAnimator, triggerHash))
                return;

            targetAnimator.SetTrigger(triggerHash);
        }

        public void MirrorKnownTrigger(int triggerHash)
        {
            if (targetAnimator == null)
                return;

            for (int i = 0; i < mirroredTriggers.Length; i++)
            {
                string triggerName = mirroredTriggers[i];

                if (Animator.StringToHash(triggerName) != triggerHash)
                    continue;

                MirrorTrigger(triggerName);
                return;
            }
        }

        private static bool HasParameter(Animator animator, int nameHash)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].nameHash == nameHash)
                    return true;
            }

            return false;
        }
    }
}