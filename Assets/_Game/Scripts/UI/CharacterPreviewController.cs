using TMPro;
using UnityEngine;

/// <summary>
/// Logiczny placeholder podglądu postaci.
/// Pole previewRoot/prefabToPreview jest przygotowane pod późniejsze podpięcie modelu 3D
/// bez naruszania głównego prefabu gracza.
/// </summary>
public class CharacterPreviewController : MonoBehaviour
{
    [SerializeField] private TMP_Text previewText;
    [SerializeField] private Transform previewRoot;
    [SerializeField] private GameObject prefabToPreview;

    private GameObject previewInstance;

    public void SetPreviewText(TMP_Text target)
    {
        previewText = target;
    }

    public void Refresh(CharacterCreationData data, CharacterClassDefinition definition)
    {
        if (previewText != null)
            previewText.text = BuildPreviewText(data, definition);

        if (prefabToPreview != null && previewRoot != null && previewInstance == null)
        {
            previewInstance = Instantiate(prefabToPreview, previewRoot);
            previewInstance.name = "CharacterPreviewInstance";
        }

        // Tu docelowo podepnij modularne meshe/materiały:
        // Face1/Face2/Face3, Hair1/Hair2/TopKnot, Beard1/Beard3/Sideburns,
        // StarterArmor_1/2/3, Robe, TravelerClothes itd.
    }

    private string BuildPreviewText(CharacterCreationData data, CharacterClassDefinition definition)
    {
        if (data == null)
            return "Brak danych podglądu.";

        string className = definition != null ? definition.displayName : "Nie wybrano";

        return
            $"PODGLĄD POSTACI\n" +
            $"Klasa: {className}\n" +
            $"Sylwetka: {data.bodyType}\n" +
            $"Twarz: {data.faceVariant}\n" +
            $"Fryzura: {data.hairVariant}\n" +
            $"Zarost: {data.facialHairVariant}\n" +
            $"Znak: {data.specialMarkVariant}\n" +
            $"Strój: {data.outfitVariant}";
    }
}
