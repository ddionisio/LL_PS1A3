using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PalettePanel : MonoBehaviour {
    public const int widgetCacheCapacity = 32;

    public M8.PoolController widgetPool;
    public Transform widgetContainer;

    public M8.Animator.AnimatorData animator;
    public string takeEditShow;
    public string takeEditHide;

    public Graphic toggleButton;

    public string toggleSoundOpenPath;
    public string toggleSoundClosePath;

    public bool isShown { get { return mIsShow; } }
    
    private M8.CacheList<PaletteItemWidget> mActiveWidgets = new M8.CacheList<PaletteItemWidget>(widgetCacheCapacity);
    private bool mIsShow = false;

    private int mTakeEditShowId;
    private int mTakeEditHideId;

    public PaletteItemWidget GetActiveWidget(int index) {
        if(index >= mActiveWidgets.Count)
            return null;

        return mActiveWidgets[index];
    }

    public PaletteItemWidget GetActiveWidget(string blockName) {
        for(int i = 0; i < mActiveWidgets.Count; i++) {
            if(mActiveWidgets[i].blockName == blockName)
                return mActiveWidgets[i];
        }

        return null;
    }

    public int GetGhostCount(string blockName) {
        for(int i = 0; i < mActiveWidgets.Count; i++) {
            if(mActiveWidgets[i].blockName == blockName)
                return mActiveWidgets[i].ghostMatterCount;
        }

        return 0;
    }

    public int GetGhostCountExclude(Block block) {
        for(int i = 0; i < mActiveWidgets.Count; i++) {
            if(mActiveWidgets[i].blockName == block.blockName)
                return mActiveWidgets[i].GetGhostMatterCount(block);
        }

        return 0;
    }

    public void Show(bool aShow) {
        if(mIsShow != aShow) {
            mIsShow = aShow;

            if(mIsShow) {
                gameObject.SetActive(true);

                //add widgets
                var blocks = GameData.instance.blocks;
                for(int i = 0; i < blocks.Length; i++) {
                    var blockInfo = blocks[i];

                    if(GameMapController.instance.PaletteCount(blockInfo.name) > 0)
                        AddNewPaletteItem(blockInfo, false);
                }

                //ensure we are positioned properly
                switch(GameMapController.instance.mode) {
                    case GameMapController.Mode.Play:
                        toggleButton.transform.localScale = new Vector3(-1f, 1f, 1f);
                        animator.ResetTake(mTakeEditShowId);
                        break;
                    case GameMapController.Mode.Edit:
                        toggleButton.transform.localScale = new Vector3(1f, 1f, 1f);
                        animator.ResetTake(mTakeEditHideId);
                        break;
                }

                GameMapController.instance.paletteUpdateCallback += OnGamePaletteUpdate;
                GameMapController.instance.modeChangeCallback += OnGameModeChange;
                GameMapController.instance.blockSelectedChangeCallback += OnGameBlockSelectChanged;
            }
            else {
                GameMapController.instance.paletteUpdateCallback -= OnGamePaletteUpdate;
                GameMapController.instance.modeChangeCallback -= OnGameModeChange;
                GameMapController.instance.blockSelectedChangeCallback -= OnGameBlockSelectChanged;

                //clear up widgets
                for(int i = 0; i < mActiveWidgets.Count; i++) {
                    if(mActiveWidgets[i]) {
                        mActiveWidgets[i].releaseCallback -= OnWidgetRelease;
                        widgetPool.Release(mActiveWidgets[i].gameObject);
                    }
                }

                mActiveWidgets.Clear();

                gameObject.SetActive(false);
            }
        }
    }

    public void ToggleEdit() {
        switch(GameMapController.instance.mode) {
            case GameMapController.Mode.Play:
                GameMapController.instance.mode = GameMapController.Mode.Edit;
                break;

            case GameMapController.Mode.Edit:
                GameMapController.instance.mode = GameMapController.Mode.Play;
                break;
        }
    }

    void OnDestroy() {
        if(animator) {
            animator.takeCompleteCallback -= OnAnimatorComplete;
        }
    }

    void Awake() {
        animator.takeCompleteCallback += OnAnimatorComplete;

        mTakeEditShowId = animator.GetTakeIndex(takeEditShow);
        mTakeEditHideId = animator.GetTakeIndex(takeEditHide);
    }

    void AddNewPaletteItem(BlockInfo blockInfo, bool showIntro) {
        var parms = new M8.GenericParams();
        parms[PaletteItemWidget.paramBlockInfo] = blockInfo;
        parms[PaletteItemWidget.paramShowIntro] = showIntro;

        PaletteItemWidget widget = widgetPool.Spawn<PaletteItemWidget>(blockInfo.name, widgetContainer, parms);

        widget.releaseCallback += OnWidgetRelease;

        //hide if we are in play mode
        if(GameMapController.instance.mode == GameMapController.Mode.Play)
            widget.gameObject.SetActive(false);

        mActiveWidgets.Add(widget);
    }

    void OnGameModeChange(GameMapController.Mode mode) {
        switch(mode) {
            case GameMapController.Mode.Play:
                toggleButton.raycastTarget = false;

                //play edit mode exit
                animator.Play(mTakeEditHideId);

                LoLManager.instance.PlaySound(toggleSoundClosePath, false, false);
                break;

            case GameMapController.Mode.Edit:
                toggleButton.raycastTarget = false;

                //show active items
                for(int i = 0; i < mActiveWidgets.Count; i++) {
                    mActiveWidgets[i].gameObject.SetActive(true);
                    mActiveWidgets[i].UpdateCount();
                }

                //play edit mode enter
                animator.Play(mTakeEditShowId);

                LoLManager.instance.PlaySound(toggleSoundOpenPath, false, false);
                break;
        }
    }

    void OnGamePaletteUpdate(string blockName, int amount, int delta) {
        if(amount == 0) //active palette item should remove itself
            return;

        //allocate new widget?
        int widgetInd = -1;
        for(int i = 0; i < mActiveWidgets.Count; i++) {
            if(mActiveWidgets[i].blockName == blockName) {
                widgetInd = i;
                break;
            }
        }

        if(widgetInd == -1) {
            var blockInfo = GameData.instance.GetBlockInfo(blockName);
            AddNewPaletteItem(blockInfo, true);
        }
    }

    void OnGameBlockSelectChanged(Block newBlock, Block prevBlock) {
        var blockEdit = HUD.instance.blockMatterExpandPanel;

        if(newBlock)
            blockEdit.Show(newBlock);
        else if(prevBlock == blockEdit.block)
            blockEdit.Hide();
    }

    void OnWidgetRelease(PaletteItemWidget widget) {
        widget.releaseCallback -= OnWidgetRelease;

        mActiveWidgets.Remove(widget);
    }

    void OnAnimatorComplete(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {
        toggleButton.raycastTarget = true;

        switch(GameMapController.instance.mode) {
            case GameMapController.Mode.Play:
                toggleButton.transform.localScale = new Vector3(-1f, 1f, 1f);

                //hide active widgets on toggle hide end
                for(int i = 0; i < mActiveWidgets.Count; i++)
                    mActiveWidgets[i].gameObject.SetActive(false);
                break;

            case GameMapController.Mode.Edit:
                toggleButton.transform.localScale = new Vector3(1f, 1f, 1f);
                break;
        }
    }
}