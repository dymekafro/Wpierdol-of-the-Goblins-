using System;

public enum CharacterClassType
{
    None,
    Warrior,
    Mage,
    Archer,
    Rogue,
    Dwarf,
    Wanderer,
    // Dopisane na końcu, aby nie psuć istniejących zserializowanych wartości.
    Barbarian,
    Ranger,
    Knight,
    Druid
}

public enum BodyType
{
    Male,
    Female,
    Slim,
    Strong,
    Dwarf
}

public enum FaceVariant
{
    Face1,
    Face2,
    Face3
}

public enum HairVariant
{
    None,
    Hair1,
    Hair2,
    TopKnot
}

public enum FacialHairVariant
{
    None,
    Beard1,
    Beard3,
    Sideburns
}

public enum SpecialMarkVariant
{
    None,
    Scar,
    Tattoo,
    BlindEye,
    WarPaint
}

public enum OutfitVariant
{
    StarterArmor1,
    StarterArmor2,
    StarterArmor3,
    Robe,
    TravelerClothes
}

public enum CharacterStatType
{
    Strength,
    Dexterity,
    Intelligence,
    Endurance,
    Perception,
    Charisma
}
