using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class PaletteItemWidget : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public const int ghostActiveCapacity = 16;
    public const string paramBlockInfo = "blockInf";
    public const string paramShowIntro = "showIntro";

    public Image blockImage;
    public Text blockNameText;
    public Text blockCountText;
    public GameObject selectActiveGO;

    public string blockName { get { return mBlockName; } }

    public int ghostMatterCount {
        get {
            int ghostCount = mGhostMatterCount;
            if(mBlockGhost)
                ghostCount += mBlockGhost.matterCount;

            return ghostCount;
        }
    }

    public bool isSelected { get { return GameMapController.instance.blockSelected && GameMapController.instance.blockSelected.blockName == mBlockName; } }

    public event Action<PaletteItemWidget> releaseCallback;

    private string mBlockName;

    private Block mBlockGhost;
    private bool mIsDragging;

    private Graphic mGraphic; //this is the one that controls interaction

    private M8.CacheList<Block> mGhostActives = new M8.CacheList<Block>(ghostActiveCapacity);
    private int mGhostMatterCount;

    public int GetGhostMatterCount(Block excludeBlock) {
        int ghostCount = 0;
        for(int i = 0; i < mGhostActives.Count; i++) {
            if(mGhostActives[i] != excludeBlock)
                ghostCount += mGhostActives[i].matterCount;
        }

        if(mBlockGhost && mBlockGhost != excludeBlock)
            ghostCount += mBlockGhost.matterCount;

        return ghostCount;
    }

    public void UpdateCount() {
        int _ghostCount = ghostMatterCount;

        int amount = !string.IsNullOrEmpty(mBlockName) ? GameMapController.instance.PaletteCount(mBlockName) : 0;

        blockCountText.text = string.Format("{0}/{1}", _ghostCount.ToString("D2"), amount.ToString("D2"));
    }

    void OnDestroy() {
        CleanUpCallbacks();
    }

    void OnEnable() {
        UpdateCount();
    }

    void Awake() {
        mGraphic = GetComponent<Graphic>();
    }

    void OnApplicationFocus(bool focus) {
        if(!focus) {
            //fail-safe
            if(mIsDragging) {
                ClearDragging();

                UpdateCount();

                UpdateInteractible();
            }
        }
    }

    void OnPaletteUpdate(string blockName, int amount, int delta) {
        if(blockName != mBlockName)
            return;

        if(amount > 0) {
            UpdateCount();
            UpdateInteractible();
        }
        else {
            //do animation thing
            //delete after animation thing
            M8.PoolController.ReleaseAuto(gameObject);
        }
    }

    void OnGameBlockSelectChange(Block newBlock, Block prevBlock) {
        bool _isSelected = isSelected;
        
        ApplySelected(_isSelected);

        if(!_isSelected)
            ClearDragging();        
    }

    void OnGameChangeMode(GameMapController.Mode mode) {
        switch(mode) {
            case GameMapController.Mode.Play:
                ClearDragging();

                //deploy active ghosts
                mGhostActives.Sort();

                for(int i = 0; i < mGhostActives.Count; i++) {
                    var ghostBlock = mGhostActives[i];
                    if(ghostBlock && !ghostBlock.isReleased) {
                        ghostBlock.dimensionChangedCallback -= OnGhostBlockDimensionChanged;
                        ghostBlock.releaseCallback -= OnGhostBlockRelease;

                        if(ghostBlock.EditIsPlacementValid()) {
                            ghostBlock.mode = Block.Mode.Solid;
                        }
                        else {
                            mGhostMatterCount -= ghostBlock.matterCount;

                            ghostBlock.Release();
                        }
                    }
                }

                mGhostActives.Clear();

                GameMapController.instance.PaletteChange(blockName, -mGhostMatterCount);

                mGhostMatterCount = 0;
                //

                UpdateCount();
                break;
        }

        UpdateInteractible();
    }
        
    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        GameMapController.instance.paletteUpdateCallback += OnPaletteUpdate;
        GameMapController.instance.blockSelectedChangeCallback += OnGameBlockSelectChange;
        GameMapController.instance.modeChangeCallback += OnGameChangeMode;

        BlockInfo blockInf = parms.GetValue<BlockInfo>(paramBlockInfo);
        bool showIntro = parms.GetValue<bool>(paramShowIntro);

        //setup data
        mBlockName = blockInf.name;

        //setup state
        UpdateInteractible();

        //setup display
        blockNameText.text = M8.Localize.Get(blockInf.nameDisplayRef);

        blockImage.sprite = blockInf.icon;

        UpdateCount();

        ApplySelected(isSelected);

        //show intro?
        if(showIntro) {
            //animation
        }
    }

    void M8.IPoolDespawn.OnDespawned() {
        CleanUpCallbacks();

        //revert dragging
        ClearDragging();

        //clear up any active ghosts
        for(int i = 0; i < mGhostActives.Count; i++) {
            var ghostBlock = mGhostActives[i];
            if(ghostBlock && !ghostBlock.isReleased) {
                ghostBlock.Release();
            }
        }

        mGhostActives.Clear();
        
        mGhostMatterCount = 0;

        if(releaseCallback != null)
            releaseCallback(this);
    }
    
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if(!mIsDragging) {
            mIsDragging = true;

            HUD.instance.paletteItemDrag.Activate(blockImage.sprite);
            HUD.instance.paletteItemDrag.transform.position = eventData.position;
        }
        
        //setup block ghost
        if(!mBlockGhost) {
            var gameCam = GameCamera.instance;
            Vector2 pos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);

            var blockInfo = GameData.instance.GetBlockInfo(mBlockName);

            mBlockGhost = blockInfo.SpawnBlock(Block.Mode.Ghost);

            mBlockGhost.dimensionChangedCallback += OnGhostBlockDimensionChanged;
            mBlockGhost.releaseCallback += OnGhostBlockRelease;

            mBlockGhost.EditStart(pos);

            GameMapController.instance.blockSelected = mBlockGhost;

            HUD.instance.blockMatterExpandPanel.isMoveMode = true;
        }
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(mIsDragging) {
            HUD.instance.paletteItemDrag.transform.position = eventData.position;
        }

        if(mBlockGhost) {
            //move the block ghost within grid as 1x1
            var gameCam = GameCamera.instance;
            Vector2 pos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);
            
            mBlockGhost.EditStart(pos);
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        //only put the block if it's in the game area, away from the panel
        if(!HUD.instance.paletteItemDrag.isShown) {
            //make sure its placement is valid
            if(mBlockGhost.EditIsPlacementValid()) {
                //make sure it's within camera bounds
                var blockBounds = mBlockGhost.editBounds;
                if(GameCamera.instance.isVisible(blockBounds)) {
                    var block = mBlockGhost;
                    mBlockGhost = null;

                    //add to active ghosts
                    mGhostActives.Add(block);
                    mGhostMatterCount += block.matterCount;

                    GameMapController.instance.PaletteBlockGhostDropped(block);
                }
            }
        }
        
        ClearDragging();

        HUD.instance.blockMatterExpandPanel.isMoveMode = false;

        UpdateCount();

        UpdateInteractible();
    }

    void OnGhostBlockDimensionChanged(Block block) {
        if(mBlockGhost != block) {
            mGhostMatterCount = 0;
            for(int i = 0; i < mGhostActives.Count; i++)
                mGhostMatterCount += mGhostActives[i].matterCount;
        }

        UpdateCount();
        UpdateInteractible();
    }

    void OnGhostBlockRelease(M8.EntityBase ent) {
        if(mBlockGhost == ent) { //we shouldn't really get here with an active ghost
            mBlockGhost.dimensionChangedCallback -= OnGhostBlockDimensionChanged;
            mBlockGhost.releaseCallback -= OnGhostBlockRelease;            
            mBlockGhost = null;

            ClearDragging();

            UpdateCount();
        }
        else {
            for(int i = 0; i < mGhostActives.Count; i++) {
                if(mGhostActives[i] == ent) {
                    mGhostActives[i].dimensionChangedCallback -= OnGhostBlockDimensionChanged;
                    mGhostActives[i].releaseCallback -= OnGhostBlockRelease;

                    mGhostMatterCount -= mGhostActives[i].matterCount;

                    mGhostActives.RemoveAt(i);

                    UpdateCount();
                    break;
                }
            }
        }

        UpdateInteractible();
    }

    private void ApplySelected(bool select) {
        //select if we are the new block
        if(select) {
            //do animation thing

            if(selectActiveGO)
                selectActiveGO.SetActive(true);
        }
        //deselect if we were the previous block, and there is no new block active
        else {
            //do animation thing

            if(selectActiveGO)
                selectActiveGO.SetActive(false);
        }
    }
        
    private void CleanUpCallbacks() {
        if(GameMapController.isInstantiated) {
            GameMapController.instance.paletteUpdateCallback -= OnPaletteUpdate;
            GameMapController.instance.blockSelectedChangeCallback -= OnGameBlockSelectChange;
            GameMapController.instance.modeChangeCallback -= OnGameChangeMode;
        }
    }

    private void ClearDragging() {
        if(mIsDragging) {
            HUD.instance.paletteItemDrag.Deactivate();
            mIsDragging = false;
        }

        if(mBlockGhost) {
            var _ghostBlock = mBlockGhost;
            mBlockGhost = null;

            _ghostBlock.dimensionChangedCallback -= OnGhostBlockDimensionChanged;
            _ghostBlock.releaseCallback -= OnGhostBlockRelease;

            _ghostBlock.Release();

            if(GameMapController.instance.blockSelected == _ghostBlock)
                GameMapController.instance.blockSelected = null;
        }
    }

    private void UpdateInteractible() {
        if(mGraphic) {
            switch(GameMapController.instance.mode) {
                case GameMapController.Mode.Edit:
                    int _ghostCount = ghostMatterCount;
                    int amount = GameMapController.instance.PaletteCount(mBlockName);

                    mGraphic.raycastTarget = _ghostCount < amount;
                    break;
                case GameMapController.Mode.Play:
                    mGraphic.raycastTarget = false;
                    break;
            }
        }
    }
}
