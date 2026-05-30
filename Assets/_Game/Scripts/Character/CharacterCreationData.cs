using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterCreationData
{
    [Header("Identity")]
    public string characterName = string.Empty;
    public CharacterClassType selectedClass = CharacterClassType.None;

    [Header("Appearance")]
    public BodyType bodyType;
    public FaceVariant faceVariant;
    public HairVariant hairVariant;
    public FacialHairVariant facialHairVariant;
    public SpecialMarkVariant specialMarkVariant;
    public Color hairColor;
    public Color skinColor;
    public OutfitVariant outfitVariant;

    [Header("Stats")]
    public CharacterStatsData baseStats = new CharacterStatsData();
    public CharacterStatsData allocatedStats = new CharacterStatsData();
    public CharacterStatsData finalStats = new CharacterStatsData();
    public int remainingStatPoints;

    [Header("Start")]
    public List<string> startingItems = new List<string>();
    public List<string> startingSkills = new List<string>();

    public static CharacterCreationData FromState(
        string characterName,
        CharacterClassDefinition definition,
        CharacterStatsData allocatedStats,
        int remainingStatPoints,
        CharacterAppearanceData appearance)
    {
        CharacterStatsData baseStats = definition != null && definition.baseStats != null
            ? definition.baseStats.Clone()
            : CharacterStatsData.Zero();

        CharacterStatsData allocated = allocatedStats != null
            ? allocatedStats.Clone()
            : CharacterStatsData.Zero();

        CharacterAppearanceData appearanceData = appearance != null
            ? appearance.Clone()
            : new CharacterAppearanceData();

        return new CharacterCreationData
        {
            characterName = characterName != null ? characterName.Trim() : string.Empty,
            selectedClass = definition != null ? definition.classType : CharacterClassType.None,

            bodyType = appearanceData.bodyType,
            faceVariant = appearanceData.faceVariant,
            hairVariant = appearanceData.hairVariant,
            facialHairVariant = appearanceData.facialHairVariant,
            specialMarkVariant = appearanceData.specialMarkVariant,
            hairColor = appearanceData.hairColor,
            skinColor = appearanceData.skinColor,
            outfitVariant = appearanceData.outfitVariant,

            baseStats = baseStats,
            allocatedStats = allocated,
            finalStats = CharacterStatsData.Add(baseStats, allocated),
            remainingStatPoints = remainingStatPoints,

            startingItems = definition != null && definition.startingItems != null
                ? new List<string>(definition.startingItems)
                : new List<string>(),

            startingSkills = definition != null && definition.startingSkills != null
                ? new List<string>(definition.startingSkills)
                : new List<string>()
        };
    }

    public CharacterAppearanceData ToAppearanceData()
    {
        return new CharacterAppearanceData
        {
            bodyType = bodyType,
            faceVariant = faceVariant,
            hairVariant = hairVariant,
            facialHairVariant = facialHairVariant,
            specialMarkVariant = specialMarkVariant,
            hairColor = hairColor,
            skinColor = skinColor,
            outfitVariant = outfitVariant
        };
    }

    public bool IsValid(out string validationMessage)
    {
        if (string.IsNullOrWhiteSpace(characterName))
        {
            validationMessage = "Wpisz nazwę postaci.";
            return false;
        }

        if (characterName.Trim().Length > 24)
        {
            validationMessage = "Nazwa postaci może mieć maksymalnie 24 znaki.";
            return false;
        }

        if (selectedClass == CharacterClassType.None)
        {
            validationMessage = "Wybierz klasę postaci.";
            return false;
        }

        if (baseStats == null || allocatedStats == null || finalStats == null)
        {
            validationMessage = "Dane statystyk są niekompletne.";
            return false;
        }

        if (!allocatedStats.HasNoNegativeValues())
        {
            validationMessage = "Rozdane statystyki nie mogą być ujemne.";
            return false;
        }

        if (allocatedStats.Sum() > CharacterCreationRules.AllocatableStatPoints)
        {
            validationMessage = "Rozdano za dużo punktów statystyk.";
            return false;
        }

        if (remainingStatPoints != CharacterCreationRules.AllocatableStatPoints - allocatedStats.Sum())
        {
            validationMessage = "Pula wolnych punktów statystyk jest niespójna.";
            return false;
        }

        if (remainingStatPoints > 0)
        {
            validationMessage = "Rozdaj wszystkie punkty statystyk.";
            return false;
        }

        CharacterStatsData expectedFinal = CharacterStatsData.Add(baseStats, allocatedStats);
        if (finalStats.strength != expectedFinal.strength
            || finalStats.dexterity != expectedFinal.dexterity
            || finalStats.intelligence != expectedFinal.intelligence
            || finalStats.endurance != expectedFinal.endurance
            || finalStats.perception != expectedFinal.perception
            || finalStats.charisma != expectedFinal.charisma)
        {
            validationMessage = "Końcowe statystyki nie są zgodne z bazą klasy i rozdanymi punktami.";
            return false;
        }

        if (startingItems == null || startingSkills == null)
        {
            validationMessage = "Brakuje danych startowego ekwipunku albo umiejętności.";
            return false;
        }

        validationMessage = string.Empty;
        return true;
    }
}
