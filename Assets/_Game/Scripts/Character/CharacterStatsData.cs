using System;
using UnityEngine;

[Serializable]
public class CharacterStatsData
{
    [Min(0)] public int strength;
    [Min(0)] public int dexterity;
    [Min(0)] public int intelligence;
    [Min(0)] public int endurance;
    [Min(0)] public int perception;
    [Min(0)] public int charisma;

    public CharacterStatsData() { }

    public CharacterStatsData(int strength, int dexterity, int intelligence, int endurance, int perception, int charisma)
    {
        this.strength = strength;
        this.dexterity = dexterity;
        this.intelligence = intelligence;
        this.endurance = endurance;
        this.perception = perception;
        this.charisma = charisma;
    }

    public CharacterStatsData Clone()
    {
        return new CharacterStatsData(strength, dexterity, intelligence, endurance, perception, charisma);
    }

    public static CharacterStatsData Zero()
    {
        return new CharacterStatsData(0, 0, 0, 0, 0, 0);
    }

    public static CharacterStatsData Add(CharacterStatsData a, CharacterStatsData b)
    {
        if (a == null) a = Zero();
        if (b == null) b = Zero();

        return new CharacterStatsData(
            a.strength + b.strength,
            a.dexterity + b.dexterity,
            a.intelligence + b.intelligence,
            a.endurance + b.endurance,
            a.perception + b.perception,
            a.charisma + b.charisma
        );
    }

    public int Sum()
    {
        return strength + dexterity + intelligence + endurance + perception + charisma;
    }

    public int GetValue(CharacterStatType type)
    {
        switch (type)
        {
            case CharacterStatType.Strength: return strength;
            case CharacterStatType.Dexterity: return dexterity;
            case CharacterStatType.Intelligence: return intelligence;
            case CharacterStatType.Endurance: return endurance;
            case CharacterStatType.Perception: return perception;
            case CharacterStatType.Charisma: return charisma;
            default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void SetValue(CharacterStatType type, int value)
    {
        value = Mathf.Max(0, value);

        switch (type)
        {
            case CharacterStatType.Strength:
                strength = value;
                break;
            case CharacterStatType.Dexterity:
                dexterity = value;
                break;
            case CharacterStatType.Intelligence:
                intelligence = value;
                break;
            case CharacterStatType.Endurance:
                endurance = value;
                break;
            case CharacterStatType.Perception:
                perception = value;
                break;
            case CharacterStatType.Charisma:
                charisma = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void AddValue(CharacterStatType type, int delta)
    {
        SetValue(type, GetValue(type) + delta);
    }

    public bool HasNoNegativeValues()
    {
        return strength >= 0
               && dexterity >= 0
               && intelligence >= 0
               && endurance >= 0
               && perception >= 0
               && charisma >= 0;
    }
}
