using System;
using UnityEngine;

[Serializable]
public class PlayerAttributes
{
    public int strength = 5;
    public int dexterity = 5;
    public int mana = 5;
    public int intelligence = 5;
    public int endurance = 5;
    public int charisma = 5;

    public int availablePoints = 10;

    public void ApplyCharacterBonus(CharacterType characterType)
    {
        switch (characterType)
        {
            case CharacterType.Dwarf:
                endurance += 3;
                strength += 2;
                dexterity -= 1;
                break;

            case CharacterType.Human:
                strength += 1;
                dexterity += 1;
                mana += 1;
                intelligence += 1;
                endurance += 1;
                charisma += 1;
                break;

            case CharacterType.Skeleton:
                mana += 3;
                dexterity += 2;
                charisma -= 2;
                break;

            case CharacterType.Orc:
                strength += 4;
                endurance += 2;
                intelligence -= 2;
                charisma -= 1;
                break;
        }

        ClampMinimumValues();
    }

    private void ClampMinimumValues()
    {
        strength = Mathf.Max(1, strength);
        dexterity = Mathf.Max(1, dexterity);
        mana = Mathf.Max(1, mana);
        intelligence = Mathf.Max(1, intelligence);
        endurance = Mathf.Max(1, endurance);
        charisma = Mathf.Max(1, charisma);
    }
}