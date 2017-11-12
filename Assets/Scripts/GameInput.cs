using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [Header("Input")]
    public float dragScale = 0.3f;
    
    private BoxCollider2D mBoxColl;

    private Vector2 mDragLastPos;

    private Block mBlockGhost;

    void OnDestroy() {
        if(GameMapController.isInstantiated) {
            GameMapController.instance.modeChangeCallback -= OnGameModeChange;
            GameMapController.instance.blockActiveChangeCallback -= OnGameBlockActiveChange;
        }
    }

    void Awake() {
        mBoxColl = GetComponent<BoxCollider2D>();

        GameMapController.instance.modeChangeCallback += OnGameModeChange;
        GameMapController.instance.blockActiveChangeCallback += OnGameBlockActiveChange;
    }
        
    void Start () {
        //setup box collider size to be fullscreen        
        mBoxColl.offset = Vector2.zero;
        mBoxColl.size = (Vector2)GameCamera.instance.cameraViewBounds.size + new Vector2(1f, 1f);
    }
    
    void OnGameModeChange(GameMapController.Mode mode) {
        //release ghost if we are changing to play mode
        switch(mode) {
            case GameMapController.Mode.Play:
                if(mBlockGhost)
                    BlockGhostRelease();
                break;
        }
    }

    void OnGameBlockActiveChange(string newBlock, string prevBlock) {
        //we shouldn't really get here with an active ghost...but just in case, respawn with a new block ghost
        if(mBlockGhost) {
            Vector2 pos = mBlockGhost.transform.position;

            BlockGhostRelease();

            if(!string.IsNullOrEmpty(newBlock))
                BlockGhostAllocate(newBlock, pos);
        }
    }

    void BlockEditStart(PointerEventData eventData) {
        if(!mBlockGhost) {
            string blockNameActive = GameMapController.instance.blockNameActive;
            if(!string.IsNullOrEmpty(blockNameActive)) {

                var gameCam = GameCamera.instance;
                Vector2 pos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);

                BlockGhostAllocate(blockNameActive, pos);
            }
        }
    }

    void BlockEditEnd(PointerEventData eventData) {
        if(mBlockGhost) {
            var gameCam = GameCamera.instance;
            Vector2 pos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);

            mBlockGhost.EditEnd(pos);

            //check if it can be placed
            if(mBlockGhost.EditIsPlacementValid()) {
                //deploy and reduce palette block
                mBlockGhost.mode = Block.Mode.Solid;

                GameMapController.instance.PaletteChange(mBlockGhost.blockName, -mBlockGhost.matterCount);

                mBlockGhost = null;
            }
            else
                BlockGhostRelease();
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        switch(GameMapController.instance.mode) {
            case GameMapController.Mode.Edit:
                BlockEditStart(eventData);
                break;
        }
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
        switch(GameMapController.instance.mode) {
            case GameMapController.Mode.Play:
                break;

            case GameMapController.Mode.Edit:
                BlockEditEnd(eventData);
                break;
        }
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        switch(GameMapController.instance.mode) {
            case GameMapController.Mode.Play:
                var gameCam = GameCamera.instance;
                mDragLastPos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);
                break;

            case GameMapController.Mode.Edit:
                BlockEditStart(eventData);
                break;
        }
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        var gameCam = GameCamera.instance;
        Vector2 curPos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);

        switch(GameMapController.instance.mode) {
            case GameMapController.Mode.Play:
                Vector2 delta = (curPos - mDragLastPos) * dragScale;

                mDragLastPos = curPos;

                gameCam.SetPosition(gameCam.position - delta);
                break;

            case GameMapController.Mode.Edit:
                if(mBlockGhost) {
                    mBlockGhost.EditUpdate(curPos);
                }
                break;
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        switch(GameMapController.instance.mode) {
            case GameMapController.Mode.Play:
                break;

            case GameMapController.Mode.Edit:
                BlockEditEnd(eventData);
                break;
        }
    }

    private void BlockGhostAllocate(string blockName, Vector2 startPos) {
        var blockInfo = GameData.instance.GetBlockInfo(blockName);
        mBlockGhost = blockInfo.SpawnBlock(Block.Mode.Ghost);
        mBlockGhost.EditStart(startPos);
    }

    private void BlockGhostRelease() {
        M8.PoolController.ReleaseAuto(mBlockGhost.gameObject);
        mBlockGhost = null;
    }
}
