﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[M8.PrefabCore]
public class GameData : M8.SingletonBehaviour<GameData> {
    public const int paletteMaxCount = 99;

    [Header("Gameflow Data")]
    public M8.SceneAssetPath[] scenes;
    public M8.SceneAssetPath endScene;

    [Header("Gameplay Data")]
    public BlockInfo[] blocks;
    public Vector2 blockSize = new Vector2(1f, 1f);
    public LayerMask blockInvalidMask;
    public float blockPickupDelay = 1.5f;
    public AnimationCurve blockGhostPulseCurve;
    public float blockGhostPulseDelay = 3f;
    public float showHintDelay = 135f; //show hints if the current gameplay time has passed
    public int scorePerLevel = 2000;
    public int scoreHintPenalty = 500;

    [Header("Audio")]
    public string soundBlockPlacePath;
    public string soundBlockInvalidPath;

    public int currentScore {
        get {
            if(!LoLManager.isInstantiated)
                return 0;

            return LoLManager.instance.curScore;
        }

        set {
            if(LoLManager.isInstantiated)
                LoLManager.instance.curScore = value;
        }
    }
    
    private Dictionary<string, BlockInfo> mBlockInfos;

    private Dictionary<string, int> mHintCounters = new Dictionary<string, int>();
    private Dictionary<string, bool> mHintVisible = new Dictionary<string, bool>();

    public bool IsHintVisible(string id) {
        bool visible;
        return mHintVisible.TryGetValue(id, out visible) && visible;
    }

    public void SetHintVisible(string id, bool visible) {
        if(mHintVisible.ContainsKey(id))
            mHintVisible[id] = visible;
        else
            mHintVisible.Add(id, visible);
    }

    public int GetHintCounter(string id) {
        int count;
        if(mHintCounters.TryGetValue(id, out count))
            return count;

        return 0;
    }

    public void SetHintCounter(string id, int count) {
        if(mHintCounters.ContainsKey(id))
            mHintCounters[id] = count;
        else
            mHintCounters.Add(id, count);
    }

    public void IncrementHintCounter(string id) {
        if(mHintCounters.ContainsKey(id))
            mHintCounters[id]++;
        else
            mHintCounters.Add(id, 1);
    }

    public int GetProgressFromCurrentScene() {
        var sceneDat = M8.SceneManager.instance.curScene;

        for(int i = 0; i < scenes.Length; i++) {
            if(scenes[i] == sceneDat)
                return i;
        }

        return -1;
    }

    public M8.SceneAssetPath GetSceneFromCurrentProgress() {
        int progress = LoLManager.instance.curProgress;

        if(progress < scenes.Length)
            return scenes[progress];

        if(string.IsNullOrEmpty(endScene.name))
            return M8.SceneManager.instance.rootScene;

        return endScene;
    }

    public BlockInfo GetBlockInfo(string blockName) {
        BlockInfo blockInf;
        mBlockInfos.TryGetValue(blockName, out blockInf);
        return blockInf;
    }

    protected override void OnInstanceInit() {
        //generate block references
        mBlockInfos = new Dictionary<string, BlockInfo>(blocks.Length);
        for(int i = 0; i < blocks.Length; i++) {
            mBlockInfos.Add(blocks[i].name, blocks[i]);
        }

        //setup the max progress of LoL based on scenes count
        if(scenes.Length > 0)
            LoLManager.instance.progressMax = scenes.Length;
    }
}
