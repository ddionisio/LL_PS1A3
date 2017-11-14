using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class PaletteItemWidget : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public const string paramBlockInfo = "blockInf";
    public const string paramShowIntro = "showIntro";

    public Image blockImage;
    public Text blockNameText;
    public Text blockCountText;
    public GameObject selectActiveGO;

    public string blockName { get { return mBlockName; } }

    public bool isSelected { get { return GameMapController.instance.blockNameActive == mBlockName; } }

    public event Action<PaletteItemWidget> releaseCallback;

    private string mBlockName;

    private Block mBlockGhost;
    private bool mIsDragging;

    private Graphic mGraphic; //this is the one that controls interaction

    void OnDestroy() {
        CleanUpCallbacks();
    }

    void Awake() {
        mGraphic = GetComponent<Graphic>();
    }

    void OnPaletteUpdate(string blockName, int amount, int delta) {
        if(amount > 0) {
            UpdateCount(amount);
        }
        else {
            //do animation thing
            //delete after animation thing
            M8.PoolController.ReleaseAuto(gameObject);
        }
    }

    void OnGameBlockActiveChange(string newBlockName, string prevBlockName) {
        //select if we are the new block
        if(mBlockName == newBlockName)
            ApplySelected(true);
        //deselect if we were the previous block, and there is no new block active
        else if(mBlockName == prevBlockName) {
            ApplySelected(false);

            ClearDragging();
        }
    }

    void OnGameChangeMode(GameMapController.Mode mode) {
        switch(mode) {
            case GameMapController.Mode.Edit:
                if(mGraphic) mGraphic.raycastTarget = true;
                break;

            case GameMapController.Mode.Play:
                if(GameMapController.instance.blockNameActive == mBlockName)
                    GameMapController.instance.blockNameActive = "";

                if(mGraphic) mGraphic.raycastTarget = false;
                break;
        }
    }
        
    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        GameMapController.instance.paletteUpdateCallback += OnPaletteUpdate;
        GameMapController.instance.blockActiveChangeCallback += OnGameBlockActiveChange;
        GameMapController.instance.modeChangeCallback += OnGameChangeMode;

        BlockInfo blockInf = parms.GetValue<BlockInfo>(paramBlockInfo);
        bool showIntro = parms.GetValue<bool>(paramShowIntro);

        //setup data
        mBlockName = blockInf.name;

        //setup state
        if(mGraphic) {
            switch(GameMapController.instance.mode) {
                case GameMapController.Mode.Edit:
                    mGraphic.raycastTarget = true;
                    break;
                case GameMapController.Mode.Play:
                    mGraphic.raycastTarget = false;
                    break;
            }
        }

        //setup display
        blockNameText.text = M8.Localize.Get(blockInf.nameDisplayRef);

        blockImage.sprite = blockInf.icon;

        UpdateCount(GameMapController.instance.PaletteCount(mBlockName));

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

        if(releaseCallback != null)
            releaseCallback(this);
    }
    
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        GameMapController.instance.blockNameActive = mBlockName;

        if(!mIsDragging) {
            mIsDragging = true;

            HUD.instance.paletteItemDrag.Activate(blockImage.sprite);
            HUD.instance.paletteItemDrag.transform.position = eventData.position;
        }

        //check to see if block interface is active, if so, clear it out and release the block ghost
        if(HUD.instance.blockMatterExpandPanel.isActive) {
            HUD.instance.blockMatterExpandPanel.Cancel(false);
        }

        //setup block ghost
        if(!mBlockGhost) {
            var gameCam = GameCamera.instance;
            Vector2 pos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);

            var blockInfo = GameData.instance.GetBlockInfo(mBlockName);

            mBlockGhost = blockInfo.SpawnBlock(Block.Mode.Ghost);
            mBlockGhost.EditStart(pos);
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
            //make sure it's within camera bounds
            var blockBounds = mBlockGhost.mainCollider.bounds;
            if(GameCamera.instance.isVisible(blockBounds)) {
                HUD.instance.blockMatterExpandPanel.Show(mBlockGhost);
                mBlockGhost = null;
            }
        }

        ClearDragging();

        if(GameMapController.instance.blockNameActive == mBlockName)
            GameMapController.instance.blockNameActive = "";
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

    private void UpdateCount(int amount) {
        blockCountText.text = amount.ToString("D2");
    }

    private void CleanUpCallbacks() {
        if(GameMapController.isInstantiated) {
            GameMapController.instance.paletteUpdateCallback -= OnPaletteUpdate;
            GameMapController.instance.blockActiveChangeCallback -= OnGameBlockActiveChange;
            GameMapController.instance.modeChangeCallback -= OnGameChangeMode;
        }
    }

    private void ClearDragging() {
        if(mIsDragging) {
            HUD.instance.paletteItemDrag.Deactivate();
            mIsDragging = false;
        }

        if(mBlockGhost) {
            M8.PoolController.ReleaseAuto(mBlockGhost.gameObject);
            mBlockGhost = null;
        }
    }
}
