using System.Collections;
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
                        M8.SceneManager.instance.Resume();
                        break;
                    case Mode.Edit:
                        M8.SceneManager.instance.Pause();
                        break;
                }

                if(modeChangeCallback != null)
                    modeChangeCallback(mMode);
            }
        }
    }

    public GameMapData mapData { get { return mMapData; } }

    public string blockNameActive {
        get { return mBlockNameActive; }
        set {
            if(mBlockNameActive != value) {
                string prevBlockName = mBlockNameActive;
                mBlockNameActive = value;

                if(blockActiveChangeCallback != null)
                    blockActiveChangeCallback(mBlockNameActive, prevBlockName);
            }
        }
    }

    public event System.Action<string, int, int> paletteUpdateCallback; //block name, new amount, delta
    public event System.Action<Mode> modeChangeCallback;
    public event System.Action<string, string> blockActiveChangeCallback; //new block, previous block

    private Dictionary<string, int> mBlockPalette = new Dictionary<string, int>();

    private Mode mMode = Mode.Play;

    private string mBlockNameActive;

    private GameMapData mMapData;
    
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

    IEnumerator Start() {
        yield return new WaitForSeconds(0.1f);

        //show game HUD elements
        if(HUD.instance) {
            HUD.instance.palettePanel.Show(true);
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
            blockInfo.GeneratePool(transform, mMapData.blockPoolStartCapacity, mMapData.blockPoolMaxCapacity);
        }
    }

    protected override void OnInstanceDeinit() {
        if(mMode == Mode.Edit) {
            if(M8.SceneManager.instance)
                M8.SceneManager.instance.Resume();
        }

        //make sure to hide game HUD elements
        if(HUD.instance) {
            HUD.instance.palettePanel.Show(false);
        }
    }
}
