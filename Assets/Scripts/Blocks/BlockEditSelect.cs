using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class BlockEditSelect : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public Block block;

    private Collider2D mColl;
    private BoxCollider2D mBoxColl;

    private bool mIsDragging;
    private Vector2 mDragStartPos;
    private CellIndex mPrevCellPos;

    void OnDestroy() {
        if(block) {
            block.spawnCallback -= OnBlockSpawned;
            block.releaseCallback -= OnBlockDespawned;
        }

        if(GameMapController.instance) {
            GameMapController.instance.modeChangeCallback -= OnGameModeChanged;
            GameMapController.instance.blockSelectedChangeCallback -= OnGameBlockSelectedChange;
        }
    }

    void Awake() {
        mColl = GetComponent<Collider2D>();
        mBoxColl = mColl as BoxCollider2D;

        block.spawnCallback += OnBlockSpawned;
        block.releaseCallback += OnBlockDespawned;
    }

    void OnApplicationFocus(bool focus) {
        if(!focus) {
            //fail-safe since we can no longer receive EndDrag
            if(mIsDragging) {
                mIsDragging = false;

                if(block && GameMapController.instance.blockSelected == block) {
                    if(!block.EditIsPlacementValid())
                        block.EditSetPosition(mDragStartPos);

                    GameMapController.instance.blockSelected = null;
                }
            }
        }
    }

    void OnBlockSpawned(M8.EntityBase ent) {
        block.modeChangedCallback += OnBlockModeChanged;
        block.dimensionChangedCallback += OnBlockDimensionChanged;

        GameMapController.instance.modeChangeCallback += OnGameModeChanged;
        GameMapController.instance.blockSelectedChangeCallback += OnGameBlockSelectedChange;
    }

    void OnBlockDespawned(M8.EntityBase ent) {
        block.modeChangedCallback -= OnBlockModeChanged;
        block.dimensionChangedCallback -= OnBlockDimensionChanged;

        if(GameMapController.instance) {
            GameMapController.instance.modeChangeCallback -= OnGameModeChanged;
            GameMapController.instance.blockSelectedChangeCallback -= OnGameBlockSelectedChange;
        }

        mIsDragging = false;
    }

    void OnBlockModeChanged(Block b) {
        UpdateCollider();
    }

    void OnBlockDimensionChanged(Block b) {
        UpdateCollider();
    }

    void OnGameModeChanged(GameMapController.Mode mode) {
        UpdateCollider();
    }

    void OnGameBlockSelectedChange(Block newBlock, Block prevBlock) {
        block.EditEnableCollision(newBlock != block);

        UpdateCollider();
    }
    
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        GameMapController.instance.blockSelected = block;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        mIsDragging = true;
        var gameCam = GameCamera.instance;
        Vector2 pos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);
        mPrevCellPos = GameMapController.instance.mapData.GetCellIndex(pos);

        mDragStartPos = block.editBounds.center;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;

        var gameCam = GameCamera.instance;
        Vector2 pos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);
        var curCellPos = GameMapController.instance.mapData.GetCellIndex(pos);

        if(mPrevCellPos != curCellPos) {
            if(HUD.instance.blockMatterExpandPanel.block == block)
                HUD.instance.blockMatterExpandPanel.isMoveMode = true;

            var cellSize = GameData.instance.blockSize;
            CellIndex deltaCell = new CellIndex(curCellPos.row - mPrevCellPos.row, curCellPos.col - mPrevCellPos.col);

            block.EditMove(new Vector2(deltaCell.col * cellSize.x, deltaCell.row * cellSize.y));

            mPrevCellPos = curCellPos;
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        mIsDragging = false;

        if(HUD.instance.blockMatterExpandPanel.block == block) {
            HUD.instance.blockMatterExpandPanel.isMoveMode = false;

            //if placement is invalid, revert to original position
            if(!block.EditIsPlacementValid()) {
                block.EditSetPosition(mDragStartPos);

                LoLManager.instance.PlaySound(GameData.instance.soundBlockInvalidPath, false, false);
            }
            else {
                LoLManager.instance.PlaySound(GameData.instance.soundBlockPlacePath, false, false);
            }
        }
    }

    void UpdateCollider() {
        if(block.mode != Block.Mode.Ghost
            || GameMapController.instance.mode != GameMapController.Mode.Edit) {
            mColl.enabled = false;
            return;
        }

        mColl.enabled = GameMapController.instance.blockSelected != block;

        //setup box size based on bound size of block
        var blockCollBounds = block.editBounds;
                        
        if(mBoxColl) {
            var size = (Vector2)blockCollBounds.size;

            mBoxColl.offset = transform.worldToLocalMatrix.MultiplyPoint3x4(blockCollBounds.center);
            mBoxColl.size = size;
        }
    }
}
