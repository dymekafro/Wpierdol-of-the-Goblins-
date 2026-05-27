using System;
using System.Collections.Generic;
using UnityEngine;
using WPG.Character;

namespace WPG.Core
{
    public enum CampState
    {
        Active,
        Cleared,
        Captured
    }

    [Serializable]
    public class CampSaveEntry
    {
        public string campId;
        public CampState state;
    }

    [Serializable]
    public class SaveData
    {
        public int saveVersion = 1;
        public string characterType = "Druid";
        public PlayerAttributes attributes = PlayerAttributes.CreateDruidBase();
        public float playerX;
        public float playerY;
        public float playerZ;
        public int currentHealth;
        public int currentMana;
        public bool hasPosition;
        public List<CampSaveEntry> camps = new List<CampSaveEntry>();
        public List<string> visitedPowerSites = new List<string>();
        public string lastZoneName = "Sady Ostatniego Strażnika";
        public string saveTimestamp = "";

        public Vector3 PlayerPosition
        {
            get => new Vector3(playerX, playerY, playerZ);
            set
            {
                playerX = value.x;
                playerY = value.y;
                playerZ = value.z;
                hasPosition = true;
            }
        }
    }
}
