using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowController : M8.SingletonBehaviour<GameFlowController> {
    public M8.SceneAssetPath startScene;
    
    public static void LoadCurrentProgressScene() {
        var nextScene = GameData.instance.GetSceneFromCurrentProgress();

        nextScene.Load();
    }

    public static void ProgressAndLoadNextScene() {
        LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1, GameData.instance.currentScore);

        LoadCurrentProgressScene();
    }

    protected override void OnInstanceInit() {
        
    }

    void Start() {
        //load start
        startScene.Load();
    }
}
