﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMapController : M8.SingletonBehaviour<GameMapController> {
    public enum Mode {
        Play,
        Edit
    }
        
    public Mode mode {
        get { return mMode; }
        set {
            if(mMode != value) {
                mMode = value;

                switch(mMode) {
                    case Mode.Play:
                        blockSelected = null;
                        
                        M8.SceneManager.instance.Resume();
                        break;
                    case Mode.Edit:
                        M8.SceneManager.instance.Pause();

                        mLastGhostPulseTime = Time.realtimeSinceStartup;
                        break;
                }

                if(modeChangeCallback != null)
                    modeChangeCallback(mMode);
            }
        }
    }

    public GameMapData mapData { get { return mMapData; } }

    public Block blockSelected {
        get { return mBlockSelected; }
        set {
            if(mBlockSelected != value) {
                var prevBlock = mBlockSelected;
                mBlockSelected = value;

                if(blockSelectedChangeCallback != null)
                    blockSelectedChangeCallback(mBlockSelected, prevBlock);
            }
        }
    }

    public EntityHero player {
        get {
            if(!mPlayer) {
                var playerGO = GameObject.FindGameObjectWithTag("Player");
                mPlayer = playerGO.GetComponent<EntityHero>();
            }

            return mPlayer;
        }
    }

    public float curGhostPulseT {
        get {
            float t = Mathf.Clamp01((Time.realtimeSinceStartup - mLastGhostPulseTime) / GameData.instance.blockGhostPulseDelay);
            return GameData.instance.blockGhostPulseCurve.Evaluate(t);
        }
    }

    public event System.Action<string, int, int> paletteUpdateCallback; //block name, new amount, delta
    public event System.Action<Mode> modeChangeCallback;
    public event System.Action<Block, Block> blockSelectedChangeCallback; //new block, previous block
    public event System.Action<Block> blockGhostDroppedCallback; //called when a block from the palette has been placed into the world (only if valid)
    public event System.Action<string> blockGhostCancelCallback; //called when a block ghost was cancelled via block controller (block name)

    private Dictionary<string, int> mBlockPalette = new Dictionary<string, int>();

    private Mode mMode = Mode.Play;

    private Block mBlockSelected;

    private GameMapData mMapData;

    private EntityHero mPlayer;

    private float mLastGhostPulseTime;
    
    public int PaletteCount(string blockName) {
        int count;
        mBlockPalette.TryGetValue(blockName, out count);
        return count;
    }

    public void PaletteChange(string blockName, int delta) {
        int amount;

        if(mBlockPalette.ContainsKey(blockName))
            amount = mBlockPalette[blockName];
        else {
            mBlockPalette.Add(blockName, 0);
            amount = 0;
        }

        amount = Mathf.Clamp(amount + delta, 0, GameData.paletteMaxCount);

        mBlockPalette[blockName] = amount;

        if(paletteUpdateCallback != null)
            paletteUpdateCallback(blockName, amount, delta);
    }

    /// <summary>
    /// Called by PaletteItemWidget when successfully dropping a block ghost into the world.
    /// </summary>
    public void PaletteBlockGhostDropped(Block blockGhost) {
        if(blockGhostDroppedCallback != null)
            blockGhostDroppedCallback(blockGhost);
    }

    /// <summary>
    /// Called by BlockMatterExpandPanel when a block ghost is cancelled
    /// </summary>
    public void PaletteBlockCancel(string blockName) {
        if(blockGhostCancelCallback != null)
            blockGhostCancelCallback(blockName);
    }

    public void Victory() {
        player.state = (int)EntityState.Victory;

        //focus camera to player
        GameCamera.instance.MoveTo(player.transform.position);

        mode = Mode.Play; //just in case

        //show victory modal
        M8.UIModal.Manager.instance.ModalCloseAll();
        M8.UIModal.Manager.instance.ModalOpen(Modals.victory);
    }

    IEnumerator Start() {
        yield return null;

        //show game HUD elements
        if(HUD.instance) {
            HUD.instance.palettePanel.Show(true);
            HUD.instance.retryButtonRoot.SetActive(true);
            HUD.instance.ShowHintButton();
        }

        while(M8.SceneManager.instance.isLoading)
            yield return null;
        
        mode = Mode.Edit;

        if(HUD.instance.CanShowHintButton())
            StartCoroutine(DoShowHintButton());
    }

    void Update() {
        switch(mMode) {
            case Mode.Edit:
                float curTime = Time.realtimeSinceStartup - mLastGhostPulseTime;
                if(curTime > GameData.instance.blockGhostPulseDelay) {
                    mLastGhostPulseTime = Time.realtimeSinceStartup;
                }
                break;
        }
    }

    protected override void OnInstanceInit() {
        mMapData = GetComponent<GameMapData>();
        if(mMapData == null) {
            Debug.LogError("GameMapData not found!");
            return;
        }

        //setup current palette
        for(int i = 0; i < mapData.initialPalette.Length; i++) {
            var paletteItem = mapData.initialPalette[i];

            if(paletteItem.amount > 0)
                mBlockPalette.Add(paletteItem.blockName, paletteItem.amount);

            //generate pool
            var blockInfo = GameData.instance.GetBlockInfo(paletteItem.blockName);
            if(blockInfo != null) {
                int poolCapacity = paletteItem.amount + paletteItem.capacityAdd;
                if(poolCapacity <= 0)
                    poolCapacity = mapData.paletteDefaultPoolCapacity;

                blockInfo.GeneratePool(transform, poolCapacity, poolCapacity);
            }
            else
                Debug.LogWarning("Block does not exists: " + paletteItem.blockName);
        }
    }

    protected override void OnInstanceDeinit() {
        if(mMode == Mode.Edit) {
            if(M8.SceneManager.instance)
                M8.SceneManager.instance.Resume();
        }

        //make sure to hide game HUD elements
        if(HUD.instance)
            HUD.instance.HideAll();
    }

    IEnumerator DoShowHintButton() {
        float timeElapsed = 0f;
        float delay = GameData.instance.showHintDelay;
        float lastTime = Time.realtimeSinceStartup;
        while(timeElapsed < delay) {            
            yield return null;
            timeElapsed += Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;
        }

        HUD.instance.EnableHintButton();
    }
}
