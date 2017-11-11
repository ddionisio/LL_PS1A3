using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockInfo {
    public const string poolGroup = "blockPool";
    public const string blockSpawnToTag = "blockSpawnTo";

    [Header("Data")]
    public string name;
    public Block prefab;

    [Header("Info")]
    public Sprite icon;
    [M8.Localize]
    public string nameDisplayRef;

    private M8.GenericParams mParms = new M8.GenericParams();

    private static Transform blockSpawnTo {
        get {
            if(!mBlockSpawnTo) {
                GameObject go = GameObject.FindGameObjectWithTag(blockSpawnToTag);
                if(!go) {
                    go = new GameObject("blockSpawns");
                    go.tag = blockSpawnToTag;
                }

                mBlockSpawnTo = go.transform;
            }

            return mBlockSpawnTo;
        }
    }

    private static Transform mBlockSpawnTo;
    
    public void GeneratePool(Transform poolParent, int startCapacity, int maxCapacity) {
        var poolCtrl = M8.PoolController.CreatePool(poolGroup, poolParent);
        poolCtrl.AddType(name, prefab.transform, startCapacity, maxCapacity);

    }
    
    public Block SpawnBlock(Block.Mode mode) {
        mParms[Block.paramName] = name;
        mParms[Block.paramMode] = mode;

        return M8.PoolController.SpawnFromGroup<Block>(poolGroup, name, name, mBlockSpawnTo, Vector3.zero, Quaternion.identity, mParms);
    }
}
