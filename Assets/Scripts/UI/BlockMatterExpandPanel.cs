using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class BlockMatterExpandPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [Header("Info")]
    public GameObject infoGO;
    public Text infoMatterCountText;
    public Color infoMatterCountValidColor = Color.white;
    public Color infoMatterCountInvalidColor = Color.red;
    public Button infoMatterDeployButton;

    [Header("Arrows")]
    public BlockMatterExpandWidget[] arrows;

    public bool isActive { get { return gameObject.activeSelf; } }

    public Block block { get { return mBlock; } }

    private RectTransform mTrans;
    private UIScreenAttachToWorld mWorldAttach;
    private Block mBlock;

    private CellIndex mPrevCellPos;
    private bool mIsExpandShown;

    public void Show(Block block) {
        gameObject.SetActive(true);

        //set block
        if(mBlock != null)
            ClearBlock();

        SetBlock(block);

        UpdateDimension();

        //setup info
        infoGO.SetActive(true);
        UpdateInfo();
    }

    public void Hide() {
        gameObject.SetActive(false);

        if(mBlock != null) {
            ClearBlock();
            mBlock = null;
        }
    }

    public void Deploy() {
        if(mBlock) {
            GameMapController.instance.PaletteChange(mBlock.blockName, -mBlock.matterCount);
            mBlock.mode = Block.Mode.Solid;            
        }
    }

    /// <summary>
    /// if forceReleaseBlock is true, release current block; if not, deploy it if it's valid
    /// </summary>
    public void Cancel(bool forceReleaseBlock) {
        var _block = mBlock;

        Hide();

        if(_block) {
            if(!forceReleaseBlock && _block.EditIsPlacementValid()) {
                GameMapController.instance.PaletteChange(_block.blockName, -_block.matterCount);
                _block.mode = Block.Mode.Solid;
            }
            else
                M8.PoolController.ReleaseAuto(_block.gameObject);
        }
    }

    void Awake() {
        mTrans = GetComponent<RectTransform>();
        mWorldAttach = GetComponent<UIScreenAttachToWorld>();

        mIsExpandShown = true;
    }

    void OnBlockDimensionChanged(Block b) {
        UpdateDimension();
        UpdateInfo();
    }

    void OnBlockModeChanged(Block b) {
        switch(b.mode) {
            case Block.Mode.Solid:
            case Block.Mode.None:
                //hide
                Hide();
                break;
        }
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        var gameCam = GameCamera.instance;
        Vector2 pos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);
        mPrevCellPos = GameMapController.instance.mapData.GetCellIndex(pos);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        var gameCam = GameCamera.instance;
        Vector2 pos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);
        var curCellPos = GameMapController.instance.mapData.GetCellIndex(pos);

        if(mPrevCellPos != curCellPos) {
            infoGO.SetActive(false);
            ShowExpand(false);

            var cellSize = GameData.instance.blockSize;
            CellIndex deltaCell = new CellIndex(curCellPos.row - mPrevCellPos.row, curCellPos.col - mPrevCellPos.col);

            mBlock.EditMove(new Vector2(deltaCell.col * cellSize.x, deltaCell.row * cellSize.y));

            mPrevCellPos = curCellPos;
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        infoGO.SetActive(true);
        
        if(mBlock.EditIsExpandable())
            ShowExpand(true);
    }

    private void UpdateInfo() {
        int paletteBlockCount = GameMapController.instance.PaletteCount(mBlock.blockName);

        int curBlockCount = mBlock.matterCount;

        bool isValid = curBlockCount <= paletteBlockCount;

        infoMatterCountText.color =  isValid ? infoMatterCountValidColor : infoMatterCountInvalidColor;

        infoMatterCountText.text = string.Format("{0}/{1}", curBlockCount.ToString("D2"), paletteBlockCount.ToString("D2"));

        if(infoMatterDeployButton)
            infoMatterDeployButton.interactable = isValid && mBlock.EditIsPlacementValid();
    }

    private void UpdateDimension() {
        //NOTE: assumes we are center pivot and anchored relative to screen
        var blockBounds = mBlock.editBounds;

        var cam = GameCamera.instance.camera2D.unityCamera;
        
        var minScreenSpace = cam.WorldToScreenPoint(blockBounds.min);
        var maxScreenSpace = cam.WorldToScreenPoint(blockBounds.max);

        mTrans.sizeDelta = new Vector2(Mathf.Abs(maxScreenSpace.x - minScreenSpace.x), Mathf.Abs(maxScreenSpace.y - minScreenSpace.y));

        mWorldAttach.position = blockBounds.center;
    }

    private void ShowExpand(bool show) {
        if(mIsExpandShown != show) {
            mIsExpandShown = show;

            for(int i = 0; i < arrows.Length; i++)
                arrows[i].gameObject.SetActive(mIsExpandShown);
        }
    }

    private void SetBlock(Block b) {
        mBlock = b;

        mBlock.dimensionChangedCallback += OnBlockDimensionChanged;
        mBlock.modeChangedCallback += OnBlockModeChanged;

        if(mBlock.EditIsExpandable()) {
            ShowExpand(true);

            for(int i = 0; i < arrows.Length; i++)
                arrows[i].block = mBlock;
        }
        else
            ShowExpand(false);

        if(mWorldAttach) {
            mWorldAttach.position = mBlock.editBounds.center;
            mWorldAttach.Update();
        }
    }

    private void ClearBlock() {
        mBlock.dimensionChangedCallback -= OnBlockDimensionChanged;
        mBlock.modeChangedCallback -= OnBlockModeChanged;

        for(int i = 0; i < arrows.Length; i++)
            arrows[i].block = null;

        if(mWorldAttach)
            mWorldAttach.worldAttach = null;
    }
}
