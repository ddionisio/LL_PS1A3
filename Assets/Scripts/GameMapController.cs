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
                        //check if block expand is active, hide if so
                        if(HUD.instance.blockMatterExpandPanel.isActive)
                            HUD.instance.blockMatterExpandPanel.Cancel(false);

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

    public EntityHero player {
        get {
            if(!mPlayer) {
                var playerGO = GameObject.FindGameObjectWithTag("Player");
                mPlayer = playerGO.GetComponent<EntityHero>();
            }

            return mPlayer;
        }
    }

    public event System.Action<string, int, int> paletteUpdateCallback; //block name, new amount, delta
    public event System.Action<Mode> modeChangeCallback;
    public event System.Action<string, string> blockActiveChangeCallback; //new block, previous block

    private Dictionary<string, int> mBlockPalette = new Dictionary<string, int>();

    private Mode mMode = Mode.Play;

    private string mBlockNameActive;

    private GameMapData mMapData;

    private EntityHero mPlayer;
    
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

    public void Victory() {
        player.state = (int)EntityState.Victory;

        //focus camera to player
        GameCamera.instance.MoveTo(player.transform.position);

        mode = Mode.Play; //just in case

        HUD.instance.HideAll();

        //show victory modal
        M8.UIModal.Manager.instance.ModalOpen(Modals.victory);
    }

    IEnumerator Start() {
        do {
            yield return null;
        } while(M8.SceneManager.instance.isLoading);

        //show game HUD elements
        if(HUD.instance) {
            HUD.instance.palettePanel.Show(true);

            HUD.instance.retryButtonRoot.SetActive(true);

            mode = Mode.Edit;
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

            int poolCapacity = paletteItem.amount + paletteItem.capacityAdd;
            if(poolCapacity <= 0)
                poolCapacity = mapData.paletteDefaultPoolCapacity;

            blockInfo.GeneratePool(transform, poolCapacity, poolCapacity);
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
}
