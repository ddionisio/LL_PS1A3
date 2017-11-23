using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockMatterExpandWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public enum Dir {
        Up,
        Down,
        Left,
        Right,

        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
    }

    public Dir dir;

    public Block block { get { return mBlock; } set { mBlock = value; } }

    private Block mBlock;

    private CellIndex mPrevCellPos;

    private int mTop;
    private int mBottom;
    private int mLeft;
    private int mRight;

    private bool mIsDragging;

    void OnApplicationFocus(bool focus) {
        //fail safe
        if(mIsDragging) {
            mIsDragging = false;

            if(mBlock)
                mBlock.EditExpand(-mTop, -mBottom, -mLeft, -mRight);
        }
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        mIsDragging = true;

        var gameCam = GameCamera.instance;
        Vector2 pos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);
        mPrevCellPos = GameMapController.instance.mapData.GetCellIndex(pos);

        mTop = mBottom = mLeft = mRight = 0;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;

        var gameCam = GameCamera.instance;
        Vector2 pos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);
        var curCellPos = GameMapController.instance.mapData.GetCellIndex(pos);

        if(curCellPos != mPrevCellPos) {
            CellIndex deltaCell = new CellIndex(curCellPos.row - mPrevCellPos.row, curCellPos.col - mPrevCellPos.col);

            switch(dir) {
                case Dir.Up:
                    if(deltaCell.row != 0 && deltaCell.row + mBlock.cellSize.row > 0) {
                        mTop += deltaCell.row;

                        mBlock.EditExpand(deltaCell.row, 0, 0, 0);
                        mPrevCellPos = curCellPos;
                    }
                    break;
                case Dir.Down:
                    if(deltaCell.row != 0 && mBlock.cellSize.row - deltaCell.row > 0) {
                        mBottom += deltaCell.row;

                        mBlock.EditExpand(0, deltaCell.row, 0, 0);
                        mPrevCellPos = curCellPos;
                    }
                    break;
                case Dir.Left:
                    if(deltaCell.col != 0 && mBlock.cellSize.col - deltaCell.col > 0) {
                        mLeft += deltaCell.col;

                        mBlock.EditExpand(0, 0, deltaCell.col, 0);
                        mPrevCellPos = curCellPos;
                    }
                    break;
                case Dir.Right:
                    if(deltaCell.col != 0 && deltaCell.col + mBlock.cellSize.col > 0) {
                        mRight += deltaCell.col;

                        mBlock.EditExpand(0, 0, 0, deltaCell.col);
                        mPrevCellPos = curCellPos;
                    }
                    break;

                case Dir.UpLeft:
                    //cull up
                    if(deltaCell.row + mBlock.cellSize.row <= 0) { deltaCell.row = 0; curCellPos.row = mPrevCellPos.row; }
                    //cull left
                    if(mBlock.cellSize.col - deltaCell.col <= 0) { deltaCell.col = 0; curCellPos.col = mPrevCellPos.col; }

                    if(deltaCell.row != 0 || deltaCell.col != 0) {
                        mTop += deltaCell.row;
                        mLeft += deltaCell.col;

                        mBlock.EditExpand(deltaCell.row, 0, deltaCell.col, 0);
                        mPrevCellPos = curCellPos;
                    }
                    break;
                case Dir.UpRight:
                    //cull up
                    if(deltaCell.row + mBlock.cellSize.row <= 0) { deltaCell.row = 0; curCellPos.row = mPrevCellPos.row; }
                    //cull right
                    if(deltaCell.col + mBlock.cellSize.col <= 0) { deltaCell.col = 0; curCellPos.col = mPrevCellPos.col; }

                    if(deltaCell.row != 0 || deltaCell.col != 0) {
                        mTop += deltaCell.row;
                        mRight += deltaCell.col;

                        mBlock.EditExpand(deltaCell.row, 0, 0, deltaCell.col);
                        mPrevCellPos = curCellPos;
                    }
                    break;
                case Dir.DownLeft:
                    //cull down
                    if(mBlock.cellSize.row - deltaCell.row <= 0) { deltaCell.row = 0; curCellPos.row = mPrevCellPos.row; }
                    //cull left
                    if(mBlock.cellSize.col - deltaCell.col <= 0) { deltaCell.col = 0; curCellPos.col = mPrevCellPos.col; }

                    if(deltaCell.row != 0 || deltaCell.col != 0) {
                        mBottom += deltaCell.row;
                        mLeft += deltaCell.col;

                        mBlock.EditExpand(0, deltaCell.row, deltaCell.col, 0);
                        mPrevCellPos = curCellPos;
                    }
                    break;
                case Dir.DownRight:
                    //cull down
                    if(mBlock.cellSize.row - deltaCell.row <= 0) { deltaCell.row = 0; curCellPos.row = mPrevCellPos.row; }
                    //cull right
                    if(deltaCell.col + mBlock.cellSize.col <= 0) { deltaCell.col = 0; curCellPos.col = mPrevCellPos.col; }

                    if(deltaCell.row != 0 || deltaCell.col != 0) {
                        mBottom += deltaCell.row;
                        mRight += deltaCell.col;

                        mBlock.EditExpand(0, deltaCell.row, 0, deltaCell.col);
                        mPrevCellPos = curCellPos;
                    }
                    break;
            }
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(mIsDragging) {
            mIsDragging = false;

            //if invalid, revert
            if(!mBlock.EditIsPlacementValid())
                mBlock.EditExpand(-mTop, -mBottom, -mLeft, -mRight);
        }
    }
}
