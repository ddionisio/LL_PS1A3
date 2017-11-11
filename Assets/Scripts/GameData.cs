using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[M8.PrefabCore]
public class GameData : M8.SingletonBehaviour<GameData> {
    public const int paletteMaxCount = 99;

    [Header("Data")]
    public BlockInfo[] blocks;
    public Vector2 blockSize = new Vector2(1f, 1f);
    public LayerMask blockInvalidMask;

    [Header("Gameplay")]
    public float gatherDelay = 1.5f;

    private Dictionary<string, BlockInfo> mBlockInfos;

    public BlockInfo GetBlockInfo(string blockName) {
        BlockInfo blockInf;
        mBlockInfos.TryGetValue(blockName, out blockInf);
        return blockInf;
    }

    protected override void OnInstanceInit() {
        //generate block references
        mBlockInfos = new Dictionary<string, BlockInfo>(blocks.Length);
        for(int i = 0; i < blocks.Length; i++) {
            mBlockInfos.Add(blocks[i].prefab.name, blocks[i]);
        }
    }
}
