using UnityEngine;

public class PlayerVisualTester : MonoBehaviour
{
    [SerializeField] private PlayerVisualManager visualManager;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            visualManager.SetVisual(PlayerVisualId.Warrior);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            visualManager.SetVisual(PlayerVisualId.Rogue);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            visualManager.SetVisual(PlayerVisualId.Mage);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            visualManager.SetVisual(PlayerVisualId.Peasant);
    }
}