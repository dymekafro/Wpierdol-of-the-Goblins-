using System;

namespace WPG.Character
{
    // Atrybuty postaci. Brak leveli — punkty dostajemy z obozów / miejsc mocy.
    [Serializable]
    public class PlayerAttributes
    {
        public int strength = 5;
        public int dexterity = 5;
        public int mana = 5;
        public int intelligence = 5;
        public int endurance = 5;
        public int charisma = 5;

        public int unallocatedPoints = 5;

        public static PlayerAttributes CreateDruidBase()
        {
            return new PlayerAttributes
            {
                strength = 4,        // STR -1
                dexterity = 5,
                mana = 8,            // MANA +3
                intelligence = 7,    // INT +2
                endurance = 6,       // END +1
                charisma = 5,
                unallocatedPoints = 5
            };
        }

        public int MaxHealth => 50 + endurance * 10;
        public int MaxMana => 20 + mana * 10;
        public float ManaRegenPerSecond => 1f + mana * 0.3f;
        public int MeleeDamage => 5 + strength * 2;
        public int SpellPower => 8 + intelligence * 3;
        public float MoveSpeed => 5f + dexterity * 0.1f;

        public PlayerAttributes Clone()
        {
            return new PlayerAttributes
            {
                strength = strength,
                dexterity = dexterity,
                mana = mana,
                intelligence = intelligence,
                endurance = endurance,
                charisma = charisma,
                unallocatedPoints = unallocatedPoints
            };
        }
    }
}
