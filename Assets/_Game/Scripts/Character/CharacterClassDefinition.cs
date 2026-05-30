using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterClassDefinition
{
    public CharacterClassType classType;
    public string displayName;
    [TextArea(2, 5)] public string description;
    [TextArea(1, 4)] public string playStyle;
    public CharacterStatsData baseStats;
    public List<string> startingItems = new List<string>();
    public List<string> startingSkills = new List<string>();

    public CharacterClassDefinition(
        CharacterClassType classType,
        string displayName,
        string description,
        string playStyle,
        CharacterStatsData baseStats,
        IEnumerable<string> startingItems,
        IEnumerable<string> startingSkills)
    {
        this.classType = classType;
        this.displayName = displayName;
        this.description = description;
        this.playStyle = playStyle;
        this.baseStats = baseStats != null ? baseStats.Clone() : CharacterStatsData.Zero();
        this.startingItems = startingItems != null ? new List<string>(startingItems) : new List<string>();
        this.startingSkills = startingSkills != null ? new List<string>(startingSkills) : new List<string>();
    }
}
