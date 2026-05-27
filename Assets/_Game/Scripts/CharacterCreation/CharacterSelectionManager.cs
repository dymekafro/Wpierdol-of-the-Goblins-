using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectionManager : MonoBehaviour
{
    public void SelectDwarf()
    {
        SelectCharacter(CharacterType.Dwarf);
    }

    public void SelectHuman()
    {
        SelectCharacter(CharacterType.Human);
    }

    public void SelectSkeleton()
    {
        SelectCharacter(CharacterType.Skeleton);
    }

    public void SelectOrc()
    {
        SelectCharacter(CharacterType.Orc);
    }

    private void SelectCharacter(CharacterType characterType)
    {
        if (GameData.Instance == null)
        {
            Debug.LogError("Brak GameData. Uruchom grę od sceny MainMenu.");
            return;
        }

        GameData.Instance.SelectCharacter(characterType);
        SceneManager.LoadScene("AttributeAllocation");
    }
}