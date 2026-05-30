using UnityEngine;

/// <summary>
/// Tymczasowy, niedestrukcyjny most pomiędzy kreatorem postaci a sceną świata.
/// Nie zastępuje GameManagera ani SaveSystemu. Pozwala WorldBootstrapowi/PlayerBuilderowi
/// pobrać ostatnio zatwierdzoną postać, dopóki dane nie zostaną trwale wpięte w SaveData.
/// </summary>
public static class CharacterCreationSession
{
    public static CharacterCreationData CurrentCharacter { get; private set; }
    public static bool HasCharacter => CurrentCharacter != null;

    public static void SetCurrentCharacter(CharacterCreationData data)
    {
        if (data == null)
        {
            Debug.LogError("[CharacterCreationSession] Próba zapisania pustych danych postaci.");
            return;
        }

        if (!data.IsValid(out string validationMessage))
        {
            Debug.LogError("[CharacterCreationSession] Dane postaci są niepoprawne: " + validationMessage);
            return;
        }

        CurrentCharacter = data;
    }

    public static void Clear()
    {
        CurrentCharacter = null;
    }
}
