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
        }
    }

    void Start () {
        if(blockAreaHintGO)
            blockAreaHintGO.SetActive(false);

        mHasDeployedBlock = false;
        mIsEditTutorialFinished = false;

        GameMapController.instance.modeChangeCallback += OnGameModeChanged;
        GameMapController.instance.blockGhostDroppedCallback += OnGameBlockGhostDropped;
    }

    IEnumerator DoEditTutorial() {
        float startTime = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup - startTime <= 1.0f)
            yield return null;

        //show palette block drop help
        mTutorialPaletteItemDragGO = HUD.instance.GetMiscHUD("tutorialPaletteItemDrag");
        mTutorialPaletteItemDragGO.SetActive(true);

        if(blockAreaHintGO)
            blockAreaHintGO.SetActive(true);
                
        //wait until block from palette has been dropped
        while(!mHasDeployedBlock)
            yield return null;

        mTutorialPaletteItemDragGO.SetActive(false);

        //show block control help
        mTutorialBlockControlGO = HUD.instance.GetMiscHUD("tutorialBlockControl");
        mTutorialBlockControlGO.SetActive(true);

        mIsEditTutorialFinished = true;
    }

    void OnGameBlockGhostDropped(Block blockGhost) {
        mHasDeployedBlock = true;
    }

    void OnGameModeChanged(GameMapController.Mode mode) {
        switch(mode) {
            case GameMapController.Mode.Edit:
                if(!mIsEditTutorialFinished)
                    StartCoroutine(DoEditTutorial());
                break;

            case GameMapController.Mode.Play:                
                if(mTutorialPaletteItemDragGO)
                    mTutorialPaletteItemDragGO.SetActive(false);

                if(mTutorialBlockControlGO)
                    mTutorialBlockControlGO.SetActive(false);

                if(blockAreaHintGO)
                    blockAreaHintGO.SetActive(false);

                var goalGO = HUD.instance.GetMiscHUD("goal");
                goalGO.SetActive(true);
                break;
        }
    }
}
