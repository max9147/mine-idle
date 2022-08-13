using UnityEngine;

[CreateAssetMenu]
public class GameSettings : ScriptableObject
{
    [Header("Player Settings")]

    public float PlayerMoveSpeed;
    public float PlayerMaxHealth;
    public float PlayerDamageReductionPerArmor;
    public float PlayerAttackDelay;
    public float PlayerAttackDamage;
    public float PlayerAttackDamageBonusPerSword;
    public float PlayerAttackKnockbackStrength;
    public float PlayerMiningStrength;
    public float PlayerMiningStrengthBonusPerPickaxe;
    public float PlayerMiningAttackingRange;

    [Header("Player Inventory")]

    public int InventoryCapacity;
    public float InventoryBlockCollectTime;
    public float InventoryCollectedJumpHeight;
    public int InventoryX;
    public int InventoryZ;
    public float InventoryMargin;

    [Header("Territories")]

    public float TerritoryBlockDelay;
    public float TerritoryBlockFlyTime;
    public int TerritoryBuiltPerBlock;

    [Header("Zombies")]

    public float ZombieNoSpawnTime;
    public float ZombieMoveSpeed;
    public float ZombieVisionRange;
    public float ZombieAttackDelay;
    public float ZombieMaxHealth;
    public float ZombieAttackRange;
    public float ZombieAttackDamage;
    public int ZombieMaxAmount;
    public float[] ZombieWaveDelays;
    public int[] ZombieWaveSize;

    [Header("Buildings")]

    public float BuildingBlockDelay;
    public float BuildingBlockFlyTime;

    [Header("Mining Machine")]

    public float MachineMineTime;
    public int BlocksMinedPerWood;

    [Header("Allies Building")]

    public float AllySpawnDelay;
    public int AllyMaxAmount;
    public float AllySpawnHealthTreshold;

    [Header("Allies")]

    public float AllyMoveSpeed;
    public float AllyAttackDelay;
    public float AllyMaxHealth;
    public float AllyAttackRange;
    public float AllyAttackDamage;

    [Header("Blocks Behaviour")]

    public float BlockFadeOutTime;
    public float BlockUnlockBounceTime;
    public float BlockUnlockBounceScale;
    public float BlockRespawnTime;

    [Header("Blocks Health")]

    public float DirtHealth;
    public float GrassHealth;
    public float WoodHealth;
    public float TreeHealth;
    public float StoneHealth;
    public float IronOreHealth;

    [Header("ItemsHealth")]

    public float[] PickaxesHealth;
    public float[] SwordsHealth;
    public float[] ArmorsHealth;
}