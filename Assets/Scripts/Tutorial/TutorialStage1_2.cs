using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStage1_2 : MonoBehaviour {
    public string dragScreenDescExitTake = "exit";
    public float cameraMoveThreshold = 0.5f;

    private bool mHasStarted;

    void OnDestroy() {
        if(GameMapController.instance) {
            GameMapController.instance.modeChangeCallback -= OnGameModeChanged;
        }
    }
    
    void Start() {
        GameMapController.instance.modeChangeCallback += OnGameModeChanged;
    }

    IEnumerator DoTutorial() {
        var dragScreenDescGO = HUD.instance.GetMiscHUD("tutorialDragScreen");
        dragScreenDescGO.SetActive(true);

        //wait for camera to move slightly
        var gameCam = GameCamera.instance;

        float cameraMoveThresholdSqr = cameraMoveThreshold * cameraMoveThreshold;

        Vector2 lastGameCamPos = gameCam.transform.position;

        while(true) {
            Vector2 curGameCamPos = gameCam.transform.position;

            float dist = (curGameCamPos - lastGameCamPos).sqrMagnitude;
            if(dist >= cameraMoveThresholdSqr)
                break;

            yield return null;
        }

        var anim = dragScreenDescGO.GetComponentInChildren<M8.Animator.AnimatorData>();
        anim.Play(dragScreenDescExitTake);

        while(anim.isPlaying)
            yield return null;

        dragScreenDescGO.SetActive(false);

        GameMapController.instance.modeChangeCallback -= OnGameModeChanged;
    }

    void OnGameModeChanged(GameMapController.Mode mode) {
        switch(mode) {
            case GameMapController.Mode.Edit:
                if(!mHasStarted) {
                    mHasStarted = true;
                    StartCoroutine(DoTutorial());
                }
                break;
        }
    }
}