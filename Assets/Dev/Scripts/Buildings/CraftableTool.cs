using Structs;
using UnityEngine;

public class CraftableTool : MonoBehaviour
{
    public GearType GearType;
    public GearQuality GearQuality;

    public BlockType[] RequiredBlocks;
    public int[] BlockAmounts;
}