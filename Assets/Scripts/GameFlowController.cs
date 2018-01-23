using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowController {

    public static void LoadCurrentProgressScene() {
        var nextScene = GameData.instance.GetSceneFromCurrentProgress();

        nextScene.Load();
    }

    public static void ProgressAndLoadNextScene() {
        LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1, GameData.instance.currentScore);

        LoadCurrentProgressScene();
    }

    public static void Complete() {
        LoLManager.instance.Complete();
    }
}
