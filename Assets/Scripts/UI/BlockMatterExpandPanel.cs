using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class BlockMatterExpandPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [Header("Info")]
    public GameObject infoGO;
    public float infoYOfsNoExpand;
    public Text infoMatterCountText;
    public Color infoMatterCountValidColor = Color.white;
    public Color infoMatterCountInvalidColor = Color.red;
    public Button infoMatterDeployButton;

    [Header("Arrows")]
    public BlockMatterExpandWidget[] arrows;

    [Header("Measure")]
    public GameObject horzRoot;
    public Text horzLabel;
    public GameObject[] horzDoodads;

    public GameObject vertRoot;
    public Text vertLabel;
    public GameObject[] vertDoodads;

    public Text areaLabel;

    public bool isActive { get { return gameObject.activeSelf; } }

    public bool isMoveMode {
        get { return mIsMoveMode; }
        set {
            if(mIsMoveMode != value) {
                mIsMoveMode = value;
                if(mIsMoveMode) {
                    infoGO.SetActive(false);
                    ShowExpand(false);
                }
                else {
                    infoGO.SetActive(true);

                    if(mBlock && mBlock.EditIsExpandable())
                        ShowExpand(true);
                }
            }
        }
    }

    public Block block { get { return mBlock; } }

    private RectTransform mTrans;
    private UIScreenAttachToWorld mWorldAttach;
    private Block mBlock;

    private bool mIsDragging;
    private Vector2 mDragStartPos;
    private CellIndex mPrevCellPos;

    private bool mIsExpandShown;
    private bool mIsMoveMode;

    private RectTransform mInfoTrans;
    private Vector2 mInfoDefaultAnchorPos;

    private int mLastRow = -1;
    private int mLastCol = -1;

    public void Show(Block block) {
        if(mBlock == block)
            return;

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
    public void Cancel() {
        var _block = mBlock;

        Hide();

        if(_block) {
            if(GameMapController.instance.blockSelected == _block)
                GameMapController.instance.blockSelected = null;

            string blockName = _block.blockName;

            _block.EditCancel();

            _block.Release();
            /*if(!forceReleaseBlock && _block.EditIsPlacementValid()) {
                GameMapController.instance.PaletteChange(_block.blockName, -_block.matterCount);
                _block.mode = Block.Mode.Solid;
            }
            else
                M8.PoolController.ReleaseAuto(_block.gameObject);*/

            GameMapController.instance.PaletteBlockCancel(blockName);
        }
    }

    void OnDestroy() {

    }

    void OnDisable() {
        mIsDragging = false;
    }

    void Awake() {
        mTrans = GetComponent<RectTransform>();
        mWorldAttach = GetComponent<UIScreenAttachToWorld>();

        mInfoTrans = infoGO.GetComponent<RectTransform>();
        mInfoDefaultAnchorPos = mInfoTrans.anchoredPosition;

        mIsExpandShown = true;
    }

    void OnApplicationFocus(bool focus) {
        if(!focus) {
            //fail-safe since we can no longer receive EndDrag
            if(mIsDragging) {
                mIsDragging = false;

                if(mBlock && GameMapController.instance.blockSelected == mBlock) {
                    if(!mBlock.EditIsPlacementValid())
                        mBlock.EditSetPosition(mDragStartPos);

                    GameMapController.instance.blockSelected = null;
                }
            }
        }
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

    //these are needed for BlockEditSelect to simulate drag

    
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        mIsDragging = true;

        var gameCam = GameCamera.instance;
        Vector2 pos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);
        mPrevCellPos = GameMapController.instance.mapData.GetCellIndex(pos);

        mDragStartPos = mBlock.editBounds.center;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;

        var gameCam = GameCamera.instance;
        Vector2 pos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);
        var curCellPos = GameMapController.instance.mapData.GetCellIndex(pos);

        if(mPrevCellPos != curCellPos) {
            isMoveMode = true;

            var cellSize = GameData.instance.blockSize;
            CellIndex deltaCell = new CellIndex(curCellPos.row - mPrevCellPos.row, curCellPos.col - mPrevCellPos.col);

            mBlock.EditMove(new Vector2(deltaCell.col * cellSize.x, deltaCell.row * cellSize.y));

            mPrevCellPos = curCellPos;
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        mIsDragging = false;

        isMoveMode = false;

        //if placement is invalid, revert to original position
        if(!mBlock.EditIsPlacementValid()) {
            mBlock.EditSetPosition(mDragStartPos);

            LoLManager.instance.PlaySound(GameData.instance.soundBlockInvalidPath, false, false);
        }
        else {
            LoLManager.instance.PlaySound(GameData.instance.soundBlockPlacePath, false, false);
        }
    }

    private void UpdateInfo() {
        int paletteBlockCount = GameMapController.instance.PaletteCount(mBlock.blockName);
        int ghostBlockCount = HUD.instance.palettePanel.GetGhostCount(mBlock.blockName);

        int curBlockCount = mBlock.matterCount;

        int maxCount = (paletteBlockCount - ghostBlockCount) + curBlockCount;

        bool isValid = curBlockCount <= maxCount && mBlock.EditIsPlacementValid();

        infoMatterCountText.color =  isValid ? infoMatterCountValidColor : infoMatterCountInvalidColor;

        if(mBlock.mainBody) {
            float mass = mBlock.mainBody.mass;

            infoMatterCountText.text = string.Format("{0}/{1}\n{2:f2} kg", curBlockCount.ToString("D2"), maxCount.ToString("D2"), mass);
        }
        else {
            infoMatterCountText.text = string.Format("{0}/{1}", curBlockCount.ToString("D2"), maxCount.ToString("D2"));
        }

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

        int numCol = mBlock.cellSize.col;
        int numRow = mBlock.cellSize.row;

        bool isAreaChange = false;

        if(mLastCol != numCol) {
            mLastCol = numCol;
            isAreaChange = true;

            horzLabel.text = numCol.ToString();

            for(int i = 0; i < horzDoodads.Length; i++)
                horzDoodads[i].SetActive(numCol > 1);
        }

        if(mLastRow != numRow) {
            mLastRow = numRow;
            isAreaChange = true;

            vertLabel.text = numRow.ToString();

            for(int i = 0; i < vertDoodads.Length; i++)
                vertDoodads[i].SetActive(numRow > 1);
        }

        if(isAreaChange) {
            if(mIsExpandShown && numCol > 1 && numRow > 1) {
                areaLabel.gameObject.SetActive(true);
                areaLabel.text = (numCol * numRow).ToString();
            }
            else
                areaLabel.gameObject.SetActive(false);
        }
    }

    private void ShowExpand(bool show) {
        if(mIsExpandShown != show) {
            mIsExpandShown = show;

            for(int i = 0; i < arrows.Length; i++)
                arrows[i].gameObject.SetActive(mIsExpandShown);

            horzRoot.SetActive(mIsExpandShown);
            vertRoot.SetActive(mIsExpandShown);

            if(mIsExpandShown && mBlock != null) {
                int numCol = mBlock.cellSize.col;
                int numRow = mBlock.cellSize.row;

                if(numCol > 1 && numRow > 1) {
                    areaLabel.gameObject.SetActive(true);
                    areaLabel.text = (numCol * numRow).ToString();
                }
                else
                    areaLabel.gameObject.SetActive(false);
            }
            else
                areaLabel.gameObject.SetActive(false);

            Vector2 anchorPos = mInfoTrans.anchoredPosition;

            if(mIsExpandShown) {
                anchorPos.y = mInfoDefaultAnchorPos.y;
            }
            else {
                anchorPos.y = mInfoDefaultAnchorPos.y + infoYOfsNoExpand;
            }

            mInfoTrans.anchoredPosition = anchorPos;
        }
    }

    private void SetBlock(Block b) {
        mBlock = b;
        mLastCol = -1;
        mLastRow = -1;

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
