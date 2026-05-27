using System;
using UnityEngine;

[Serializable]
public class ItemData
{
    public string itemName;
    public ItemType itemType;
    public int damageBonus;
    public int armorBonus;
    public int requiredStrength;

    public ItemData(string itemName, ItemType itemType, int damageBonus, int armorBonus, int requiredStrength)
    {
        this.itemName = itemName;
        this.itemType = itemType;
        this.damageBonus = damageBonus;
        this.armorBonus = armorBonus;
        this.requiredStrength = requiredStrength;
    }
}