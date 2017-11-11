using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockInfo {
    public const string poolGroup = "blocks";

    public string name;

    public Block prefab;

    public Sprite icon;
    
    public Block SpawnBlock(Transform toParent, M8.GenericParams parms) {
        return M8.PoolController.SpawnFromGroup<Block>(poolGroup, prefab.name, prefab.name, toParent, parms);
    }
}
