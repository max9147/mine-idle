using System.Collections.Generic;
using UnityEngine;

namespace Structs
{
    public enum BlockType
    {
        dirt, grass, wood, tree, stone, ironOre, gold, diamond
    }

    public enum TerritoryType
    {
        grass, desert, mountain
    }

    public enum GearType
    {
        sword, pickaxe, helmet, armor, bracers, legs
    }

    public enum GearQuality
    {
        none, wood, stone, iron, gold, diamond
    }

    [System.Serializable]
    struct DataStruct
    {
        public int TutorialStep;
        public float TimePlayed;
        public int PlayerPickaxe;
        public float PickaxeHealth;
        public int PlayerSword;
        public float SwordHealth;
        public int[] PlayerArmors;
        public float[] ArmorsHealth;
        public List<BlockType> CollectedBlocks;
        public bool[] TerritoriesUnlocked;
        public bool[] BuildingsUnlocked;
    }
}