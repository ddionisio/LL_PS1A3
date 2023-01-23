using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStage1 : MonoBehaviour {
    public GameObject blockAreaHintGO;

    private bool mHasDeployedBlock;
    private bool mIsEditTutorialFinished;

    private GameObject mTutorialPaletteItemDragGO;
    private GameObject mTutorialBlockControlGO;

    void OnDestroy() {
        if(GameMapController.instance) {
            GameMapController.instance.modeChangeCallback -= OnGameModeChanged;
            GameMapController.instance.blockGhostDroppedCallback -= OnGameBlockGhostDropped;
            GameMapController.instance.blockGhostCancelCallback -= OnGameBlockGhostCancel;
        }
    }

    void Start () {
        if(blockAreaHintGO)
            blockAreaHintGO.SetActive(false);

        mHasDeployedBlock = false;
        mIsEditTutorialFinished = false;

        GameMapController.instance.modeChangeCallback += OnGameModeChanged;
        GameMapController.instance.blockGhostDroppedCallback += OnGameBlockGhostDropped;
        GameMapController.instance.blockGhostCancelCallback += OnGameBlockGhostCancel;
    }

    IEnumerator DoEditTutorial() {
        float startTime = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup - startTime <= 0.5f)
            yield return null;

        //show palette block drop help
        if(!mTutorialPaletteItemDragGO)
            mTutorialPaletteItemDragGO = HUD.instance.GetMiscHUD("tutorialPaletteItemDrag");
        mTutorialPaletteItemDragGO.SetActive(true);

        if(blockAreaHintGO)
            blockAreaHintGO.SetActive(true);
                
        //wait until block from palette has been dropped
        while(!mHasDeployedBlock)
            yield return null;

        mTutorialPaletteItemDragGO.SetActive(false);

        //show block control help
        if(!mTutorialBlockControlGO)
            mTutorialBlockControlGO = HUD.instance.GetMiscHUD("tutorialBlockControl");
        mTutorialBlockControlGO.SetActive(true);
    }

    void OnGameBlockGhostDropped(Block blockGhost) {
        mHasDeployedBlock = true;

        var playReadyDescGO = HUD.instance.GetMiscHUD("tutorialPlayReady");
        playReadyDescGO.SetActive(true);
    }

    void OnGameBlockGhostCancel(string blockName) {
        mHasDeployedBlock = false;

        if(!mIsEditTutorialFinished) {
            if(mTutorialBlockControlGO)
                mTutorialBlockControlGO.SetActive(false);

            var playReadyDescGO = HUD.instance.GetMiscHUD("tutorialPlayReady");
            playReadyDescGO.SetActive(false);

            StopAllCoroutines();
            StartCoroutine(DoEditTutorial());
        }
    }

    void OnGameModeChanged(GameMapController.Mode mode) {
        var goalGO = HUD.instance.GetMiscHUD("goal");
        var restartDescGO = HUD.instance.GetMiscHUD("tutorialRestart");

        switch(mode) {
            case GameMapController.Mode.Edit:
                if(!mIsEditTutorialFinished)
                    StartCoroutine(DoEditTutorial());
                else {
                    if(mTutorialBlockControlGO)
                        mTutorialBlockControlGO.SetActive(true);
                }
                
                goalGO.SetActive(false);
                restartDescGO.SetActive(false);
                break;

            case GameMapController.Mode.Play:                
                if(mTutorialPaletteItemDragGO)
                    mTutorialPaletteItemDragGO.SetActive(false);

                if(mTutorialBlockControlGO)
                    mTutorialBlockControlGO.SetActive(false);

                if(blockAreaHintGO)
                    blockAreaHintGO.SetActive(false);
                
                goalGO.SetActive(true);
                restartDescGO.SetActive(true);

                var playReadyDescGO = HUD.instance.GetMiscHUD("tutorialPlayReady");
                playReadyDescGO.SetActive(false);

                if(mHasDeployedBlock)
                    mIsEditTutorialFinished = true;
                break;
        }
    }
}
