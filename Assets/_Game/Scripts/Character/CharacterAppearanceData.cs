using System;
using UnityEngine;

[Serializable]
public class CharacterAppearanceData
{
    public BodyType bodyType = BodyType.Male;
    public FaceVariant faceVariant = FaceVariant.Face1;
    public HairVariant hairVariant = HairVariant.Hair1;
    public FacialHairVariant facialHairVariant = FacialHairVariant.None;
    public SpecialMarkVariant specialMarkVariant = SpecialMarkVariant.None;
    public Color hairColor = new Color(0.18f, 0.10f, 0.04f, 1f);
    public Color skinColor = new Color(0.86f, 0.63f, 0.46f, 1f);
    public OutfitVariant outfitVariant = OutfitVariant.StarterArmor1;

    public CharacterAppearanceData Clone()
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

    public void Randomize()
    {
        bodyType = RandomEnum<BodyType>();
        faceVariant = RandomEnum<FaceVariant>();
        hairVariant = RandomEnum<HairVariant>();
        facialHairVariant = RandomEnum<FacialHairVariant>();
        specialMarkVariant = RandomEnum<SpecialMarkVariant>();
        outfitVariant = RandomEnum<OutfitVariant>();

        hairColor = RandomFromPalette(
            new Color(0.06f, 0.04f, 0.02f, 1f),
            new Color(0.22f, 0.11f, 0.04f, 1f),
            new Color(0.68f, 0.48f, 0.20f, 1f),
            new Color(0.72f, 0.20f, 0.08f, 1f),
            new Color(0.55f, 0.55f, 0.55f, 1f)
        );

        skinColor = RandomFromPalette(
            new Color(0.96f, 0.76f, 0.58f, 1f),
            new Color(0.86f, 0.63f, 0.46f, 1f),
            new Color(0.64f, 0.42f, 0.28f, 1f),
            new Color(0.42f, 0.28f, 0.18f, 1f)
        );
    }

    private static T RandomEnum<T>() where T : Enum
    {
        Array values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }

    private static Color RandomFromPalette(params Color[] colors)
    {
        return colors[UnityEngine.Random.Range(0, colors.Length)];
    }
}
