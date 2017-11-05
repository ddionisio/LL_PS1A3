using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoLStart : MonoBehaviour {
    public GameObject loadingGO;
    public GameObject loadingDoneGO;

    public M8.SceneAssetPath toNextScene;

    private void Awake() {
        if(loadingGO)
            loadingGO.SetActive(true);

        if(loadingDoneGO)
            loadingDoneGO.SetActive(false);
    }

    void OnGameReady(LoLManager mgr) {
        if(loadingGO)
            loadingGO.SetActive(false);

        if(loadingDoneGO)
            loadingDoneGO.SetActive(true);

        if(!string.IsNullOrEmpty(toNextScene.path))
            toNextScene.Load();
    }
}
